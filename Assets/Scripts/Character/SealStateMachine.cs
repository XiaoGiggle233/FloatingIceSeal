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

    public void SetState(LifeState state) => CurrentState = state;
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

    public void SetState(OxygenState state) => CurrentState = state;
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

    public void SetState(EnvironmentState state) => CurrentState = state;
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

    public void SetState(ActionState state) => CurrentState = state;
}

#endregion
