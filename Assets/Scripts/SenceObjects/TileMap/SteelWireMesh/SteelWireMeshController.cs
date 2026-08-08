using UnityEngine;
using UnityEngine.Tilemaps;

public class SteelWireMeshController : MonoBehaviour, IDestroyable
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
}
