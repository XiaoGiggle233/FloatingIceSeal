using System;
using UnityEngine;
using UnityEngine.UI;

#region ========== 暂停菜单视图 ==========

/// <summary>
/// 暂停菜单视图 —— 绑定暂停菜单 UI，配置滚动视图，处理返回按钮交互
/// 通过事件通知 Controller
/// </summary>
public class PauseMenuView : BaseView
{
    /// <summary>返回（继续游戏）按钮点击事件</summary>
    public event Action BackClicked;

    private Button _backButton;
    private ScrollRect _scrollRect;
    private RectTransform _scrollContent;
    private RectTransform _scrollViewport;
    private Scrollbar _scrollbarVertical;

    protected override void OnAwake()
    {
        base.OnAwake();

        _backButton = Find<Button>("SettingsBackButton");
        _scrollRect = Find<ScrollRect>("Panel/Pannel/Scroll View");
        _scrollContent = Find<RectTransform>("Panel/Pannel/Scroll View/Viewport/Content");
        _scrollViewport = Find<RectTransform>("Panel/Pannel/Scroll View/Viewport");
        _scrollbarVertical = Find<Scrollbar>("Panel/Pannel/Scroll View/Scrollbar Vertical");

        _backButton?.onClick.AddListener(() => BackClicked?.Invoke());

        SetupScrollView();
    }

    /// <summary>配置滚动视图（由 PauseMenuController 原逻辑迁移而来）</summary>
    private void SetupScrollView()
    {
        if (_scrollRect != null)
        {
            _scrollRect.content = _scrollContent;
            _scrollRect.viewport = _scrollViewport;
            _scrollRect.verticalScrollbar = _scrollbarVertical;
            _scrollRect.horizontal = false;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
        }
    }

    /// <summary>显示/隐藏暂停菜单（由 Controller 调用）</summary>
    public void SetMenuVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void OnDestroy()
    {
        _backButton?.onClick.RemoveAllListeners();
    }
}

#endregion
