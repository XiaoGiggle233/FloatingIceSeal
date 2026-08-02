using UnityEngine;
#region ========== 摄像机状态枚举 ==========

public enum CameraState { Follow, Lock, Overview, Vertical }

#endregion

#region ========== 摄像机状态基类 ==========

public abstract class CameraStateBase
{
    protected SealCameraFollow camera;
    protected CameraStateMachine fsm;

    protected CameraStateBase(SealCameraFollow camera, CameraStateMachine fsm)
    {
        this.camera = camera;
        this.fsm = fsm;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}

#endregion

#region ========== 跟随状态 ==========

public class CameraFollowState : CameraStateBase
{
    public CameraFollowState(SealCameraFollow camera, CameraStateMachine fsm) : base(camera, fsm) { }
}

#endregion

#region ========== 锁定状态 ==========

public class CameraLockState : CameraStateBase
{
    public CameraLockState(SealCameraFollow camera, CameraStateMachine fsm) : base(camera, fsm) { }
}

#endregion

#region ========== 关卡全览状态 ==========

public class CameraOverviewState : CameraStateBase
{
    public CameraOverviewState(SealCameraFollow camera, CameraStateMachine fsm) : base(camera, fsm) { }
}

#endregion

#region ========== 纵向关卡状态 ==========

public class CameraVerticalState : CameraStateBase
{
    public CameraVerticalState(SealCameraFollow camera, CameraStateMachine fsm) : base(camera, fsm) { }
}

#endregion

#region ========== 摄像机状态机 ==========

public class CameraStateMachine
{
    private SealCameraFollow camera;

    public CameraState CurrentState { get; private set; }
    private CameraStateBase currentLeafState;

    public CameraFollowState FollowState { get; }
    public CameraLockState LockState { get; }
    public CameraOverviewState OverviewState { get; }
    public CameraVerticalState VerticalState { get; }

    public CameraStateMachine(SealCameraFollow camera)
    {
        this.camera = camera;
        FollowState = new CameraFollowState(camera, this);
        LockState = new CameraLockState(camera, this);
        OverviewState = new CameraOverviewState(camera, this);
        VerticalState = new CameraVerticalState(camera, this);
    }

    public void Update() => currentLeafState?.Update();

    public void EnterLeafState(CameraStateBase newState)
    {
        if (currentLeafState == newState) return;
        currentLeafState?.Exit();
        currentLeafState = newState;
        currentLeafState?.Enter();
    }

    public void SetState(CameraState state)
    {
        if (CurrentState == state) return;
        CurrentState = state;
    }

    public bool IsFollow() => CurrentState == CameraState.Follow;
    public bool IsLock() => CurrentState == CameraState.Lock;
    public bool IsOverview() => CurrentState == CameraState.Overview;
    public bool IsVertical() => CurrentState == CameraState.Vertical;
}

#endregion
