using UnityEngine;

/// <summary>
/// 小鱼气泡保护检测 —— 小鱼碰撞体被泡泡碰撞体覆盖 ≥50% 时视为受保护，水雷不会炸死小鱼
/// （逻辑仿照 SealProtectionStateMachineController）
/// </summary>
public class SmallFishProtectionController : MonoBehaviour
{
    [Header("覆盖判定阈值")]
    [SerializeField, Range(0f, 1f)] private float protectedThreshold = 0.5f;

    [Header("采样网格")]
    [SerializeField, Range(1, 20)] private int sampleGridSize = 8;

    private Collider2D fishCollider;
    private LayerMask bubbleLayerMask;

    /// <summary>是否受气泡保护（泡泡覆盖比例 ≥ 阈值）</summary>
    public bool IsProtected { get; private set; }

    private void Awake()
    {
        fishCollider = GetComponent<Collider2D>();
        bubbleLayerMask = LayerMask.GetMask("Bubble");
    }

    private void Update()
    {
        IsProtected = GetBubbleCoverageRatio() >= protectedThreshold;
    }

    /// <summary>小鱼碰撞体被泡泡覆盖的比例（0~1），采样法估算</summary>
    private float GetBubbleCoverageRatio()
    {
        if (fishCollider == null) return 0f;

        Bounds bounds = fishCollider.bounds;
        int total = 0, covered = 0;

        for (int i = 0; i <= sampleGridSize; i++)
        {
            for (int j = 0; j <= sampleGridSize; j++)
            {
                float u = i / (float)sampleGridSize;
                float v = j / (float)sampleGridSize;
                Vector2 point = new Vector2(bounds.min.x + u * bounds.size.x, bounds.min.y + v * bounds.size.y);

                // 只统计小鱼碰撞体内的采样点
                if (!fishCollider.OverlapPoint(point)) continue;
                total++;

                if (Physics2D.OverlapPoint(point, bubbleLayerMask) != null)
                    covered++;
            }
        }

        return total > 0 ? (float)covered / total : 0f;
    }
}
