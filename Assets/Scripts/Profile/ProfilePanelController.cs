using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text chooseAvatarTitle;
    [SerializeField] private TMP_Text startScoreSectionTitle;

    [Header("Avatars (25) — scroll + prefab spawn")]
    [SerializeField] private ScrollRect avatarScrollRect;
    [SerializeField] private RectTransform avatarGridContent;
    [SerializeField] private ProfileAvatarItem avatarItemPrefab;
    [Tooltip("Optional: leave empty if using avatarItemPrefab to spawn 25 slots at runtime.")]
    [SerializeField] private ProfileAvatarItem[] avatarSlots;

    [Header("Start Score")]
    [SerializeField] private ProfileStartScoreItem[] startScoreSlots = new ProfileStartScoreItem[ProgressionData.StartScoreOptionCount];

    [Header("Refs")]
    [SerializeField] private ProgressionService progressionService;
    [SerializeField] private LocalizationLite localization;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameManager gameManager;

    private bool _avatarSlotsBuilt;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        EnsureAvatarSlotsBuilt();
    }

    public void Open()
    {
        EnsureAvatarSlotsBuilt();

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (avatarScrollRect != null)
        {
            avatarScrollRect.verticalNormalizedPosition = 1f;
        }

        RefreshAllSlots();
    }

    public void Close()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void NotifyAvatarClicked(int slotIndex)
    {
        if (progressionService == null)
        {
            return;
        }

        if (!progressionService.TrySelectAvatar(slotIndex))
        {
            return;
        }

        gameManager?.RefreshProgressionHeaderUi();
        RefreshAllSlots();
    }

    public void NotifyStartScoreClicked(int optionIndex)
    {
        if (progressionService == null)
        {
            return;
        }

        if (optionIndex < 0 || optionIndex >= ProgressionData.StartScoreOptionCount)
        {
            return;
        }

        int score = ProgressionData.StartScores[optionIndex];
        if (!progressionService.TrySelectStartScore(score))
        {
            return;
        }

        RefreshAllSlots();
    }

    public void RefreshAllSlots()
    {
        RefreshAvatarSlots();
        RefreshStartScoreSlots();
        RefreshTitles();
    }

    private void EnsureAvatarSlotsBuilt()
    {
        if (_avatarSlotsBuilt)
        {
            return;
        }

        if (avatarItemPrefab == null || avatarGridContent == null)
        {
            _avatarSlotsBuilt = avatarSlots != null && avatarSlots.Length > 0;
            return;
        }

        while (avatarGridContent.childCount > 0)
        {
            DestroyImmediate(avatarGridContent.GetChild(0).gameObject);
        }

        avatarSlots = new ProfileAvatarItem[ProgressionData.AvatarCount];
        for (int i = 0; i < ProgressionData.AvatarCount; i++)
        {
            ProfileAvatarItem item = Instantiate(avatarItemPrefab, avatarGridContent);
            item.Configure(i);
            avatarSlots[i] = item;
        }

        _avatarSlotsBuilt = true;
    }

    private void RefreshAvatarSlots()
    {
        if (progressionService == null || uiManager == null || avatarSlots == null)
        {
            return;
        }

        ProgressionSnapshot snap = progressionService.BuildSnapshot();
        for (int i = 0; i < avatarSlots.Length && i < ProgressionData.AvatarCount; i++)
        {
            ProfileAvatarItem item = avatarSlots[i];
            if (item == null)
            {
                continue;
            }

            bool unlocked = progressionService.IsAvatarUnlocked(i);
            bool selected = snap.SelectedAvatarIndex == i;
            Sprite spr = uiManager.GetProfileAvatarSprite(i);
            string hint = string.Empty;
            if (!unlocked && localization != null)
            {
                hint = localization.FormatUnlocksAtLevel(ProgressionData.GetAvatarUnlockLevel(i));
            }

            item.ApplyVisual(unlocked, selected, spr, hint);
        }
    }

    private void RefreshStartScoreSlots()
    {
        if (progressionService == null)
        {
            return;
        }

        ProgressionSnapshot snap = progressionService.BuildSnapshot();
        for (int i = 0; i < startScoreSlots.Length && i < ProgressionData.StartScoreOptionCount; i++)
        {
            ProfileStartScoreItem item = startScoreSlots[i];
            if (item == null)
            {
                continue;
            }

            int score = ProgressionData.StartScores[i];
            bool unlocked = progressionService.IsStartScoreUnlocked(score);
            bool selected = snap.SelectedStartScore == score;
            string hint = string.Empty;
            if (!unlocked && localization != null)
            {
                hint = localization.FormatUnlocksAtLevel(ProgressionData.StartScoreUnlockLevels[i]);
            }

            item.ApplyVisual(unlocked, selected, score, hint);
        }
    }

    private void RefreshTitles()
    {
        if (localization == null)
        {
            return;
        }

        if (titleLabel != null)
        {
            titleLabel.text = localization.GetProfileTitle();
        }

        if (chooseAvatarTitle != null)
        {
            chooseAvatarTitle.text = localization.GetChooseAvatarTitle();
        }

        if (startScoreSectionTitle != null)
        {
            startScoreSectionTitle.text = localization.GetStartScoreSectionTitle();
        }
    }
}
