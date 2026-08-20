using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string BestScoreKey = "BestScore";
    private const string LastScoreKey = "LastScore";
    private static readonly Color32 SuccessFeedbackColor = new Color32(0x22, 0xC5, 0x5E, 0xFF);
    private static readonly Color32 ErrorFeedbackColor = new Color32(0xEF, 0x44, 0x44, 0xFF);

    private enum GameState
    {
        MainMenu,
        Playing,
        SpecialOrderPlaying,
        ResolvingFailure,
        GameOver
    }

    [Header("Core References")]
    [SerializeField] private TimingController timingController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SpecialOrderController specialOrderController;
    [SerializeField] private ProgressionService progressionService;
    [SerializeField] private LocalizationLite localizationLite;
    [SerializeField] private ProfilePanelController profilePanel;

    [Header("Game Rules")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private int continueBonusChances = 1;
    [SerializeField] private float gameOverDelaySeconds = 0.75f;
    [SerializeField] private float specialOrderTransitionDelaySeconds = 0.35f;

    [Header("Reset Scores Safety")]
    [SerializeField] private TMP_Text resetScoresButtonLabel;
    [SerializeField] private TMP_Text resetScoresHintLabel;
    [SerializeField] private string resetButtonDefaultText = "Reset Scores";
    [SerializeField] private string resetButtonConfirmText = "Tap Again to Confirm";
    [SerializeField] private string resetHintConfirmText = "Tap again within 3 seconds to reset scores.";
    [SerializeField] private TMP_Text resetXpButtonLabel;
    [SerializeField] private TMP_Text resetXpHintLabel;
    [SerializeField] private string resetXpButtonDefaultText = "Reset XP";
    [SerializeField] private string resetXpButtonConfirmText = "Tap Again to Confirm";
    [SerializeField] private string resetXpHintConfirmText = "Tap again within 3 seconds to reset XP.";
    [SerializeField] private float resetConfirmWindowSeconds = 3f;

    [Header("Feedback Variations")]
    [SerializeField] private string[] perfectMessages = { "Perfect", "Great", "Nice", "Awesome", "Clean Tap" };
    [SerializeField] private string[] streakMessages = { "On Fire!", "Unstoppable!", "Godlike!", "Legend!" };
    [SerializeField] private string[] tooEarlyMessages = { "Too Early", "Early", "Bit Early", "Wait More" };
    [SerializeField] private string[] tooLateMessages = { "Too Late", "Late", "Bit Late", "Missed It" };

    private GameState currentState;
    private int score;
    private int lives;
    private int lastScore;
    private int bestScore;
    private int remainingContinueChances;
    private Coroutine gameOverRoutine;
    private Coroutine specialOrderFlowRoutine;
    private bool resetConfirmationArmed;
    private bool resetXpConfirmationArmed;
    private float resetConfirmationExpiresAt;
    private float resetXpConfirmationExpiresAt;
    private readonly HashSet<int> triggeredSpecialOrders = new HashSet<int>();
    private bool runXpFinalized;

    private void Awake()
    {
        if (progressionService == null)
        {
            progressionService = GetComponent<ProgressionService>();
        }
    }

    private void Start()
    {
        lastScore = PlayerPrefs.GetInt(LastScoreKey, 0);
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        OpenMainMenu();
    }

    private void Update()
    {
        ProcessResetConfirmationTimeout();

        if (currentState != GameState.Playing)
        {
            return;
        }

        if (timingController.HasFullHoldExpired)
        {
            HandleFailure(GetRandomMessage(tooLateMessages, "Too Late"));
            return;
        }

        if (inputHandler != null && inputHandler.GetTapDown())
        {
            if (!timingController.IsInputArmed)
            {
                return;
            }

            EvaluateTap();
        }
    }

    public void OnPlayPressed()
    {
        ClearResetConfirmation();
        StopPendingGameOverRoutine();
        StopSpecialOrderFlowRoutine();
        triggeredSpecialOrders.Clear();
        runXpFinalized = false;
        int startScore = progressionService != null ? progressionService.GetStartScoreForCurrentProgress() : 0;
        score = startScore;
        lives = Mathf.Max(1, startingLives);
        remainingContinueChances = continueBonusChances;
        currentState = GameState.Playing;

        uiManager.ShowGame(score, lives);
        timingController.ApplyScoreDifficulty(score);
        timingController.Begin();
        if (audioManager != null)
        {
            audioManager.PlayGameplayMusic();
        }
    }

    public void OnTryAgainPressed()
    {
        FinalizeRunXpIfNeeded();
        OnPlayPressed();
    }

    public void OnContinuePressed()
    {
        if (currentState != GameState.GameOver || remainingContinueChances <= 0)
        {
            return;
        }

        StopPendingGameOverRoutine();
        remainingContinueChances--;
        lives = Mathf.Clamp(lives + 1, 1, Mathf.Max(1, startingLives));
        currentState = GameState.Playing;
        uiManager.ShowGame(score, lives);
        timingController.ApplyScoreDifficulty(score);
        timingController.Begin();
        if (audioManager != null)
        {
            audioManager.PlayGameplayMusic();
        }
    }

    public void OnBackToMenuPressed()
    {
        if (currentState == GameState.GameOver)
        {
            FinalizeRunXpIfNeeded();
        }

        OpenMainMenu();
    }

    public void OnProfileHeaderClicked()
    {
        if (currentState != GameState.MainMenu || profilePanel == null)
        {
            return;
        }

        profilePanel.Open();
    }

    public void RefreshProgressionHeaderUi()
    {
        if (progressionService == null || uiManager == null)
        {
            return;
        }

        ProgressionSnapshot snap = progressionService.BuildSnapshot();
        string rankName = localizationLite != null
            ? localizationLite.GetRankNameForLevel(snap.Level)
            : $"Rank {snap.Level}";
        string levelLine = localizationLite != null
            ? localizationLite.FormatLevelRankLine(snap.Level, rankName)
            : $"Level {snap.Level} - {rankName}";
        string xpLine;
        if (snap.Level >= ProgressionData.MaxLevel)
        {
            xpLine = localizationLite != null ? localizationLite.GetMaxLevelXpLabel() : "MAX";
        }
        else
        {
            xpLine = localizationLite != null
                ? localizationLite.FormatXpProgressLine(snap.XpIntoCurrentLevel, snap.XpNeededForNext)
                : $"{snap.XpIntoCurrentLevel} / {snap.XpNeededForNext}";
        }

        Sprite avatarSprite = uiManager.GetProfileAvatarSprite(snap.SelectedAvatarIndex);
        uiManager.UpdateProfileProgressHeader(snap, levelLine, xpLine, avatarSprite);
    }

    public void OnOpenSettingsPressed()
    {
        if (currentState != GameState.MainMenu)
        {
            return;
        }

        uiManager.SetSettingsPanelVisible(true);
    }

    public void OnCloseSettingsPressed()
    {
        if (currentState != GameState.MainMenu)
        {
            return;
        }

        ClearResetConfirmation();
        ClearResetXpConfirmation();
        uiManager.SetSettingsPanelVisible(false);
    }

    public void OnResetScoresPressed()
    {
        if (resetXpConfirmationArmed)
        {
            ClearResetXpConfirmation();
        }

        if (!resetConfirmationArmed || Time.unscaledTime > resetConfirmationExpiresAt)
        {
            resetConfirmationArmed = true;
            resetConfirmationExpiresAt = Time.unscaledTime + Mathf.Max(0.5f, resetConfirmWindowSeconds);
            UpdateResetButtonLabel(resetButtonConfirmText);
            UpdateResetHintLabel(resetHintConfirmText);
            return;
        }

        StopPendingGameOverRoutine();
        timingController.Stop();

        score = 0;
        lives = Mathf.Max(1, startingLives);
        lastScore = 0;
        bestScore = 0;
        remainingContinueChances = continueBonusChances;

        PlayerPrefs.SetInt(BestScoreKey, 0);
        PlayerPrefs.SetInt(LastScoreKey, 0);
        PlayerPrefs.Save();

        ClearResetConfirmation();
        OpenMainMenu();
    }

    public void OnResetXpPressed()
    {
        if (currentState != GameState.MainMenu)
        {
            return;
        }

        if (resetConfirmationArmed)
        {
            ClearResetConfirmation();
        }

        if (!resetXpConfirmationArmed || Time.unscaledTime > resetXpConfirmationExpiresAt)
        {
            resetXpConfirmationArmed = true;
            resetXpConfirmationExpiresAt = Time.unscaledTime + Mathf.Max(0.5f, resetConfirmWindowSeconds);
            UpdateResetXpButtonLabel(resetXpButtonConfirmText);
            UpdateResetXpHintLabel(resetXpHintConfirmText);
            return;
        }

        if (progressionService != null)
        {
            progressionService.ResetProgressionToDefaults();
        }

        runXpFinalized = false;
        ClearResetXpConfirmation();
        RefreshProgressionHeaderUi();
        if (profilePanel != null)
        {
            profilePanel.RefreshAllSlots();
        }
    }

    public void ConfigureLocalization(
        string nextResetDefault,
        string nextResetConfirm,
        string nextResetHintConfirm,
        string nextResetXpDefault,
        string nextResetXpConfirm,
        string nextResetXpHintConfirm,
        string[] nextPerfectMessages,
        string[] nextStreakMessages,
        string[] nextTooEarlyMessages,
        string[] nextTooLateMessages)
    {
        if (!string.IsNullOrWhiteSpace(nextResetDefault)) resetButtonDefaultText = nextResetDefault;
        if (!string.IsNullOrWhiteSpace(nextResetConfirm)) resetButtonConfirmText = nextResetConfirm;
        if (!string.IsNullOrWhiteSpace(nextResetHintConfirm)) resetHintConfirmText = nextResetHintConfirm;
        if (!string.IsNullOrWhiteSpace(nextResetXpDefault)) resetXpButtonDefaultText = nextResetXpDefault;
        if (!string.IsNullOrWhiteSpace(nextResetXpConfirm)) resetXpButtonConfirmText = nextResetXpConfirm;
        if (!string.IsNullOrWhiteSpace(nextResetXpHintConfirm)) resetXpHintConfirmText = nextResetXpHintConfirm;
        if (nextPerfectMessages != null && nextPerfectMessages.Length > 0) perfectMessages = nextPerfectMessages;
        if (nextStreakMessages != null && nextStreakMessages.Length > 0) streakMessages = nextStreakMessages;
        if (nextTooEarlyMessages != null && nextTooEarlyMessages.Length > 0) tooEarlyMessages = nextTooEarlyMessages;
        if (nextTooLateMessages != null && nextTooLateMessages.Length > 0) tooLateMessages = nextTooLateMessages;

        ClearResetConfirmation();
        ClearResetXpConfirmation();
    }

    public void RefreshLocalizedTexts()
    {
        if (profilePanel != null)
        {
            profilePanel.RefreshAllSlots();
        }

        switch (currentState)
        {
            case GameState.MainMenu:
                uiManager.RefreshMainMenuScores(bestScore, lastScore);
                RefreshProgressionHeaderUi();
                break;
            case GameState.Playing:
            case GameState.ResolvingFailure:
            case GameState.SpecialOrderPlaying:
                uiManager.RefreshGameHud(score, lives);
                break;
            case GameState.GameOver:
                uiManager.RefreshGameOverScores(score, bestScore);
                break;
        }
    }

    private void OpenMainMenu()
    {
        ClearResetConfirmation();
        ClearResetXpConfirmation();
        StopPendingGameOverRoutine();
        StopSpecialOrderFlowRoutine();
        if (specialOrderController != null) specialOrderController.ResetToIdle();
        uiManager.ExitSpecialOrder();
        currentState = GameState.MainMenu;
        timingController.Clear();
        uiManager.ShowMainMenu(bestScore, lastScore);
        RefreshProgressionHeaderUi();
        if (audioManager != null)
        {
            audioManager.PlayMenuMusic();
        }
    }

    private void FinalizeRunXpIfNeeded()
    {
        if (runXpFinalized || progressionService == null)
        {
            return;
        }

        runXpFinalized = true;
        progressionService.ApplyRunXp(score);
    }

    private void EvaluateTap()
    {
        float fillAmount = timingController.CurrentFillAmount;

        if (fillAmount < timingController.PerfectStart)
        {
            HandleFailure(GetRandomMessage(tooEarlyMessages, "Too Early"));
            return;
        }

        // Too Late is handled in Update before tap polling; remaining window is Perfect.
        HandlePerfect();
    }

    private void HandlePerfect()
    {
        score++;
        timingController.ApplyScoreDifficulty(score);
        uiManager.UpdateScore(score);
        uiManager.ShowFeedback(GetPerfectFeedbackMessage(), SuccessFeedbackColor, true);
        uiManager.FlashSuccess();
        if (audioManager != null)
        {
            audioManager.PlayPerfectSfx();
        }

        if (IsSpecialOrderTriggerScore(score) && !triggeredSpecialOrders.Contains(score))
        {
            triggeredSpecialOrders.Add(score);
            timingController.Stop();
            currentState = GameState.SpecialOrderPlaying;
            StopSpecialOrderFlowRoutine();
            specialOrderFlowRoutine = StartCoroutine(CoStartSpecialOrder(score));
            return;
        }

        timingController.ResetCycle(true);
    }

    private static bool IsSpecialOrderTriggerScore(int s)
    {
        if (s == 3 || s == 30 || s == 50) return true;
        if (s >= 100 && s % 50 == 0) return true;
        return false;
    }

    private static float GetSpecialOrderTimeForScore(int triggerScore)
    {
        if (triggerScore == 3) return 20f;
        if (triggerScore == 30) return 15f;
        return 10f;
    }

    private IEnumerator CoStartSpecialOrder(int triggerScore)
    {
        float transitionDelay = Mathf.Max(0f, specialOrderTransitionDelaySeconds);
        if (transitionDelay > 0f)
        {
            yield return new WaitForSeconds(transitionDelay);
        }

        if (audioManager != null)
        {
            audioManager.PlaySpecialOrderStart();
        }

        yield return uiManager.PlaySpecialOrderIntro();
        if (specialOrderController == null)
        {
            currentState = GameState.Playing;
            timingController.Begin();
            yield break;
        }

        float t = GetSpecialOrderTimeForScore(triggerScore);
        specialOrderController.BeginRound(
            t,
            () =>
            {
                StopSpecialOrderFlowRoutine();
                specialOrderFlowRoutine = StartCoroutine(CoSpecialOrderSuccess());
            },
            () =>
            {
                StopSpecialOrderFlowRoutine();
                specialOrderFlowRoutine = StartCoroutine(CoSpecialOrderFail());
            },
            uiManager.SetSpecialOrderTimer);
    }

    private IEnumerator CoSpecialOrderSuccess()
    {
        if (specialOrderController != null)
        {
            specialOrderController.ForceStop();
            specialOrderController.ResetToIdle();
        }

        if (audioManager != null)
        {
            audioManager.PlaySpecialOrderSuccess();
        }

        score++;
        timingController.ApplyScoreDifficulty(score);
        uiManager.UpdateScore(score);
        yield return uiManager.PlayNiceEffect();
        currentState = GameState.Playing;
        uiManager.ExitSpecialOrder();
        timingController.Begin();
        if (audioManager != null)
        {
            audioManager.PlayGameplayMusic();
        }
    }

    private IEnumerator CoSpecialOrderFail()
    {
        if (specialOrderController != null)
        {
            specialOrderController.ForceStop();
            specialOrderController.ResetToIdle();
        }

        lives = 0;
        currentState = GameState.ResolvingFailure;
        timingController.Stop();
        lastScore = score;
        PlayerPrefs.SetInt(LastScoreKey, lastScore);
        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
        }

        PlayerPrefs.Save();
        StopPendingGameOverRoutine();

        if (audioManager != null)
        {
            audioManager.PlayTimeUpAlarmSfx();
        }

        yield return uiManager.PlayTimeUpEffect();

        uiManager.ExitSpecialOrder();
        currentState = GameState.GameOver;
        uiManager.ShowGameOver(score, bestScore, remainingContinueChances > 0);
        if (remainingContinueChances <= 0)
        {
            FinalizeRunXpIfNeeded();
        }

        specialOrderFlowRoutine = null;
    }

    private void HandleFailure(string feedback)
    {
        lives = Mathf.Max(0, lives - 1);
        uiManager.AnimateLifeLoss(lives);
        uiManager.ShowFeedback(feedback, ErrorFeedbackColor, true);
        uiManager.FlashFailure();
        uiManager.PlayWrongTimeSplash();
        if (audioManager != null)
        {
            audioManager.PlayGameOverSfx();
        }
        if (lives > 0)
        {
            timingController.ResetCycle(true);
            return;
        }

        currentState = GameState.ResolvingFailure;
        timingController.Stop();
        lastScore = score;
        PlayerPrefs.SetInt(LastScoreKey, lastScore);

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
        }

        PlayerPrefs.Save();

        StopPendingGameOverRoutine();
        gameOverRoutine = StartCoroutine(ShowGameOverWithDelay());
    }

    private string GetRandomMessage(string[] pool, string fallback)
    {
        if (pool == null || pool.Length == 0)
        {
            return fallback;
        }

        int randomIndex = Random.Range(0, pool.Length);
        string picked = pool[randomIndex];
        return string.IsNullOrWhiteSpace(picked) ? fallback : picked;
    }

    private string GetPerfectFeedbackMessage()
    {
        if (score >= 8)
        {
            return GetRandomMessage(streakMessages, "On Fire!");
        }

        return GetRandomMessage(perfectMessages, "Perfect");
    }

    private IEnumerator ShowGameOverWithDelay()
    {
        float delay = Mathf.Max(0f, gameOverDelaySeconds);
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        currentState = GameState.GameOver;
        uiManager.ShowGameOver(score, bestScore, remainingContinueChances > 0);
        if (remainingContinueChances <= 0)
        {
            FinalizeRunXpIfNeeded();
        }

        gameOverRoutine = null;
    }

    private void StopPendingGameOverRoutine()
    {
        if (gameOverRoutine != null)
        {
            StopCoroutine(gameOverRoutine);
            gameOverRoutine = null;
        }
    }

    private void StopSpecialOrderFlowRoutine()
    {
        if (specialOrderFlowRoutine != null)
        {
            StopCoroutine(specialOrderFlowRoutine);
            specialOrderFlowRoutine = null;
        }
    }

    private void ClearResetConfirmation()
    {
        resetConfirmationArmed = false;
        resetConfirmationExpiresAt = 0f;
        UpdateResetButtonLabel(resetButtonDefaultText);
        UpdateResetHintLabel(string.Empty);
    }

    private void ClearResetXpConfirmation()
    {
        resetXpConfirmationArmed = false;
        resetXpConfirmationExpiresAt = 0f;
        UpdateResetXpButtonLabel(resetXpButtonDefaultText);
        UpdateResetXpHintLabel(string.Empty);
    }

    private void UpdateResetButtonLabel(string value)
    {
        if (resetScoresButtonLabel != null)
        {
            resetScoresButtonLabel.text = value;
        }
    }

    private void UpdateResetHintLabel(string value)
    {
        if (resetScoresHintLabel != null)
        {
            resetScoresHintLabel.text = value;
        }
    }

    private void UpdateResetXpButtonLabel(string value)
    {
        if (resetXpButtonLabel != null)
        {
            resetXpButtonLabel.text = value;
        }
    }

    private void UpdateResetXpHintLabel(string value)
    {
        if (resetXpHintLabel != null)
        {
            resetXpHintLabel.text = value;
        }
    }

    private void ProcessResetConfirmationTimeout()
    {
        if (!resetConfirmationArmed && !resetXpConfirmationArmed)
        {
            return;
        }

        if (resetConfirmationArmed && Time.unscaledTime > resetConfirmationExpiresAt)
        {
            ClearResetConfirmation();
        }

        if (resetXpConfirmationArmed && Time.unscaledTime > resetXpConfirmationExpiresAt)
        {
            ClearResetXpConfirmation();
        }
    }
}
