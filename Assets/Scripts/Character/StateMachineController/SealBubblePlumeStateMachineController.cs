using UnityEngine;

/// <summary>
/// 气泡柱状态机控制器 —— 检测角色碰撞体是否与气泡柱（BubblePlume）触发器重叠
/// </summary>
public class SealBubblePlumeStateMachineController : MonoBehaviour
{
    private Seal seal;
    private BubblePlumeStateMachine fsm;
    private Collider2D sealCollider;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        fsm = seal.BubblePlumeStateMachine;
        sealCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        bool inPlume = IsInBubblePlume();
        fsm.SetState(inPlume ? BubblePlumeState.InBubblePlume : BubblePlumeState.NotInBubblePlume);
        fsm.EnterLeafState(inPlume ? fsm.InBubblePlumeState : fsm.NotInBubblePlumeState);
    }

    /// <summary>角色碰撞体是否与任意气泡柱重叠</summary>
    private bool IsInBubblePlume()
    {
        if (sealCollider == null) return false;

        var hits = Physics2D.OverlapBoxAll(sealCollider.bounds.center, sealCollider.bounds.size, 0f);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<BubblePlume>() != null) return true;
        }
        return false;
    }
}
