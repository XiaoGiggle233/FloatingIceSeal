using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>水雷爆炸参数快照 —— 关卡重置从 prefab 重建后还原场景 Inspector 覆盖值</summary>
public struct MineExplosionSettings
{
    public float detectRadius;
    public float explosionRadius;
    public float explosionDelay;
    public float spawnGracePeriod;

    public MineExplosionSettings(float detectRadius, float explosionRadius, float explosionDelay, float spawnGracePeriod)
    {
        this.detectRadius = detectRadius;
        this.explosionRadius = explosionRadius;
        this.explosionDelay = explosionDelay;
        this.spawnGracePeriod = spawnGracePeriod;
    }
}

/// <summary>
/// 水雷爆炸控制器 —— 检测范围内有 Seal 或小鱼或连锁水雷爆炸时引爆；
/// 爆炸破坏爆炸范围内可破坏物体（IDestroyable），Tilemap 机制仅移除范围内瓦片
/// </summary>
public class MineExplosionController : MonoBehaviour
{
    [FoldoutGroup("爆炸设置", expanded: true)]
    [LabelText("检测范围"), MinValue(0), SuffixLabel("m", Overlay = true)]
    [SerializeField] private float detectRadius = 1.5f;

    [FoldoutGroup("爆炸设置")]
    [LabelText("爆炸范围"), MinValue(0), SuffixLabel("m", Overlay = true)]
    [SerializeField] private float explosionRadius = 2f;

    [FoldoutGroup("爆炸设置")]
    [LabelText("爆炸延迟"), MinValue(0), SuffixLabel("秒", Overlay = true)]
    [SerializeField] private float explosionDelay = 0f;

    [FoldoutGroup("爆炸设置")]
    [LabelText("出生保护时间"), MinValue(0), SuffixLabel("秒", Overlay = true)]
    [SerializeField] private float spawnGracePeriod = 1f;

    private bool hasExploded;
    private bool isTriggered;
    private float delayTimer;
    private float spawnGraceTimer;

    private void Awake()
    {
        // 出生保护：防止关卡重置重建后立即检测到重生角色而再次引爆
        spawnGraceTimer = spawnGracePeriod;
    }

    /// <summary>捕获当前爆炸参数（关卡重置重建后由 LevelResetSystem 还原）</summary>
    public MineExplosionSettings CaptureSettings() =>
        new MineExplosionSettings(detectRadius, explosionRadius, explosionDelay, spawnGracePeriod);

    /// <summary>还原爆炸参数（关卡重置重建后由 LevelResetSystem 调用，同时重新进入出生保护期）</summary>
    public void RestoreSettings(MineExplosionSettings settings)
    {
        detectRadius = settings.detectRadius;
        explosionRadius = settings.explosionRadius;
        explosionDelay = settings.explosionDelay;
        spawnGracePeriod = settings.spawnGracePeriod;
        spawnGraceTimer = spawnGracePeriod;
    }

    /// <summary>Scene 视图选中时显示检测范围（黄）与爆炸范围（红）</summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = new Color(1f, 0.25f, 0.1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    private void FixedUpdate()
    {
        if (hasExploded) return;

        // 出生保护期内不检测
        if (spawnGraceTimer > 0f)
        {
            spawnGraceTimer -= Time.fixedDeltaTime;
            return;
        }

        // 已触发 → 等待延迟后爆炸
        if (isTriggered)
        {
            delayTimer -= Time.fixedDeltaTime;
            if (delayTimer <= 0f)
                Explode();
            return;
        }

        // 检测范围内有 Seal 或小鱼 → 触发（延迟后爆炸）
        var hits = Physics2D.OverlapCircleAll(transform.position, detectRadius);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Seal>() != null || hit.GetComponent<SmallFishController>() != null)
            {
                isTriggered = true;
                delayTimer = explosionDelay;
                return;
            }
        }
    }

    /// <summary>引爆水雷</summary>
    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 连锁：爆炸范围内其它水雷引爆
        var chainHits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in chainHits)
        {
            var mine = hit.GetComponent<MineController>();
            if (mine != null && mine.gameObject != gameObject)
            {
                var explosion = mine.GetComponent<MineExplosionController>();
                if (explosion != null) explosion.Explode();
            }
        }

        // 爆炸范围内角色死亡 + 小鱼死亡 + 破坏可破坏物体
        var expHits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in expHits)
        {
            // 受气泡保护的海豹不会被炸死
            var seal = hit.GetComponent<Seal>();
            if (seal != null && !(seal.ProtectionStateMachine?.IsProtected() ?? false))
            {
                GameEvents.Publish(EventType.PLAYER_EVENT_ON_DEATH,
                    new PlayerEventArgs(hit.gameObject));
            }

            // 受气泡保护的小鱼不会被炸死，其余小鱼会被炸死（关卡重置时由 LevelResetSystem 重建）
            var fish = hit.GetComponent<SmallFishController>();
            if (fish != null && !fish.IsProtected())
            {
                Destroy(fish.gameObject);
            }

            // 水雷爆炸会炸掉范围内气泡（蓄力中未释放的气泡由 Burst 内部保护）
            var bubble = hit.GetComponent<BubbleBase>();
            if (bubble != null)
                bubble.Burst();

            var destroyable = hit.GetComponent<IDestroyable>();
            if (destroyable != null)
                destroyable.DestroyByExplosion(transform.position, explosionRadius);
        }

        Destroy(gameObject);
    }
}
