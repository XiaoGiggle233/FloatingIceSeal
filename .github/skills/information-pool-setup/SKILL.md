---
name: information-pool-setup
description: '为 MonoBehaviour 注册/注销信息池监听，读取/写入共享数据。Use when: 需要跨模块共享数据、监听信息池变更、在 OnEnable/OnDisable 中订阅 INFO_POOL_EVENT_ON_CHANGE、通过 InformationPool.Get/Set 存取数据。触发词：信息池、InformationPool、共享数据、全局数据、注册信息池、注销信息池、InfoPool。'
argument-hint: '[功能描述]'
---

# Unity InformationPool 信息池集成

为 MonoBehaviour 接入项目已有的 `InformationPool` 全局信息池系统，实现模块间数据共享与变更监听。

## 项目文件位置

- 核心池: [InformationPool.cs](../../../Assets/Scripts/InformationPool/InformationPool.cs)
- 事件参数: [InfoPoolEventArgs.cs](../../../Assets/Scripts/InformationPool/InfoPoolEventArgs.cs)
- 事件类型: [EventTypes_Template.cs](../../../Assets/Scripts/Events/EventTypes_Template.cs)（`INFO_POOL_EVENT_ON_CHANGE`）

## 核心 API

### 数据存取

```csharp
// 存入/更新（自动发布 INFO_POOL_EVENT_ON_CHANGE 事件）
InformationPool.Set(string key, object value);

// 按类型读取，不存在返回默认值
T value = InformationPool.Get<T>(string key, T defaultValue = default);

// 安全获取，返回是否成功
bool success = InformationPool.TryGet<T>(string key, out T value);

// 检查键是否存在
bool exists = InformationPool.Has(string key);

// 移除（发布事件）
bool removed = InformationPool.Remove(string key);

// 清空全部数据（发布事件）
InformationPool.Clear();
```

### 监听变更

```csharp
// 订阅（OnEnable 中调用）
GameEvents.Listen(EventType.INFO_POOL_EVENT_ON_CHANGE, HandlerMethod);

// 取消订阅（OnDisable 中调用，必须配对）
GameEvents.Unlisten(EventType.INFO_POOL_EVENT_ON_CHANGE, HandlerMethod);
```

### 事件参数（InfoPoolEventArgs）

| 属性 | 类型 | 说明 |
|------|------|------|
| `Key` | `string` | 变更的键名（Clear 时为 null） |
| `OldValue` | `object` | 变更前的值（新增时为 null） |
| `NewValue` | `object` | 变更后的值（删除时为 null） |
| `Type` | `ChangeType` | 变更类型：`Set` / `Remove` / `Clear` |

## 工作流程

### 步骤 1：分析需求

确认功能是否适合使用信息池。典型场景：
- **全局状态共享**（玩家HP、分数、关卡进度）→ 使用 `Set`/`Get`
- **UI 响应数据变化**（血条、分数显示）→ 监听 `INFO_POOL_EVENT_ON_CHANGE`
- **模块间数据传递**（避免直接引用）→ 一方 `Set`，另一方监听或 `Get`
- **配置/设置共享**（难度设置、音量等）→ 使用 `Set` 存储，`Get` 读取

如果数据**仅在单个模块内使用**，不需要信息池，直接用成员变量即可。

### 步骤 2：确定数据键名

定义清晰、唯一的键名，避免冲突。建议命名规范：

| 类别 | 示例键名 |
|------|----------|
| 玩家属性 | `"PlayerHP"`, `"PlayerMaxHP"`, `"PlayerScore"` |
| 关卡信息 | `"CurrentLevel"`, `"LevelProgress"` |
| 游戏设置 | `"MusicVolume"`, `"Difficulty"` |
| 系统状态 | `"IsPaused"`, `"GamePhase"` |

> 提示：可将常用键名定义为常量类，避免拼写错误。

### 步骤 3：在 MonoBehaviour 中搭建订阅框架

#### 仅读取数据（不监听变更）

```csharp
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    private void Start()
    {
        int score = InformationPool.Get("PlayerScore", 0);
        UpdateDisplay(score);
    }

    private void UpdateDisplay(int score) { /* ... */ }
}
```

#### 监听特定键的变更

```csharp
using UnityEngine;

public class PlayerHPBar : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.Listen(EventType.INFO_POOL_EVENT_ON_CHANGE, OnInfoPoolChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.INFO_POOL_EVENT_ON_CHANGE, OnInfoPoolChanged);
    }

    private void OnInfoPoolChanged(IGameEvent evt)
    {
        var args = evt as InfoPoolEventArgs;
        if (args == null) return;

        // 只关心 HP 相关的变更
        if (args.Key == "PlayerHP" || args.Key == "PlayerMaxHP")
        {
            UpdateHPBar();
        }
    }

    private void UpdateHPBar()
    {
        int hp = InformationPool.Get("PlayerHP", 0);
        int maxHp = InformationPool.Get("PlayerMaxHP", 100);
        // 更新血条 UI
    }
}
```

#### 写入数据并通知其他模块

```csharp
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHp = 100;
    private int currentHp;

    private void Start()
    {
        currentHp = maxHp;
        InformationPool.Set("PlayerMaxHP", maxHp);
        InformationPool.Set("PlayerHP", currentHp);
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        InformationPool.Set("PlayerHP", currentHp);  // 自动通知所有监听者
    }
}
```

### 步骤 4：验证

完成后检查以下要点：

| 检查项 | 说明 |
|--------|------|
| 订阅/取消配对 | `OnEnable` 中每个 `Listen(INFO_POOL_EVENT_ON_CHANGE)` 在 `OnDisable` 中有对应的 `Unlisten` |
| 键名一致 | 写入和读取使用的键名字符串完全一致 |
| 类型匹配 | `Set` 存入的类型与 `Get<T>` 读取的泛型类型一致 |
| 空键防护 | `Set`/`Get` 已内置 `string.IsNullOrEmpty` 检查，但仍建议业务层确保键名非空 |
| 不过度监听 | 只在回调中处理自己关心的键（通过 `args.Key == "xxx"` 过滤） |

### 步骤 5：调试验证

```csharp
// 查看当前池中所有键
string[] keys = InformationPool.GetAllKeys();
foreach (var k in keys) Debug.Log($"池中键: {k}");

// 查看条目数
Debug.Log($"信息池条目数: {InformationPool.Count}");

// 检查监听器数量
int listeners = GameEvents.GetListenerCount(EventType.INFO_POOL_EVENT_ON_CHANGE);
Debug.Log($"信息池监听器数量: {listeners}");
```
