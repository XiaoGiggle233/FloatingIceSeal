using UnityEngine;

/// <summary>
/// 海豹氧气状态机控制器
/// 监听氧气变化事件，根据临界值切换氧气状态机，并发布状态切换事件
/// </summary>
public class SealOxygenStateMachineController : MonoBehaviour
{
    private Seal seal;
    private SealModel model;
    private OxygenState previousState;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        model = GetComponent<SealModel>();
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_OXYGEN_CONSUME, OnOxygenChanged);
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_OXYGEN_RECOVER, OnOxygenChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_OXYGEN_CONSUME, OnOxygenChanged);
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_OXYGEN_RECOVER, OnOxygenChanged);
    }

    private void Start()
    {
        previousState = EvaluateState();
        ApplyState(previousState);
    }

    private void OnOxygenChanged(IGameEvent evt)
    {
        var args = evt as PlayerValueEventArgs;
        if (args == null) return;
        if ((args.Player as GameObject) != gameObject) return;

        UpdateOxygenState();
    }

    private void UpdateOxygenState()
    {
        OxygenState newState = EvaluateState();
        if (newState == previousState) return;

        ApplyState(newState);
        previousState = newState;

        GameEvents.Publish(EventType.PLAYER_EVENT_ON_STATE_CHANGE,
            new GameStateEventArgs(newState.ToString()));
    }

    private OxygenState EvaluateState()
    {
        float ratio = model.OxygenValue / model.OxygenMaxValue;

        if (ratio >= 1f)
            return OxygenState.OxygenFull;
        if (ratio >= model.OxygenSufficientThreshold)
            return OxygenState.OxygenSufficient;
        if (ratio >= model.OxygenInsufficientThreshold)
            return OxygenState.OxygenInsufficient;
        if (ratio > 0f)
            return OxygenState.OxygenCritical;

        return OxygenState.Suffocating;
    }

    private void ApplyState(OxygenState state)
    {
        seal.OxygenStateMachine.SetState(state);
        seal.OxygenStateMachine.EnterLeafState(GetLeafState(state));
    }

    private OxygenStateBase GetLeafState(OxygenState state)
    {
        switch (state)
        {
            case OxygenState.OxygenFull:
                return seal.OxygenStateMachine.OxygenFullState;
            case OxygenState.OxygenSufficient:
                return seal.OxygenStateMachine.OxygenSufficientState;
            case OxygenState.OxygenInsufficient:
                return seal.OxygenStateMachine.OxygenInsufficientState;
            case OxygenState.OxygenCritical:
                return seal.OxygenStateMachine.OxygenCriticalState;
            case OxygenState.Suffocating:
                return seal.OxygenStateMachine.SuffocatingState;
            default:
                return seal.OxygenStateMachine.OxygenFullState;
        }
    }
}
