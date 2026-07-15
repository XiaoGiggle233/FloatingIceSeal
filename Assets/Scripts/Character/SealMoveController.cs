using UnityEngine;

/// <summary>
/// 海豹移动控制器（临时简易版）
/// 临时键位：
///   WASD / 方向键 —— 移动
///   LeftShift      —— 冲刺（仅水中）
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SealMoveController : MonoBehaviour
{
    [Header("临时键位")]
    [SerializeField] private KeyCode moveUpKey = KeyCode.W;
    [SerializeField] private KeyCode moveDownKey = KeyCode.S;
    [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    private Seal seal;
    private SealModel model;
    private Rigidbody2D rb;

    private Vector2 currentVelocity;
    private bool isDashing;
    private float dashTimer;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        model = GetComponent<SealModel>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // 自行管理重力逻辑
    }

    private void Update()
    {
        if (!IsAlive()) return;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
            return; // 冲刺中不接受移动输入
        }

        Vector2 input = GetMovementInput();
        HandleDash(input);
        Move(input);
    }

    private void FixedUpdate()
    {
        if (!IsAlive()) return;

        Vector2 targetVelocity;
        if (isDashing)
        {
            targetVelocity = currentVelocity;
        }
        else
        {
            targetVelocity = currentVelocity;
        }

        rb.velocity = Vector2.SmoothDamp(
            rb.velocity,
            targetVelocity,
            ref currentVelocity,
            model.VelocitySmoothTime);
    }

    #region 输入处理

    private Vector2 GetMovementInput()
    {
        Vector2 input = Vector2.zero;

        if (Input.GetKey(moveLeftKey) || Input.GetKey(KeyCode.LeftArrow))
            input.x = -1f;
        else if (Input.GetKey(moveRightKey) || Input.GetKey(KeyCode.RightArrow))
            input.x = 1f;

        // 仅水中允许上下移动
        if (IsInWater())
        {
            if (Input.GetKey(moveUpKey) || Input.GetKey(KeyCode.UpArrow))
                input.y = 1f;
            else if (Input.GetKey(moveDownKey) || Input.GetKey(KeyCode.DownArrow))
                input.y = -1f;
        }

        return input;
    }

    private void HandleDash(Vector2 input)
    {
        if (Input.GetKeyDown(dashKey) && IsInWater())
        {
            // 检查氧气是否足够
            if (model.OxygenValue < model.DashOxygenCost) return;

            // 确定冲刺方向：有输入则用输入方向，否则用当前朝向
            Vector2 dashDir = input.sqrMagnitude > 0.01f ? input.normalized : Vector2.right;
            StartDash(dashDir);
        }
    }

    #endregion

    #region 移动执行

    private void Move(Vector2 input)
    {
        float speed = GetCurrentMoveSpeed();
        currentVelocity = input * speed;

        // 更新动作状态
        if (input.sqrMagnitude > 0.01f)
        {
            if (IsInWater())
                seal.ActionStateMachine.SetState(ActionState.InWaterMoving);
            else if (IsOnLand())
                seal.ActionStateMachine.SetState(ActionState.OnLandMoving);
        }
        else
        {
            seal.ActionStateMachine.SetState(ActionState.Idle);
        }
    }

    private float GetCurrentMoveSpeed()
    {
        if (IsInWater()) return model.WaterMoveSpeed;
        if (IsOnLand()) return model.LandMoveSpeed;
        return model.AirMoveSpeed;
    }

    #endregion

    #region 冲刺

    private void StartDash(Vector2 direction)
    {
        model.OxygenValue -= model.DashOxygenCost;
        isDashing = true;
        dashTimer = model.DashDistance / model.DashSpeed;
        currentVelocity = direction * model.DashSpeed;
        seal.ActionStateMachine.SetState(ActionState.Dashing);
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_DASH,
            new PlayerEventArgs(this.gameObject));
    }

    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
        currentVelocity = Vector2.zero;
        seal.ActionStateMachine.SetState(ActionState.Idle);
    }

    #endregion

    #region 状态查询

    private bool IsAlive()
    {
        return seal.LifeStateMachine.CurrentState == LifeState.Alive;
    }

    private bool IsInWater()
    {
        return seal.EnvironmentStateMachine.CurrentState == EnvironmentState.InWater;
    }

    private bool IsOnLand()
    {
        return seal.EnvironmentStateMachine.CurrentState == EnvironmentState.OnLand;
    }

    #endregion
}
