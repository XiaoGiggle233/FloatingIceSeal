using UnityEngine;

/// <summary>
/// 动作状态机控制器
/// 根据角色移动速度切换 Idle/Moving，监听冲刺事件切换 Dashing，
/// 通过累计移动距离判断冲刺是否结束。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SealActionStateMachineController : MonoBehaviour
{
    [Header("阈值")]
    [SerializeField] private float idleSpeedThreshold = 0.01f;

    private Seal seal;
    private SealModel model;
    private ActionStateMachine fsm;
    private Rigidbody2D rb;

    private bool isDashing;
    private float dashDistanceTraveled;
    private Vector2 previousPosition;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        model = GetComponent<SealModel>();
        fsm = seal.ActionStateMachine;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_DASH, OnDash);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_DASH, OnDash);
    }

    private void Start()
    {
        previousPosition = transform.position;
        fsm.SetState(ActionState.Idle);
    }

    private void Update()
    {
        if (!seal.LifeStateMachine.IsAlive()) return;

        if (isDashing)
        {
            TrackDashDistance();
            if (ShouldExitDash())
            {
                EndDash();
            }
        }
        else
        {
            previousPosition = transform.position;
            UpdateMoveState();
        }
    }

    #region 静止/移动判断

    private void UpdateMoveState()
    {
        if (rb.velocity.magnitude > idleSpeedThreshold)
            fsm.SetState(ActionState.Moving);
        else
            fsm.SetState(ActionState.Idle);
    }

    #endregion

    #region 冲刺

    private void OnDash(IGameEvent evt)
    {
        var args = evt as PlayerEventArgs;
        if (args == null) return;
        if ((GameObject)args.Player != gameObject) return;

        // 仅水中可以冲刺
        if (!seal.EnvironmentStateMachine.IsInWater()) return;

        StartDash();
    }

    private void StartDash()
    {
        isDashing = true;
        dashDistanceTraveled = 0f;
        previousPosition = transform.position;
        fsm.SetState(ActionState.Dashing);
    }

    private void TrackDashDistance()
    {
        Vector2 currentPos = transform.position;
        dashDistanceTraveled += Vector2.Distance(previousPosition, currentPos);
        previousPosition = currentPos;
    }

    private bool ShouldExitDash()
    {
        // 累计移动距离达到冲刺距离
        if (dashDistanceTraveled >= model.DashDistance) return true;
        // 离开水中则退出冲刺
        if (!seal.EnvironmentStateMachine.IsInWater()) return true;
        return false;
    }

    private void EndDash()
    {
        isDashing = false;
        dashDistanceTraveled = 0f;
        // 下一帧由 UpdateMoveState 根据实际速度重新判断状态
        fsm.SetState(ActionState.Idle);
    }

    #endregion
}
