using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#region ========== 存档槽位按钮 ==========

/// <summary>
/// 存档槽位按钮 —— 根据（有无存档 × 交互状态）切换 8 种 Sprite
///
/// 状态优先级：按住 > 悬停 > 选中 > 正常
/// </summary>
public class SaveSlotButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("有存档状态")]
    [SerializeField] private Sprite hasSaveNormal;
    [SerializeField] private Sprite hasSaveHover;
    [SerializeField] private Sprite hasSavePressed;
    [SerializeField] private Sprite hasSaveSelected;

    [Header("无存档状态")]
    [SerializeField] private Sprite noSaveNormal;
    [SerializeField] private Sprite noSaveHover;
    [SerializeField] private Sprite noSavePressed;
    [SerializeField] private Sprite noSaveSelected;

    private Image _image;
    private bool _hasSave;
    private bool _selected;
    private bool _hovered;
    private bool _pressed;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    /// <summary>设置该槽位是否有存档</summary>
    public void SetHasSave(bool hasSave)
    {
        _hasSave = hasSave;
        UpdateSprite();
    }

    /// <summary>设置该槽位是否为当前选中</summary>
    public void SetSelected(bool selected)
    {
        _selected = selected;
        UpdateSprite();
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

        Sprite sprite = _hasSave ? ResolveSprite(hasSaveNormal, hasSaveHover, hasSavePressed, hasSaveSelected)
                                 : ResolveSprite(noSaveNormal, noSaveHover, noSavePressed, noSaveSelected);

        if (sprite != null)
            _image.sprite = sprite;
    }

    private Sprite ResolveSprite(Sprite normal, Sprite hover, Sprite pressed, Sprite selected)
    {
        if (_pressed && pressed != null) return pressed;
        if (_hovered && hover != null) return hover;
        if (_selected && selected != null) return selected;
        return normal;
    }
}

#endregion
