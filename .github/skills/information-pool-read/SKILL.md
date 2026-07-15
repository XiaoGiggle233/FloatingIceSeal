---
name: information-pool-read
description: '从 InformationPool 中读取共享数据、监听特定键的变更。Use when: 需要获取其他模块存入的信息池数据、响应数据变更刷新 UI/状态、通过 Get/TryGet/Has 读取、使用 INFO_POOL_EVENT_ON_CHANGE 监听特定键。触发词：读取信息池、获取信息池、信息池监听、信息池回调、Get、TryGet、Has、订阅信息池、InfoPoolEventArgs。'
argument-hint: '[要读取的键名或功能描述]'
---

# Unity InformationPool 读取与监听

从 `InformationPool` 获取共享数据，并响应特定键的变更事件。

## 项目文件位置

- 核心池: [InformationPool.cs](../../../Assets/Scripts/InformationPool/InformationPool.cs)
- 事件参数: [InfoPoolEventArgs.cs](../../../Assets/Scripts/InformationPool/InfoPoolEventArgs.cs)

## 读取 API

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `Get<T>(key, defaultValue)` | `T` | 读取指定键的值，不存在时返回默认值 |
| `TryGet<T>(key, out value)` | `bool` | 安全读取，通过返回值判断是否成功 |
| `Has(key)` | `bool` | 检查键是否存在 |

## 工作流程

### 步骤 1：选择读取时机

根据需求选择合适的读取策略：

| 场景 | 策略 | 示例 |
|------|------|------|
| 初始化时一次性读取 | `Start`/`Awake` 中 `Get` | 读取关卡配置 |
| 需要响应变化 | `OnEnable` 监听 `INFO_POOL_EVENT_ON_CHANGE` | 血条实时更新 |
| 按需查询（如点击时） | 在回调方法中 `Get`/`TryGet` | 查询当前难度 |
| 周期性轮询 | `Update` 中 `Get`（不推荐，优先用事件监听） | — |

### 步骤 2：实现读取

#### 基础读取 — Get

```csharp
private void Start()
{
    // 带默认值读取
    int hp = InformationPool.Get("PlayerHP", 100);
    string level = InformationPool.Get("CurrentLevel", "Level_01");
    float volume = InformationPool.Get("MusicVolume", 0.8f);
}
```

#### 安全读取 — TryGet

适用于不确定键是否存在的场景：

```csharp
public void RefreshFromPool()
{
    if (InformationPool.TryGet("PlayerHP", out int hp))
    {
        UpdateHPDisplay(hp);
    }
    else
    {
        Debug.LogWarning("PlayerHP 尚未存入信息池");
    }
}
```

#### 存在性检查 — Has

```csharp
private void Start()
{
    if (InformationPool.Has("GameConfig"))
    {
        var config = InformationPool.Get("GameConfig", defaultConfig);
        ApplyConfig(config);
    }
    else
    {
        UseDefaultConfig();
    }
}
```

### 步骤 3：监听特定键变更（推荐）

通过 `INFO_POOL_EVENT_ON_CHANGE` 订阅，按 `Key` 过滤：

```csharp
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.Listen(EventType.INFO_POOL_EVENT_ON_CHANGE, OnPoolChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.INFO_POOL_EVENT_ON_CHANGE, OnPoolChanged);
    }

    private void OnPoolChanged(IGameEvent evt)
    {
        var args = evt as InfoPoolEventArgs;
        if (args == null) return;

        switch (args.Key)
        {
            case "PlayerHP":
                UpdateHP((int)args.NewValue);
                break;
            case "PlayerMaxHP":
                UpdateMaxHP((int)args.NewValue);
                break;
            case "PlayerScore":
                UpdateScore((int)args.NewValue);
                // 平滑过渡用 OldValue
                int oldScore = (int)args.OldValue;
                StartCoroutine(AnimateScore(oldScore, (int)args.NewValue));
                break;
        }
    }

    private void UpdateHP(int hp) { /* ... */ }
    private void UpdateMaxHP(int maxHp) { /* ... */ }
    private void UpdateScore(int score) { /* ... */ }
    private IEnumerator AnimateScore(int from, int to) { /* ... */ yield break; }
}
```

> 也可以直接重新从池中读取，避免类型转换：
> ```csharp
> case "PlayerHP":
>     UpdateHP(InformationPool.Get("PlayerHP", 0));
>     break;
> ```

### 步骤 4：处理 ChangeType

`InfoPoolEventArgs.Type` 区分三种变更类型：

```csharp
private void OnPoolChanged(IGameEvent evt)
{
    var args = evt as InfoPoolEventArgs;
    if (args == null || args.Key != "PlayerHP") return;

    switch (args.Type)
    {
        case InfoPoolEventArgs.ChangeType.Set:
            // 新增或更新
            OnHPChanged((int)args.NewValue);
            break;
        case InfoPoolEventArgs.ChangeType.Remove:
            // 键被移除，可能需要隐藏相关 UI
            OnHPRemoved();
            break;
        case InfoPoolEventArgs.ChangeType.Clear:
            // 全部清空，重置所有状态
            OnPoolCleared();
            break;
    }
}
```

### 步骤 5：验证

| 检查项 | 说明 |
|--------|------|
| 监听配对 | `OnEnable` 的 `Listen` 在 `OnDisable` 有对应 `Unlisten` |
| 键名过滤 | 回调中通过 `args.Key` 过滤，避免无关变更的无效处理 |
| 空值处理 | `args.NewValue`/`OldValue` 在 Remove/Clear 时可能为 null |
| 类型安全 | 使用 `Get<T>` 或在回调中做 `as` 转换前检查 |
| 性能 | 避免在 `Update` 中频繁 `Get`，优先使用事件驱动 |
