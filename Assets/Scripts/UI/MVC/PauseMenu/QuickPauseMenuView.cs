using System;
using UnityEngine;
using UnityEngine.UI;

#region ========== 快速暂停菜单视图 ==========

/// <summary>
/// 快速暂停菜单视图 —— 绑定三个按钮（继续游戏 / 设置 / 主菜单）
/// 通过事件通知 Controller
/// </summary>
public class QuickPauseMenuView : BaseView
{
    /// <summary>继续游戏按钮点击事件</summary>
    public event Action ResumeClicked;
    /// <summary>设置按钮点击事件</summary>
    public event Action SettingsClicked;
    /// <summary>主菜单按钮点击事件</summary>
    public event Action MainMenuClicked;

    private Button _resumeButton;
    private Button _settingsButton;
    private Button _mainMenuButton;

    protected override void OnAwake()
    {
        base.OnAwake();

        _resumeButton = Find<Button>("ResumeButton");
        _settingsButton = Find<Button>("SettingsButton");
        _mainMenuButton = Find<Button>("MainMenuButton");

        _resumeButton?.onClick.AddListener(() => ResumeClicked?.Invoke());
        _settingsButton?.onClick.AddListener(() => SettingsClicked?.Invoke());
        _mainMenuButton?.onClick.AddListener(() => MainMenuClicked?.Invoke());
    }

    /// <summary>显示/隐藏快速暂停菜单（由 Controller 调用）</summary>
    public void SetMenuVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void OnDestroy()
    {
        _resumeButton?.onClick.RemoveAllListeners();
        _settingsButton?.onClick.RemoveAllListeners();
        _mainMenuButton?.onClick.RemoveAllListeners();
    }
}

#endregion
