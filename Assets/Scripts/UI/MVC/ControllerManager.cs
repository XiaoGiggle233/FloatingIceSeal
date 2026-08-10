using System.Collections.Generic;
using System.Linq;

#region ========== 控制器管理器 ==========

/// <summary>
/// 控制器管理器
/// 负责所有 Controller 的注册、初始化、销毁和跨控制器通信
/// 
/// 使用方式：
///   var cm = new ControllerManager();
///   cm.Register(MyControllerType.Player, new PlayerController());
///   cm.InitAllModule();  // 批量调用所有 Controller.Init()
/// </summary>
public class ControllerManager
{
    /// <summary>控制器字典：控制器ID -> 控制器实例</summary>
    private Dictionary<int, BaseController> _modules;

    public ControllerManager()
    {
        _modules = new Dictionary<int, BaseController>();
    }

    #region 注册与注销

    /// <summary>注册控制器（通过枚举）</summary>
    public void Register(System.Enum controllerType, BaseController controller)
    {
        Register(System.Convert.ToInt32(controllerType), controller);
    }

    /// <summary>注册控制器（通过整型 ID）</summary>
    public void Register(int controllerKey, BaseController controller)
    {
        if (!_modules.ContainsKey(controllerKey))
        {
            _modules.Add(controllerKey, controller);
        }
    }

    /// <summary>注销控制器</summary>
    public void Unregister(int controllerKey)
    {
        if (_modules.ContainsKey(controllerKey))
        {
            _modules.Remove(controllerKey);
        }
    }

    /// <summary>清空所有控制器（不销毁，仅清除字典）</summary>
    public void Clear()
    {
        _modules.Clear();
    }

    /// <summary>销毁并清空所有控制器</summary>
    public void ClearAllModules()
    {
        List<int> keys = _modules.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            _modules[keys[i]].Destroy();
            _modules.Remove(keys[i]);
        }
    }

    #endregion

    #region 控制器操作

    /// <summary>初始化所有已注册的控制器</summary>
    public void InitAllModule()
    {
        foreach (var item in _modules)
        {
            item.Value.Init();
        }
    }

    /// <summary>
    /// 向指定控制器发送消息
    /// </summary>
    /// <param name="controllerKey">控制器 ID</param>
    /// <param name="eventName">消息名称</param>
    /// <param name="args">消息参数</param>
    public void ApplyFunc(int controllerKey, string eventName, params object[] args)
    {
        if (_modules.ContainsKey(controllerKey))
        {
            _modules[controllerKey].ApplyFunc(eventName, args);
        }
    }

    /// <summary>
    /// 向指定控制器发送消息（通过枚举）
    /// </summary>
    public void ApplyFunc(System.Enum controllerType, string eventName, params object[] args)
    {
        ApplyFunc(System.Convert.ToInt32(controllerType), eventName, args);
    }

    /// <summary>
    /// 获取指定控制器的 Model
    /// </summary>
    public BaseModel GetControllerModel(int controllerKey)
    {
        if (_modules.ContainsKey(controllerKey))
        {
            return _modules[controllerKey].GetModel();
        }
        return null;
    }

    /// <summary>
    /// 获取指定控制器（类型安全）
    /// </summary>
    public T GetController<T>(int controllerKey) where T : BaseController
    {
        if (_modules.ContainsKey(controllerKey))
        {
            return _modules[controllerKey] as T;
        }
        return null;
    }

    #endregion
}

#endregion
