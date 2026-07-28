using System.Collections.Generic;
using UnityEngine;

public class FloatingIceController : MonoBehaviour
{
    private static readonly string ListKey = "FloatingIceList";

    private void OnEnable()
    {
        // 注册到浮冰列表
        if (!InformationPool.TryGet(ListKey, out List<FloatingIceController> list) || list == null)
        {
            list = new List<FloatingIceController>();
            InformationPool.Set(ListKey, list);
        }
        if (!list.Contains(this))
            list.Add(this);

        // 兼容单引用方式
        InformationPool.Set("FloatingIce", this);
    }

    private void OnDisable()
    {
        // 从列表移除
        if (InformationPool.TryGet(ListKey, out List<FloatingIceController> list) && list != null)
        {
            list.Remove(this);
            if (list.Count == 0)
                InformationPool.Remove(ListKey);
        }

        // 只有当前是"FloatingIce"指向自己时才移除
        if (InformationPool.TryGet("FloatingIce", out object obj) && ReferenceEquals(obj, this))
            InformationPool.Remove("FloatingIce");
    }
}
