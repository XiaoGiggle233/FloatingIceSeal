---
name: level-mechanisms
description: '关卡机制（TileMap 类场景物件）的实现方法与扩展接入指南——墙、浮冰、刺、水草、铁丝网、水体等。Use when: 开发/修改关卡机制物件、新增一种机制、理解现有机制的脚本划分与存放位置、为机制接入碰撞/触发器逻辑、添加机制专用 Layer。触发词：关卡机制、墙、浮冰、刺、水草、铁丝网、TileMap 机制、WallController、SpikeController、FloatingIceController、SeagrassController、SteelWireMeshController、WatterController、CompositeCollider2D、TilemapCollider2D、机制脚本、扩展机制。'
argument-hint: '[机制名或需求描述]'
---

# 关卡机制（TileMap 类）实现方法与扩展指南

本 skill 总结项目中所有**基于 TileMap 的关卡机制**（墙、浮冰、刺、水草、铁丝网、水体）的统一实现模式，以及如何新增一种机制（Hooks 扩展点）。

## 通用架构模式

所有机制遵循同一套模式：

| 层面 | 位置 | 说明 |
|------|------|------|
| 控制器脚本 | `Assets/Scripts/SenceObjects/TileMap/<机制名>/` | 每个机制一个文件夹，内含 Controller 脚本 |
| Prefab | `Assets/Prefabs/TileMap/<机制名>.prefab` | 挂载 TileMap 组件族 |
| 组件组成 | Tilemap + TilemapRenderer + TilemapCollider2D + Rigidbody2D + CompositeCollider2D | 机制物件标准组件组合 |
| 注册方式 | Controller 在 `OnEnable`/`OnDisable` 注册/移除 InformationPool | 供其他系统查询引用 |
| 交互方式 | 角色侧通过 `GameEvents` 碰撞事件 + `InformationPool` 查询机制引用 | 机制脚本本身不做交互逻辑 |

> 机制脚本**只负责注册**（标记自身存在），实际交互逻辑全部写在角色控制器/监听脚本中，通过 InformationPool 取引用、通过 GameEvents 收事件。

## 可摧毁接口 IDestroyable

[IDestroyable.cs](../../../Assets/Scripts/Interfaces/IDestroyable.cs)（位于 `Assets/Scripts/Interfaces/`）是"可被爆炸摧毁"的契约接口：

```csharp
public interface IDestroyable
{
    /// <summary>被爆炸摧毁（按爆炸中心与半径破坏自身）</summary>
    void DestroyByExplosion(Vector2 explosionCenter, float explosionRadius);
}
```

- 已实现该接口的机制：刺（`SpikeController`）、水草（`SeagrassController`）、铁丝网（`SteelWireMeshController`）、木箱（`WoodBoxController`）、浮冰（`FloatingIceController`）
- 这些均为 Tilemap 机制，`DestroyByExplosion` 统一调用 [TilemapDestroyUtils.cs](../../../Assets/Scripts/SenceObjects/TileMap/TilemapDestroyUtils.cs) 的 `DestroyTilesInRadius(tilemap, center, radius)`——**只移除爆炸中心半径范围内的瓦片，保留范围外瓦片，不销毁整个 GameObject**（CompositeCollider2D 会随瓦片移除自动重建）
- 爆炸系统（如 `MineExplosionController`）对爆炸范围内命中物体检测 `IDestroyable` 并调用该方法；没有实现该接口的物体（如墙 `WallController`）不会被爆炸破坏

## 关卡重置系统 ILevelResetable / LevelResetSystem

角色死亡或重置关卡时，需把关卡恢复到初始状态（瓦片、动态对象位置、被销毁的对象重建）。由 [ILevelResetable.cs](../../../Assets/Scripts/Interfaces/ILevelResetable.cs) + [LevelResetSystem.cs](../../../Assets/Scripts/GameProcess/LevelResetSystem/LevelResetSystem.cs) 实现：

```csharp
public interface ILevelResetable
{
    /// <summary>关卡状态恢复完成后回调（用于重算内部缓存等）</summary>
    void OnLevelRestore();
}
```

- **`LevelResetSystem`**（挂在场景 `Managers` 下）：
  - `CaptureState()`：记录关卡初始状态——场景所有 Tilemap 的瓦片快照 + 所有 `ILevelResetable` 对象的位置/旋转（按名称匹配 `resetablePrefabs` 数组记录重建用的 prefab）
  - **自动记录**：监听 `PLAYER_EVENT_ON_SPAWN`（角色出生）→ 延迟一帧执行 `CaptureState()`（仅首次，重生不重记录；`CaptureState` 仍是公开方法可手动调用）
  - 监听 `UI_EVENT_ON_RESET_LEVEL` + `PLAYER_EVENT_ON_DEATH` → `RestoreState()`：**先重建/恢复对象，再铺瓦片**。瓦片快照**按 Tilemap 名称匹配**恢复（支持对象销毁重建后引用不失效）；动态对象快照**总是从 prefab 重建**（先按名称清理所有同名旧对象含 inactive，再 `Instantiate` 到记录位置并恢复父级），无 prefab 时退回恢复位置；最后调用所有 `ILevelResetable.OnLevelRestore()`
  - **重建还原 Inspector 覆盖参数**：`ObjectSnapshot` 捕获时记录 `localScale` + `Collider2D` 参数（isTrigger/offset/Circle 的 radius）+ 水雷爆炸参数（`MineExplosionController.CaptureSettings()`）；`RebuildFromPrefab()` 重建后依次写回，避免场景中调过的水雷参数（检测范围/爆炸范围/爆炸延迟/出生保护时间）变回 prefab 初始值
  - ⚠️ 必须"总是重建"的原因：水雷 `Explode()` 同步发布死亡事件，此时 `Destroy` 尚在排队，若只"存在则恢复位置"，水雷随后销毁却不会重建；且多次死亡后旧对象残留同名，需按名称全量清理
  - `resetablePrefabs`：Inspector 配置可重建对象 prefab（水雷 `Assets/Prefabs/Mine.prefab`、木箱 `Assets/Prefabs/TileMap/WoodBox.prefab`、浮冰 `Assets/Prefabs/FlowingIce/FloatingIce.prefab`）
- **已实现 `ILevelResetable`**：`MineController`、`WoodBoxController`（`OnLevelRestore` 重算浮力偏移）、`FloatingIceController`
- 使用注意：初始状态在角色首次出生（`PLAYER_EVENT_ON_SPAWN`）后延迟一帧自动记录；`CaptureState()` 也可手动调用

## 各机制实现明细

### 墙（Wall）—— 物理阻挡 + 陆地判定

- 脚本：`SenceObjects/TileMap/Wall/WallController.cs`，仅 `InformationPool.Set("Wall", this)`
- Prefab 组件：`TilemapCollider2D`(UsedByComposite=1) + `CompositeCollider2D`(GeometryType=**Polygons**=1) + Rigidbody2D
- 交互：`SealEnvironmentStateMachineController` 通过 `GetComponent<WallController>()` 识别碰撞对象是否为墙，统计**垂直接触数**（`IsVerticalContact` 判定法线 |y|>|x|）决定 `EnvironmentState.OnLand`
- 关键点：墙同时承担「地面」职责，角色的陆地/空中状态切换依赖它

### 浮冰（FloatingIce）—— 可移动平台 + 自动回归

- 脚本（两个）：
  - `FloatingIceController.cs`：注册到 `"FloatingIceList"`（列表）+ 兼容单引用 `"FloatingIce"`（列表模式写法见「扩展模式」）
  - `FloatingIceMovingController.cs`：`[RequireComponent(typeof(Rigidbody2D))]`，在 `FixedUpdate` 中让 TileMap 从偏移位置以不同速度回归初始位置
- Prefab：注意存放于 `Assets/Prefabs/FlowingIce/FlowtingIce.prefab`（拼写为 FlowtingIce），组件含 TilemapCollider2D(UsedByComposite=1) + CompositeCollider2D(GeometryType=Polygons)
- 参数：`waterReturnSpeed`（水中回归速度）/ `airReturnSpeed`（空中回归速度）/ `waterCheckDistance` / `snapDistance`（归位阈值）
- 关键点：用 `WatterUtils.HasWaterBelow()` 判断自身是否在水中，从而选择回归速度

### 木箱（WoodBox）—— 浮力上浮 + 固定水面

- 脚本（两个）：`SenceObjects/TileMap/WoodBox/WoodBoxController.cs`（注册 `"WoodBoxList"` 列表 + 兼容单引用 `"WoodBox"`）+ `WoodBoxBuoyancyController.cs`（浮力行为）
- Prefab：`Assets/Prefabs/TileMap/WoodBox.prefab`，组件含 TilemapCollider2D(UsedByComposite=1) + CompositeCollider2D(GeometryType=Polygons) + Rigidbody2D(**Dynamic**，受重力，靠浮力对抗)
- 参数（`WoodBoxBuoyancyController`，Inspector 可调）：`buoyancyForce`（浮力大小）/ `mass`（物体质量，写入 Rigidbody2D）/ `linearDrag`（移动阻力，写入 Rigidbody2D.drag）/ `snapDistance`（归位阈值）
- 逻辑：`Start` 时基于非空 tile 遍历缓存中心 X 与底部 Y 相对 transform 的偏移（`RecacheTileGeometry`，内含 tilemap 懒获取）；`FixedUpdate` 中用 tile 中心 X 调 `WatterUtils.GetWaterSurfaceY()` 取水面高度，目标 Y = 水面 − tile 底部偏移（**tile 底部贴水面**）；到达目标（阈值内）则固定（`MovePosition` + 速度清零）；在水下则 `AddForce(Vector2.up * buoyancyForce)` 上浮
- 关键点：目标基于 **tile 位置**而非 Tilemap 物体 transform 计算；固定位置是**水面**而非世界坐标，水面变化时木箱跟随；被压入水下会自动浮回；**爆炸破坏瓦片后** `WoodBoxController.DestroyByExplosion` 会调用 `RecacheTileGeometry()` 重新计算偏移（避免剩余瓦片悬空），瓦片全部被炸毁则销毁木箱

### 水雷（Mine）—— 非 TileMap 的 2D 物体机制

> 水雷是**不使用 Tilemap** 的机制示例：普通 GameObject + SpriteRenderer + CircleCollider2D + Rigidbody2D。

- 脚本（三个）：`SenceObjects/Mine/MineController.cs`（注册 `"MineList"` 列表 + 兼容单引用 `"Mine"`）+ `MineMovingController.cs`（移动逻辑）+ `MineExplosionController.cs`（爆炸机制）
- Prefab：`Assets/Prefabs/Mine.prefab`，组件：SpriteRenderer(水雷精灵) + CircleCollider2D(**IsTrigger=0**，非 Trigger，radius 0.12) + Rigidbody2D(Dynamic, gravityScale=0, freezeRotation) + 三个脚本
- 参数（`MineMovingController`）：`returnSpeed`（回归速度）/ `bubblePushForce`（气泡推力）/ `snapDistance`（归位阈值）
- 参数（`MineExplosionController`）：`detectRadius`（检测范围）/ `explosionRadius`（爆炸范围）/ `explosionDelay`（爆炸延迟）/ `spawnGracePeriod`（出生保护时间）；`CaptureSettings()`/`RestoreSettings()` 供 LevelResetSystem 重建后还原 Inspector 覆盖参数（`RestoreSettings` 同时重置出生保护计时）
- 移动逻辑：与浮冰相同的**回归**——`Start` 记录初始位置，`FixedUpdate` 中 `MovePosition` 向初始位置移动（`returnSpeed` 体现"回归倾向"）；**被气泡推动**——`MineMovingController` 中 `OnCollisionEnter2D`/`OnCollisionStay2D` 检测 `GetComponent<BubbleBase>()`，沿接触法线方向 `AddForce(Impulse)` 推开（水雷 collider 为非 Trigger，物理碰撞触发回调）
- 爆炸逻辑（`MineExplosionController`）：
  - 触发：`FixedUpdate` 中 `OverlapCircleAll(detectRadius)` 检测范围内有 `Seal`（未来小鱼同样判断）→ `Explode()`；其它水雷 `Explode()` 时连锁引爆检测范围内水雷
  - 效果：`Explode()` 中对 `OverlapCircleAll(explosionRadius)` 命中对象——有 `Seal` 且**未受气泡保护**（`seal.ProtectionStateMachine.IsProtected()` 为假）则发布 `PLAYER_EVENT_ON_DEATH` 使其死亡；有 `SmallFishController` 且**未受气泡保护**（`fish.IsProtected()` 为假）则 `Destroy` 小鱼；有 `BubbleBase` 则调用 `Burst()` 炸掉气泡（`BigBubble.Burst()` 对未释放气泡有内部保护）；命中 `IDestroyable` 调用 `DestroyByExplosion`；最后销毁自身（`hasExploded` 防止重复/连锁死循环）
  - 角色死亡复用现有事件链：`SealSpawnAndDeathManager` 监听 `PLAYER_EVENT_ON_DEATH` 重生角色
- 关键点：水雷 collider 为非 Trigger（IsTrigger=0），与角色/气泡之间是**物理碰撞**（海豹可推动水雷）；气泡碰到水雷不会破裂（`BubbleControllerBase` 只对 Seal/Spike Burst）

### 小鱼（SmallFish）—— 非 TileMap 2D 物体 + 追踪玩家大气泡

> 小鱼是**不使用 Tilemap** 的 2D 物体：SpriteRenderer + CircleCollider2D(非 Trigger) + Rigidbody2D(Dynamic, gravityScale=0)。

- 脚本（三个）：`SenceObjects/SmallFish/SmallFishController.cs`（注册 `"SmallFishList"` + `"SmallFish"`、移动/追踪/吸泡核心逻辑、暴露 `IsProtected()`）+ `SmallFishSpriteController.cs`（按移动方向 `flipX`，模仿海豹 `SealSpriteController`）+ `SmallFishProtectionController.cs`（气泡保护检测，仿照海豹 `SealProtectionStateMachineController`）
- Prefab：`Assets/Prefabs/SmallFish.prefab`（SpriteRenderer 素材由用户提供）
- 参数（`SmallFishController`，全部 Inspector 可调）：`detectRadius`（气泡检测范围）/ `moveSpeed`（正常速度）/ `chaseSpeed`（追踪速度）/ `obstacleCheckDistance` / `initialDirection` / `waitDuration`（发现气泡后等待，默认 0.5s）/ `contactRadius`（接触判定半径）/ `absorbDuration`（吸泡计时）
- 参数（`SmallFishProtectionController`）：`protectedThreshold`（覆盖判定阈值，默认 0.5）/ `sampleGridSize`（采样网格，默认 8）
- 逻辑：
  - **漫游**：沿当前方向移动，`RaycastAll` 检测前方非 Trigger 障碍 → 反向（海豹、水草不算障碍）
  - **检测**：`OverlapCircleAll(detectRadius)` 找 `BigBubble` 且 `State == AfterRelease`（玩家释放）→ 停止 → 等待 `waitDuration` → 追踪；目标需射线可达（海豹、水草、铁丝网不算障碍）
  - **追踪**：向目标气泡移动（`chaseSpeed`）；距离 ≤ `contactRadius` 开始吸泡计时，计时结束 `targetBubble.Burst()`，小鱼恢复漫游
  - **气泡保护**：`Update` 中采样小鱼碰撞体，统计被 Bubble 层碰撞体覆盖的采样点比例 ≥ `protectedThreshold` 则 `IsProtected=true`；水雷爆炸时受保护的小鱼不会被炸死（与海豹保护判定一致）
- 物理规则：小鱼使用 **Fish 层（index 11，TagManager 新增）**，与 **Bubble 层禁用碰撞**（Physics2DSettings 碰撞矩阵清零，与海豹-气泡同方案），与 **Player 层保持碰撞**（小鱼与海豹有物理碰撞）；气泡检测用 `OverlapCircleAll`（不受层碰撞矩阵影响）

### 刺（Spike）—— 冲刺碰撞伤害

- 脚本：`SenceObjects/TileMap/Spike/SpikeController.cs`，仅 `InformationPool.Set("Spike", this)`
- Prefab 组件：`TilemapCollider2D`(UsedByComposite=1) + `CompositeCollider2D`(GeometryType=**Outlines**=0) ← 与其他机制的 Polygons 不同
- 交互（角色侧）：
  - `SealMoveController.OnCollisionEvent`（监听 `COLLISION_EVENT_ON_ENTER`）：冲刺中碰到刺 → 停止冲刺 `EndDash()` + 击退反弹 `ApplyHurtBounce(normal)` + 进入无敌 `EnterInvincibility()`；通过 `IsSpike()` 用信息池比对目标对象
  - `BubbleControllerBase`：气泡碰到刺会破裂
- 关键点：刺的伤害只在**冲刺状态**下触发；普通行走碰到刺暂无伤害逻辑

### 水草（Seagrass）—— 物理阻挡（预留扩展）

- 脚本：`SenceObjects/TileMap/Seagrass/SeagrassController.cs`，仅 `InformationPool.Set("Seagrass", this)`
- Prefab 组件：`TilemapCollider2D`(UsedByComposite=**0**) + `CompositeCollider2D`(GeometryType=Polygons=1)
- 图层：`Seagrass`（自定义 Layer，index 9）
- 现状：仅物理碰撞阻挡 + 信息池注册，**尚无特殊交互逻辑**，为后续功能预留接入点

### 铁丝网（SteelWireMesh）—— 物理阻挡（预留扩展）

- 脚本：`SenceObjects/TileMap/SteelWireMesh/SteelWireMeshController.cs`，仅 `InformationPool.Set("SteelWireMesh", this)`
- Prefab 组件：同水草（TilemapCollider2D UsedByComposite=0 + CompositeCollider2D Polygons）
- 图层：`SteelWireMesh`（自定义 Layer，index 8）
- 现状：仅物理碰撞阻挡 + 信息池注册，尚无特殊交互逻辑

### 水体（Watter）—— 环境判定 + 流动推力

- 脚本（三个）：`WatterController.cs`（注册 `"WatterList"` 列表 + 兼容单引用）+ `FlowingWatter.cs`（Odin 序列化，`FlowDirection` + `forceAmount`，`OnTriggerStay2D` 给刚体加力）+ `WatterUtils.cs`（静态工具类）
- Prefab 组件：`TilemapCollider2D`(IsTrigger=1, UsedByComposite=1) + `CompositeCollider2D`(IsTrigger=1, GeometryType=Polygons=1)
- 图层：`Watter`（自定义 Layer，index 10；**注意不是内置的 Water 层**）
- 交互：
  - 环境状态机：`CheckIsInWater()` 遍历 `WatterList`，用 `Collider2D.Distance` 判定与 `CompositeCollider2D` 是否重叠
  - 氧气控制器：取重叠水体后用 `WatterUtils.GetWaterSurfaceY()` 计算露出比例，决定氧气回复/消耗
  - 通用水体检测：统一用 `WatterUtils`（`HasWaterBelow`/`GetWaterSurfaceY`/`RaycastToWater`），详见 `water-system` skill
- ⚠️ 关键坑：判定与复合碰撞体的重叠统一用 `Collider2D.Distance`（`isOverlapped`），不要用 `OverlapPoint`——后者依赖 GeometryType 且对 Outlines 边缘碰撞体不可靠

## Hooks：新增一种机制的接入点

按以下步骤为项目添加新机制：

1. **创建脚本目录**：`Assets/Scripts/SenceObjects/TileMap/<机制名>/<机制名>Controller.cs`
2. **编写注册脚本**（最简形式，参考 WallController）：
   ```csharp
   public class NewMechanismController : MonoBehaviour
   {
       private void OnEnable()  => InformationPool.Set("NewMechanism", this);
       private void OnDisable() => InformationPool.Remove("NewMechanism");
   }
   ```
   - 单实例机制：直接 `Set`/`Remove` 字符串键
   - 多实例机制：参考 `FloatingIceController`/`WatterController` 的**列表模式**（`TryGet` 列表 → 追加自身；`OnDisable` 移除自身，空列表则删键；单引用字段用 `ReferenceEquals` 保护避免误删）
3. **创建 Prefab**：`Assets/Prefabs/TileMap/<机制名>.prefab`，挂载标准组件族：
   - `Tilemap` + `TilemapRenderer`（绘制关卡地形）
   - `TilemapCollider2D`（碰撞体）
   - `Rigidbody2D` + `CompositeCollider2D`（合并碰撞体，静态机制保持 `bodyType=Static`，动态机制如浮冰设 Dynamic/Kinematic）
   - 触发类机制：`IsTrigger=1`（如水体、触发器）；阻挡类：`IsTrigger=0`
   - 项目惯例：普通地形机制用 `GeometryType=Polygons`（墙/水草/铁丝网/水体/浮冰均如此），刺用的是 `Outlines`
4. **需要专属图层**：在 `ProjectSettings/TagManager.asset` 的 `layers` 中添加，然后用 `LayerMask.GetMask("<Layer>")` 引用
5. **接入交互逻辑**（任选其一或组合）：
   - 监听碰撞事件：`GameEvents.Listen(EventType.COLLISION_EVENT_ON_ENTER, ...)` 中比对 `CollisionEventArgs.Source/Target` 与信息池中的机制引用（参考 `SwitchLevels`、`SealMoveController`、`FlowingWatter`）
   - 使用 Trigger 回调：`OnTriggerEnter2D`/`OnTriggerStay2D` 直接处理（参考 `FlowingWatter.OnTriggerStay2D` 加力、`CameraLockTriggerController` 锁镜头）
   - 被角色状态机感知：在对应角色控制器中 `GetComponent<XxxController>()` 或信息池查询（参考环境状态机对 `WallController`、移动控制器对 `"Spike"` 的用法）
6. **若机制影响角色状态**（氧气/环境/生命）：在 `SealOxygenController`/`SealEnvironmentStateMachineController`/`SealMoveController` 的对应判断分支中加入对机制的识别

## 参考实现

- 注册脚本：`WallController`、`SpikeController`、`SeagrassController`、`SteelWireMeshController`
- 多实例列表注册：`FloatingIceController`、`WatterController`、`WoodBoxController`
- 动态机制：`FloatingIceMovingController`、`WoodBoxBuoyancyController`（浮力）
- 触发交互：`FlowingWatter`、`CameraLockTriggerController`、`CameraOverviewTriggerController`、`SwitchLevels`
- 水体检测工具：`WatterUtils`（详见 `water-system` skill）
