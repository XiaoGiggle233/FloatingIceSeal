using System;
using UnityEngine;

/// <summary>
/// 海豹体型控制器
/// 监听氧气状态机变化，根据氧气状态调整胶囊碰撞体尺寸
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
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_STATE_CHANGE, OnOxygenStateChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_STATE_CHANGE, OnOxygenStateChanged);
    }

    private void Start()
    {
        ApplyShapeByState(seal.OxygenStateMachine.CurrentState);
    }

    private void OnOxygenStateChanged(IGameEvent evt)
    {
        var args = evt as GameStateEventArgs;
        if (args == null) return;

        if (!Enum.TryParse(args.StateName, out OxygenState state)) return;

        ApplyShapeByState(state);
    }

    private void ApplyShapeByState(OxygenState state)
    {
        SealShapeData shape = model.GetShapeData(state);
        capsuleCollider.size = new Vector2(shape.rectHeight + 2f * shape.radius, 2f * shape.radius);
    }
}
