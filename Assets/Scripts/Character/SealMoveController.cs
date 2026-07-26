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
    private SealOxygenController oxygenController;
    private Rigidbody2D rb;

    private Vector2 moveTargetVelocity;
    private Vector2 velocityRef;
    private Vector2 lastInputDirection = Vector2.right;
    private float gravityVelocity;
    private bool isDashing;
    private float dashTimer;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        model = GetComponent<SealModel>();
        oxygenController = GetComponent<SealOxygenController>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // 自行管理重力逻辑
    }

    private void Update()
    {
        if (!seal.LifeStateMachine.IsAlive()) return;

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

        if (input.sqrMagnitude > 0.01f)
            lastInputDirection = input.normalized;

        Move(input);
        HandleDash();
    }

    private void FixedUpdate()
    {
        if (!seal.LifeStateMachine.IsAlive()) return;

        ApplyGravity();

        if (isDashing)
        {
            rb.velocity = moveTargetVelocity;
            return;
        }

        Vector2 targetVelocity = moveTargetVelocity;
        targetVelocity.y += gravityVelocity;

        rb.velocity = Vector2.SmoothDamp(
            rb.velocity,
            targetVelocity,
            ref velocityRef,
            model.VelocitySmoothTime);
    }

    private void ApplyGravity()
    {
        if (!seal.EnvironmentStateMachine.IsInAir())
        {
            gravityVelocity = 0f;
            return;
        }

        gravityVelocity -= model.GravityScale * Time.fixedDeltaTime;
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
        if (seal.EnvironmentStateMachine.IsInWater())
        {
            if (Input.GetKey(moveUpKey) || Input.GetKey(KeyCode.UpArrow))
                input.y = 1f;
            else if (Input.GetKey(moveDownKey) || Input.GetKey(KeyCode.DownArrow))
                input.y = -1f;
        }

        // 归一化，保证斜向速度与正向一致
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        return input;
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(dashKey) && seal.EnvironmentStateMachine.IsInWater())
        {
            if (model.OxygenValue < model.DashOxygenCost) return;

            Vector2 dashDir = GetDashDirection();
            if (dashDir.sqrMagnitude < 0.01f)
                dashDir = lastInputDirection;

            StartDash(dashDir);
        }
    }

    private Vector2 GetDashDirection()
    {
        Vector2 dir = Vector2.zero;

        if (Input.GetKey(moveLeftKey) || Input.GetKey(KeyCode.LeftArrow))
            dir.x = -1f;
        else if (Input.GetKey(moveRightKey) || Input.GetKey(KeyCode.RightArrow))
            dir.x = 1f;

        if (Input.GetKey(moveUpKey) || Input.GetKey(KeyCode.UpArrow))
            dir.y = 1f;
        else if (Input.GetKey(moveDownKey) || Input.GetKey(KeyCode.DownArrow))
            dir.y = -1f;

        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        return dir;
    }

    #endregion

    #region 移动执行

    private void Move(Vector2 input)
    {
        float speed = GetCurrentMoveSpeed();
        moveTargetVelocity = input * speed;
    }

    private float GetCurrentMoveSpeed()
    {
        if (seal.EnvironmentStateMachine.IsInWater()) return model.WaterMoveSpeed;
        if (seal.EnvironmentStateMachine.IsOnLand()) return model.LandMoveSpeed;
        return model.AirMoveSpeed;
    }

    #endregion

    #region 冲刺

    private void StartDash(Vector2 direction)
    {
        oxygenController.ConsumeOxygen(model.DashOxygenCost);
        isDashing = true;
        dashTimer = model.DashDistance / model.DashSpeed;
        moveTargetVelocity = direction * model.DashSpeed;
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_DASH,
            new PlayerEventArgs(this.gameObject));
    }

    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
        moveTargetVelocity = Vector2.zero;
        velocityRef = Vector2.zero;
        gravityVelocity = 0f;
    }

    #endregion
}
