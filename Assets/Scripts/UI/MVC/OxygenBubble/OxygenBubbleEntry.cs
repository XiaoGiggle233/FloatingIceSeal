using System;
using UnityEngine;
using UnityEngine.UI;

#region ========== 氧气气泡入口 ==========

/// <summary>
/// 氧气气泡入口 —— 挂到 Canvas 上，自动装配 MVC 并在 Update 中驱动刷新
/// </summary>
public class OxygenBubbleEntry : MonoBehaviour
{
    [Header("气泡图片设置")]
    [SerializeField] private Sprite _bubbleSprite;
    [SerializeField] private Vector2 _bubbleSize = new Vector2(120, 160);
    [SerializeField] private Vector2 _bubblePosition = new Vector2(0, -80);

    private OxygenBubbleView _view;
    private OxygenBubbleController _controller;

    private void Start()
    {
        // 查找场景中已存在的 View，没有则自动创建
        _view = GetComponentInChildren<OxygenBubbleView>();
        if (_view == null)
            _view = CreateBubbleUI();

        if (_view == null)
        {
            Debug.LogError("[OxygenBubbleEntry] 无法创建氧气气泡 UI");
            return;
        }

        _controller = new OxygenBubbleController();
        _controller.Init();
        _view.Controller = _controller;
        _controller.OnLoadView(_view);

        MVCManager.ControllerManager.Register(ControllerType.OxygenBubble, _controller);
    }

    private void Update()
    {
        _controller?.Tick();
    }

    private void OnDestroy()
    {
        if (_controller != null)
        {
            MVCManager.ControllerManager.Unregister(Convert.ToInt32(ControllerType.OxygenBubble));
            _controller.Destroy();
        }
    }

    /// <summary>运行时自动创建气泡 Image GameObject</summary>
    private OxygenBubbleView CreateBubbleUI()
    {
        var go = new GameObject("OxygenBubble", typeof(RectTransform));
        go.transform.SetParent(transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = _bubbleSize;
        rt.anchoredPosition = _bubblePosition;

        var img = go.AddComponent<Image>();
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Vertical;
        img.fillOrigin = (int)Image.OriginVertical.Bottom;
        img.fillAmount = 1f;

        if (_bubbleSprite != null)
            img.sprite = _bubbleSprite;
        else
            img.color = new Color(0.3f, 0.8f, 1f, 0.8f); // 占位颜色

        return go.AddComponent<OxygenBubbleView>();
    }
}

#endregion
