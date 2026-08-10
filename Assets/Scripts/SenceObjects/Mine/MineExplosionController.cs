using UnityEngine;

/// <summary>
/// 水雷爆炸控制器 —— 检测范围内有 Seal（未来含小鱼）或连锁水雷爆炸时引爆；
/// 爆炸破坏爆炸范围内可破坏物体（IDestroyable），Tilemap 机制仅移除范围内瓦片
/// </summary>
public class MineExplosionController : MonoBehaviour
{
    [Header("检测范围")]
    [SerializeField] private float detectRadius = 1.5f;

    [Header("爆炸范围")]
    [SerializeField] private float explosionRadius = 2f;

    private bool hasExploded;

    private void FixedUpdate()
    {
        if (hasExploded) return;

        // 检测范围内有 Seal（未来：小鱼也在此判断）→ 爆炸
        var hits = Physics2D.OverlapCircleAll(transform.position, detectRadius);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Seal>() != null)
            {
                Explode();
                return;
            }
        }
    }

    /// <summary>引爆水雷</summary>
    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 连锁：检测范围内其它水雷引爆
        var chainHits = Physics2D.OverlapCircleAll(transform.position, detectRadius);
        foreach (var hit in chainHits)
        {
            var mine = hit.GetComponent<MineController>();
            if (mine != null && mine.gameObject != gameObject)
            {
                var explosion = mine.GetComponent<MineExplosionController>();
                if (explosion != null) explosion.Explode();
            }
        }

        // 爆炸范围内角色死亡（未来：小鱼也在此判断）+ 破坏可破坏物体
        var expHits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in expHits)
        {
            if (hit.GetComponent<Seal>() != null)
            {
                GameEvents.Publish(EventType.PLAYER_EVENT_ON_DEATH,
                    new PlayerEventArgs(hit.gameObject));
            }

            var destroyable = hit.GetComponent<IDestroyable>();
            if (destroyable != null)
                destroyable.DestroyByExplosion(transform.position, explosionRadius);
        }

        Destroy(gameObject);
    }
}
