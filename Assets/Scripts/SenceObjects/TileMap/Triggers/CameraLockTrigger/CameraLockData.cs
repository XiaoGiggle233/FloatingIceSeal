using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 摄像机锁定数据 —— 记录锁定时摄像机的位置与视野（正交大小）配置。
/// </summary>
[System.Serializable]
public class CameraLockData
{
    [Title("锁定位置")]
    [SerializeField, LabelText("启用移动"), ToggleLeft, Tooltip("锁定时是否将摄像机移动到指定位置")]
    private bool _moveToPosition = true;

    [SerializeField, LabelText("锁定位置"), ShowIf(nameof(_moveToPosition))]
    private Vector3 _lockPosition = Vector3.zero;

    [Title("视野")]
    [SerializeField, LabelText("调整视野"), ToggleLeft, Tooltip("锁定时是否调整摄像机的正交视野大小")]
    private bool _adjustViewSize = false;

    [SerializeField, LabelText("目标视野大小"), MinValue(0.1f), SuffixLabel("unit", Overlay = true), ShowIf(nameof(_adjustViewSize))]
    private float _viewSize = 5f;

    [SerializeField, LabelText("视野过渡速度"), MinValue(0.01f), SuffixLabel("unit/s", Overlay = true), ShowIf(nameof(_adjustViewSize))]
    private float _viewTransitionSpeed = 2f;

    #region 属性

    public bool MoveToPosition => _moveToPosition;
    public Vector3 LockPosition => _lockPosition;
    public bool AdjustViewSize => _adjustViewSize;
    public float ViewSize => _viewSize;
    public float ViewTransitionSpeed => _viewTransitionSpeed;

    #endregion
}
