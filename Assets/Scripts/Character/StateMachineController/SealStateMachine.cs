#region ========== 状态基类 ==========

public abstract class SealState<TStateMachine>
{
    protected Seal owner;
    protected TStateMachine fsm;

    protected SealState(Seal owner, TStateMachine fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}

#endregion

#region ========== 1. 生命状态机 (LifeStateMachine) ==========

public enum LifeState { Alive, Dead }

public abstract class LifeStateBase : SealState<LifeStateMachine>
{
    protected LifeStateBase(Seal owner, LifeStateMachine fsm) : base(owner, fsm) { }
}

public class AliveState : LifeStateBase
{
    public AliveState(Seal owner, LifeStateMachine fsm) : base(owner, fsm) { }
}

public class DeadState : LifeStateBase
{
    public DeadState(Seal owner, LifeStateMachine fsm) : base(owner, fsm) { }
}

public class LifeStateMachine
{
    private Seal owner;

    public LifeState CurrentState { get; private set; }
    private LifeStateBase currentLeafState;

    public AliveState AliveState { get; }
    public DeadState DeadState { get; }

    public LifeStateMachine(Seal owner)
    {
        this.owner = owner;
        AliveState = new AliveState(owner, this);
        DeadState = new DeadState(owner, this);
    }

    public void Update() => currentLeafState?.Update();
    public void FixedUpdate() => currentLeafState?.FixedUpdate();

    public void EnterLeafState(LifeStateBase newState)
    {
        if (currentLeafState == newState) return;
        currentLeafState?.Exit();
        currentLeafState = newState;
        currentLeafState?.Enter();
    }

    public void SetState(LifeState state)
    {
        if (CurrentState == state) return;
        CurrentState = state;
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_STATE_CHANGE,
            new GameStateEventArgs(state.ToString()));
    }

    public bool IsAlive() => CurrentState == LifeState.Alive;
    public bool IsDead() => CurrentState == LifeState.Dead;
}

#endregion

#region ========== 2. 氧气状态机 (OxygenStateMachine) ==========

public enum OxygenState { OxygenFull, OxygenSufficient, OxygenInsufficient, OxygenCritical, Suffocating }

public abstract class OxygenStateBase : SealState<OxygenStateMachine>
{
    protected OxygenStateBase(Seal owner, OxygenStateMachine fsm) : base(owner, fsm) { }
}

public class OxygenFullState : OxygenStateBase
{
    public OxygenFullState(Seal owner, OxygenStateMachine fsm) : base(owner, fsm) { }
}

public class OxygenSufficientState : OxygenStateBase
{
    public OxygenSufficientState(Seal owner, OxygenStateMachine fsm) : base(owner, fsm) { }
}

public class OxygenInsufficientState : OxygenStateBase
{
    public OxygenInsufficientState(Seal owner, OxygenStateMachine fsm) : base(owner, fsm) { }
}

public class OxygenCriticalState : OxygenStateBase
{
    public OxygenCriticalState(Seal owner, OxygenStateMachine fsm) : base(owner, fsm) { }
}

public class SuffocatingState : OxygenStateBase
{
    public SuffocatingState(Seal owner, OxygenStateMachine fsm) : base(owner, fsm) { }
}

public class OxygenStateMachine
{
    private Seal owner;

    public OxygenState CurrentState { get; private set; }
    private OxygenStateBase currentLeafState;

    public OxygenFullState OxygenFullState { get; }
    public OxygenSufficientState OxygenSufficientState { get; }
    public OxygenInsufficientState OxygenInsufficientState { get; }
    public OxygenCriticalState OxygenCriticalState { get; }
    public SuffocatingState SuffocatingState { get; }

    public OxygenStateMachine(Seal owner)
    {
        this.owner = owner;
        OxygenFullState = new OxygenFullState(owner, this);
        OxygenSufficientState = new OxygenSufficientState(owner, this);
        OxygenInsufficientState = new OxygenInsufficientState(owner, this);
        OxygenCriticalState = new OxygenCriticalState(owner, this);
        SuffocatingState = new SuffocatingState(owner, this);
    }

    public void Update() => currentLeafState?.Update();
    public void FixedUpdate() => currentLeafState?.FixedUpdate();

    public void EnterLeafState(OxygenStateBase newState)
    {
        if (currentLeafState == newState) return;
        currentLeafState?.Exit();
        currentLeafState = newState;
        currentLeafState?.Enter();
    }

    public void SetState(OxygenState state)
    {
        if (CurrentState == state) return;
        CurrentState = state;
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_STATE_CHANGE,
            new GameStateEventArgs(state.ToString()));
    }

    public bool IsOxygenFull() => CurrentState == OxygenState.OxygenFull;
    public bool IsOxygenSufficient() => CurrentState == OxygenState.OxygenSufficient;
    public bool IsOxygenInsufficient() => CurrentState == OxygenState.OxygenInsufficient;
    public bool IsOxygenCritical() => CurrentState == OxygenState.OxygenCritical;
    public bool IsSuffocating() => CurrentState == OxygenState.Suffocating;
}

#endregion

#region ========== 3. 环境状态机 (EnvironmentStateMachine) ==========

public enum EnvironmentState { InAir, OnLand, InWater }

public abstract class EnvironmentStateBase : SealState<EnvironmentStateMachine>
{
    protected EnvironmentStateBase(Seal owner, EnvironmentStateMachine fsm) : base(owner, fsm) { }
}

public class InAirState : EnvironmentStateBase
{
    public InAirState(Seal owner, EnvironmentStateMachine fsm) : base(owner, fsm) { }
}

public class OnLandState : EnvironmentStateBase
{
    public OnLandState(Seal owner, EnvironmentStateMachine fsm) : base(owner, fsm) { }
}

public class InWaterState : EnvironmentStateBase
{
    public InWaterState(Seal owner, EnvironmentStateMachine fsm) : base(owner, fsm) { }
}

public class EnvironmentStateMachine
{
    private Seal owner;

    public EnvironmentState CurrentState { get; private set; }
    private EnvironmentStateBase currentLeafState;

    public InAirState InAirState { get; }
    public OnLandState OnLandState { get; }
    public InWaterState InWaterState { get; }

    public EnvironmentStateMachine(Seal owner)
    {
        this.owner = owner;
        InAirState = new InAirState(owner, this);
        OnLandState = new OnLandState(owner, this);
        InWaterState = new InWaterState(owner, this);
    }

    public void Update() => currentLeafState?.Update();
    public void FixedUpdate() => currentLeafState?.FixedUpdate();

    public void EnterLeafState(EnvironmentStateBase newState)
    {
        if (currentLeafState == newState) return;
        currentLeafState?.Exit();
        currentLeafState = newState;
        currentLeafState?.Enter();
    }

    public void SetState(EnvironmentState state)
    {
        if (CurrentState == state) return;
        CurrentState = state;
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_STATE_CHANGE,
            new GameStateEventArgs(state.ToString()));
    }

    public bool IsInAir() => CurrentState == EnvironmentState.InAir;
    public bool IsOnLand() => CurrentState == EnvironmentState.OnLand;
    public bool IsInWater() => CurrentState == EnvironmentState.InWater;
}

#endregion

#region ========== 4. 动作状态机 (ActionStateMachine) ==========

public enum ActionState { Idle, OnLandMoving, InWaterMoving, Dashing }

public abstract class ActionStateBase : SealState<ActionStateMachine>
{
    protected ActionStateBase(Seal owner, ActionStateMachine fsm) : base(owner, fsm) { }
}

public class IdleState : ActionStateBase
{
    public IdleState(Seal owner, ActionStateMachine fsm) : base(owner, fsm) { }
}

public class OnLandMovingState : ActionStateBase
{
    public OnLandMovingState(Seal owner, ActionStateMachine fsm) : base(owner, fsm) { }
}

public class InWaterMovingState : ActionStateBase
{
    public InWaterMovingState(Seal owner, ActionStateMachine fsm) : base(owner, fsm) { }
}

public class DashingState : ActionStateBase
{
    public DashingState(Seal owner, ActionStateMachine fsm) : base(owner, fsm) { }
}

public class ActionStateMachine
{
    private Seal owner;

    public ActionState CurrentState { get; private set; }
    private ActionStateBase currentLeafState;

    public IdleState IdleState { get; }
    public OnLandMovingState OnLandMovingState { get; }
    public InWaterMovingState InWaterMovingState { get; }
    public DashingState DashingState { get; }

    public ActionStateMachine(Seal owner)
    {
        this.owner = owner;
        IdleState = new IdleState(owner, this);
        OnLandMovingState = new OnLandMovingState(owner, this);
        InWaterMovingState = new InWaterMovingState(owner, this);
        DashingState = new DashingState(owner, this);
    }

    public void Update() => currentLeafState?.Update();
    public void FixedUpdate() => currentLeafState?.FixedUpdate();

    public void EnterLeafState(ActionStateBase newState)
    {
        if (currentLeafState == newState) return;
        currentLeafState?.Exit();
        currentLeafState = newState;
        currentLeafState?.Enter();
    }

    public void SetState(ActionState state)
    {
        if (CurrentState == state) return;
        CurrentState = state;
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_STATE_CHANGE,
            new GameStateEventArgs(state.ToString()));
    }

    public bool IsIdle() => CurrentState == ActionState.Idle;
    public bool IsOnLandMoving() => CurrentState == ActionState.OnLandMoving;
    public bool IsInWaterMoving() => CurrentState == ActionState.InWaterMoving;
    public bool IsDashing() => CurrentState == ActionState.Dashing;
}

#endregion

#region ========== 5. 移动方向状态机 (DirectionStateMachine) ==========

public enum DirectionState { Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight }

public abstract class DirectionStateBase : SealState<DirectionStateMachine>
{
    protected DirectionStateBase(Seal owner, DirectionStateMachine fsm) : base(owner, fsm) { }
}

public class UpState : DirectionStateBase
{
    public UpState(Seal owner, DirectionStateMachine fsm) : base(owner, fsm) { }
}

public class DownState : DirectionStateBase
{
    public DownState(Seal owner, DirectionStateMachine fsm) : base(owner, fsm) { }
}

public class LeftState : DirectionStateBase
{
    public LeftState(Seal owner, DirectionStateMachine fsm) : base(owner, fsm) { }
}

public class RightState : DirectionStateBase
{
    public RightState(Seal owner, DirectionStateMachine fsm) : base(owner, fsm) { }
}

public class UpLeftState : DirectionStateBase
{
    public UpLeftState(Seal owner, DirectionStateMachine fsm) : base(owner, fsm) { }
}

public class UpRightState : DirectionStateBase
{
    public UpRightState(Seal owner, DirectionStateMachine fsm) : base(owner, fsm) { }
}

public class DownLeftState : DirectionStateBase
{
    public DownLeftState(Seal owner, DirectionStateMachine fsm) : base(owner, fsm) { }
}

public class DownRightState : DirectionStateBase
{
    public DownRightState(Seal owner, DirectionStateMachine fsm) : base(owner, fsm) { }
}

public class DirectionStateMachine
{
    private Seal owner;

    public DirectionState CurrentState { get; private set; }
    private DirectionStateBase currentLeafState;

    public UpState UpState { get; }
    public DownState DownState { get; }
    public LeftState LeftState { get; }
    public RightState RightState { get; }
    public UpLeftState UpLeftState { get; }
    public UpRightState UpRightState { get; }
    public DownLeftState DownLeftState { get; }
    public DownRightState DownRightState { get; }

    public DirectionStateMachine(Seal owner)
    {
        this.owner = owner;
        UpState = new UpState(owner, this);
        DownState = new DownState(owner, this);
        LeftState = new LeftState(owner, this);
        RightState = new RightState(owner, this);
        UpLeftState = new UpLeftState(owner, this);
        UpRightState = new UpRightState(owner, this);
        DownLeftState = new DownLeftState(owner, this);
        DownRightState = new DownRightState(owner, this);
    }

    public void Update() => currentLeafState?.Update();
    public void FixedUpdate() => currentLeafState?.FixedUpdate();

    public void EnterLeafState(DirectionStateBase newState)
    {
        if (currentLeafState == newState) return;
        currentLeafState?.Exit();
        currentLeafState = newState;
        currentLeafState?.Enter();
    }

    public void SetState(DirectionState state)
    {
        if (CurrentState == state) return;
        CurrentState = state;
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_STATE_CHANGE,
            new GameStateEventArgs(state.ToString()));
    }

    public bool IsUp() => CurrentState == DirectionState.Up;
    public bool IsDown() => CurrentState == DirectionState.Down;
    public bool IsLeft() => CurrentState == DirectionState.Left;
    public bool IsRight() => CurrentState == DirectionState.Right;
    public bool IsUpLeft() => CurrentState == DirectionState.UpLeft;
    public bool IsUpRight() => CurrentState == DirectionState.UpRight;
    public bool IsDownLeft() => CurrentState == DirectionState.DownLeft;
    public bool IsDownRight() => CurrentState == DirectionState.DownRight;
}

#endregion
