using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 木箱控制器 —— 注册到木箱列表，供其他系统查询
/// </summary>
public class WoodBoxController : MonoBehaviour
{
    private static readonly string ListKey = "WoodBoxList";

    private void OnEnable()
    {
        // 注册到木箱列表
        if (!InformationPool.TryGet(ListKey, out List<WoodBoxController> list) || list == null)
        {
            list = new List<WoodBoxController>();
            InformationPool.Set(ListKey, list);
        }
        if (!list.Contains(this))
            list.Add(this);

        // 兼容单引用方式
        InformationPool.Set("WoodBox", this);
    }

    private void OnDisable()
    {
        // 从列表移除
        if (InformationPool.TryGet(ListKey, out List<WoodBoxController> list) && list != null)
        {
            list.Remove(this);
            if (list.Count == 0)
                InformationPool.Remove(ListKey);
        }

        // 只有当前是"WoodBox"指向自己时才移除
        if (InformationPool.TryGet("WoodBox", out object obj) && ReferenceEquals(obj, this))
            InformationPool.Remove("WoodBox");
    }
}
