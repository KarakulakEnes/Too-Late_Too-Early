using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileAvatarItem : MonoBehaviour
{
    [SerializeField] private int slotIndex;
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private TMP_Text lockHintText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedOutline;

    private ProfilePanelController _owner;

    private void Awake()
    {
        _owner = GetComponentInParent<ProfilePanelController>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnClicked);
            button.onClick.AddListener(OnClicked);
        }
    }

    public void Configure(int index)
    {
        slotIndex = index;
        gameObject.name = $"AvatarSlot_{index}";
        if (_owner == null)
        {
            _owner = GetComponentInParent<ProfilePanelController>();
        }
    }

    private void OnClicked()
    {
        if (_owner != null)
        {
            _owner.NotifyAvatarClicked(slotIndex);
        }
    }

    public void ApplyVisual(
        bool unlocked,
        bool selected,
        Sprite sprite,
        string lockedHintOrEmpty)
    {
        if (avatarImage != null)
        {
            if (sprite != null)
            {
                avatarImage.sprite = sprite;
            }

            // Sprite'lar kendi tema renklerine sahip; Image rengi beyaz = filtre yok.
            avatarImage.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.45f);
        }

        if (lockOverlay != null)
        {
            lockOverlay.SetActive(!unlocked);
        }

        if (lockHintText != null)
        {
            lockHintText.gameObject.SetActive(!unlocked);
            lockHintText.text = unlocked ? string.Empty : lockedHintOrEmpty;
        }

        if (selectedOutline != null)
        {
            selectedOutline.SetActive(selected && unlocked);
        }

        if (button != null)
        {
            button.interactable = unlocked;
        }
    }
}
