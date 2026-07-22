using System.Collections.Generic;
using UnityEngine;

public class FloatingIceController : MonoBehaviour
{
    private static readonly string ListKey = "FloatingIceList";

    private void OnEnable()
    {
        if (!InformationPool.TryGet(ListKey, out List<FloatingIceController> list) || list == null)
        {
            list = new List<FloatingIceController>();
            InformationPool.Set(ListKey, list);
        }
        if (!list.Contains(this))
            list.Add(this);

        InformationPool.Set("FloatingIce", this);
    }

    private void OnDisable()
    {
        if (InformationPool.TryGet(ListKey, out List<FloatingIceController> list) && list != null)
        {
            list.Remove(this);
            if (list.Count == 0)
                InformationPool.Remove(ListKey);
        }

        if (InformationPool.TryGet("FloatingIce", out object obj) && ReferenceEquals(obj, this))
            InformationPool.Remove("FloatingIce");
    }
}
