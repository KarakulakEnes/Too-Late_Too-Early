using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableIngredient : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private IngredientType ingredient;
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform dragRoot;

    private RectTransform _rect;
    private Vector2 _startAnchored;
    private Transform _startParent;
    private int _startSibling;
    private bool _placing;
    private bool _succeededThisDrag;
    private float _canvasScaleFactor = 1f;
    public IngredientType Ingredient => ingredient;
    public Sprite IngredientSprite => iconImage != null ? iconImage.sprite : null;

    public void SetIngredient(IngredientType type)
    {
        ingredient = type;
    }

    public void ResetForRound()
    {
        EnsureHomeLayout();
        _placing = false;
        _succeededThisDrag = false;
        if (canvasGroup != null) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; }
        if (_rect != null && _startParent != null)
        {
            _rect.SetParent(_startParent);
            _rect.SetSiblingIndex(_startSibling);
            _rect.anchoredPosition = _startAnchored;
        }
    }

    public void EnsureHomeLayout()
    {
        if (dragRoot != null) { _rect = dragRoot; }
        if (_rect == null && iconImage != null && iconImage.rectTransform != (RectTransform)transform)
        {
            _rect = iconImage.rectTransform;
        }
        if (_rect == null) _rect = (RectTransform)transform;
        if (_startParent == null) _startParent = _rect.parent;
        _startSibling = _rect.GetSiblingIndex();
        _startAnchored = _rect.anchoredPosition;
    }

    public void MarkDropSuccess()
    {
        _succeededThisDrag = true;
        _placing = true;
        if (_rect != null && _startParent != null)
        {
            _rect.SetParent(_startParent);
            _rect.SetSiblingIndex(_startSibling);
            _rect.anchoredPosition = _startAnchored;
        }
        if (canvasGroup != null) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; }
        _placing = false;
    }

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (iconImage == null) iconImage = GetComponentInChildren<Image>();
        if (dragRoot != null) { _rect = dragRoot; }
        else if (iconImage != null && iconImage.rectTransform != (RectTransform)transform) { _rect = iconImage.rectTransform; }
        else { _rect = (RectTransform)transform; }
        if (_startParent == null) _startParent = _rect.parent;
        _startSibling = _rect.GetSiblingIndex();
        _startAnchored = _rect.anchoredPosition;

        Canvas c = GetComponentInParent<Canvas>();
        _canvasScaleFactor = c != null ? c.scaleFactor : 1f;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_placing) return;
        _succeededThisDrag = false;
        if (_rect == null) return;
        if (_startParent == null) _startParent = _rect.parent;
        _startSibling = _rect.GetSiblingIndex();
        _startAnchored = _rect.anchoredPosition;
        _rect.SetAsLastSibling();
        if (canvasGroup != null) { canvasGroup.alpha = 0.65f; canvasGroup.blocksRaycasts = false; }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_placing) return;
        if (_rect == null) return;
        _rect.anchoredPosition += eventData.delta / _canvasScaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_placing) return;
        if (_succeededThisDrag)
        {
            _succeededThisDrag = false;
            return;
        }
        if (canvasGroup != null) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; }
        if (_rect == null) return;
        if (_startParent == null) return;
        _rect.SetParent(_startParent);
        _rect.SetSiblingIndex(_startSibling);
        _rect.anchoredPosition = _startAnchored;
    }
}
