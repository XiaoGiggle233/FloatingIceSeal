using System;
using UnityEngine;

/// <summary>
/// 海豹体型控制器
/// 监听氧气/方向状态机变化，根据氧气状态调整胶囊碰撞体尺寸，竖向时 x、y 对调
/// </summary>
[RequireComponent(typeof(CapsuleCollider2D))]
public class SealShapeController : MonoBehaviour
{
    private Seal seal;
    private SealModel model;
    private CapsuleCollider2D capsuleCollider;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        model = GetComponent<SealModel>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        capsuleCollider.direction = CapsuleDirection2D.Horizontal;
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_STATE_CHANGE, OnStateChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_STATE_CHANGE, OnStateChanged);
    }

    private void Start()
    {
        ApplyShapeByState(seal.OxygenStateMachine.CurrentState, seal.DirectionStateMachine.CurrentState);
    }

    private void OnStateChanged(IGameEvent evt)
    {
        var args = evt as GameStateEventArgs;
        if (args == null) return;

        if (Enum.TryParse(args.StateName, out OxygenState oxygenState))
        {
            ApplyShapeByState(oxygenState, seal.DirectionStateMachine.CurrentState);
            return;
        }

        // 方向变化时按新方向重算尺寸，使竖向对调生效
        if (Enum.TryParse(args.StateName, out DirectionState directionState))
            ApplyShapeByState(seal.OxygenStateMachine.CurrentState, directionState);
    }

    private void ApplyShapeByState(OxygenState state, DirectionState direction)
    {
        SealShapeData shape = model.GetShapeData(state);
        Vector2 size = new Vector2(shape.rectHeight + 2f * shape.radius, 2f * shape.radius);

        // 竖向时胶囊体 x、y 对调
        bool isVertical = direction == DirectionState.Up || direction == DirectionState.Down;
        if (isVertical)
            size = new Vector2(size.y, size.x);

        capsuleCollider.size = size;
    }
}
