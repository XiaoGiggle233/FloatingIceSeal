using System;
using UnityEngine;

#region ========== 暂停系统入口 ==========

/// <summary>
/// 暂停系统入口 —— 监听暂停键输入，驱动新暂停菜单（快速暂停菜单）的 MVC 控制器
/// 挂载到场景中始终激活的 PauseSystem GameObject 上
/// </summary>
public class PauseEntry : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private QuickPauseMenuView menuView;
    [SerializeField] private PauseMenuView settingsView;

    private QuickPauseMenuView _menuView;
    private PauseMenuView _settingsView;
    private QuickPauseMenuController _controller;

    private void Start()
    {
        _menuView = menuView;
        if (_menuView == null)
        {
            // 兜底：快速暂停菜单默认未激活，FindObjectOfType 查不到，改用包含未激活对象的查找
            var all = Resources.FindObjectsOfTypeAll<QuickPauseMenuView>();
            if (all != null && all.Length > 0)
                _menuView = all[0];
        }

        _settingsView = settingsView;
        if (_settingsView == null)
        {
            var allSettings = Resources.FindObjectsOfTypeAll<PauseMenuView>();
            if (allSettings != null && allSettings.Length > 0)
                _settingsView = allSettings[0];
        }

        if (_menuView == null)
        {
            Debug.LogError("[PauseEntry] 未找到 QuickPauseMenuView，暂停系统 MVC 初始化失败");
            return;
        }

        _controller = new QuickPauseMenuController();

        // 生命周期：Init（创建 Model）→ OnLoadView（绑定 View 事件）→ BindSettingsView
        _controller.Init();
        _menuView.Controller = _controller;
        _controller.OnLoadView(_menuView);
        _controller.BindSettingsView(_settingsView);

        MVCManager.ControllerManager.Register(ControllerType.Pause, _controller);

        // 初始隐藏暂停菜单与设置菜单
        _menuView.SetMenuVisible(false);
        _settingsView?.SetMenuVisible(false);
    }

    private void Update()
    {
        if (_controller == null) return;

        if (Input.GetKeyDown(pauseKey))
        {
            _controller.TogglePause();
        }
    }

    private void OnDestroy()
    {
        if (_controller != null)
        {
            MVCManager.ControllerManager.Unregister(Convert.ToInt32(ControllerType.Pause));
            _controller.Destroy();
        }
    }
}

#endregion
