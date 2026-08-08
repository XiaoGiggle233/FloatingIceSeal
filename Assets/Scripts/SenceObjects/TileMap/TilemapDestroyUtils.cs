using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Tilemap 破坏工具集 —— 爆炸时仅移除范围内瓦片，不破坏整个瓦片地图
/// </summary>
public static class TilemapDestroyUtils
{
    /// <summary>移除指定中心、半径范围内的瓦片（保留范围外瓦片）</summary>
    public static void DestroyTilesInRadius(Tilemap tilemap, Vector2 center, float radius)
    {
        if (tilemap == null) return;

        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (!tilemap.HasTile(cell)) continue;

                Vector3 worldPos = tilemap.GetCellCenterWorld(cell);
                if (Vector2.Distance(worldPos, center) <= radius)
                    tilemap.SetTile(cell, null);
            }
        }
    }

    /// <summary>判断 tilemap 是否还有剩余瓦片</summary>
    public static bool HasAnyTile(Tilemap tilemap)
    {
        if (tilemap == null) return false;

        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                if (tilemap.HasTile(new Vector3Int(x, y, 0))) return true;
            }
        }
        return false;
    }
}
