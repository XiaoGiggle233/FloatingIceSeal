using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>小鱼行为参数快照 —— 关卡重置从 prefab 重建后还原场景 Inspector 覆盖值</summary>
public struct SmallFishSettings
{
    public float detectRadius;
    public float moveSpeed;
    public float chaseSpeed;
    public float obstacleCheckDistance;
    public Vector2 initialDirection;
    public float waitDuration;
    public float contactRadius;
    public float stopDistance;
    public float absorbDuration;

    public SmallFishSettings(float detectRadius, float moveSpeed, float chaseSpeed,
        float obstacleCheckDistance, Vector2 initialDirection, float waitDuration,
        float contactRadius, float stopDistance, float absorbDuration)
    {
        this.detectRadius = detectRadius;
        this.moveSpeed = moveSpeed;
        this.chaseSpeed = chaseSpeed;
        this.obstacleCheckDistance = obstacleCheckDistance;
        this.initialDirection = initialDirection;
        this.waitDuration = waitDuration;
        this.contactRadius = contactRadius;
        this.stopDistance = stopDistance;
        this.absorbDuration = absorbDuration;
    }
}

/// <summary>
/// 小鱼控制器 —— 漫游移动（遇障碍转向）、检测玩家释放的大气泡并追踪、
/// 接触气泡后计时破裂（计时结束恢复漫游）
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SmallFishController : MonoBehaviour, ILevelResetable
{
    private static readonly string ListKey = "SmallFishList";

    [FoldoutGroup("气泡检测", expanded: true)]
    [LabelText("气泡检测范围"), MinValue(0), SuffixLabel("m", Overlay = true)]
    [SerializeField] private float detectRadius = 3f;

    [FoldoutGroup("移动", expanded: true)]
    [LabelText("正常速度"), MinValue(0), SuffixLabel("m/s", Overlay = true)]
    [SerializeField] private float moveSpeed = 1f;

    [FoldoutGroup("移动")]
    [LabelText("追踪速度"), MinValue(0), SuffixLabel("m/s", Overlay = true)]
    [SerializeField] private float chaseSpeed = 3f;

    [FoldoutGroup("移动")]
    [LabelText("障碍检测距离"), MinValue(0), SuffixLabel("m", Overlay = true)]
    [SerializeField] private float obstacleCheckDistance = 1f;

    [FoldoutGroup("移动")]
    [LabelText("初始移动方向")]
    [SerializeField] private Vector2 initialDirection = Vector2.right;

    [FoldoutGroup("追踪", expanded: true)]
    [LabelText("发现气泡后等待时间"), MinValue(0), SuffixLabel("秒", Overlay = true)]
    [SerializeField] private float waitDuration = 0.5f;

    [FoldoutGroup("吸泡计时", expanded: true)]
    [LabelText("接触判定半径"), MinValue(0), SuffixLabel("m", Overlay = true)]
    [SerializeField] private float contactRadius = 0.4f;

    [FoldoutGroup("吸泡计时")]
    [LabelText("追击停止距离"), MinValue(0), SuffixLabel("m", Overlay = true)]
    [SerializeField] private float stopDistance = 0.4f;

    [FoldoutGroup("吸泡计时")]
    [LabelText("吸泡计时时长"), MinValue(0), SuffixLabel("秒", Overlay = true)]
    [SerializeField] private float absorbDuration = 1.5f;

    [FoldoutGroup("死亡", expanded: true)]
    [LabelText("死亡动画后销毁延迟"), MinValue(0), SuffixLabel("秒", Overlay = true)]
    [SerializeField] private float deathDestroyDelay = 1f;

    private Rigidbody2D rb;
    private Collider2D fishCollider;
    private SmallFishStateMachine fsm;
    private SmallFishProtectionController protection;
    private Vector2 moveDirection;
    private BigBubble targetBubble;
    private float waitTimer;
    private float absorbTimer;
    private float deathTimer;

    /// <summary>当前移动方向（供 sprite 翻转等使用）</summary>
    public Vector2 MoveDirection => moveDirection;

    /// <summary>当前状态机状态（供 sprite 切换等使用）</summary>
    public SmallFishState CurrentState => fsm.CurrentState;

    /// <summary>是否被气泡保护（气泡覆盖 ≥ 阈值时水雷不会炸死小鱼）</summary>
    public bool IsProtected() => protection != null && protection.IsProtected;

    /// <summary>死亡：进入死亡状态，播放死亡动画后延迟销毁（水雷爆炸等调用）</summary>
    public void Die()
    {
        if (fsm.CurrentState == SmallFishState.Dead) return;

        fsm.SetState(SmallFishState.Dead);
        deathTimer = deathDestroyDelay;
        targetBubble = null;
        rb.velocity = Vector2.zero;
        if (fishCollider != null) fishCollider.enabled = false; // 避免再次触发水雷/碰撞
    }

    /// <summary>捕获当前行为参数（关卡重置重建后由 LevelResetSystem 还原）</summary>
    public SmallFishSettings CaptureSettings() =>
        new SmallFishSettings(detectRadius, moveSpeed, chaseSpeed, obstacleCheckDistance,
            initialDirection, waitDuration, contactRadius, stopDistance, absorbDuration);

    /// <summary>还原行为参数（关卡重置重建后由 LevelResetSystem 调用，同步移动方向）</summary>
    public void RestoreSettings(SmallFishSettings settings)
    {
        detectRadius = settings.detectRadius;
        moveSpeed = settings.moveSpeed;
        chaseSpeed = settings.chaseSpeed;
        obstacleCheckDistance = settings.obstacleCheckDistance;
        initialDirection = settings.initialDirection;
        waitDuration = settings.waitDuration;
        contactRadius = settings.contactRadius;
        stopDistance = settings.stopDistance;
        absorbDuration = settings.absorbDuration;

        moveDirection = initialDirection.normalized;
        if (moveDirection == Vector2.zero) moveDirection = Vector2.right;
    }

    private void OnEnable()
    {
        // 注册到小鱼列表
        if (!InformationPool.TryGet(ListKey, out List<SmallFishController> list) || list == null)
        {
            list = new List<SmallFishController>();
            InformationPool.Set(ListKey, list);
        }
        if (!list.Contains(this))
            list.Add(this);

        InformationPool.Set("SmallFish", this);
    }

    private void OnDisable()
    {
        if (InformationPool.TryGet(ListKey, out List<SmallFishController> list) && list != null)
        {
            list.Remove(this);
            if (list.Count == 0)
                InformationPool.Remove(ListKey);
        }

        if (InformationPool.TryGet("SmallFish", out object obj) && ReferenceEquals(obj, this))
            InformationPool.Remove("SmallFish");
    }

    /// <summary>关卡恢复完成回调（位置由 LevelResetSystem 恢复）</summary>
    public void OnLevelRestore()
    {
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fishCollider = GetComponent<Collider2D>();
        fsm = new SmallFishStateMachine();
        protection = GetComponent<SmallFishProtectionController>();
        moveDirection = initialDirection.normalized;
        if (moveDirection == Vector2.zero) moveDirection = Vector2.right;
    }

    /// <summary>Scene 视图选中时显示气泡检测范围（黄）</summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }

    private void Update()
    {
        switch (fsm.CurrentState)
        {
            case SmallFishState.Patrol: UpdatePatrol(); break;
            case SmallFishState.Wait: UpdateWait(); break;
            case SmallFishState.Chase: UpdateChase(); break;
            case SmallFishState.Break: UpdateBreak(); break;
            case SmallFishState.Dead: UpdateDead(); break;
        }
    }

    private void FixedUpdate()
    {
        switch (fsm.CurrentState)
        {
            case SmallFishState.Patrol: MovePatrol(); break;
            case SmallFishState.Wait: MoveStop(); break;
            case SmallFishState.Chase: MoveChase(); break;
            case SmallFishState.Break: MoveChase(); break; // 破坏时仍跟随气泡（气泡在移动）
            case SmallFishState.Dead: MoveStop(); break;
        }
    }

    #region 状态逻辑

    /// <summary>巡逻：检测范围内出现玩家释放的大气泡 → 等待</summary>
    private void UpdatePatrol()
    {
        var bubble = FindTargetBubble();
        if (bubble != null)
        {
            targetBubble = bubble;
            waitTimer = waitDuration;
            fsm.SetState(SmallFishState.Wait);
        }
    }

    /// <summary>等待：计时结束 → 追击；目标失效 → 巡逻（恢复左右移动）</summary>
    private void UpdateWait()
    {
        if (!IsTargetValid())
        {
            targetBubble = null;
            RestoreHorizontalMovement();
            fsm.SetState(SmallFishState.Patrol);
            return;
        }

        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
            fsm.SetState(SmallFishState.Chase);
    }

    /// <summary>追击：目标失效 → 巡逻（恢复左右移动）；接触气泡 → 破坏</summary>
    private void UpdateChase()
    {
        if (!IsTargetValid())
        {
            targetBubble = null;
            RestoreHorizontalMovement();
            fsm.SetState(SmallFishState.Patrol);
            return;
        }

        if (Vector2.Distance(transform.position, targetBubble.transform.position) <= contactRadius)
        {
            targetBubble.SetBreaking(true); // 通知大气泡进入被破坏状态（上浮速度降低）
            absorbTimer = absorbDuration;
            fsm.SetState(SmallFishState.Break);
        }
    }

    /// <summary>破坏：计时结束气泡破裂 → 巡逻（恢复左右移动）；目标已被销毁则直接恢复</summary>
    private void UpdateBreak()
    {
        // 目标气泡已被销毁（被吸收/破裂等）→ 直接恢复巡逻
        if (targetBubble == null)
        {
            RestoreHorizontalMovement();
            fsm.SetState(SmallFishState.Patrol);
            return;
        }

        absorbTimer -= Time.deltaTime;
        if (absorbTimer <= 0f)
        {
            targetBubble.Burst();
            targetBubble = null;
            RestoreHorizontalMovement();
            fsm.SetState(SmallFishState.Patrol);
        }
    }

    /// <summary>死亡：计时结束销毁（关卡重置时由 LevelResetSystem 重建）</summary>
    private void UpdateDead()
    {
        deathTimer -= Time.deltaTime;
        if (deathTimer <= 0f)
            Destroy(gameObject);
    }

    /// <summary>把移动方向重置为水平（保持当前左右朝向）</summary>
    private void RestoreHorizontalMovement()
    {
        moveDirection = new Vector2(moveDirection.x >= 0f ? 1f : -1f, 0f);
    }

    #endregion

    #region 移动逻辑

    private void MovePatrol()
    {
        // 巡逻一定是左右移动（进入状态时可能残留上下方向）
        if (Mathf.Abs(moveDirection.y) > Mathf.Abs(moveDirection.x))
            RestoreHorizontalMovement();

        if (HasObstacleAhead())
            moveDirection = -moveDirection;

        // 不在水中 → 抑制向上移动（不允许游出水面；不强制向下，避免抽搐）
        Vector2 velocity = moveDirection * moveSpeed;
        if (!IsInWater())
            velocity.y = Mathf.Min(velocity.y, 0f);

        rb.velocity = velocity;
    }

    private void MoveStop()
    {
        rb.velocity = Vector2.zero;
    }

    private void MoveChase()
    {
        if (targetBubble == null) return;

        // 距离过近 → 停止移动，避免抽搐（气泡移动时超出距离再继续追）
        if (Vector2.Distance(transform.position, targetBubble.transform.position) <= stopDistance)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 toBubble = (Vector2)targetBubble.transform.position - rb.position;
        if (toBubble.sqrMagnitude > 0.01f)
        {
            moveDirection = toBubble.normalized;

            // 不在水中 → 抑制向上移动（不允许游出水面）
            Vector2 velocity = moveDirection * chaseSpeed;
            if (!IsInWater())
                velocity.y = Mathf.Min(velocity.y, 0f);

            rb.velocity = velocity;
        }
    }

    #endregion

    #region 辅助

    /// <summary>查找检测范围内玩家释放的大气泡（气泡方向被障碍物阻挡则视为未发现）</summary>
    private BigBubble FindTargetBubble()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, detectRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<BigBubble>(out var bigBubble) &&
                bigBubble.State == BigBubbleState.AfterRelease &&
                IsBubbleReachable(bigBubble))
            {
                return bigBubble;
            }
        }
        return null;
    }

    /// <summary>气泡方向是否可达（向气泡发射射线，被障碍物阻挡则不可达）</summary>
    private bool IsBubbleReachable(BigBubble bubble)
    {
        Vector2 toBubble = (Vector2)bubble.transform.position - rb.position;
        float distance = toBubble.magnitude;
        if (distance <= 0.01f) return true;

        Vector2 direction = toBubble.normalized;
        var rayHits = Physics2D.RaycastAll(rb.position, direction, distance);
        foreach (var rayHit in rayHits)
        {
            if (rayHit.collider == null || rayHit.collider.isTrigger) continue;
            if (rayHit.collider.gameObject == gameObject) continue;          // 自身
            if (rayHit.collider.gameObject == bubble.gameObject) continue;   // 目标气泡本身
            if (rayHit.collider.GetComponent<Seal>() != null) continue;      // 海豹不算障碍
            if (rayHit.collider.GetComponent<SeagrassController>() != null) continue;       // 水草不算障碍
            if (rayHit.collider.GetComponent<SteelWireMeshController>() != null) continue;  // 铁丝网不算障碍
            return false; // 有障碍物阻挡
        }
        return true;
    }

    /// <summary>当前 X 处是否有水面且小鱼低于水面（在水中）</summary>
    private bool IsInWater()
    {
        float surfaceY = WatterUtils.GetWaterSurfaceY(transform.position.x);
        return surfaceY != float.MinValue && transform.position.y < surfaceY;
    }

    /// <summary>
    /// 目标气泡是否仍有效（存在、在检测范围内、视觉可达——模拟小鱼视觉）
    /// </summary>
    private bool IsTargetValid()
    {
        return targetBubble != null &&
               Vector2.Distance(transform.position, targetBubble.transform.position) <= detectRadius &&
               IsBubbleReachable(targetBubble);
    }

    /// <summary>检测移动方向前方是否有障碍（非触发器、非自身、海豹/水草不算障碍）</summary>
    private bool HasObstacleAhead()
    {
        var hits = Physics2D.RaycastAll(rb.position, moveDirection, obstacleCheckDistance);
        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider.isTrigger) continue;
            if (hit.collider.gameObject == gameObject) continue;
            if (hit.collider.GetComponent<Seal>() != null) continue;              // 海豹不算障碍
            if (hit.collider.GetComponent<SeagrassController>() != null) continue; // 水草不算障碍
            return true;
        }
        return false;
    }

    #endregion
}
