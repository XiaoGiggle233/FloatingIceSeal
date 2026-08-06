using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#region ========== 按钮 Sprite 切换 ==========

/// <summary>
/// 按钮 Sprite 切换 —— 根据交互状态切换 3 种 Sprite（正常/悬停/按住）
/// 状态优先级：按住 > 悬停 > 正常
/// </summary>
public class ButtonSpriteSwitcher : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("状态 Sprite")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite pressedSprite;

    private Image _image;
    private bool _hovered;
    private bool _pressed;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        UpdateSprite();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        _pressed = false;
        UpdateSprite();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        UpdateSprite();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        UpdateSprite();
    }

    private void OnDisable()
    {
        _hovered = false;
        _pressed = false;
    }

    /// <summary>根据当前状态选择并应用 Sprite</summary>
    private void UpdateSprite()
    {
        if (_image == null) return;

        Sprite sprite = null;
        if (_pressed && pressedSprite != null) sprite = pressedSprite;
        else if (_hovered && hoverSprite != null) sprite = hoverSprite;
        else sprite = normalSprite;

        if (sprite != null)
            _image.sprite = sprite;
    }
}

#endregion
