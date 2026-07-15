using UnityEngine;

/// <summary>
/// 海豹氧气控制器
/// 负责氧气的持续消耗/回复、氧气值操作接口、氧气状态机更新
/// </summary>
public class SealOxygenController : MonoBehaviour
{
    private Seal seal;
    private SealModel model;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        model = GetComponent<SealModel>();
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_SPAWN, OnPlayerSpawn);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_SPAWN, OnPlayerSpawn);
    }

    private void Update()
    {
        if (!seal.LifeStateMachine.IsAlive()) return;

        if (seal.EnvironmentStateMachine.IsInWater())
        {
            ConsumeOxygen(model.OxygenConsumeRate * Time.deltaTime);
        }
        else
        {
            RecoverOxygen(model.OxygenRecoverRate * Time.deltaTime);
        }
    }

    #region 事件处理

    private void OnPlayerSpawn(IGameEvent evt)
    {
        SetOxygenToMax();
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
