using System;
using System.Collections.Generic;
using UnityEngine;

#region ========== 控制器基类 ==========

/// <summary>
/// 控制器基类
/// 负责协调 Model 与 View 之间的交互，处理业务逻辑
/// 
/// 职责：
///   - 持有 Model 引用，管理数据
///   - 在 View 加载后（OnLoadView）绑定 View 事件
///   - 订阅全局事件（GameEvents），响应后将数据传递给 View 渲染
///   - 处理 View 的交互事件（View → Controller），执行业务逻辑
/// 
/// 生命周期：
///   构造 → SetModel() → Register() → Init() → OnLoadView() → ... → Destroy()
///       ↳ InitModuleEvent()         ↳ OpenView() / CloseView()
///       ↳ InitGlobalEvent()
/// 
/// 消息系统：
///   控制器内部支持字符串键的消息传递（RegisterFunc / ApplyFunc）
///   跨控制器通信使用 ApplyControllerFunc（需配合 ControllerManager）
/// </summary>
public class BaseController
{
    #region 字段

    /// <summary>控制器内部消息字典：事件名 -> 回调</summary>
    private Dictionary<string, Action<object[]>> _message;

    /// <summary>该控制器关联的数据模型</summary>
    protected BaseModel _model;

    #endregion

    #region 构造与生命周期

    public BaseController()
    {
        _message = new Dictionary<string, Action<object[]>>();
    }

    /// <summary>
    /// 初始化控制器
    /// 子类重写以执行初始化逻辑（如订阅全局事件、创建 Model 等）
    /// </summary>
    public virtual void Init()
    {
    }

    /// <summary>
    /// View 加载完成后的回调
    /// 在此处绑定 View 的事件（如按钮点击）到 Controller 的处理方法
    /// </summary>
    /// <param name="view">加载完成的视图实例</param>
    public virtual void OnLoadView(IBaseView view)
    {
    }

    /// <summary>
    /// View 打开时的回调
    /// 可用于在 View 显示时刷新数据
    /// </summary>
    public virtual void OpenView(IBaseView view)
    {
    }

    /// <summary>
    /// View 关闭时的回调
    /// </summary>
    public virtual void CloseView(IBaseView view)
    {
    }

    /// <summary>
    /// 销毁控制器
    /// 清理模块事件和全局事件订阅
    /// </summary>
    public virtual void Destroy()
    {
        RemoveModuleEvent();
        RemoveGlobalEvent();
    }

    #endregion

    #region Model 管理

    /// <summary>设置模型并双向绑定</summary>
    public void SetModel(BaseModel model)
    {
        _model = model;
        _model.Controller = this;
    }

    /// <summary>获取模型（基类）</summary>
    public BaseModel GetModel()
    {
        return _model;
    }

    /// <summary>获取类型安全的模型</summary>
    public T GetModel<T>() where T : BaseModel
    {
        return _model as T;
    }

    /// <summary>
    /// 获取指定控制器的 Model（需配合 ControllerManager）
    /// 默认返回 null，子类可根据项目实际情况实现
    /// </summary>
    public virtual BaseModel GetControllerModel(int controllerKey)
    {
        return null;
    }

    #endregion

    #region 模块消息（Controller 内部事件）

    /// <summary>
    /// 注册控制器内部消息
    /// </summary>
    /// <param name="eventName">消息名称</param>
    /// <param name="callback">回调</param>
    public void RegisterFunc(string eventName, Action<object[]> callback)
    {
        if (_message.ContainsKey(eventName))
        {
            _message[eventName] += callback;
        }
        else
        {
            _message.Add(eventName, callback);
        }
    }

    /// <summary>移除控制器内部消息</summary>
    public void UnRegisterFunc(string eventName)
    {
        if (_message.ContainsKey(eventName))
        {
            _message.Remove(eventName);
        }
    }

    /// <summary>触发控制器内部消息</summary>
    public void ApplyFunc(string eventName, params object[] args)
    {
        if (_message.ContainsKey(eventName))
        {
            _message[eventName].Invoke(args);
        }
        else
        {
            Debug.LogWarning($"[BaseController.ApplyFunc] 消息 '{eventName}' 未注册");
        }
    }

    /// <summary>
    /// 向其他 Controller 发送跨控制器消息（需配合 ControllerManager）
    /// 默认不做任何操作，子类可重写以接入 ControllerManager
    /// </summary>
    public virtual void ApplyControllerFunc(int controllerKey, string eventName, params object[] args)
    {
        // 默认空实现，接入 ControllerManager 后可改为实际转发
    }

    #endregion

    #region 全局事件（GameEvents 集成）

    /// <summary>
    /// 初始化模块事件（Controller 自定义的内部事件）
    /// 子类重写以注册 RegisterFunc
    /// </summary>
    public virtual void InitModuleEvent()
    {
    }

    /// <summary>
    /// 移除模块事件
    /// 子类重写以调用 UnRegisterFunc
    /// </summary>
    public virtual void RemoveModuleEvent()
    {
    }

    /// <summary>
    /// 初始化全局事件订阅（GameEvents）
    /// 子类重写以调用 GameEvents.Listen()
    /// </summary>
    public virtual void InitGlobalEvent()
    {
    }

    /// <summary>
    /// 移除全局事件订阅（GameEvents）
    /// 子类重写以调用 GameEvents.Unlisten()
    /// </summary>
    public virtual void RemoveGlobalEvent()
    {
    }

    #endregion
}

#endregion
