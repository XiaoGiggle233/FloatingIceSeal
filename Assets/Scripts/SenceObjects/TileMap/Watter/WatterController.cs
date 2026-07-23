using System.Collections.Generic;
using UnityEngine;

public class WatterController : MonoBehaviour
{
    private static readonly string ListKey = "WatterList";

    private void OnEnable()
    {
        // 注册到水体列表
        if (!InformationPool.TryGet(ListKey, out List<WatterController> list) || list == null)
        {
            list = new List<WatterController>();
            InformationPool.Set(ListKey, list);
        }
        if (!list.Contains(this))
            list.Add(this);

        // 兼容旧的单引用方式（BubbleController 的 GameObject 比较等场景）
        InformationPool.Set("Watter", this);
    }

    private void OnDisable()
    {
        // 从列表移除
        if (InformationPool.TryGet(ListKey, out List<WatterController> list) && list != null)
        {
            list.Remove(this);
            if (list.Count == 0)
                InformationPool.Remove(ListKey);
        }

        // 只有当前是"Watter"指向自己时才移除，避免误删其他水体的引用
        if (InformationPool.TryGet("Watter", out object obj) && ReferenceEquals(obj, this))
            InformationPool.Remove("Watter");
    }
}
