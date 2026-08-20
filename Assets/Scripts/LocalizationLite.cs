using TMPro;
using UnityEngine;

public class LocalizationLite : MonoBehaviour
{
    private const string LanguagePrefKey = "settings.language";

    private enum SupportedLanguage
    {
        English,
        Turkish
    }

    private static readonly string[] RankNamesTr =
    {
        "Stajyer",
        "Çırak",
        "Yardımcı",
        "Usta Çırak",
        "Kalfa",
        "Usta Kalfa",
        "Şef Yardımcı",
        "Şef",
        "Baş Şef",
        "Patron Şefi",
        "Gurme Uzmanı",
        "Efsane Şef",
        "Altın Önlük",
        "Mutfak İmparatoru",
        "Zamansız Usta"
    };

    private static readonly string[] RankNamesEn =
    {
        "Intern",
        "Apprentice",
        "Commis",
        "Junior Cook",
        "Line Cook",
        "Senior Cook",
        "Sous Chef",
        "Chef de Partie",
        "Head Chef",
        "Executive Chef",
        "Master Chef",
        "Legend Chef",
        "Golden Apron",
        "Kitchen Emperor",
        "Timeless Master"
    };

    private SupportedLanguage _activeLanguage;

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
    [SerializeField] private TMP_Text resetXpButtonLabel;

    [Header("Language")]
    [SerializeField] private SupportedLanguage defaultLanguage = SupportedLanguage.Turkish;

    private void Awake()
    {
        Apply(LoadSavedLanguage(), persist: false);
    }

    public void ApplyEnglish()
    {
        Apply(SupportedLanguage.English, persist: true);
    }

    public void ApplyTurkish()
    {
        Apply(SupportedLanguage.Turkish, persist: true);
    }

    private SupportedLanguage LoadSavedLanguage()
    {
        if (!PlayerPrefs.HasKey(LanguagePrefKey))
        {
            return defaultLanguage;
        }

        int saved = PlayerPrefs.GetInt(LanguagePrefKey, (int)defaultLanguage);
        if (saved == (int)SupportedLanguage.English || saved == (int)SupportedLanguage.Turkish)
        {
            return (SupportedLanguage)saved;
        }

        return defaultLanguage;
    }

    private void Apply(SupportedLanguage language, bool persist)
    {
        _activeLanguage = language;

        if (persist)
        {
            PlayerPrefs.SetInt(LanguagePrefKey, (int)language);
            PlayerPrefs.Save();
        }

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
            SetLabel(resetXpButtonLabel, "XP Sıfırla");

            if (uiManager != null)
            {
                uiManager.ConfigureLocalizedFormats(
                    "En İyi Skor: {0}",
                    "Son Skor: {0}",
                    "Skor: {0}",
                    "Final Skor: {0}",
                    "En İyi Skor: {0}");
                uiManager.SetSpecialOrderCopy("Özel Sipariş", "Harika!");
                uiManager.SetTimeUpCopy("Süre Doldu!");
                uiManager.SetHowToPlayLanguage(true);
            }

            if (gameManager != null)
            {
                gameManager.ConfigureLocalization(
                    "Skorları Sıfırla",
                    "Onay için tekrar dokun",
                    "Skorları sıfırlamak için 3 saniye içinde tekrar dokun.",
                    "XP Sıfırla",
                    "Onay için tekrar dokun",
                    "XP'yi sıfırlamak için 3 saniye içinde tekrar dokun.",
                    new[] { "Mükemmel", "Harika", "Süper", "Temiz Vuruş", "Nefis" },
                    new[] { "Alev Aldın!", "Durmak Yok!", "Efsane!", "Canavar Gibi!" },
                    new[] { "Çok Erken", "Erken", "Biraz Erken", "Biraz Bekle" },
                    new[] { "Çok Geç", "Geç", "Biraz Geç", "Kaçırdın" });
                gameManager.RefreshLocalizedTexts();
                gameManager.RefreshProgressionHeaderUi();
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
        SetLabel(resetXpButtonLabel, "Reset XP");

        if (uiManager != null)
        {
            uiManager.ConfigureLocalizedFormats(
                "Best Score: {0}",
                "Last Score: {0}",
                "Score: {0}",
                "Final Score: {0}",
                "Best Score: {0}");
            uiManager.SetSpecialOrderCopy("Special Order", "Nice!");
            uiManager.SetTimeUpCopy("Time's Up!");
            uiManager.SetHowToPlayLanguage(false);
        }

        if (gameManager != null)
        {
            gameManager.ConfigureLocalization(
                "Reset Scores",
                "Tap Again to Confirm",
                "Tap again within 3 seconds to reset scores.",
                "Reset XP",
                "Tap Again to Confirm",
                "Tap again within 3 seconds to reset XP.",
                new[] { "Perfect", "Great", "Nice", "Awesome", "Clean Tap" },
                new[] { "On Fire!", "Unstoppable!", "Godlike!", "Legend!" },
                new[] { "Too Early", "Early", "Bit Early", "Wait More" },
                new[] { "Too Late", "Late", "Bit Late", "Missed It" });
            gameManager.RefreshLocalizedTexts();
            gameManager.RefreshProgressionHeaderUi();
        }
    }

    public string GetRankNameForLevel(int level)
    {
        level = Mathf.Clamp(level, ProgressionData.MinLevel, ProgressionData.MaxLevel);
        int index = level - 1;
        return _activeLanguage == SupportedLanguage.Turkish ? RankNamesTr[index] : RankNamesEn[index];
    }

    public string FormatLevelRankLine(int level, string rankName)
    {
        return _activeLanguage == SupportedLanguage.Turkish
            ? string.Format("Seviye {0} - {1}", level, rankName)
            : string.Format("Level {0} - {1}", level, rankName);
    }

    public string FormatXpProgressLine(int currentXpInLevel, int xpNeededForNext)
    {
        return _activeLanguage == SupportedLanguage.Turkish
            ? string.Format("{0} / {1} TP", currentXpInLevel, xpNeededForNext)
            : string.Format("{0} / {1} XP", currentXpInLevel, xpNeededForNext);
    }

    public string GetMaxLevelXpLabel()
    {
        return _activeLanguage == SupportedLanguage.Turkish ? "Maksimum seviye" : "Max level";
    }

    public string GetProfileTitle()
    {
        return _activeLanguage == SupportedLanguage.Turkish ? "Profil" : "Profile";
    }

    public string GetStartScoreSectionTitle()
    {
        return _activeLanguage == SupportedLanguage.Turkish ? "Başlangıç Skoru" : "Start Score";
    }

    public string GetChooseAvatarTitle()
    {
        return _activeLanguage == SupportedLanguage.Turkish ? "Avatarını Seç" : "Choose Your Avatar";
    }

    public string FormatUnlocksAtLevel(int levelRequired)
    {
        return _activeLanguage == SupportedLanguage.Turkish
            ? string.Format("Açılıyor: Seviye {0}", levelRequired)
            : string.Format("Unlocks at Level {0}", levelRequired);
    }

    private void SetLabel(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
