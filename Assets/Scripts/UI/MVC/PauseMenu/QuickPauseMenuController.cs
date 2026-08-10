using UnityEngine;

#region ========== 快速暂停菜单控制器 ==========

/// <summary>
/// 快速暂停菜单控制器 —— 处理暂停/恢复逻辑，切换时间缩放，发布事件
/// 负责新暂停菜单与设置视图（旧的暂停菜单）之间的显隐切换
/// </summary>
public class QuickPauseMenuController : BaseController
{
    private QuickPauseMenuView _view;
    private PauseMenuView _settingsView;
    private QuickPauseMenuModel _pauseModel;

    public override void Init()
    {
        _pauseModel = new QuickPauseMenuModel();
        SetModel(_pauseModel);
        _pauseModel.Init();
    }

    public override void OnLoadView(IBaseView view)
    {
        _view = view as QuickPauseMenuView;
        if (_view != null)
        {
            _view.ResumeClicked += OnResumeClicked;
            _view.SettingsClicked += OnSettingsClicked;
            _view.MainMenuClicked += OnMainMenuClicked;
        }
    }

    /// <summary>绑定设置视图（即旧的暂停菜单），由入口装配时调用</summary>
    public void BindSettingsView(PauseMenuView settingsView)
    {
        _settingsView = settingsView;
        if (_settingsView != null)
        {
            _settingsView.BackClicked += OnSettingsBackClicked;
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        if (_view != null)
        {
            _view.ResumeClicked -= OnResumeClicked;
            _view.SettingsClicked -= OnSettingsClicked;
            _view.MainMenuClicked -= OnMainMenuClicked;
        }
        if (_settingsView != null)
        {
            _settingsView.BackClicked -= OnSettingsBackClicked;
        }
    }

    /// <summary>切换暂停/恢复（由入口在检测到暂停键时调用）</summary>
    public void TogglePause()
    {
        if (_pauseModel.IsPaused)
            Resume();
        else
            Pause();
    }

    private void Pause()
    {
        _pauseModel.IsPaused = true;
        Time.timeScale = 0f;

        _settingsView?.SetMenuVisible(false);
        _view?.SetMenuVisible(true);

        GameEvents.Publish(EventType.GAME_EVENT_ON_PAUSE, new GameEventBase());
    }

    public void Resume()
    {
        _pauseModel.IsPaused = false;
        Time.timeScale = 1f;

        _view?.SetMenuVisible(false);
        _settingsView?.SetMenuVisible(false);

        GameEvents.Publish(EventType.GAME_EVENT_ON_RESUME, new GameEventBase());
    }

    private void OnResumeClicked()
    {
        Resume();
    }

    private void OnSettingsClicked()
    {
        // 跳到设置菜单（旧的暂停菜单）
        _view?.SetMenuVisible(false);
        _settingsView?.SetMenuVisible(true);
    }

    private void OnMainMenuClicked()
    {
        // 主菜单功能暂未实现
    }

    private void OnSettingsBackClicked()
    {
        // 从设置菜单返回新暂停菜单
        _settingsView?.SetMenuVisible(false);
        _view?.SetMenuVisible(true);
    }
}

#endregion
