using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用动画控制器
/// 
/// 封装 Animator 的常用操作，提供：
/// - 参数哈希缓存（避免字符串查找的 GC 分配）
/// - 动画状态追踪（当前状态名、标准化时间）
/// - 安全的跨模块动画调用
/// 
/// 使用方式：
///   挂载到任意带 Animator 的 GameObject 上，直接调用 Play/SetBool/SetFloat 等方法。
/// </summary>
[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    #region 字段

    private Animator _animator;
    private readonly Dictionary<string, int> _paramHashCache = new Dictionary<string, int>();

    /// <summary>当前正在播放的动画状态完整路径名</summary>
    public string CurrentStateName { get; private set; }

    /// <summary>上一个动画状态完整路径名</summary>
    public string PreviousStateName { get; private set; }

    /// <summary>当前动画状态的标准化时间（0~1）</summary>
    public float NormalizedTime
    {
        get
        {
            if (_animator == null) return 0f;
            var state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.normalizedTime % 1f;
        }
    }

    /// <summary>Animator 组件是否可用</summary>
    public bool IsValid => _animator != null;

    #endregion

    #region Unity 生命周期

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    #endregion

    #region 播放动画

    /// <summary>
    /// 播放指定动画（立刻切换）
    /// </summary>
    /// <param name="stateName">动画状态名称（可以是完整路径或短名称）</param>
    /// <param name="layer">动画层索引，默认 0</param>
    /// <param name="normalizedTime">起始标准化时间，默认 0</param>
    public void Play(string stateName, int layer = 0, float normalizedTime = 0f)
    {
        if (_animator == null || string.IsNullOrEmpty(stateName)) return;

        PreviousStateName = CurrentStateName;
        _animator.Play(stateName, layer, normalizedTime);
        CurrentStateName = stateName;

        PublishStateChangeEvent();
    }

    /// <summary>
    /// 以交叉淡入方式播放动画
    /// </summary>
    /// <param name="stateName">动画状态名称</param>
    /// <param name="crossFadeDuration">过渡时长（秒）</param>
    /// <param name="layer">动画层索引，默认 0</param>
    /// <param name="normalizedTimeOffset">起始标准化时间偏移</param>
    public void CrossFade(string stateName, float crossFadeDuration = 0.1f, int layer = 0, float normalizedTimeOffset = 0f)
    {
        if (_animator == null || string.IsNullOrEmpty(stateName)) return;

        PreviousStateName = CurrentStateName;
        int hash = GetParamHash(stateName);
        _animator.CrossFadeInFixedTime(hash, crossFadeDuration, layer, normalizedTimeOffset);
        CurrentStateName = stateName;

        PublishStateChangeEvent();
    }

    /// <summary>
    /// 通过 Hash 播放动画（更高性能）
    /// </summary>
    public void PlayByHash(int stateHash, int layer = 0, float normalizedTime = 0f)
    {
        if (_animator == null) return;
        _animator.Play(stateHash, layer, normalizedTime);
    }

    /// <summary>
    /// 通过 Hash 交叉淡入播放动画（更高性能）
    /// </summary>
    public void CrossFadeByHash(int stateHash, float crossFadeDuration = 0.1f, int layer = 0, float normalizedTimeOffset = 0f)
    {
        if (_animator == null) return;
        _animator.CrossFadeInFixedTime(stateHash, crossFadeDuration, layer, normalizedTimeOffset);
    }

    #endregion

    #region 参数设置

    /// <summary>设置 Bool 参数</summary>
    public void SetBool(string paramName, bool value)
    {
        if (_animator == null) return;
        _animator.SetBool(GetParamHash(paramName), value);
    }

    /// <summary>设置 Float 参数</summary>
    public void SetFloat(string paramName, float value)
    {
        if (_animator == null) return;
        _animator.SetFloat(GetParamHash(paramName), value);
    }

    /// <summary>设置 Int 参数</summary>
    public void SetInt(string paramName, int value)
    {
        if (_animator == null) return;
        _animator.SetInteger(GetParamHash(paramName), value);
    }

    /// <summary>设置 Trigger 参数</summary>
    public void SetTrigger(string paramName)
    {
        if (_animator == null) return;
        _animator.SetTrigger(GetParamHash(paramName));
    }

    /// <summary>重置 Trigger 参数</summary>
    public void ResetTrigger(string paramName)
    {
        if (_animator == null) return;
        _animator.ResetTrigger(GetParamHash(paramName));
    }

    #endregion

    #region 参数读取

    /// <summary>读取 Bool 参数</summary>
    public bool GetBool(string paramName)
    {
        if (_animator == null) return false;
        return _animator.GetBool(GetParamHash(paramName));
    }

    /// <summary>读取 Float 参数</summary>
    public float GetFloat(string paramName)
    {
        if (_animator == null) return 0f;
        return _animator.GetFloat(GetParamHash(paramName));
    }

    /// <summary>读取 Int 参数</summary>
    public int GetInt(string paramName)
    {
        if (_animator == null) return 0;
        return _animator.GetInteger(GetParamHash(paramName));
    }

    #endregion

    #region 状态查询

    /// <summary>检查当前是否在播放指定动画</summary>
    public bool IsPlaying(string stateName, int layer = 0)
    {
        if (_animator == null) return false;
        return _animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName);
    }

    /// <summary>当前动画是否已播放完毕（非循环动画）</summary>
    public bool HasFinished(int layer = 0)
    {
        if (_animator == null) return false;
        var state = _animator.GetCurrentAnimatorStateInfo(layer);
        return state.normalizedTime >= 1f && !_animator.IsInTransition(layer);
    }

    /// <summary>获取指定层的动画状态信息</summary>
    public AnimatorStateInfo GetStateInfo(int layer = 0)
    {
        return _animator != null ? _animator.GetCurrentAnimatorStateInfo(layer) : default;
    }

    /// <summary>是否正在过渡中</summary>
    public bool IsInTransition(int layer = 0)
    {
        return _animator != null && _animator.IsInTransition(layer);
    }

    #endregion

    #region 速度控制

    /// <summary>获取/设置动画播放速度（1 = 正常速度）</summary>
    public float Speed
    {
        get => _animator != null ? _animator.speed : 1f;
        set { if (_animator != null) _animator.speed = value; }
    }

    #endregion

    #region 内部方法

    /// <summary>获取参数哈希值（带缓存）</summary>
    private int GetParamHash(string paramName)
    {
        if (!_paramHashCache.TryGetValue(paramName, out int hash))
        {
            hash = Animator.StringToHash(paramName);
            _paramHashCache[paramName] = hash;
        }
        return hash;
    }

    /// <summary>发布动画状态变更事件</summary>
    private void PublishStateChangeEvent()
    {
        GameEvents.Publish(EventType.ANIMATION_EVENT_ON_STATE_CHANGE,
            new AnimationStateEventArgs(gameObject, PreviousStateName, CurrentStateName));
    }

    #endregion
}
