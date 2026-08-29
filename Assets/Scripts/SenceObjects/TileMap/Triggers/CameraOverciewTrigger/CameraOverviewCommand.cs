using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>全览指令类型</summary>
public enum CameraOverviewCommandType
{
    [LabelText("起始位置")] StartPosition,
    [LabelText("方向移动")] MoveDirection,
    [LabelText("停留")] Stay
}

/// <summary>
/// 全览执行指令 —— 起始位置 / 朝某方向移动一段距离 / 停留一段时间，按类型显示对应字段。
/// </summary>
[System.Serializable]
public class CameraOverviewCommand
{
    [Title("指令类型")]
    [EnumToggleButtons]
    [SerializeField]
    private CameraOverviewCommandType _type = CameraOverviewCommandType.StartPosition;

    [Title("起始位置")]
    [SerializeField, LabelText("起始位置"), ShowIf(nameof(_type), CameraOverviewCommandType.StartPosition)]
    private Vector2 _startPosition = Vector2.zero;

    [SerializeField, LabelText("移动速度"), MinValue(0.01f), SuffixLabel("unit/s", Overlay = true), ShowIf(nameof(_type), CameraOverviewCommandType.StartPosition)]
    private float _startSpeed = 5f;

    [Title("方向移动")]
    [SerializeField, LabelText("方向"), ShowIf(nameof(_type), CameraOverviewCommandType.MoveDirection)]
    private Vector2 _moveDirection = Vector2.right;

    [SerializeField, LabelText("移动距离"), MinValue(0f), SuffixLabel("unit", Overlay = true), ShowIf(nameof(_type), CameraOverviewCommandType.MoveDirection)]
    private float _moveDistance = 5f;

    [SerializeField, LabelText("移动速度"), MinValue(0.01f), SuffixLabel("unit/s", Overlay = true), ShowIf(nameof(_type), CameraOverviewCommandType.MoveDirection)]
    private float _moveSpeed = 2f;

    [Title("停留")]
    [SerializeField, LabelText("停留时间"), MinValue(0f), SuffixLabel("s", Overlay = true), ShowIf(nameof(_type), CameraOverviewCommandType.Stay)]
    private float _stayTime = 1f;

    #region 属性

    public CameraOverviewCommandType Type => _type;
    public Vector2 StartPosition => _startPosition;
    public float StartSpeed => _startSpeed;
    public Vector2 MoveDirection => _moveDirection;
    public float MoveDistance => _moveDistance;
    public float MoveSpeed => _moveSpeed;
    public float StayTime => _stayTime;

    #endregion
}
