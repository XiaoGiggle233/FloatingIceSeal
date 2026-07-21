using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>胶囊体体型数据</summary>
[System.Serializable]
public struct SealShapeData
{
    [LabelText("半径"), SuffixLabel("m", Overlay = true)]
    public float radius;
    [LabelText("矩形高度"), SuffixLabel("m", Overlay = true)]
    public float rectHeight;
}

/// <summary>
/// Seal 数据模型 —— 存储 Seal 的数值数据
/// </summary>
public class SealModel : MonoBehaviour
{
    #region 氧气系统

    [FoldoutGroup("氧气系统", expanded: true)]
    [ShowInInspector, ReadOnly, ProgressBar(0, nameof(OxygenMaxValue)), LabelText("当前氧气")]
    public float OxygenValue
    {
        get => oxygenValue;
        set => oxygenValue = value;
    }

    [SerializeField, HideInInspector]
    private float oxygenValue;

    [FoldoutGroup("氧气系统")]
    [LabelText("最大氧气"), MinValue(1), SuffixLabel("单位", Overlay = true)]
    [SerializeField] public float OxygenMaxValue = 100f;

    [FoldoutGroup("氧气系统")]
    [LabelText("消耗速率"), MinValue(0), SuffixLabel("/秒", Overlay = true)]
    [SerializeField] public float OxygenConsumeRate = 5f;

    [FoldoutGroup("氧气系统")]
    [LabelText("回复速率"), MinValue(0), SuffixLabel("/秒", Overlay = true)]
    [SerializeField] public float OxygenRecoverRate = 20f;

    [FoldoutGroup("氧气系统")]
    [LabelText("满氧阈值"), PropertyRange(0, 1), SuffixLabel("%", Overlay = true)]
    [PropertyTooltip("氧气百分比高于此值视为满氧")]
    [SerializeField] public float OxygenFullThreshold = 1f;

    [FoldoutGroup("氧气系统")]
    [LabelText("充足阈值"), PropertyRange(0, 1), SuffixLabel("%", Overlay = true)]
    [PropertyTooltip("氧气百分比高于此值视为充足")]
    [SerializeField] public float OxygenSufficientThreshold = 0.5f;

    [FoldoutGroup("氧气系统")]
    [LabelText("不足阈值"), PropertyRange(0, 1), SuffixLabel("%", Overlay = true)]
    [PropertyTooltip("氧气百分比低于此值视为不足")]
    [SerializeField] public float OxygenInsufficientThreshold = 0.25f;
    
    [FoldoutGroup("氧气系统")]
    [LabelText("濒危阈值"), PropertyRange(0, 1), SuffixLabel("%", Overlay = true)]
    [PropertyTooltip("氧气百分比低于此值视为窒息")]
    [SerializeField] public float OxygenCriticalThreshold = 0f;

    [FoldoutGroup("氧气系统")]
    [LabelText("露出水面回复阈值"), PropertyRange(0, 1), SuffixLabel("%", Overlay = true)]
    [PropertyTooltip("角色露出水面的比例高于此值时开始回复氧气")]
    [SerializeField] public float OxygenRecoverExposeRatio = 0.5f;

    #endregion

    #region 移动速度

    [FoldoutGroup("移动速度", expanded: true)]
    [LabelText("空中速度"), MinValue(0), SuffixLabel("m/s", Overlay = true)]
    [SerializeField] public float AirMoveSpeed = 3f;

    [FoldoutGroup("移动速度")]
    [LabelText("陆地速度"), MinValue(0), SuffixLabel("m/s", Overlay = true)]
    [SerializeField] public float LandMoveSpeed = 2f;

    [FoldoutGroup("移动速度")]
    [LabelText("水中速度"), MinValue(0), SuffixLabel("m/s", Overlay = true)]
    [SerializeField] public float WaterMoveSpeed = 4f;

    [FoldoutGroup("移动速度")]
    [LabelText("冲刺速度"), MinValue(0), SuffixLabel("m/s", Overlay = true)]
    [SerializeField] public float DashSpeed = 8f;

    #endregion

    #region 冲刺参数

    [FoldoutGroup("冲刺", expanded: false)]
    [LabelText("冲刺距离"), MinValue(0), SuffixLabel("m", Overlay = true)]
    [SerializeField] public float DashDistance = 2f;

    [FoldoutGroup("冲刺")]
    [LabelText("氧气消耗"), MinValue(0), SuffixLabel("单位", Overlay = true)]
    [SerializeField] public float DashOxygenCost = 15f;

    [FoldoutGroup("冲刺")]
    [LabelText("速度平滑"), Range(0, 1), SuffixLabel("秒", Overlay = true)]
    [SerializeField] public float VelocitySmoothTime = 0.1f;

    #endregion

    #region 吐泡泡

    [FoldoutGroup("吐泡泡", expanded: false)]
    [LabelText("蓄力消耗速率"), MinValue(0), SuffixLabel("/秒", Overlay = true)]
    [SerializeField] public float BlowBubbleChargeOxygenRate = 15f;

    [FoldoutGroup("吐泡泡")]
    [LabelText("生成偏移"), SuffixLabel("m", Overlay = true)]
    [SerializeField] public float BlowBubbleSpawnOffset = 1.5f;

    [FoldoutGroup("吐泡泡")]
    [LabelText("氧气回复比例"), MinValue(0), SuffixLabel("倍", Overlay = true)]
    [SerializeField] public float BubbleOxygenRecoverRatio = 1f;

    #endregion

    #region 重力

    [FoldoutGroup("重力", expanded: false)]
    [LabelText("重力"), MinValue(0), SuffixLabel("m/s²", Overlay = true)]
    [SerializeField] public float GravityScale = 2f;

    #endregion

    #region 体型

    [FoldoutGroup("体型（胶囊体参数）", expanded: false)]
    [LabelText("满氧气体型"), InlineProperty]
    [SerializeField] public SealShapeData OxygenFullShape = new SealShapeData { radius = 1.8f, rectHeight = 0f };

    [FoldoutGroup("体型（胶囊体参数）")]
    [LabelText("充足体型"), InlineProperty]
    [SerializeField] public SealShapeData OxygenSufficientShape = new SealShapeData { radius = 1.2f, rectHeight = 0.4f };

    [FoldoutGroup("体型（胶囊体参数）")]
    [LabelText("不足体型"), InlineProperty]
    [SerializeField] public SealShapeData OxygenInsufficientShape = new SealShapeData { radius = 0.7f, rectHeight = 1f };

    [FoldoutGroup("体型（胶囊体参数）")]
    [LabelText("危险体型"), InlineProperty]
    [SerializeField] public SealShapeData OxygenCriticalShape = new SealShapeData { radius = 0.5f, rectHeight = 1.6f };

    [FoldoutGroup("体型（胶囊体参数）")]
    [LabelText("窒息体型"), InlineProperty]
    [SerializeField] public SealShapeData SuffocatingShape = new SealShapeData { radius = 0.4f, rectHeight = 2.2f };

    public SealShapeData GetShapeData(OxygenState state)
    {
        switch (state)
        {
            case OxygenState.OxygenFull:        return OxygenFullShape;
            case OxygenState.OxygenSufficient:   return OxygenSufficientShape;
            case OxygenState.OxygenInsufficient: return OxygenInsufficientShape;
            case OxygenState.OxygenCritical:     return OxygenCriticalShape;
            case OxygenState.Suffocating:        return SuffocatingShape;
            default:                             return OxygenFullShape;
        }
    }

    #endregion
}
