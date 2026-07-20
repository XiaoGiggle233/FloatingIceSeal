---
name: animation-control
description: '为 GameObject 快速接入通用动画控制系统——播放动画、设置参数、监听动画事件。Use when: 为角色/物体添加动画控制、调用 Play/CrossFade 播放动画、设置 Animator 参数（SetBool/SetFloat/SetInt/SetTrigger）、通过 Animation Event 触发逻辑、监听动画开始/结束/关键帧事件、AnimationController 接入。触发词：动画控制、播放动画、AnimationController、CrossFade、SetBool、SetFloat、SetTrigger、动画事件、AnimationEvent、动画开始、动画结束、关键帧、AnimationEventReceiver。'
argument-hint: '[GameObject名或操作描述]'
---

# 通用动画控制系统

为任意带 Animator 的 GameObject 接入动画控制。根据需求选择**轻量模式**或**事件模式**：

| 模式 | 挂载组件 | 适用场景 |
|------|----------|----------|
| 轻量模式 | 只需 `AnimationController` | 仅需播放动画、设置参数，不需要在特定动画帧触发逻辑 |
| 事件模式 | `AnimationController` + `AnimationEventReceiver` | 需要在动画的特定帧触发逻辑（攻击判定、音效、粒子等），或需要其他模块监听动画状态变化 |

两种模式**互不依赖**——轻量模式可以独立工作，事件模式在轻量模式基础上扩展。

## 系统文件位置

- 动画控制器: [AnimationController.cs](../../../Assets/Scripts/Animation/AnimationController.cs)
- 事件桥接器: [AnimationEventReceiver.cs](../../../Assets/Scripts/Animation/AnimationEventReceiver.cs)
- 动画事件类型: `EventType` 枚举中 `ANIMATION_EVENT_ON_*` 系列
- 动画事件参数: `AnimationStateEventArgs`, `AnimationCustomEventArgs`

---

## 轻量模式（仅 AnimationController）

只需播放动画和控制参数，不涉及事件通信。

### 步骤 1：挂载 AnimationController

在 Inspector 中点击 **Add Component → AnimationController**。`[RequireComponent(typeof(Animator))]` 会在缺少时自动添加 Animator。

### 步骤 2：在业务脚本中控制动画

在业务脚本中通过 `GetComponent<AnimationController>()` 获取引用：

```csharp
private AnimationController anim;

private void Awake()
{
    anim = GetComponent<AnimationController>();
}

// 播放动画
anim.Play("Idle");

// 带过渡的播放
anim.CrossFade("Run", 0.2f);

// 设置参数
anim.SetBool("IsGrounded", true);
anim.SetFloat("Speed", moveSpeed);
anim.SetInt("State", 1);
anim.SetTrigger("Jump");
anim.ResetTrigger("Jump");

// 查询状态
if (anim.IsPlaying("Attack"))
    Debug.Log("正在播放攻击动画");

if (anim.HasFinished())
    Debug.Log("当前动画已播放完毕");

// 控制速度
anim.Speed = 1.5f;  // 1.5 倍速
```

**可用的全部方法**：

| 类别 | 方法 |
|------|------|
| 播放 | `Play()`, `CrossFade()`, `PlayByHash()`, `CrossFadeByHash()` |
| 设置参数 | `SetBool()`, `SetFloat()`, `SetInt()`, `SetTrigger()`, `ResetTrigger()` |
| 读取参数 | `GetBool()`, `GetFloat()`, `GetInt()` |
| 状态查询 | `IsPlaying()`, `HasFinished()`, `IsInTransition()`, `GetStateInfo()` |
| 属性 | `Speed`, `NormalizedTime`, `CurrentStateName`, `PreviousStateName`, `IsValid` |

---

## 事件模式（AnimationController + AnimationEventReceiver）

在轻量模式基础上，增加动画帧级别的逻辑触发能力。**挂载 AnimationEventReceiver 前先确认：是否真的有需要在特定动画帧触发的逻辑？如果没有，保持轻量模式即可。**

### 步骤 1：挂载 AnimationEventReceiver

在已有 AnimationController 的 GameObject 上，Add Component → **AnimationEventReceiver**。

### 步骤 2：在 Animation Clip 中添加 Animation Event

在 Unity 编辑器中打开 Animation 窗口，在目标帧位置右键 → **Add Animation Event**，Function 选择 `AnimationEventReceiver` 的方法：

| Function | 用途 | 典型场景 |
|------|------|------|
| `OnAnimationStart(string)` | 动画开始时触发 | 进入攻击动画时启用武器碰撞体 |
| `OnAnimationEnd(string)` | 动画结束时触发 | 攻击动画结束时切回 Idle |
| `OnKeyFrame(string)` | 关键帧触发 | 攻击判定帧、音效帧、粒子生成帧 |
| `SendEvent(string)` | 通用无参事件 | 自定义逻辑触发 |
| `SendStringEvent(string, string)` | 带字符串参数 | 传递标识符 |
| `SendIntEvent(string, int)` | 带整数参数 | 传递伤害值等数值 |
| `SendFloatEvent(string, float)` | 带浮点参数 | 传递力度等浮点值 |

### 步骤 3：在业务脚本中监听动画事件

```csharp
private void OnEnable()
{
    // 只监听需要的事件类型，不必全部订阅
    GameEvents.Listen(EventType.ANIMATION_EVENT_ON_END, OnAnimEnd);
    GameEvents.Listen(EventType.ANIMATION_EVENT_ON_KEYFRAME, OnKeyFrame);
}

private void OnDisable()
{
    GameEvents.Unlisten(EventType.ANIMATION_EVENT_ON_END, OnAnimEnd);
    GameEvents.Unlisten(EventType.ANIMATION_EVENT_ON_KEYFRAME, OnKeyFrame);
}

private void OnAnimEnd(IGameEvent evt)
{
    var args = evt as AnimationStateEventArgs;
    if (args?.AnimatorOwner != gameObject) return;
    // 只过滤自己对象的动画事件
}

private void OnKeyFrame(IGameEvent evt)
{
    var args = evt as AnimationCustomEventArgs;
    if (args?.AnimatorOwner != gameObject) return;

    if (args.EventKey == "AttackHit")
    {
        // 攻击判定逻辑
    }
}
```

> **关键模式**：事件处理器中始终用 `args.AnimatorOwner != gameObject` 过滤，防止多个对象的动画事件互相干扰。

### 步骤 4：动画状态变更监听（可选，无需 AnimationEventReceiver）

`AnimationController.Play()` / `CrossFade()` 会自动发布 `ANIMATION_EVENT_ON_STATE_CHANGE` 事件，**不依赖 AnimationEventReceiver**——即使只使用轻量模式也能监听：

```csharp
private void OnEnable()
{
    GameEvents.Listen(EventType.ANIMATION_EVENT_ON_STATE_CHANGE, OnStateChange);
}

private void OnStateChange(IGameEvent evt)
{
    var args = evt as AnimationStateEventArgs;
    if (args?.AnimatorOwner != gameObject) return;
    Debug.Log($"动画切换: {args.PreviousState} → {args.CurrentState}");
}
```

---

## 性能优化（Hash 模式）

`AnimationController` 内部已对参数名做哈希缓存，直接传字符串即可。如需极致性能，用 Hash 版方法：

```csharp
private static readonly int RunHash = Animator.StringToHash("Run");

anim.PlayByHash(RunHash);
anim.CrossFadeByHash(RunHash, 0.2f);
```

## 可用事件类型

| EventType | 触发时机 | 需要 AnimationEventReceiver？ |
|------|------|------|
| `ANIMATION_EVENT_ON_STATE_CHANGE` | `AnimationController.Play()` / `CrossFade()` 调用时 | 否 |
| `ANIMATION_EVENT_ON_START` | Animation Clip 中调用 `OnAnimationStart` | 是 |
| `ANIMATION_EVENT_ON_END` | Animation Clip 中调用 `OnAnimationEnd` | 是 |
| `ANIMATION_EVENT_ON_KEYFRAME` | Animation Clip 中调用 `OnKeyFrame` | 是 |
| `ANIMATION_EVENT_ON_CUSTOM` | Animation Clip 中调用 `SendEvent` 系列 | 是 |

---

## 完整接入模板

### 轻量模式模板

```csharp
using UnityEngine;

[RequireComponent(typeof(AnimationController))]
public class SomeAnimatedObject : MonoBehaviour
{
    private AnimationController anim;

    private void Awake()
    {
        anim = GetComponent<AnimationController>();
    }

    private void Update()
    {
        float speed = GetComponent<Rigidbody2D>().velocity.magnitude;
        anim.SetFloat("Speed", speed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("Jump");
            anim.CrossFade("Jump", 0.1f);
        }
    }
}
```

### 事件模式模板（在轻量模式基础上扩展）

```csharp
using UnityEngine;

[RequireComponent(typeof(AnimationController))]
public class SomeAnimatedObject : MonoBehaviour
{
    private AnimationController anim;

    private void Awake()
    {
        anim = GetComponent<AnimationController>();
    }

    private void OnEnable()
    {
        // 只在需要时订阅
        GameEvents.Listen(EventType.ANIMATION_EVENT_ON_END, OnAnimEnd);
        GameEvents.Listen(EventType.ANIMATION_EVENT_ON_KEYFRAME, OnKeyFrame);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.ANIMATION_EVENT_ON_END, OnAnimEnd);
        GameEvents.Unlisten(EventType.ANIMATION_EVENT_ON_KEYFRAME, OnKeyFrame);
    }

    private void Update()
    {
        float speed = GetComponent<Rigidbody2D>().velocity.magnitude;
        anim.SetFloat("Speed", speed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("Jump");
            anim.CrossFade("Jump", 0.1f);
        }
    }

    private void OnAnimEnd(IGameEvent evt)
    {
        var args = evt as AnimationStateEventArgs;
        if (args?.AnimatorOwner != gameObject) return;

        if (args.CurrentState == "Attack")
            anim.CrossFade("Idle", 0.15f);
    }

    private void OnKeyFrame(IGameEvent evt)
    {
        var args = evt as AnimationCustomEventArgs;
        if (args?.AnimatorOwner != gameObject) return;

        if (args.EventKey == "AttackHit")
            Debug.Log("攻击判定帧！");
    }
}
```
    {
        var args = evt as AnimationCustomEventArgs;
        if (args?.AnimatorOwner != gameObject) return;

        if (args.EventKey == "AttackHit")
            Debug.Log("攻击判定帧！");
    }
}
```
