using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 水雷控制器 —— 注册到水雷列表，供其他系统查询
/// </summary>
public class MineController : MonoBehaviour, ILevelResetable
{
    private static readonly string ListKey = "MineList";

    private void OnEnable()
    {
        // 注册到水雷列表
        if (!InformationPool.TryGet(ListKey, out List<MineController> list) || list == null)
        {
            list = new List<MineController>();
            InformationPool.Set(ListKey, list);
        }
        if (!list.Contains(this))
            list.Add(this);

        // 兼容单引用方式
        InformationPool.Set("Mine", this);
    }

    private void OnDisable()
    {
        // 从列表移除
        if (InformationPool.TryGet(ListKey, out List<MineController> list) && list != null)
        {
            list.Remove(this);
            if (list.Count == 0)
                InformationPool.Remove(ListKey);
        }

        // 只有当前是"Mine"指向自己时才移除
        if (InformationPool.TryGet("Mine", out object obj) && ReferenceEquals(obj, this))
            InformationPool.Remove("Mine");
    }

    /// <summary>关卡恢复完成回调（位置由 LevelResetSystem 恢复）</summary>
    public void OnLevelRestore()
    {
    }
}
