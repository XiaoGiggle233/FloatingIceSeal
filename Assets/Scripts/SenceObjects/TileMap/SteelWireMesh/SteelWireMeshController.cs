using UnityEngine;
using UnityEngine.Tilemaps;

public class SteelWireMeshController : MonoBehaviour, IDestroyable, ILevelResetable
{
    private void OnEnable()
    {
        InformationPool.Set("SteelWireMesh", this);
    }

    private void OnDisable()
    {
        InformationPool.Remove("SteelWireMesh");
    }

    /// <summary>被爆炸摧毁（移除爆炸范围内瓦片）</summary>
    public void DestroyByExplosion(Vector2 explosionCenter, float explosionRadius)
    {
        TilemapDestroyUtils.DestroyTilesInRadius(GetComponent<Tilemap>(), explosionCenter, explosionRadius);
    }

    /// <summary>关卡恢复完成回调（位置/子物体由 LevelResetSystem 恢复）</summary>
    public void OnLevelRestore()
    {
    }
}
