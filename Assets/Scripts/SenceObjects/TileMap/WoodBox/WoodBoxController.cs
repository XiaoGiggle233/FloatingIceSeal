using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 木箱控制器 —— 注册到木箱列表，供其他系统查询
/// </summary>
public class WoodBoxController : MonoBehaviour, IDestroyable, ILevelResetable
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

    /// <summary>被爆炸摧毁（移除爆炸范围内瓦片，并重新计算浮力偏移）</summary>
    public void DestroyByExplosion(Vector2 explosionCenter, float explosionRadius)
    {
        TilemapDestroyUtils.DestroyTilesInRadius(GetComponent<Tilemap>(), explosionCenter, explosionRadius);

        // 瓦片被破坏后重新缓存几何，避免剩余瓦片悬空
        GetComponent<WoodBoxBuoyancyController>()?.RecacheTileGeometry();

        // 瓦片全部被炸毁 → 销毁木箱
        if (!TilemapDestroyUtils.HasAnyTile(GetComponent<Tilemap>()))
            Destroy(gameObject);
    }

    /// <summary>关卡恢复完成回调（瓦片恢复后重算浮力偏移）</summary>
    public void OnLevelRestore()
    {
        GetComponent<WoodBoxBuoyancyController>()?.RecacheTileGeometry();
    }
}
