---
name: event-subscribe-publish
description: '在 MonoBehaviour 中快速添加 GameEvents 的订阅(Listen)、取消订阅(Unlisten)、发布(Publish)代码。Use when: 为脚本添加事件监听、搭建 OnEnable/OnDisable 订阅框架、发布事件通知其他模块、BubbleController 等业务脚本接入事件系统。触发词：订阅事件、取消订阅、发布事件、Listen、Unlisten、Publish、OnEnable、OnDisable、事件回调、事件处理器。'
argument-hint: '[脚本名或操作描述]'
---

# GameEvents 订阅/发布操作

为 MonoBehaviour 快速添加事件订阅、取消订阅和发布代码。比 `unity-event-setup` 更轻量——不涉及新增事件类型或参数类，只做接入。

## 快速参考

```csharp
// 订阅 —— OnEnable 中
GameEvents.Listen(EventType.XXX, Handler);

// 取消订阅 —— OnDisable 中（必须配对）
GameEvents.Unlisten(EventType.XXX, Handler);

// 发布 —— 任意方法中
GameEvents.Publish(EventType.XXX, new SomeEventArgs(data));
```

## 操作步骤

### 1. 添加订阅（Listen）

在 `OnEnable` 中为每个需要监听的事件添加 `GameEvents.Listen`：

```csharp
private void OnEnable()
{
    GameEvents.Listen(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEnter);
    GameEvents.Listen(EventType.PLAYER_EVENT_ON_DEATH, OnPlayerDeath);
}
```

如果没有 `OnEnable` 方法，创建它。

### 2. 添加取消订阅（Unlisten）

在 `OnDisable` 中为每个 `Listen` 添加对应的 `Unlisten`：

```csharp
private void OnDisable()
{
    GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEnter);
    GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_DEATH, OnPlayerDeath);
}
```

**规则**：OnEnable 中的每个 `Listen` 必须在 OnDisable 中有对应的 `Unlisten`，一一配对。否则对象销毁后触发事件会空引用。

### 3. 实现事件处理器

处理器签名固定为 `void MethodName(IGameEvent evt)`：

```csharp
private void OnCollisionEnter(IGameEvent evt)
{
    var args = evt as CollisionEventArgs;
    if (args == null) return;

    // 用 args 的字段处理逻辑
}
```

**关键模式**：
- 用 `as` 转型获取具体参数类
- 转型失败时 `return` 防御
- 处理器方法始终为 `private`

### 4. 添加事件发布（Publish）

在业务方法中需要通知其他模块时发布事件：

```csharp
public void TakeDamage(float damage)
{
    health -= damage;

    // 发布带数据的事件
    GameEvents.Publish(EventType.PLAYER_EVENT_ON_DAMAGE,
        new PlayerValueEventArgs(this.gameObject, damage));

    if (health <= 0)
    {
        // 发布简单事件（无数据）
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_DEATH,
            new PlayerEventArgs(this.gameObject));
    }
}
```

### 5. 移除发布（OnDisable 清理）

如果脚本在 `OnDisable` 后不应该再发事件（例如对象被回收），确保发布逻辑只在激活状态下执行：

```csharp
public void SomeMethod()
{
    if (!enabled) return;  // 防御：禁用后不发事件

    GameEvents.Publish(EventType.XXX, new GameEventBase());
}
```

或者在 `OnDisable` 中取消订阅的同时，设置标志位阻止后续发布。

## 完整模板

直接按此模板修改现有 MonoBehaviour：

```csharp
using UnityEngine;

public class YourScript : MonoBehaviour
{
    private void OnEnable()
    {
        // === 订阅事件 ===
        GameEvents.Listen(EventType.XXX, OnXxx);
    }

    private void OnDisable()
    {
        // === 取消订阅（与上面一一对应）===
        GameEvents.Unlisten(EventType.XXX, OnXxx);
    }

    // === 事件处理器 ===
    private void OnXxx(IGameEvent evt)
    {
        var args = evt as XxxEventArgs;
        if (args == null) return;
        // 处理逻辑
    }

    // === 业务方法中发布事件 ===
    public void DoAction()
    {
        // ...
        GameEvents.Publish(EventType.YYY, new YyyEventArgs(data));
    }
}
```

## 对照检查

| # | 检查项 |
|---|--------|
| 1 | OnEnable 里每个 `Listen` 在 OnDisable 里都有 `Unlisten` |
| 2 | 事件处理器用 `as` 转型且判 null |
| 3 | 处理器是 `private void (IGameEvent)` 签名 |
| 4 | 使用的 EventType 和 EventArgs 已存在 |
| 5 | 不在 `OnDisable` 之后发布事件 |
