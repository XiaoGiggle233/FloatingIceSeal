using System;
using UnityEngine;
using UnityEngine.UI;

#region ========== 主菜单视图 ==========

/// <summary>
/// 主菜单视图 —— 绑定主菜单按钮，将用户交互通过事件通知 Controller
/// </summary>
public class MainMenuView : BaseView
{
    /// <summary>开始游戏按钮点击事件</summary>
    public event Action StartGameClicked;

    /// <summary>切换存档按钮点击事件</summary>
    public event Action SwitchSaveClicked;

    /// <summary>设置按钮点击事件</summary>
    public event Action SettingsClicked;

    /// <summary>制作人员表按钮点击事件</summary>
    public event Action CreditsClicked;

    /// <summary>退出游戏按钮点击事件</summary>
    public event Action ExitGameClicked;

    private Button _startBtn;
    private Button _switchSaveBtn;
    private Button _settingsBtn;
    private Button _creditsBtn;
    private Button _exitBtn;

    protected override void OnAwake()
    {
        base.OnAwake();

        _startBtn = Find<Button>("StartGameBtn");
        _switchSaveBtn = Find<Button>("SwitchSaveBtn");
        _settingsBtn = Find<Button>("SettingsBtn");
        _creditsBtn = Find<Button>("CreditsBtn");
        _exitBtn = Find<Button>("ExitGameBtn");

        _startBtn?.onClick.AddListener(() => StartGameClicked?.Invoke());
        _switchSaveBtn?.onClick.AddListener(() => SwitchSaveClicked?.Invoke());
        _settingsBtn?.onClick.AddListener(() => SettingsClicked?.Invoke());
        _creditsBtn?.onClick.AddListener(() => CreditsClicked?.Invoke());
        _exitBtn?.onClick.AddListener(() => ExitGameClicked?.Invoke());
    }

    private void OnDestroy()
    {
        _startBtn?.onClick.RemoveAllListeners();
        _switchSaveBtn?.onClick.RemoveAllListeners();
        _settingsBtn?.onClick.RemoveAllListeners();
        _creditsBtn?.onClick.RemoveAllListeners();
        _exitBtn?.onClick.RemoveAllListeners();
    }
}

#endregion
