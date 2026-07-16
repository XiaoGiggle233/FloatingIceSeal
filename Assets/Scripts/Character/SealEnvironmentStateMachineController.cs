using UnityEngine;

/// <summary>
/// 环境状态机控制器 —— 根据海豹所处的环境（水中/陆地/空中）驱动 EnvironmentStateMachine 切换状态
/// </summary>
public class SealEnvironmentStateMachineController : MonoBehaviour
{
    private Seal seal;
    private EnvironmentStateMachine fsm;
    private Collider2D sealCollider;
    private CompositeCollider2D waterCollider;

    private bool isInWater;
    private int wallGroundContactCount;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        fsm = seal.EnvironmentStateMachine;
        sealCollider = GetComponent<Collider2D>();
        ResolveWaterCollider();
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_SPAWN, OnPlayerSpawn);
        GameEvents.Listen(EventType.INFO_POOL_EVENT_ON_CHANGE, OnPoolChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_SPAWN, OnPlayerSpawn);
        GameEvents.Unlisten(EventType.INFO_POOL_EVENT_ON_CHANGE, OnPoolChanged);
    }

    #region 角色生成初始化

    private void OnPlayerSpawn(IGameEvent evt)
    {
        var args = evt as PlayerEventArgs;
        if (args == null) return;
        if ((GameObject)args.Player != gameObject) return;

        ResolveWaterCollider();
        InitializeState();
    }

    private void InitializeState()
    {
        isInWater = CheckIsInWater();
        RecalculateWallGroundContacts();
        UpdateEnvironment();
    }

    #endregion

    #region 信息池监听（水体动态注册/注销）

    private void OnPoolChanged(IGameEvent evt)
    {
        var args = evt as InfoPoolEventArgs;
        if (args == null || args.Key != "Watter") return;

        if (args.Type == InfoPoolEventArgs.ChangeType.Set)
            ResolveWaterCollider();
        else if (args.Type == InfoPoolEventArgs.ChangeType.Remove)
            waterCollider = null;
    }

    private void ResolveWaterCollider()
    {
        if (InformationPool.TryGet("Watter", out object obj) && obj is Component comp)
            waterCollider = comp.GetComponent<CompositeCollider2D>();
    }

    #endregion

    #region FixedUpdate 持续检测

    private void FixedUpdate()
    {
        bool wasInWater = isInWater;
        isInWater = CheckIsInWater();

        if (isInWater != wasInWater)
            UpdateEnvironment();
    }

    /// <summary>
    /// 使用 Collider2D.Distance 判断海豹碰撞体是否与水体碰撞体重叠。
    /// 替代 OverlapPoint，因为 CompositeCollider2D 的 GeometryType=Outlines
    /// 生成的是边缘碰撞体（没有"内部"），OverlapPoint 永远无法命中。
    /// Distance 方法直接比较两个碰撞体的几何关系，不受 GeometryType 影响。
    /// </summary>
    private bool CheckIsInWater()
    {
        if (waterCollider == null || sealCollider == null) return false;
        var dist = waterCollider.Distance(sealCollider);
        return dist.isOverlapped;
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
        if (isInWater)
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
