using System;
using System.Collections.Generic;

#region ========== 核心事件总线 ==========

/// <summary>
/// 全局事件总线（通用版）
/// 
/// 特性：
/// - 基于枚举的事件类型分发
/// - 线程安全（lock 保护）
/// - 委托链防重复订阅
/// - 锁外回调，避免死锁
/// 
/// 使用方式：
///   1. 定义你的事件枚举（如 EventType）
///   2. 定义你的事件参数类（实现 IGameEvent 接口）
///   3. 通过 GameEvents.Listen() 订阅、GameEvents.Publish() 发布
/// </summary>
public static class GameEvents
{
    /// <summary>事件处理器委托：接收一个 IGameEvent 参数</summary>
    public delegate void GameEventHandler(IGameEvent evt);

    /// <summary>事件监听器字典：事件类型 -> 事件处理器委托链</summary>
    private static readonly Dictionary<Enum, GameEventHandler> _eventTable = new Dictionary<Enum, GameEventHandler>();

    /// <summary>线程锁，保证订阅/取消订阅/发布的线程安全</summary>
    private static readonly object _lock = new object();

    #region 发布事件

    /// <summary>
    /// 发布事件
    /// </summary>
    /// <param name="eventType">事件类型枚举值</param>
    /// <param name="evt">事件参数（需实现 IGameEvent）</param>
    public static void Publish(Enum eventType, IGameEvent evt)
    {
        GameEventHandler handlerCopy = null;

        lock (_lock)
        {
            if (_eventTable.TryGetValue(eventType, out var handler))
            {
                // 复制委托链，防止在回调中修改字典导致异常
                handlerCopy = handler;
            }
        }

        // 在锁外调用，避免回调中再次操作事件系统时死锁
        handlerCopy?.Invoke(evt);
    }

    #endregion

    #region 订阅事件

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <param name="eventType">事件类型枚举值</param>
    /// <param name="handler">事件处理器</param>
    public static void Listen(Enum eventType, GameEventHandler handler)
    {
        if (eventType == null || handler == null) return;

        lock (_lock)
        {
            if (_eventTable.TryGetValue(eventType, out var existing))
            {
                // 防止重复订阅同一个处理器
                if (!IsHandlerInChain(existing, handler))
                {
                    _eventTable[eventType] = (GameEventHandler)Delegate.Combine(existing, handler);
                }
            }
            else
            {
                _eventTable[eventType] = handler;
            }
        }
    }

    #endregion

    #region 取消订阅

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    /// <param name="eventType">事件类型枚举值</param>
    /// <param name="handler">之前订阅的事件处理器</param>
    public static void Unlisten(Enum eventType, GameEventHandler handler)
    {
        if (eventType == null || handler == null) return;

        lock (_lock)
        {
            if (_eventTable.TryGetValue(eventType, out var existing))
            {
                var newHandler = (GameEventHandler)Delegate.Remove(existing, handler);
                if (newHandler == null)
                    _eventTable.Remove(eventType);  // 没有监听器了，清理字典
                else
                    _eventTable[eventType] = newHandler;
            }
        }
    }

    #endregion

    #region 工具方法

    /// <summary>清空所有事件监听器（场景切换/游戏退出时调用）</summary>
    public static void ClearAllListeners()
    {
        lock (_lock)
        {
            _eventTable.Clear();
        }
    }

    /// <summary>获取某事件类型的监听器数量（调试用）</summary>
    public static int GetListenerCount(Enum eventType)
    {
        lock (_lock)
        {
            return _eventTable.TryGetValue(eventType, out var handler) ? handler.GetInvocationList().Length : 0;
        }
    }

    /// <summary>获取所有已注册的事件类型（调试用）</summary>
    public static Enum[] GetAllEventTypes()
    {
        lock (_lock)
        {
            var keys = new Enum[_eventTable.Count];
            _eventTable.Keys.CopyTo(keys, 0);
            return keys;
        }
    }

    /// <summary>检查处理器链中是否已包含目标处理器</summary>
    private static bool IsHandlerInChain(GameEventHandler chain, GameEventHandler target)
    {
        if (chain == null || target == null) return false;
        var list = chain.GetInvocationList();
        foreach (var d in list)
        {
            if (d == (Delegate)target)
                return true;
        }
        return false;
    }

    #endregion
}

#endregion

#region ========== 事件接口与基类 ==========

/// <summary>
/// 游戏事件接口
/// 所有事件参数类需实现此接口
/// 
/// 示例：
///   public class MyEventArgs : IGameEvent { public int Value; }
/// </summary>
public interface IGameEvent
{
    // 可在此扩展通用属性，如事件来源、优先级等
}

/// <summary>
/// 游戏事件基类（可选继承）
/// 提供 Timestamp 属性，记录事件发生时间
/// 
/// 注意：默认使用 UnityEngine.Time.time 获取时间戳。
///       非 Unity 项目请修改或移除此类。
/// </summary>
public class GameEventBase : IGameEvent
{
    /// <summary>事件发生的时间戳（游戏时间）</summary>
    public float Timestamp { get; private set; }

    public GameEventBase()
    {
#if UNITY_ENGINE
        Timestamp = UnityEngine.Time.time;
#else
        Timestamp = (float)DateTime.Now.TimeOfDay.TotalSeconds;
#endif
    }
}

#endregion
