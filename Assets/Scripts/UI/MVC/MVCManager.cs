#region ========== MVC 管理器（全局入口） ==========

/// <summary>
/// MVC 框架全局入口 —— 持有 ControllerManager / ViewManager 单例
/// 供各场景的 View / Controller 注册、获取与跨控制器通信
/// </summary>
public static class MVCManager
{
    private static ControllerManager _controllerManager;

    /// <summary>
    /// 控制器管理器（懒加载单例）
    /// 各场景入口将 Controller 注册到这里，便于跨控制器通信与统一管理
    /// </summary>
    public static ControllerManager ControllerManager
    {
        get
        {
            if (_controllerManager == null)
                _controllerManager = new ControllerManager();
            return _controllerManager;
        }
    }
}

#endregion
