using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 摄像机全览数据 —— 记录全览时依次执行的指令列表。
/// </summary>
[System.Serializable]
public class CameraOverviewData
{
    [Title("指令列表")]
    [SerializeField, LabelText("执行指令")]
    private List<CameraOverviewCommand> _commands = new List<CameraOverviewCommand>();

    #region 属性

    public IReadOnlyList<CameraOverviewCommand> Commands => _commands;

    #endregion
}
