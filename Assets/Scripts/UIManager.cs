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

    [Header("Game UI")]
    [SerializeField] private TMP_Text scoreText;
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
    private Coroutine pulseRoutine;

    public void ShowMainMenu(int bestScore, int lastScore)
    {
        SetPanelState(true, false, false);
        SetSettingsPanelVisible(false);
        SetText(menuBestScoreText, string.Format(bestScoreFormat, bestScore));
        SetText(menuLastScoreText, string.Format(lastScoreFormat, lastScore));
        SetText(feedbackText, string.Empty);
    }

    public void ShowGame(int score)
    {
        SetPanelState(false, true, false);
        SetSettingsPanelVisible(false);
        UpdateScore(score);
        ShowFeedback(string.Empty, accentColor, false);
    }

    public void ShowGameOver(int finalScore, int bestScore, bool canContinue)
    {
        SetPanelState(false, false, true);
        SetSettingsPanelVisible(false);
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

    public void ConfigureLocalizedFormats(
        string nextBestScoreFormat,
        string nextLastScoreFormat,
        string nextScoreFormat,
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

    public void RefreshGameScore(int currentScore)
    {
        SetText(scoreText, string.Format(scoreFormat, currentScore));
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
}
