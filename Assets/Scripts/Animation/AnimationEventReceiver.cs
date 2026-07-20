using UnityEngine;

/// <summary>
/// 动画事件桥接器
/// 
/// 将 Unity 原生的 Animation Event 转发到项目的 GameEvents 事件系统，
/// 让其他模块可以通过标准事件机制响应动画事件。
/// 
/// 使用方式：
///   1. 挂载到带 Animator 的 GameObject 上
///   2. 在 Animation Clip 中添加 Animation Event
///   3. 选择此脚本的公开方法作为回调
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    #region 无参事件回调（供 Animation Event 调用）

    /// <summary>发送通用动画事件（无参数）</summary>
    public void SendEvent(string eventKey)
    {
        if (string.IsNullOrEmpty(eventKey)) return;

        GameEvents.Publish(EventType.ANIMATION_EVENT_ON_CUSTOM,
            new AnimationCustomEventArgs(gameObject, eventKey));
    }

    #endregion

    #region 带参事件回调（供 Animation Event 调用）

    /// <summary>发送带字符串参数的动画事件</summary>
    public void SendStringEvent(string eventKey, string value)
    {
        if (string.IsNullOrEmpty(eventKey)) return;

        GameEvents.Publish(EventType.ANIMATION_EVENT_ON_CUSTOM,
            new AnimationCustomEventArgs(gameObject, eventKey, value));
    }

    /// <summary>发送带整数参数的动画事件</summary>
    public void SendIntEvent(string eventKey, int value)
    {
        if (string.IsNullOrEmpty(eventKey)) return;

        GameEvents.Publish(EventType.ANIMATION_EVENT_ON_CUSTOM,
            new AnimationCustomEventArgs(gameObject, eventKey, value));
    }

    /// <summary>发送带浮点参数的动画事件</summary>
    public void SendFloatEvent(string eventKey, float value)
    {
        if (string.IsNullOrEmpty(eventKey)) return;

        GameEvents.Publish(EventType.ANIMATION_EVENT_ON_CUSTOM,
            new AnimationCustomEventArgs(gameObject, eventKey, value));
    }

    #endregion

    #region 常用语义化回调

    /// <summary>动画开始</summary>
    public void OnAnimationStart(string stateName)
    {
        GameEvents.Publish(EventType.ANIMATION_EVENT_ON_START,
            new AnimationStateEventArgs(gameObject, null, stateName));
    }

    /// <summary>动画结束</summary>
    public void OnAnimationEnd(string stateName)
    {
        GameEvents.Publish(EventType.ANIMATION_EVENT_ON_END,
            new AnimationStateEventArgs(gameObject, stateName, null));
    }

    /// <summary>动画关键帧（如攻击判定帧、音效帧）</summary>
    public void OnKeyFrame(string key)
    {
        GameEvents.Publish(EventType.ANIMATION_EVENT_ON_KEYFRAME,
            new AnimationCustomEventArgs(gameObject, key));
    }

    #endregion
}
