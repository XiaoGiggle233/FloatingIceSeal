using UnityEngine;

/// <summary>
/// 海豹氧气控制器
/// 通过位置检测判断角色露出水面的比例，高于阈值回复氧气，否则消耗氧气
/// </summary>
public class SealOxygenController : MonoBehaviour
{
    private Seal seal;
    private SealModel model;
    private Collider2D sealCollider;
    private CompositeCollider2D waterCollider;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        model = GetComponent<SealModel>();
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

    private void Update()
    {
        if (!seal.LifeStateMachine.IsAlive()) return;

        float exposeRatio = GetExposeRatio();

        if (exposeRatio >= model.OxygenRecoverExposeRatio)
        {
            RecoverOxygen(model.OxygenRecoverRate * Time.deltaTime);
        }
        else
        {
            ConsumeOxygen(model.OxygenConsumeRate * Time.deltaTime);
        }
    }

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

    #region 事件处理

    private void OnPlayerSpawn(IGameEvent evt)
    {
        SetOxygenToMax();
        ResolveWaterCollider();
    }

    #endregion

    #region 位置检测

    /// <summary>获取角色露出水面的比例（0=完全浸没，1=完全露出）</summary>
    private float GetExposeRatio()
    {
        if (sealCollider == null) return 1f;
        if (waterCollider == null) return 1f;

        Bounds bounds = sealCollider.bounds;
        float charTop = bounds.max.y;
        float charBottom = bounds.min.y;
        float charHeight = charTop - charBottom;
        if (charHeight <= 0f) return 1f;

        float waterSurfaceY = GetWaterSurfaceY(bounds.center.x);
        if (waterSurfaceY <= charBottom) return 1f;
        if (waterSurfaceY >= charTop) return 0f;

        return (charTop - waterSurfaceY) / charHeight;
    }

    /// <summary>通过射线检测获取指定 X 坐标处的水面 Y 值</summary>
    private float GetWaterSurfaceY(float x)
    {
        Vector2 origin = new Vector2(x, waterCollider.bounds.max.y + 10f);
        int layerMask = 1 << waterCollider.gameObject.layer;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, Mathf.Infinity, layerMask);
        if (hit.collider != null)
            return hit.point.y;

        return float.MinValue;
    }

    #endregion

    #region 氧气操作接口

    /// <summary>消耗指定数值的氧气，返回实际消耗量</summary>
    public float ConsumeOxygen(float amount)
    {
        if (amount <= 0f || model.OxygenValue <= 0f) return 0f;

        float actual = Mathf.Min(amount, model.OxygenValue);
        model.OxygenValue -= actual;

        GameEvents.Publish(EventType.PLAYER_EVENT_ON_OXYGEN_CONSUME,
            new PlayerValueEventArgs(this.gameObject, actual));
        return actual;
    }

    /// <summary>回复指定数值的氧气，返回实际回复量</summary>
    public float RecoverOxygen(float amount)
    {
        if (amount <= 0f || model.OxygenValue >= model.OxygenMaxValue) return 0f;

        float actual = Mathf.Min(amount, model.OxygenMaxValue - model.OxygenValue);
        model.OxygenValue += actual;

        GameEvents.Publish(EventType.PLAYER_EVENT_ON_OXYGEN_RECOVER,
            new PlayerValueEventArgs(this.gameObject, actual));
        return actual;
    }

    /// <summary>检查氧气是否足够指定数值</summary>
    public bool HasEnoughOxygen(float amount)
    {
        return model.OxygenValue >= amount;
    }

    /// <summary>将氧气设置为最大值</summary>
    public void SetOxygenToMax()
    {
        model.OxygenValue = model.OxygenMaxValue;
    }

    /// <summary>获取当前氧气百分比（0~1）</summary>
    public float GetOxygenPercentage()
    {
        if (model.OxygenMaxValue <= 0f) return 0f;
        return model.OxygenValue / model.OxygenMaxValue;
    }

    #endregion
}
