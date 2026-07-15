using UnityEngine;

/// <summary>
/// Seal 数据模型 —— 存储 Seal 的数值数据
/// </summary>
public class SealModel : MonoBehaviour
{
    #region 氧气系统

    public float OxygenValue { get; set; }

    [SerializeField] public float OxygenMaxValue = 100f;

    #endregion

    #region 移动速度

    [Header("移动速度")]
    [SerializeField] public float AirMoveSpeed = 3f;
    [SerializeField] public float LandMoveSpeed = 2f;
    [SerializeField] public float WaterMoveSpeed = 4f;
    [SerializeField] public float DashSpeed = 12f;

    #endregion

    #region 冲刺参数

    [Header("冲刺")]
    [SerializeField] public float DashDistance = 3f;
    [SerializeField] public float DashOxygenCost = 15f;
    [SerializeField] public float VelocitySmoothTime = 0.1f;

    #endregion

    #region 吐泡泡

    [Header("吐泡泡")]
    [SerializeField] public float BlowBubbleOxygenCost = 10f;

    #endregion
}
