---
name: modify-state-machine
description: '修改项目现有的 Seal 状态机——添加新状态、移除状态、修改状态行为。Use when: 为 LifeStateMachine/OxygenStateMachine/EnvironmentStateMachine/ActionStateMachine 添加新状态、新增 enum 值、创建 State 类、修改状态转换逻辑。触发词：状态机、添加状态、新增状态、StateMachine、State、enum 状态、EnterLeafState、SetState。'
argument-hint: '[状态机名称 操作描述]'
---

# 修改现有状态机

为 `SealStateMachine.cs` 中已有的状态机（Life/Oxygen/Environment/Action）添加、移除或修改状态。

## 状态机文件位置

- 状态机定义: [SealStateMachine.cs](../../../Assets/Scripts/Character/SealStateMachine.cs)
- 宿主类: [Seal.cs](../../../Assets/Scripts/Character/Seal.cs)

## 状态机架构

项目使用四层状态机，每层结构一致：

```
enum XxxState { ... }                    // 枚举所有状态
abstract class XxxStateBase              // 抽象基类，继承 SealState<XxxStateMachine>
class ConcreteState : XxxStateBase       // 每个枚举值对应一个具体状态类
class XxxStateMachine                    // 状态机主控类
```

每个状态机类的核心成员：

```csharp
public class XxxStateMachine
{
    private Seal owner;
    public XxxState CurrentState { get; private set; }   // 当前状态的枚举值
    private XxxStateBase currentLeafState;               // 当前状态实例
    // 每个具体状态一个 public 属性
    // 构造函数中初始化所有状态
    // Update() / FixedUpdate() 委托给 currentLeafState
    // EnterLeafState(newState) 执行状态转换
    // SetState(state) 设置枚举值，内置去重并自动发布 PLAYER_EVENT_ON_STATE_CHANGE 事件
    // IsXxx() 判断当前状态（每个枚举值对应一个方法）
}
```

## 操作流程

### 1. 添加新状态

#### 步骤 1：在枚举中添加新值

在对应 `enum` 末尾追加：

```csharp
public enum ActionState { Idle, OnLandMoving, InWaterMoving, Dashing, NewState }
```

#### 步骤 2：创建具体状态类

在对应 `#region` 内、状态机类之前添加。遵循命名：`{状态名}State`，继承对应的抽象基类：

```csharp
public class NewStateState : XxxStateBase
{
    public NewStateState(Seal owner, XxxStateMachine fsm) : base(owner, fsm) { }

    public override void Enter() { /* 进入逻辑 */ }
    public override void Exit() { /* 退出逻辑 */ }
    public override void Update() { /* 每帧逻辑 */ }
}
```

**注意**：只 override 需要的方法，不必全部 override。

#### 步骤 3：在状态机类中注册

3a. 添加 public 属性：

```csharp
public NewStateState NewStateState { get; }
```

3b. 在构造函数中初始化：

```csharp
NewStateState = new NewStateState(owner, this);
```

#### 步骤 4：添加转换逻辑（按需）

在业务代码中调用状态转换：

```csharp
// 设置枚举
fsm.SetState(ActionState.NewState);
// 进入叶子状态
fsm.EnterLeafState(fsm.NewStateState);
```

### 2. 移除状态

#### 步骤 1：从 enum 中移除对应值

检查是否有其他代码引用该枚举值（通过 `grep_search` 搜索枚举值名称），确认无引用后删除。

#### 步骤 2：删除具体状态类

#### 步骤 3：从状态机类中移除

- 删除 public 属性
- 删除构造函数中的初始化行

### 3. 修改现有状态行为

直接编辑对应具体状态类的 `Enter()` / `Exit()` / `Update()` / `FixedUpdate()` 方法。

## 现有状态机清单

| 状态机 | 枚举 | 当前状态 |
|--------|------|----------|
| `LifeStateMachine` | `LifeState` | Alive, Dead |
| `OxygenStateMachine` | `OxygenState` | OxygenFull, OxygenSufficient, OxygenInsufficient, OxygenCritical, Suffocating |
| `EnvironmentStateMachine` | `EnvironmentState` | InAir, OnLand, InWater |
| `ActionStateMachine` | `ActionState` | Idle, OnLandMoving, InWaterMoving, Dashing |
| `DirectionStateMachine` | `DirectionState` | Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight |

## 注意事项

- 状态类名以具体状态名 + `State` 结尾，如 `AliveState`、`DeadState`
- 抽象基类名以 `XxxStateBase` 命名
- 状态机类的 public 属性命名与类名一致（如 `AliveState` → `AliveState`）
- 所有状态类构造函数签名统一：`(Seal owner, XxxStateMachine fsm)`
- 添加新状态后，为枚举中每个新值添加对应的 `IsXxx()` 判断方法
- 检查 `Seal.cs` 的 `Awake()` 中是否需要在初始状态设置中体现
- 跨状态机协作时，在状态方法中通过 `owner.XxxStateMachine` 访问其他状态机
