using System.Collections;
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
        ResolvingFailure,
        GameOver
    }

    [Header("Core References")]
    [SerializeField] private TimingController timingController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private AudioManager audioManager;

    [Header("Game Rules")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private int continueBonusChances = 1;
    [SerializeField] private float gameOverDelaySeconds = 0.75f;

    [Header("Reset Scores Safety")]
    [SerializeField] private TMP_Text resetScoresButtonLabel;
    [SerializeField] private TMP_Text resetScoresHintLabel;
    [SerializeField] private string resetButtonDefaultText = "Reset Scores";
    [SerializeField] private string resetButtonConfirmText = "Tap Again to Confirm";
    [SerializeField] private string resetHintConfirmText = "Tap again within 3 seconds to reset scores.";
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
    private bool resetConfirmationArmed;
    private float resetConfirmationExpiresAt;

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

        if (timingController.CurrentFillAmount > timingController.PerfectEnd)
        {
            HandleFailure(GetRandomMessage(tooLateMessages, "Too Late"));
            return;
        }

        if (inputHandler != null && inputHandler.GetTapDown())
        {
            EvaluateTap();
        }
    }

    public void OnPlayPressed()
    {
        ClearResetConfirmation();
        StopPendingGameOverRoutine();
        score = 0;
        lives = Mathf.Max(1, startingLives);
        remainingContinueChances = continueBonusChances;
        currentState = GameState.Playing;

        uiManager.ShowGame(score, lives);
        timingController.Begin();
        if (audioManager != null)
        {
            audioManager.PlayGameplayMusic();
        }
    }

    public void OnTryAgainPressed()
    {
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
        timingController.Begin();
        if (audioManager != null)
        {
            audioManager.PlayGameplayMusic();
        }
    }

    public void OnBackToMenuPressed()
    {
        OpenMainMenu();
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
        uiManager.SetSettingsPanelVisible(false);
    }

    public void OnResetScoresPressed()
    {
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

    public void ConfigureLocalization(
        string nextResetDefault,
        string nextResetConfirm,
        string nextResetHintConfirm,
        string[] nextPerfectMessages,
        string[] nextStreakMessages,
        string[] nextTooEarlyMessages,
        string[] nextTooLateMessages)
    {
        if (!string.IsNullOrWhiteSpace(nextResetDefault)) resetButtonDefaultText = nextResetDefault;
        if (!string.IsNullOrWhiteSpace(nextResetConfirm)) resetButtonConfirmText = nextResetConfirm;
        if (!string.IsNullOrWhiteSpace(nextResetHintConfirm)) resetHintConfirmText = nextResetHintConfirm;
        if (nextPerfectMessages != null && nextPerfectMessages.Length > 0) perfectMessages = nextPerfectMessages;
        if (nextStreakMessages != null && nextStreakMessages.Length > 0) streakMessages = nextStreakMessages;
        if (nextTooEarlyMessages != null && nextTooEarlyMessages.Length > 0) tooEarlyMessages = nextTooEarlyMessages;
        if (nextTooLateMessages != null && nextTooLateMessages.Length > 0) tooLateMessages = nextTooLateMessages;

        ClearResetConfirmation();
    }

    public void RefreshLocalizedTexts()
    {
        switch (currentState)
        {
            case GameState.MainMenu:
                uiManager.RefreshMainMenuScores(bestScore, lastScore);
                break;
            case GameState.Playing:
            case GameState.ResolvingFailure:
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
        StopPendingGameOverRoutine();
        currentState = GameState.MainMenu;
        timingController.Clear();
        uiManager.ShowMainMenu(bestScore, lastScore);
        if (audioManager != null)
        {
            audioManager.PlayMenuMusic();
        }
    }

    private void EvaluateTap()
    {
        float fillAmount = timingController.CurrentFillAmount;

        if (fillAmount < timingController.PerfectStart)
        {
            HandleFailure(GetRandomMessage(tooEarlyMessages, "Too Early"));
            return;
        }

        if (fillAmount > timingController.PerfectEnd)
        {
            HandleFailure(GetRandomMessage(tooLateMessages, "Too Late"));
            return;
        }

        HandlePerfect();
    }

    private void HandlePerfect()
    {
        score++;
        uiManager.UpdateScore(score);
        uiManager.ShowFeedback(GetPerfectFeedbackMessage(), SuccessFeedbackColor, true);
        uiManager.FlashSuccess();
        uiManager.PlaySuccessSfx();
        if (audioManager != null)
        {
            audioManager.PlayPerfectSfx();
        }
        timingController.ResetCycle(true);
    }

    private void HandleFailure(string feedback)
    {
        lives = Mathf.Max(0, lives - 1);
        uiManager.AnimateLifeLoss(lives);
        uiManager.ShowFeedback(feedback, ErrorFeedbackColor, true);
        uiManager.FlashFailure();
        uiManager.PlayFailSfx();
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

    private void ClearResetConfirmation()
    {
        resetConfirmationArmed = false;
        resetConfirmationExpiresAt = 0f;
        UpdateResetButtonLabel(resetButtonDefaultText);
        UpdateResetHintLabel(string.Empty);
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

    private void ProcessResetConfirmationTimeout()
    {
        if (!resetConfirmationArmed)
        {
            return;
        }

        if (Time.unscaledTime > resetConfirmationExpiresAt)
        {
            ClearResetConfirmation();
        }
    }
}
