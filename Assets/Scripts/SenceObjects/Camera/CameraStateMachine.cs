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

    public override void Update()
    {
        // 退出锁定后恢复原视野
        if (camera.RestoreView)
        {
            camera.TransitionViewSize(camera.OriginalViewSize, camera.ViewRestoreSpeed);

            // 视野恢复完成后重新启用 Pixel Perfect Camera
            if (camera.IsViewRestored())
                camera.ReenablePixelPerfect();
        }

        if (camera.TargetRb == null) return;

        Vector3 targetPos = camera.TargetRb.transform.position + camera.Offset;
        Vector3 cameraPos = camera.transform.position;

        float halfW = camera.FollowBufferWidth * 0.5f;
        float halfH = camera.FollowBufferHeight * 0.5f;

        Vector3 delta = targetPos - cameraPos;

        // 角色在缓冲区内，不移动
        if (Mathf.Abs(delta.x) <= halfW && Mathf.Abs(delta.y) <= halfH) return;

        // 角色超出缓冲区的溢出距离
        float overflowX = Mathf.Max(0f, Mathf.Abs(delta.x) - halfW);
        float overflowY = Mathf.Max(0f, Mathf.Abs(delta.y) - halfH);
        float overflow = new Vector2(overflowX, overflowY).magnitude;

        // 等比速度：速度 = 基准速度 * 等比系数 ^ (溢出距离 / 速度等级距离)
        float speed = camera.MoveSpeed * Mathf.Pow(camera.RatioMultiplier, overflow / camera.SpeedStepDistance);

        // 目标为把角色拉回缓冲区边缘的位置
        Vector3 moveTarget = cameraPos + new Vector3(
            Mathf.Clamp(delta.x, -halfW, halfW),
            Mathf.Clamp(delta.y, -halfH, halfH),
            0f);

        camera.MoveTo(moveTarget, speed);
    }
}

#endregion

#region ========== 锁定状态 ==========

public class CameraLockState : CameraStateBase
{
    public CameraLockState(SealCameraFollow camera, CameraStateMachine fsm) : base(camera, fsm) { }

    public override void Update()
    {
        CameraLockData data = camera.ActiveLockData;
        if (data == null) return;

        // 移动到锁定位置
        if (data.MoveToPosition)
            camera.MoveTo(data.LockPosition, camera.MoveSpeed);

        // 过渡到锁定视野
        if (data.AdjustViewSize)
            camera.TransitionViewSize(data.ViewSize, data.ViewTransitionSpeed);
    }
}

#endregion

#region ========== 关卡全览状态 ==========

public class CameraOverviewState : CameraStateBase
{
    private int _commandIndex;
    private float _stayTimer;
    private bool _returning;
    private Vector3 _moveTarget;
    private bool _hasMoveTarget;

    public CameraOverviewState(SealCameraFollow camera, CameraStateMachine fsm) : base(camera, fsm) { }

    public override void Enter()
    {
        _commandIndex = 0;
        _stayTimer = 0f;
        _returning = false;
        _hasMoveTarget = false;
    }

    public override void Update()
    {
        CameraOverviewData data = camera.ActiveOverviewData;
        if (data == null || camera.TargetRb == null) return;

        // 指令全部执行完毕，移回角色位置并恢复跟随
        if (_returning)
        {
            Vector3 playerPos = camera.TargetRb.transform.position;
            camera.MoveToUnclamped(playerPos, camera.OverviewReturnSpeed);

            // 仅比较 XY，忽略相机与角色的 Z 差异
            if (Vector2.Distance(camera.transform.position, playerPos) < 0.01f)
                camera.ReleaseOverview();
            return;
        }

        if (_commandIndex >= data.Commands.Count)
        {
            _returning = true;
            return;
        }

        CameraOverviewCommand cmd = data.Commands[_commandIndex];
        switch (cmd.Type)
        {
            case CameraOverviewCommandType.StartPosition:
                camera.MoveToUnclamped(cmd.StartPosition, camera.OverviewStartSpeed);
                // 仅比较 XY，忽略相机与指令位置的 Z 差异
                if (Vector2.Distance(camera.transform.position, cmd.StartPosition) < 0.01f)
                    _commandIndex++;
                break;

            case CameraOverviewCommandType.MoveDirection:
                // 进入指令时计算一次固定终点，避免每帧重算导致永远追不上
                if (!_hasMoveTarget)
                {
                    _moveTarget = camera.transform.position + (Vector3)cmd.MoveDirection.normalized * cmd.MoveDistance;
                    _hasMoveTarget = true;
                }
                camera.MoveToUnclamped(_moveTarget, cmd.MoveSpeed);
                if (Vector2.Distance(camera.transform.position, _moveTarget) < 0.01f)
                {
                    _commandIndex++;
                    _hasMoveTarget = false;
                }
                break;

            case CameraOverviewCommandType.Stay:
                _stayTimer += Time.deltaTime;
                if (_stayTimer >= cmd.StayTime)
                {
                    _stayTimer = 0f;
                    _commandIndex++;
                }
                break;
        }
    }
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

    public string CurrentLeafStateName => currentLeafState?.GetType().Name ?? "Null";

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
