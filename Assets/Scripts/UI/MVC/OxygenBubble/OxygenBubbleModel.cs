using UnityEngine;

#region ========== 氧气气泡模型 ==========

/// <summary>
/// 氧气气泡模型 —— 持有氧气显示数据
/// </summary>
public class OxygenBubbleModel : BaseModel
{
    /// <summary>当前氧气值</summary>
    public float OxygenValue;

    /// <summary>最大氧气值</summary>
    public float OxygenMaxValue = 100f;

    /// <summary>当前氧气百分比 (0~1)</summary>
    public float OxygenPercentage
    {
        get
        {
            if (OxygenMaxValue <= 0f) return 0f;
            return Mathf.Clamp01(OxygenValue / OxygenMaxValue);
        }
    }

    public override void Init()
    {
        OxygenValue = OxygenMaxValue;
    }
}

#endregion
