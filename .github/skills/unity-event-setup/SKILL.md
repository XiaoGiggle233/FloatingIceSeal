---
name: unity-event-setup
description: '为 Unity 功能开发集成 GameEvents 事件系统。Use when: 开发新功能需要模块间通信、添加事件类型、创建事件参数类、搭建订阅/取消订阅框架、MonoBehaviour 中接入 GameEvents.Listen/Unlisten/Publish。触发词：事件系统、GameEvents、事件总线、模块通信、解耦、订阅事件、发布事件、EventType、EventArgs。'
argument-hint: '[功能描述]'
---

# Unity GameEvents 事件系统集成

为新功能模块自动集成项目已有的 `GameEvents` 事件总线系统，确保模块间松耦合通信。

## 项目事件系统文件位置

- 核心总线: [GameEvents.cs](../../../Assets/Scripts/Events/GameEvents.cs)
- 事件类型模板: [EventTypes_Template.cs](../../../Assets/Scripts/Events/EventTypes_Template.cs)
- 日志类型模板: [GameLogTypes_Template.cs](../../../Assets/Scripts/Events/GameLogTypes_Template.cs)
- 使用文档: [使用文档.md](../../../Assets/Scripts/Events/使用文档.md)

## 核心 API

```csharp
// 订阅（OnEnable/Start 中调用）
GameEvents.Listen(EventType.XXX, HandlerMethod);

// 取消订阅（OnDisable/OnDestroy 中调用，必须配对）
GameEvents.Unlisten(EventType.XXX, HandlerMethod);

// 发布事件
GameEvents.Publish(EventType.XXX, new SomeEventArgs(data));
```

## 工作流程

### 步骤 1：分析功能需求

确认当前开发的功能是否需要通过事件与其他模块通信。典型场景：
- 玩家状态变化（升级、死亡、受伤）→ 需要通知 UI、音效、存档等模块
- UI 操作（按钮点击、面板开关）→ 需要通知游戏逻辑层
- 物品/道具操作（拾取、使用、购买）→ 需要通知背包、任务、日志等模块
- 场景/关卡变化 → 需要通知所有模块清理状态

如果功能**不需要**跨模块通信，则跳过事件集成。

### 步骤 2：检查现有事件类型

查看 `EventTypes_Template.cs` 中的 `EventType` 枚举，确认是否已有适合的事件类型：
- 如果有完全匹配的枚举值 → 直接使用，跳到步骤 4
- 如果语义接近但不完全匹配 → 考虑复用或添加新值
- 如果完全没有相关事件 → 添加新的事件类型

### 步骤 3：添加新事件类型（按需）

在 `EventType` 枚举中添加新值，遵循现有命名规范：

**命名规范**：`{模块前缀}_EVENT_ON_{动作}`

| 模块前缀 | 示例 |
|----------|------|
| `GAME` | `GAME_EVENT_ON_START` |
| `SCENE` | `SCENE_EVENT_ON_LOADED` |
| `PLAYER` | `PLAYER_EVENT_ON_DEATH` |
| `UI` | `UI_EVENT_ON_PANEL_OPEN` |
| `ITEM` | `ITEM_EVENT_ON_PICKUP` |
| `INPUT` | `INPUT_EVENT_ON_SELECT` |
| `RESOURCE` | `RESOURCE_EVENT_ON_CHANGE` |
| `AUDIO` | `AUDIO_EVENT_ON_PLAY_SFX` |

```csharp
// 在对应模块的 #region 内添加
PLAYER_EVENT_ON_LEVEL_UP,       // 玩家升级
PLAYER_EVENT_ON_SKILL_LEARN,    // 学习技能
```

如果需要携带数据，创建对应的事件参数类：

**命名规范**：以 `EventArgs` 结尾，继承 `GameEventBase`

```csharp
public class PlayerLevelUpEventArgs : GameEventBase
{
    public int PlayerId { get; }
    public int NewLevel { get; }

    public PlayerLevelUpEventArgs(int playerId, int newLevel)
    {
        PlayerId = playerId;
        NewLevel = newLevel;
    }
}
```

不需要数据的简单事件直接使用 `GameEventBase` 即可。

### 步骤 4：在 MonoBehaviour 中搭建订阅框架

为需要监听或发布事件的 MonoBehaviour 添加标准的事件订阅/取消订阅代码：

```csharp
using UnityEngine;

public class SomeFeature : MonoBehaviour
{
    private void OnEnable()
    {
        // 订阅需要监听的事件
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_LEVEL_UP, OnPlayerLevelUp);
        GameEvents.Listen(EventType.GAME_EVENT_ON_PAUSE, OnGamePause);
    }

    private void OnDisable()
    {
        // 必须取消订阅，防止对象销毁后空引用
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_LEVEL_UP, OnPlayerLevelUp);
        GameEvents.Unlisten(EventType.GAME_EVENT_ON_PAUSE, OnGamePause);
    }

    // 事件处理器签名: void MethodName(IGameEvent evt)
    private void OnPlayerLevelUp(IGameEvent evt)
    {
        var args = evt as PlayerLevelUpEventArgs;
        // 处理升级逻辑
    }

    private void OnGamePause(IGameEvent evt)
    {
        // 处理暂停逻辑
    }

    // 发布事件的时机
    public void DoSomething()
    {
        // 发布带数据的事件
        GameEvents.Publish(EventType.UI_EVENT_SHOW_MESSAGE,
            new UIMessageEventArgs("操作完成", "Info"));

        // 发布简单事件
        GameEvents.Publish(EventType.AUDIO_EVENT_ON_PLAY_SFX,
            new GameEventBase());
    }
}
```

### 步骤 5：验证

完成后检查以下要点：

| 检查项 | 说明 |
|--------|------|
| 订阅/取消配对 | `OnEnable` 中的每个 `Listen` 在 `OnDisable` 中都有对应的 `Unlisten` |
| 事件类型存在 | 使用的 `EventType` 枚举值已在 `EventTypes_Template.cs` 中定义 |
| 参数类正确 | 事件参数类继承 `GameEventBase`，构造函数正确赋值 |
| 命名规范 | 枚举值遵循 `模块_EVENT_ON_动作` 格式，参数类以 `EventArgs` 结尾 |
| 无循环发布 | 不在事件回调中发布可能触发自身的同类事件 |

### 步骤 6：集成游戏日志（按需）

如果功能需要记录游戏日志（战斗记录、操作历史、任务日志等），使用 `GameLogTypes_Template.cs` 中的日志系统：

```csharp
// 创建日志条目
var logEntry = new GameLogEntry
{
    index = 0,                    // 日志系统自行管理序号
    phase = "Gameplay",           // 阶段标签
    action = GameLogAction.ItemUse,
    sourceName = "Player",
    targetName = "HealthPotion",
    value = 50f,
    flags = "",
    extra = ""
};

// 通过事件总线发布日志
GameEvents.Publish(EventType.LOG_EVENT_ENTRY, new GameLogEventArgs(logEntry));
```

**日志动作类型速查**（`GameLogAction` 枚举）：

| 类别 | 枚举值 | 用途 |
|------|--------|------|
| 游戏流程 | `GameStart`, `GameEnd`, `LevelStart`, `LevelEnd`, `PhaseChange` | 游戏/关卡生命周期 |
| 玩家行为 | `PlayerAction`, `ItemUse`, `ItemPickup`, `Dialogue` | 玩家操作记录 |
| 系统 | `Achievement`, `Error`, `Warning`, `Info` | 系统和状态日志 |
| 数值变化 | `ScoreChange`, `StatChange` | 分数/属性变动 |

如果现有 `GameLogAction` 不够用，直接在枚举末尾添加新值即可。

### 步骤 7：调试验证

使用 `GameEvents` 内置的调试方法排查事件问题：

```csharp
// 检查某个事件的监听器数量（确认订阅成功）
int count = GameEvents.GetListenerCount(EventType.PLAYER_EVENT_ON_DEATH);
Debug.Log($"PLAYER_ON_DEATH 监听器数量: {count}");

// 列出所有已注册的事件类型（排查忘记取消订阅的事件）
Enum[] allTypes = GameEvents.GetAllEventTypes();
foreach (var t in allTypes)
{
    Debug.Log($"事件: {t}, 监听器: {GameEvents.GetListenerCount(t)}");
}
```

**常见问题排查**：

| 症状 | 可能原因 | 检查方法 |
|------|----------|----------|
| 事件回调不触发 | 未订阅或订阅了错误的事件类型 | `GetListenerCount` 确认监听器数量 |
| 空引用异常 | `OnDisable` 未取消订阅，对象已销毁 | 检查 `Listen`/`Unlisten` 是否配对 |
| 回调被多次触发 | 在 `OnEnable` 中重复订阅（已防重复）或多次调用 `OnEnable` | 在回调中打印 `Debug.Log` 确认次数 |
| 事件未收到数据 | 参数类转型失败（`as` 返回 null） | 检查发布时使用的 EventArgs 类型与回调中转型的类型是否一致 |

## 注意事项

- **必须在 OnDisable 中取消订阅**，否则对象销毁后触发事件会导致空引用异常
- **事件处理器签名固定为 `void (IGameEvent)`**，通过 `as` 转型获取具体参数类
- **GameEventBase 自动记录 Timestamp**（Unity 下使用 `Time.time`）
- **同一处理器重复订阅会被自动忽略**（幂等操作）
- **不订阅任何 Unity 生命周期事件**（如 Update、FixedUpdate），GameEvents 是应用层事件总线
