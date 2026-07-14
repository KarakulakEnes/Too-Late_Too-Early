using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileStartScoreItem : MonoBehaviour
{
    [SerializeField] private int optionIndex;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private TMP_Text lockHintText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedOutline;
    [SerializeField] private Image backgroundImage;

    private ProfilePanelController _owner;

    private void Awake()
    {
        _owner = GetComponentInParent<ProfilePanelController>();
        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (_owner != null)
        {
            _owner.NotifyStartScoreClicked(optionIndex);
        }
    }

    public void ApplyVisual(bool unlocked, bool selected, int startScore, string lockedHintOrEmpty)
    {
        if (scoreLabel != null)
        {
            scoreLabel.text = startScore.ToString();
            scoreLabel.color = unlocked
                ? new Color(0.12f, 0.14f, 0.2f, 1f)
                : new Color(0.12f, 0.14f, 0.2f, 0.45f);
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = unlocked
                ? Color.white
                : new Color(1f, 1f, 1f, 0.55f);
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
