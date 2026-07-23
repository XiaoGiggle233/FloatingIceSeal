using UnityEngine;

/// <summary>水体检测工具集 —— 向下射线检测水面</summary>
public static class WatterUtils
{
    /// <summary>水体的 LayerMask（缓存）</summary>
    private static LayerMask? _waterLayerMask;
    public static LayerMask WaterLayerMask
    {
        get
        {
            if (_waterLayerMask == null)
                _waterLayerMask = LayerMask.GetMask("Watter");
            return _waterLayerMask.Value;
        }
    }

    /// <summary>从指定位置向下发射射线检测水面，返回命中结果</summary>
    public static RaycastHit2D RaycastToWater(Vector2 origin, float maxDistance = Mathf.Infinity)
    {
        return Physics2D.Raycast(origin, Vector2.down, maxDistance, WaterLayerMask);
    }

    /// <summary>检查指定位置下方指定距离内是否有水面</summary>
    public static bool HasWaterBelow(Vector2 origin, float maxDistance)
    {
        return RaycastToWater(origin, maxDistance).collider != null;
    }

    /// <summary>获取指定 X 坐标处的水面 Y 值，若未命中则返回 float.MinValue</summary>
    public static float GetWaterSurfaceY(float x)
    {
        // 从足够高处向下发射射线
        Vector2 origin = new Vector2(x, 9999f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, Mathf.Infinity, WaterLayerMask);
        return hit.collider != null ? hit.point.y : float.MinValue;
    }
}
