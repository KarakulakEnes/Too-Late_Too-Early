using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrderSlotDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visuals")]
    [SerializeField] private Image slotImage;
    [SerializeField] private Image filledIcon;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private float pendingAlpha = 0.35f;

    private IngredientType _required;
    private bool _filled;
    private Color _baseColor;
    public IngredientType RequiredType => _required;
    public bool IsFilled => _filled;

    private void Awake()
    {
        if (slotImage == null) slotImage = GetComponent<Image>();
        ResolveFilledIcon();
        if (slotImage != null) _baseColor = slotImage.color;
        if (filledIcon != null) filledIcon.enabled = false;
    }

    public void Configure(IngredientType required, Sprite displaySprite = null)
    {
        ResolveFilledIcon();
        _required = required;
        _filled = false;
        if (slotImage != null) slotImage.color = _baseColor;
        if (filledIcon != null)
        {
            if (displaySprite != null)
            {
                filledIcon.sprite = displaySprite;
                filledIcon.enabled = true;
                SetImageAlpha(filledIcon, pendingAlpha);
            }
            else
            {
                filledIcon.enabled = false;
            }
        }
    }

    public void ClearSlot()
    {
        ResolveFilledIcon();
        _filled = false;
        if (slotImage != null) slotImage.color = _baseColor;
        if (filledIcon != null) filledIcon.enabled = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_filled) return;
        if (eventData.pointerDrag == null) return;
        var drag = eventData.pointerDrag.GetComponent<DraggableIngredient>();
        if (drag == null)
        {
            drag = eventData.pointerDrag.GetComponentInParent<DraggableIngredient>();
        }
        if (drag == null) return;
        if (drag.Ingredient != _required) return;

        _filled = true;
        if (filledIcon != null)
        {
            filledIcon.enabled = true;
            SetImageAlpha(filledIcon, 1f);
        }
        if (slotImage != null) slotImage.color = _baseColor;
        drag.MarkDropSuccess();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_filled) return;
        if (eventData.pointerDrag == null) return;
        if (eventData.pointerDrag.GetComponent<DraggableIngredient>() == null) return;
        if (slotImage != null) slotImage.color = Color.Lerp(_baseColor, highlightColor, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slotImage != null) slotImage.color = _baseColor;
    }

    private void ResolveFilledIcon()
    {
        if (filledIcon != null)
        {
            return;
        }

        Image[] images = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image candidate = images[i];
            if (candidate == null)
            {
                continue;
            }

            if (candidate == slotImage)
            {
                continue;
            }

            if (candidate.gameObject == gameObject)
            {
                continue;
            }

            filledIcon = candidate;
            break;
        }
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color c = image.color;
        c.a = Mathf.Clamp01(alpha);
        image.color = c;
    }
}
