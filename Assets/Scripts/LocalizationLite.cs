using TMPro;
using UnityEngine;

public class LocalizationLite : MonoBehaviour
{
    private enum SupportedLanguage
    {
        English,
        Turkish
    }

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;

    [Header("Menu Buttons")]
    [SerializeField] private TMP_Text playButtonLabel;
    [SerializeField] private TMP_Text settingsButtonLabel;
    [SerializeField] private TMP_Text settingsTitleLabel;
    [SerializeField] private TMP_Text languageTitleLabel;
    [SerializeField] private TMP_Text soundTitleLabel;
    [SerializeField] private TMP_Text musicLabel;
    [SerializeField] private TMP_Text sfxLabel;
    [SerializeField] private TMP_Text turkishButtonLabel;
    [SerializeField] private TMP_Text englishButtonLabel;
    [SerializeField] private TMP_Text tryAgainButtonLabel;
    [SerializeField] private TMP_Text continueButtonLabel;
    [SerializeField] private TMP_Text backToMenuButtonLabel;
    [SerializeField] private TMP_Text resetButtonLabel;

    [Header("Language")]
    [SerializeField] private SupportedLanguage defaultLanguage = SupportedLanguage.Turkish;

    private void Start()
    {
        Apply(defaultLanguage);
    }

    public void ApplyEnglish()
    {
        Apply(SupportedLanguage.English);
    }

    public void ApplyTurkish()
    {
        Apply(SupportedLanguage.Turkish);
    }

    private void Apply(SupportedLanguage language)
    {
        if (language == SupportedLanguage.Turkish)
        {
            SetLabel(playButtonLabel, "Oyna");
            SetLabel(settingsButtonLabel, "Ayarlar");
            SetLabel(settingsTitleLabel, "Ayarlar");
            SetLabel(languageTitleLabel, "Dil");
            SetLabel(soundTitleLabel, "Ses");
            SetLabel(musicLabel, "Müzik");
            SetLabel(sfxLabel, "Efekt");
            SetLabel(turkishButtonLabel, "Türkçe");
            SetLabel(englishButtonLabel, "İngilizce");
            SetLabel(tryAgainButtonLabel, "Tekrar Dene");
            SetLabel(continueButtonLabel, "Devam Et (+1 hak)");
            SetLabel(backToMenuButtonLabel, "Ana Menü");
            SetLabel(resetButtonLabel, "Skorları Sıfırla");

            if (uiManager != null)
            {
                uiManager.ConfigureLocalizedFormats(
                    "En İyi Skor: {0}",
                    "Son Skor: {0}",
                    "Skor: {0}",
                    "Can: {0}",
                    "Final Skor: {0}",
                    "En İyi Skor: {0}");
                uiManager.SetSpecialOrderCopy("Özel Sipariş", "Harika!");
            }

            if (gameManager != null)
            {
                gameManager.ConfigureLocalization(
                    "Skorları Sıfırla",
                    "Onay için tekrar dokun",
                    "Skorları sıfırlamak için 3 saniye içinde tekrar dokun.",
                    new[] { "Mükemmel", "Harika", "Süper", "Temiz Vuruş", "Nefis" },
                    new[] { "Alev Aldın!", "Durmak Yok!", "Efsane!", "Canavar Gibi!" },
                    new[] { "Çok Erken", "Erken", "Biraz Erken", "Biraz Bekle" },
                    new[] { "Çok Geç", "Geç", "Biraz Geç", "Kaçırdın" });
                gameManager.RefreshLocalizedTexts();
            }

            return;
        }

        SetLabel(playButtonLabel, "Play");
        SetLabel(settingsButtonLabel, "Settings");
        SetLabel(settingsTitleLabel, "Settings");
        SetLabel(languageTitleLabel, "Language");
        SetLabel(soundTitleLabel, "Sound");
        SetLabel(musicLabel, "Music");
        SetLabel(sfxLabel, "SFX");
        SetLabel(turkishButtonLabel, "Turkish");
        SetLabel(englishButtonLabel, "English");
        SetLabel(tryAgainButtonLabel, "Try Again");
        SetLabel(continueButtonLabel, "Continue (+1 chance)");
        SetLabel(backToMenuButtonLabel, "Main Menu");
        SetLabel(resetButtonLabel, "Reset Scores");

        if (uiManager != null)
        {
            uiManager.ConfigureLocalizedFormats(
                "Best Score: {0}",
                "Last Score: {0}",
                "Score: {0}",
                "Lives: {0}",
                "Final Score: {0}",
                "Best Score: {0}");
            uiManager.SetSpecialOrderCopy("Special Order", "Nice!");
        }

        if (gameManager != null)
        {
            gameManager.ConfigureLocalization(
                "Reset Scores",
                "Tap Again to Confirm",
                "Tap again within 3 seconds to reset scores.",
                new[] { "Perfect", "Great", "Nice", "Awesome", "Clean Tap" },
                new[] { "On Fire!", "Unstoppable!", "Godlike!", "Legend!" },
                new[] { "Too Early", "Early", "Bit Early", "Wait More" },
                new[] { "Too Late", "Late", "Bit Late", "Missed It" });
            gameManager.RefreshLocalizedTexts();
        }
    }

    private void SetLabel(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
