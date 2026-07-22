---
name: water-system
description: '水体检测工具集 WatterUtils 的使用指南——向下射线检测水面、判断是否在水下、获取水面高度。Use when: 需要检测GameObject与水面的距离、判断是否离开水面、获取水面Y坐标、替换手写的 Physics2D.Raycast 水体检测代码。触发词：水面检测、WatterUtils、HasWaterBelow、GetWaterSurfaceY、RaycastToWater、水体射线、离开水面、距离水面、水面高度。'
---

# 水体检测工具集 WatterUtils

[WatterUtils.cs](../../../Assets/Scripts/SenceObjects/Watter/WatterUtils.cs) 是一个静态工具类，封装了项目中所有向下射线检测水面的通用逻辑。所有需要判断"是否在水下"或"距离水面多远"的脚本都应使用此工具集，而非手写 `Physics2D.Raycast`。

## 核心 API

| 方法 | 返回值 | 用途 |
|------|--------|------|
| `HasWaterBelow(Vector2 origin, float maxDistance)` | `bool` | 判断指定位置下方是否有水面 |
| `GetWaterSurfaceY(float x)` | `float` | 获取指定 X 坐标处的水面 Y 值，无水面返回 `float.MinValue` |
| `RaycastToWater(Vector2 origin, float maxDistance)` | `RaycastHit2D` | 向下发射射线，返回完整命中信息 |
| `WaterLayerMask` | `LayerMask` | 缓存的水体 LayerMask，等同于 `LayerMask.GetMask("Watter")` |

## 使用场景

### 场景 1：检测是否离开水面（替代手写 Raycast）

**旧代码（BubbleControllerBase 重构前）：**
```csharp
// 需要自己维护 waterLayerMask 字段
protected int waterLayerMask;

void Start() {
    waterLayerMask = 1 << LayerMask.NameToLayer("Watter");
}

void Update() {
    RaycastHit2D hit = Physics2D.Raycast(
        transform.position, Vector2.down,
        bubbleBase.outOfWaterBurstDistance, waterLayerMask);
    if (hit.collider == null)
        bubbleBase.Burst();
}
```

**新代码（使用 WatterUtils）：**
```csharp
void Update() {
    if (!WatterUtils.HasWaterBelow(transform.position, bubbleBase.outOfWaterBurstDistance))
        bubbleBase.Burst();
}
```

### 场景 2：获取水面高度

**旧代码（SealOxygenController 重构前）：**
```csharp
[SerializeField] private LayerMask waterLayerMask;

void Awake() {
    waterLayerMask = LayerMask.GetMask("Watter");
}

float GetWaterSurfaceY(float x, CompositeCollider2D waterCollider) {
    Vector2 origin = new Vector2(x, waterCollider.bounds.max.y + 10f);
    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down,
        Mathf.Infinity, waterLayerMask);
    return hit.collider != null ? hit.point.y : float.MinValue;
}
```

**新代码（使用 WatterUtils）：**
```csharp
float waterSurfaceY = WatterUtils.GetWaterSurfaceY(x);
```

## 迁移步骤

1. **添加调用** — 将手写的 `Physics2D.Raycast(..., waterLayerMask)` 替换为对应的 `WatterUtils` 方法
2. **删除字段** — 移除脚本中不再需要的 `waterLayerMask` / `LayerMask` 字段及其初始化代码
3. **删除私有方法** — 如果整个私有方法已被工具集覆盖，直接删除该方法

## 参考实现

- 泡泡破裂检测: [BubbleControllerBase.cs](../../../Assets/Scripts/Bubble/BubbleControllerBase.cs) — `HasWaterBelow` 示例
- 角色氧气恢复: [SealOxygenController.cs](../../../Assets/Scripts/Character/SealOxygenController.cs) — `GetWaterSurfaceY` 示例
