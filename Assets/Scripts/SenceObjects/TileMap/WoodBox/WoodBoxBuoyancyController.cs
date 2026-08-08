using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 木箱浮力控制器 —— 在水中受浮力自动上浮至水面，到达后固定在水面
/// 目标位置基于木箱 tile 的包围盒计算（tile 底部贴水面），而非 Tilemap 物体本身
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class WoodBoxBuoyancyController : MonoBehaviour
{
    [Header("浮力设置")]
    [SerializeField] private float buoyancyForce = 10f;

    [Header("物理参数")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float linearDrag = 1f;

    [Header("归位阈值")]
    [SerializeField] private float snapDistance = 0.05f;

    private Rigidbody2D rb;
    private Tilemap tilemap;

    // tile 包围盒相对 transform 的偏移（用于对齐水面）
    private float tileCenterXOffset;
    private float tileBottomOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        tilemap = GetComponent<Tilemap>();
    }

    private void Start()
    {
        rb.mass = mass;
        rb.drag = linearDrag;
        CacheTileGeometry();
    }

    /// <summary>遍历非空 tile 计算包围盒，缓存中心 X 与底部 Y 相对 transform 的偏移</summary>
    private void CacheTileGeometry()
    {
        if (tilemap == null) return;

        BoundsInt bounds = tilemap.cellBounds;
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue;
        bool found = false;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (!tilemap.HasTile(cellPos)) continue;

                Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
                minX = Mathf.Min(minX, worldPos.x);
                maxX = Mathf.Max(maxX, worldPos.x);
                minY = Mathf.Min(minY, worldPos.y);
                found = true;
            }
        }

        if (!found) return;

        tileCenterXOffset = (minX + maxX) * 0.5f - transform.position.x;
        tileBottomOffset = minY - tilemap.cellSize.y * 0.5f - transform.position.y;
    }

    private void FixedUpdate()
    {
        // 用 tile 中心 X 检测水面
        float centerX = rb.position.x + tileCenterXOffset;
        float waterSurfaceY = WatterUtils.GetWaterSurfaceY(centerX);
        if (waterSurfaceY == float.MinValue) return;

        Vector2 pos = rb.position;
        // 目标：tile 底部正好贴住水面
        float targetY = waterSurfaceY - tileBottomOffset;

        // 已在水面 → 固定
        if (Mathf.Abs(pos.y - targetY) < snapDistance)
        {
            rb.MovePosition(new Vector2(pos.x, targetY));
            rb.velocity = Vector2.zero;
            return;
        }

        // 在水下 → 施加浮力上浮
        if (pos.y < targetY)
        {
            rb.AddForce(Vector2.up * buoyancyForce, ForceMode2D.Force);
        }
    }
}
