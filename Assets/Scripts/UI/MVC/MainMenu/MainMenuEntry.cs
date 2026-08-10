using System;
using UnityEngine;

#region ========== 主菜单入口 ==========

/// <summary>
/// 主菜单入口 —— 场景启动时创建并绑定主菜单的 MVC 组件
/// 挂载到 StartSenece 场景的 Managers GameObject 上
/// </summary>
public class MainMenuEntry : MonoBehaviour
{
    private MainMenuView _menuView;
    private MainMenuController _controller;

    private void Start()
    {
        if (_menuView == null)
            _menuView = FindObjectOfType<MainMenuView>();

        if (_menuView == null)
        {
            Debug.LogError("[MainMenuEntry] 未找到 MainMenuView，主菜单 MVC 初始化失败");
            return;
        }

        _controller = new MainMenuController();

        // 生命周期：Init（创建 Model）→ OnLoadView（绑定 View 事件并渲染）
        _controller.Init();
        _menuView.Controller = _controller;
        _controller.OnLoadView(_menuView);

        MVCManager.ControllerManager.Register(ControllerType.MainMenu, _controller);
    }

    private void OnDestroy()
    {
        if (_controller != null)
        {
            MVCManager.ControllerManager.Unregister(Convert.ToInt32(ControllerType.MainMenu));
            _controller.Destroy();
        }
    }
}

#endregion
