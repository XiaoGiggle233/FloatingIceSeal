using UnityEngine;

/// <summary>
/// 气泡保护状态机控制器 —— 角色碰撞体被泡泡碰撞体覆盖 ≥50% 时进入受保护状态，否则不受保护
/// </summary>
public class SealProtectionStateMachineController : MonoBehaviour
{
    [Header("覆盖判定阈值")]
    [SerializeField, Range(0f, 1f)] private float protectedThreshold = 0.5f;

    [Header("采样网格")]
    [SerializeField, Range(1, 20)] private int sampleGridSize = 8;

    private Seal seal;
    private ProtectionStateMachine fsm;
    private Collider2D sealCollider;
    private LayerMask bubbleLayerMask;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        fsm = seal.ProtectionStateMachine;
        sealCollider = GetComponent<Collider2D>();
        bubbleLayerMask = LayerMask.GetMask("Bubble");
    }

    private void Update()
    {
        float ratio = GetBubbleCoverageRatio();

        if (ratio >= protectedThreshold)
        {
            fsm.SetState(ProtectionState.Protected);
            fsm.EnterLeafState(fsm.ProtectedState);
        }
        else
        {
            fsm.SetState(ProtectionState.Unprotected);
            fsm.EnterLeafState(fsm.UnprotectedState);
        }
    }

    /// <summary>角色碰撞体被泡泡覆盖的比例（0~1），采样法估算</summary>
    private float GetBubbleCoverageRatio()
    {
        if (sealCollider == null) return 0f;

        Bounds bounds = sealCollider.bounds;
        int total = 0, covered = 0;

        for (int i = 0; i <= sampleGridSize; i++)
        {
            for (int j = 0; j <= sampleGridSize; j++)
            {
                float u = i / (float)sampleGridSize;
                float v = j / (float)sampleGridSize;
                Vector2 point = new Vector2(bounds.min.x + u * bounds.size.x, bounds.min.y + v * bounds.size.y);

                // 只统计角色碰撞体内的采样点
                if (!sealCollider.OverlapPoint(point)) continue;
                total++;

                if (Physics2D.OverlapPoint(point, bubbleLayerMask) != null)
                    covered++;
            }
        }

        return total > 0 ? (float)covered / total : 0f;
    }
}
