using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Main Menu")]
    [SerializeField] private TMP_Text menuBestScoreText;
    [SerializeField] private TMP_Text menuLastScoreText;

    [Header("Main Menu — Progression Header")]
    [SerializeField] private GameObject profileHeaderRoot;
    [SerializeField] private Button profileOpenButton;
    [SerializeField] private Image profileAvatarImage;
    [SerializeField] private TMP_Text profileLevelRankText;
    [SerializeField] private TMP_Text profileXpProgressText;
    [SerializeField] private Image profileXpFillImage;
    [SerializeField] private Sprite[] profileAvatarSprites = new Sprite[25];

    [Header("Game UI")]
    [SerializeField] private GameObject classicGameHudRoot;
    [SerializeField] private GameObject specialOrderRoot;
    [SerializeField] private RectTransform counterTop;
    [SerializeField] private float counterIntroSlide = -420f;
    [SerializeField] private float counterIntroDuration = 0.4f;
    [SerializeField] private TMP_Text specialOrderBanner;
    [SerializeField] private TMP_Text specialOrderTimer;
    [SerializeField] private float specialOrderTitleHoldSeconds = 2f;
    [SerializeField] private GameObject specialOrderTouchBlocker;
    [SerializeField] private GameObject niceOverlay;
    [SerializeField] private TMP_Text niceText;
    [SerializeField] private float niceDisplaySeconds = 2.5f;
    [SerializeField] private string timerFormat = "{0:0.0}s";
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image[] lifeHeartImages;
    [SerializeField] private Sprite heartFullSprite;
    [SerializeField] private Sprite heartEmptySprite;
    [SerializeField] private float heartLoseAnimDuration = 0.16f;
    [SerializeField] private float heartLoseShakeStrength = 7f;
    [SerializeField] private float heartLoseFadeMinAlpha = 0.35f;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private RectTransform progressCircleRoot;

    [Header("Game Over UI")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text gameOverBestScoreText;
    [SerializeField] private Button continueButton;

    [Header("Flash Effect")]
    [SerializeField] private Image flashOverlay;
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private float flashAlpha = 0.35f;

    [Header("Wrong Time Splash")]
    [SerializeField] private Image wrongTimeSplashImage;
    [SerializeField] private float wrongTimeSplashHoldSeconds = 0.05f;
    [SerializeField] private float wrongTimeSplashFadeSeconds = 0.35f;

    [Header("Colors")]
    [SerializeField] private Color successColor = new Color32(0x22, 0xC5, 0x5E, 0xFF);
    [SerializeField] private Color errorColor = new Color32(0xEF, 0x44, 0x44, 0xFF);
    [SerializeField] private Color accentColor = new Color32(0x3B, 0x82, 0xF6, 0xFF);

    [Header("Localized Formats")]
    [SerializeField] private string bestScoreFormat = "Best Score: {0}";
    [SerializeField] private string lastScoreFormat = "Last Score: {0}";
    [SerializeField] private string scoreFormat = "Score: {0}";
    [SerializeField] private string finalScoreFormat = "Final Score: {0}";
    [SerializeField] private string gameOverBestScoreFormat = "Best Score: {0}";

    private Coroutine flashRoutine;
    private Coroutine wrongTimeSplashRoutine;
    private Coroutine pulseRoutine;
    private Coroutine heartLoseRoutine;
    private Coroutine specialIntroRoutine;
    private Vector2 _counterStartAnchored;
    private bool _cachedCounter;

    private void Awake()
    {
        GameManager gm = GetComponent<GameManager>();
        if (profileOpenButton != null && gm != null)
        {
            profileOpenButton.onClick.AddListener(gm.OnProfileHeaderClicked);
        }
    }

    public void ShowMainMenu(int bestScore, int lastScore)
    {
        SetPanelState(true, false, false);
        SetSettingsPanelVisible(false);
        SetText(menuBestScoreText, string.Format(bestScoreFormat, bestScore));
        SetText(menuLastScoreText, string.Format(lastScoreFormat, lastScore));
        SetText(feedbackText, string.Empty);
        HideWrongTimeSplash();
        if (profileHeaderRoot != null)
        {
            profileHeaderRoot.SetActive(true);
        }
    }

    public Sprite GetProfileAvatarSprite(int index)
    {
        index = Mathf.Clamp(index, 0, ProgressionData.AvatarCount - 1);
        if (profileAvatarSprites != null && index < profileAvatarSprites.Length && profileAvatarSprites[index] != null)
        {
            return profileAvatarSprites[index];
        }

        return heartFullSprite;
    }

    public void UpdateProfileProgressHeader(
        ProgressionSnapshot snap,
        string levelRankLine,
        string xpProgressLine,
        Sprite avatarSprite)
    {
        if (profileHeaderRoot != null)
        {
            profileHeaderRoot.SetActive(true);
        }

        if (profileAvatarImage != null)
        {
            if (avatarSprite != null)
            {
                profileAvatarImage.sprite = avatarSprite;
            }

            profileAvatarImage.color = Color.white;
        }

        if (profileLevelRankText != null)
        {
            profileLevelRankText.text = levelRankLine;
            profileLevelRankText.color = ProgressionData.GetTierColor(snap.Tier);
        }

        SetText(profileXpProgressText, xpProgressLine);

        if (profileXpFillImage != null)
        {
            float fill = snap.Level >= ProgressionData.MaxLevel
                ? 1f
                : snap.XpNeededForNext > 0
                    ? Mathf.Clamp01((float)snap.XpIntoCurrentLevel / snap.XpNeededForNext)
                    : 0f;
            profileXpFillImage.fillAmount = fill;
        }
    }

    public void ShowGame(int score, int lives)
    {
        SetPanelState(false, true, false);
        SetSettingsPanelVisible(false);
        if (specialOrderRoot != null) specialOrderRoot.SetActive(false);
        if (specialOrderTouchBlocker != null) specialOrderTouchBlocker.SetActive(false);
        if (niceOverlay != null) niceOverlay.SetActive(false);
        if (classicGameHudRoot != null) classicGameHudRoot.SetActive(true);
        HideWrongTimeSplash();
        UpdateScore(score);
        UpdateLives(lives);
        ShowFeedback(string.Empty, accentColor, false);
    }

    public void ShowGameOver(int finalScore, int bestScore, bool canContinue)
    {
        SetPanelState(false, false, true);
        SetSettingsPanelVisible(false);
        HideWrongTimeSplash();
        SetText(finalScoreText, string.Format(finalScoreFormat, finalScore));
        SetText(gameOverBestScoreText, string.Format(gameOverBestScoreFormat, bestScore));
        if (continueButton != null)
        {
            continueButton.interactable = canContinue;
            continueButton.gameObject.SetActive(true);
        }
    }

    public void UpdateScore(int score)
    {
        SetText(scoreText, string.Format(scoreFormat, score));
    }

    public void UpdateLives(int lives)
    {
        ApplyLivesVisuals(lives);
    }

    public void AnimateLifeLoss(int livesRemaining)
    {
        if (heartLoseRoutine != null)
        {
            StopCoroutine(heartLoseRoutine);
            heartLoseRoutine = null;
        }

        int lostHeartIndex = Mathf.Clamp(livesRemaining, 0, lifeHeartImages.Length - 1);
        heartLoseRoutine = StartCoroutine(AnimateLifeLossRoutine(livesRemaining, lostHeartIndex));
    }

    private string _niceMessage = "Nice!";

    public void SetSpecialOrderCopy(string title, string niceMessage)
    {
        if (!string.IsNullOrEmpty(title)) SetText(specialOrderBanner, title);
        if (!string.IsNullOrEmpty(niceMessage)) _niceMessage = niceMessage;
    }

    public IEnumerator PlaySpecialOrderIntro()
    {
        if (niceOverlay != null) niceOverlay.SetActive(false);
        if (classicGameHudRoot != null) classicGameHudRoot.SetActive(false);
        if (specialOrderRoot != null) specialOrderRoot.SetActive(true);
        if (specialOrderTouchBlocker != null) specialOrderTouchBlocker.SetActive(true);
        if (specialOrderBanner != null) specialOrderBanner.gameObject.SetActive(true);
        ClearSpecialOrderTimer();
        if (counterTop != null)
        {
            if (!_cachedCounter)
            {
                _counterStartAnchored = counterTop.anchoredPosition;
                _cachedCounter = true;
            }

            Vector2 end = _counterStartAnchored;
            Vector2 start = end + new Vector2(0f, counterIntroSlide);
            counterTop.anchoredPosition = start;
            float d = Mathf.Max(0.05f, counterIntroDuration);
            float e = 0f;
            while (e < d)
            {
                e += Time.deltaTime;
                float u = Mathf.Clamp01(e / d);
                counterTop.anchoredPosition = Vector2.Lerp(start, end, u);
                yield return null;
            }

            counterTop.anchoredPosition = end;
        }
        else
        {
            yield return null;
        }

        float titleHold = Mathf.Max(0f, specialOrderTitleHoldSeconds);
        if (titleHold > 0f)
        {
            yield return new WaitForSeconds(titleHold);
        }

        if (specialOrderBanner != null) specialOrderBanner.gameObject.SetActive(false);
        if (specialOrderTouchBlocker != null) specialOrderTouchBlocker.SetActive(false);
    }

    public void SetSpecialOrderTimer(float secondsRemaining)
    {
        if (specialOrderTimer == null)
        {
            return;
        }

        specialOrderTimer.text = string.Format(timerFormat, Mathf.Max(0f, secondsRemaining));
    }

    public void ClearSpecialOrderTimer()
    {
        if (specialOrderTimer != null) specialOrderTimer.text = string.Empty;
    }

    public IEnumerator PlayNiceEffect()
    {
        if (niceText != null) SetText(niceText, _niceMessage);
        if (niceOverlay != null) niceOverlay.SetActive(true);
        float w = Mathf.Max(0.1f, niceDisplaySeconds);
        yield return new WaitForSeconds(w);
        if (niceOverlay != null) niceOverlay.SetActive(false);
    }

    public void ExitSpecialOrder()
    {
        ClearSpecialOrderTimer();
        if (specialOrderRoot != null) specialOrderRoot.SetActive(false);
        if (specialOrderTouchBlocker != null) specialOrderTouchBlocker.SetActive(false);
        if (specialOrderBanner != null) specialOrderBanner.gameObject.SetActive(true);
        if (niceOverlay != null) niceOverlay.SetActive(false);
        if (classicGameHudRoot != null) classicGameHudRoot.SetActive(true);
        if (counterTop != null && _cachedCounter) counterTop.anchoredPosition = _counterStartAnchored;
    }

    public void ConfigureLocalizedFormats(
        string nextBestScoreFormat,
        string nextLastScoreFormat,
        string nextScoreFormat,
        string nextLivesFormat,
        string nextFinalScoreFormat,
        string nextGameOverBestScoreFormat)
    {
        bestScoreFormat = string.IsNullOrWhiteSpace(nextBestScoreFormat) ? bestScoreFormat : nextBestScoreFormat;
        lastScoreFormat = string.IsNullOrWhiteSpace(nextLastScoreFormat) ? lastScoreFormat : nextLastScoreFormat;
        scoreFormat = string.IsNullOrWhiteSpace(nextScoreFormat) ? scoreFormat : nextScoreFormat;
        finalScoreFormat = string.IsNullOrWhiteSpace(nextFinalScoreFormat) ? finalScoreFormat : nextFinalScoreFormat;
        gameOverBestScoreFormat = string.IsNullOrWhiteSpace(nextGameOverBestScoreFormat)
            ? gameOverBestScoreFormat
            : nextGameOverBestScoreFormat;
    }

    public void RefreshMainMenuScores(int bestScore, int lastScore)
    {
        SetText(menuBestScoreText, string.Format(bestScoreFormat, bestScore));
        SetText(menuLastScoreText, string.Format(lastScoreFormat, lastScore));
    }

    public void RefreshGameHud(int currentScore, int currentLives)
    {
        SetText(scoreText, string.Format(scoreFormat, currentScore));
        ApplyLivesVisuals(currentLives);
    }

    public void RefreshGameOverScores(int finalScore, int bestScore)
    {
        SetText(finalScoreText, string.Format(finalScoreFormat, finalScore));
        SetText(gameOverBestScoreText, string.Format(gameOverBestScoreFormat, bestScore));
    }

    public void SetSettingsPanelVisible(bool isVisible)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(isVisible);
        }
    }

    public void ShowFeedback(string message, Color color, bool animatePulse)
    {
        SetText(feedbackText, message);
        if (feedbackText != null)
        {
            feedbackText.color = color;
        }

        if (animatePulse)
        {
            PlayPulse();
        }
    }

    public void FlashSuccess()
    {
        PlayFlash(successColor);
    }

    public void FlashFailure()
    {
        PlayFlash(errorColor);
    }

    public void PlayWrongTimeSplash()
    {
        if (wrongTimeSplashImage == null)
        {
            return;
        }

        if (wrongTimeSplashRoutine != null)
        {
            StopCoroutine(wrongTimeSplashRoutine);
        }

        wrongTimeSplashRoutine = StartCoroutine(WrongTimeSplashRoutine());
    }

    public void PlaySuccessSfx()
    {
        // Placeholder for future audio hook.
    }

    public void PlayFailSfx()
    {
        // Placeholder for future audio hook.
    }

    private void SetPanelState(bool showMenu, bool showGame, bool showGameOver)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(showMenu);
        if (gamePanel != null) gamePanel.SetActive(showGame);
        if (gameOverPanel != null) gameOverPanel.SetActive(showGameOver);
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private void PlayFlash(Color color)
    {
        if (flashOverlay == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine(color));
    }

    private IEnumerator FlashRoutine(Color color)
    {
        flashOverlay.gameObject.SetActive(true);
        Color startColor = color;
        startColor.a = flashAlpha;
        flashOverlay.color = startColor;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);
            Color next = color;
            next.a = Mathf.Lerp(flashAlpha, 0f, t);
            flashOverlay.color = next;
            yield return null;
        }

        flashOverlay.gameObject.SetActive(false);
        flashRoutine = null;
    }

    private void HideWrongTimeSplash()
    {
        if (wrongTimeSplashRoutine != null)
        {
            StopCoroutine(wrongTimeSplashRoutine);
            wrongTimeSplashRoutine = null;
        }

        if (wrongTimeSplashImage == null)
        {
            return;
        }

        Color c = wrongTimeSplashImage.color;
        c.a = 0f;
        wrongTimeSplashImage.color = c;
        wrongTimeSplashImage.gameObject.SetActive(false);
    }

    private IEnumerator WrongTimeSplashRoutine()
    {
        wrongTimeSplashImage.gameObject.SetActive(true);
        Color opaque = wrongTimeSplashImage.color;
        opaque.a = 1f;
        wrongTimeSplashImage.color = opaque;

        float hold = Mathf.Max(0f, wrongTimeSplashHoldSeconds);
        if (hold > 0f)
        {
            yield return new WaitForSeconds(hold);
        }

        float fade = Mathf.Max(0.01f, wrongTimeSplashFadeSeconds);
        float elapsed = 0f;
        while (elapsed < fade)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fade);
            Color next = opaque;
            next.a = Mathf.Lerp(1f, 0f, t);
            wrongTimeSplashImage.color = next;
            yield return null;
        }

        HideWrongTimeSplash();
    }

    private void PlayPulse()
    {
        if (progressCircleRoot == null)
        {
            return;
        }

        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
        }

        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        const float duration = 0.12f;
        const float scaleUp = 1.08f;
        Vector3 originalScale = Vector3.one;
        progressCircleRoot.localScale = originalScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.Lerp(1f, scaleUp, t);
            progressCircleRoot.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.Lerp(scaleUp, 1f, t);
            progressCircleRoot.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        progressCircleRoot.localScale = Vector3.one;
        pulseRoutine = null;
    }

    private void ApplyLivesVisuals(int lives)
    {
        if (lifeHeartImages == null || lifeHeartImages.Length == 0)
        {
            return;
        }

        int clampedLives = Mathf.Clamp(lives, 0, lifeHeartImages.Length);
        for (int i = 0; i < lifeHeartImages.Length; i++)
        {
            Image heartImage = lifeHeartImages[i];
            if (heartImage == null)
            {
                continue;
            }

            bool isFull = i < clampedLives;
            if (heartFullSprite != null && heartEmptySprite != null)
            {
                heartImage.sprite = isFull ? heartFullSprite : heartEmptySprite;
            }

            heartImage.color = Color.white;
            heartImage.transform.localScale = Vector3.one;
        }
    }

    private IEnumerator AnimateLifeLossRoutine(int livesRemaining, int lostHeartIndex)
    {
        if (lifeHeartImages == null || lifeHeartImages.Length == 0 || lostHeartIndex < 0 || lostHeartIndex >= lifeHeartImages.Length)
        {
            ApplyLivesVisuals(livesRemaining);
            yield break;
        }

        Image lostHeartImage = lifeHeartImages[lostHeartIndex];
        if (lostHeartImage == null)
        {
            ApplyLivesVisuals(livesRemaining);
            yield break;
        }

        float duration = Mathf.Max(0.05f, heartLoseAnimDuration);
        float halfDuration = duration * 0.5f;
        float elapsed = 0f;
        RectTransform lostHeartRect = lostHeartImage.rectTransform;
        Vector2 originalAnchoredPos = lostHeartRect.anchoredPosition;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float scale = Mathf.Lerp(1f, 1.22f, t);
            lostHeartImage.transform.localScale = new Vector3(scale, scale, scale);
            lostHeartRect.anchoredPosition = originalAnchoredPos + Random.insideUnitCircle * heartLoseShakeStrength * (1f - t * 0.4f);
            SetImageAlpha(lostHeartImage, Mathf.Lerp(1f, heartLoseFadeMinAlpha, t));
            yield return null;
        }

        if (heartEmptySprite != null)
        {
            lostHeartImage.sprite = heartEmptySprite;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float scale = Mathf.Lerp(1.22f, 1f, t);
            lostHeartImage.transform.localScale = new Vector3(scale, scale, scale);
            lostHeartRect.anchoredPosition = originalAnchoredPos + Random.insideUnitCircle * heartLoseShakeStrength * 0.35f * (1f - t);
            SetImageAlpha(lostHeartImage, Mathf.Lerp(heartLoseFadeMinAlpha, 1f, t));
            yield return null;
        }

        lostHeartRect.anchoredPosition = originalAnchoredPos;
        ApplyLivesVisuals(livesRemaining);
        heartLoseRoutine = null;
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }
}
