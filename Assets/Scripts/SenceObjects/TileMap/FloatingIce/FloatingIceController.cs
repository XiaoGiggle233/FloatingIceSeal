using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloatingIceController : MonoBehaviour, IDestroyable, ILevelResetable
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

    /// <summary>被爆炸摧毁（移除爆炸范围内瓦片）</summary>
    public void DestroyByExplosion(Vector2 explosionCenter, float explosionRadius)
    {
        TilemapDestroyUtils.DestroyTilesInRadius(GetComponent<Tilemap>(), explosionCenter, explosionRadius);
    }

    /// <summary>关卡恢复完成回调（位置由 LevelResetSystem 恢复）</summary>
    public void OnLevelRestore()
    {
    }
}
