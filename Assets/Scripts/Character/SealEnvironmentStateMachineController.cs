using UnityEngine;

/// <summary>
/// 环境状态机控制器 —— 根据海豹所处的环境（水中/陆地/空中）驱动 EnvironmentStateMachine 切换状态
/// </summary>
public class SealEnvironmentStateMachineController : MonoBehaviour
{
    private Seal seal;
    private EnvironmentStateMachine fsm;

    private int waterTriggerCount;
    private int wallGroundContactCount;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        fsm = seal.EnvironmentStateMachine;
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_SPAWN, OnPlayerSpawn);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_SPAWN, OnPlayerSpawn);
    }

    #region 角色生成初始化

    private void OnPlayerSpawn(IGameEvent evt)
    {
        var args = evt as PlayerEventArgs;
        if (args == null) return;
        if ((GameObject)args.Player != gameObject) return;

        InitializeState();
    }

    private void InitializeState()
    {
        // 检测初始位置是否在水体中
        waterTriggerCount = 0;
        var overlaps = Physics2D.OverlapPointAll(transform.position);
        foreach (var col in overlaps)
        {
            if (col.isTrigger && col.GetComponent<WatterController>() != null)
                waterTriggerCount++;
        }

        // 检测初始位置的墙体接触
        RecalculateWallGroundContacts();

        UpdateEnvironment();
    }

    #endregion

    #region 水体触发器检测

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<WatterController>() != null)
        {
            waterTriggerCount++;
            UpdateEnvironment();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<WatterController>() != null)
        {
            waterTriggerCount = Mathf.Max(0, waterTriggerCount - 1);
            UpdateEnvironment();
        }
    }

    #endregion

    #region 墙体碰撞检测

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<WallController>() == null) return;

        RecalculateWallGroundContacts();
        UpdateEnvironment();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<WallController>() == null) return;

        RecalculateWallGroundContacts();
        UpdateEnvironment();
    }

    private void RecalculateWallGroundContacts()
    {
        wallGroundContactCount = 0;
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        var contacts = new ContactPoint2D[20];
        int count = rb.GetContacts(contacts);
        for (int i = 0; i < count; i++)
        {
            if (contacts[i].collider.GetComponent<WallController>() != null
                && IsVerticalContact(contacts[i]))
            {
                wallGroundContactCount++;
            }
        }
    }

    private static bool IsVerticalContact(ContactPoint2D contact)
    {
        return Mathf.Abs(contact.normal.y) > Mathf.Abs(contact.normal.x);
    }

    #endregion

    #region 状态切换

    private void UpdateEnvironment()
    {
        if (waterTriggerCount > 0)
        {
            fsm.SetState(EnvironmentState.InWater);
            fsm.EnterLeafState(fsm.InWaterState);
        }
        else if (wallGroundContactCount > 0)
        {
            fsm.SetState(EnvironmentState.OnLand);
            fsm.EnterLeafState(fsm.OnLandState);
        }
        else
        {
            fsm.SetState(EnvironmentState.InAir);
            fsm.EnterLeafState(fsm.InAirState);
        }
    }

    #endregion
}
