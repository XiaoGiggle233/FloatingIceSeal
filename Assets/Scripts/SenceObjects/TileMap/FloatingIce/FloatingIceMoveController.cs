using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class FloatingIceMoveController : MonoBehaviour
{
    [Title("浮冰运动参数")]
    [LabelText("露出距离")]
    [PropertyRange(0f, 5f)]
    [SerializeField, Tooltip("物体露出水面的距离超过此值时，开始受重力影响")]
    private float exposureDistance = 0.5f;

    [LabelText("重力系数")]
    [PropertyRange(0f, 3f)]
    [SerializeField, Tooltip("受重力影响时的重力系数")]
    private float gravityScale = 1f;

    private Rigidbody2D rb;
    private Tilemap tilemap;
    private Vector3Int lowestCellPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        tilemap = GetComponent<Tilemap>();
        CacheLowestTile();
    }

    void Update()
    {
        if (tilemap == null) return;

        Vector3 bottomPos = tilemap.GetCellCenterWorld(lowestCellPos);
        float waterSurfaceY = WatterUtils.GetWaterSurfaceY(bottomPos.x);
        float aboveWater = bottomPos.y - waterSurfaceY;

        if (aboveWater > exposureDistance)
            rb.gravityScale = gravityScale;
        else
            rb.gravityScale = 0f;
    }

    private void CacheLowestTile()
    {
        if (tilemap == null) return;

        BoundsInt bounds = tilemap.cellBounds;
        int lowestY = int.MaxValue;
        int lowestX = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(cellPos) && y < lowestY)
                {
                    lowestY = y;
                    lowestX = x;
                }
            }
        }

        lowestCellPos = new Vector3Int(lowestX, lowestY, 0);
    }
}
