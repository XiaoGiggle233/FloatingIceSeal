using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 挂在 FlowingWatter(TileMap 水体)上：按流动方向在水体 tile 覆盖范围内自动生成水流效果，
/// 并让流速跟随推力大小
/// </summary>
[RequireComponent(typeof(FlowingWatter))]
public class FlowingWatterEffectController : MonoBehaviour
{
    private static readonly int FlowSpeedId = Shader.PropertyToID("_FlowSpeed");

    [Tooltip("流速 = 推力大小 * 该系数")]
    [SerializeField] private float flowFactor = 0.1f;

    [Tooltip("水流效果预制体，运行时自动生成，覆盖所有非空水体 tile")]
    [SerializeField] private GameObject effectPrefab;

    [Tooltip("生成面的 SortingOrder（水体 tilemap 为 -1，角色为 8，默认 7 在水体之上、角色之下）")]
    [SerializeField] private int sortingOrder = 7;

    [Tooltip("水流区域在 tile 覆盖范围外，每条边向外延伸的距离")]
    [SerializeField] private float extendDistance = 0.1f;

    private FlowingWatter flowingWatter;
    private Tilemap tilemap;
    private Transform effectTransform;
    private Material effectMaterial;
    private Vector3 effectSize;
    private FlowDirection currentDirection = FlowDirection.None;

    private void OnEnable()
    {
        flowingWatter = GetComponent<FlowingWatter>();
        if (flowingWatter == null)
            flowingWatter = GetComponentInParent<FlowingWatter>();
        tilemap = GetComponent<Tilemap>();

        RefreshEffect();
    }

    private void Update()
    {
        if (flowingWatter == null) return;

        if (flowingWatter.FlowDirection != currentDirection)
            RefreshEffect();

        if (effectMaterial != null)
            effectMaterial.SetFloat(FlowSpeedId, flowingWatter.ForceAmount * flowFactor);
    }

    private void RefreshEffect()
    {
        if (flowingWatter == null) return;
        currentDirection = flowingWatter.FlowDirection;

        if (currentDirection == FlowDirection.None)
        {
            DestroyEffect();
            return;
        }

        if (effectTransform == null)
            CreateEffect();
        if (effectTransform == null)
            return;

        // 水平流动时交换宽高，保证旋转后仍完全覆盖所有 tile
        bool horizontal = currentDirection == FlowDirection.Left || currentDirection == FlowDirection.Right;
        effectTransform.localScale = horizontal
            ? new Vector3(effectSize.y, effectSize.x, 1f)
            : effectSize;
        effectTransform.localRotation = Quaternion.Euler(0f, 0f, GetEffectAngle(currentDirection));
    }

    private void CreateEffect()
    {
        if (effectPrefab == null || tilemap == null) return;

        Bounds? bounds = CalculateTilesBounds();
        if (!bounds.HasValue) return;

        // 在 tile 覆盖范围外各边延伸 extendDistance
        Bounds effectBounds = bounds.Value;
        effectBounds.Expand(extendDistance * 2f);

        GameObject effect = Instantiate(effectPrefab, transform);
        effectTransform = effect.transform;
        effectSize = effectBounds.size;

        Vector3 localPos = transform.InverseTransformPoint(effectBounds.center);
        localPos.z = 0f;
        effectTransform.localPosition = localPos;

        Renderer effectRenderer = effect.GetComponent<Renderer>();
        if (effectRenderer != null)
        {
            effectRenderer.sortingOrder = sortingOrder;
            effectMaterial = effectRenderer.material;
        }
    }

    private void DestroyEffect()
    {
        if (effectTransform != null)
            Destroy(effectTransform.gameObject);
        effectTransform = null;
        effectMaterial = null;
    }

    /// <summary>
    /// 遍历 tilemap 所有非空 tile，计算能完全覆盖它们的世界空间包围盒
    /// </summary>
    private Bounds? CalculateTilesBounds()
    {
        Bounds? result = null;
        Vector3 cellSize = tilemap.cellSize;

        foreach (Vector3Int cell in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cell)) continue;

            Matrix4x4 matrix = tilemap.GetTransformMatrix(cell);
            Vector3 half = new Vector3(
                Mathf.Abs(matrix.m00) * cellSize.x + Mathf.Abs(matrix.m01) * cellSize.y,
                Mathf.Abs(matrix.m10) * cellSize.x + Mathf.Abs(matrix.m11) * cellSize.y,
                0f) * 0.5f;

            Bounds tileBounds = new Bounds(tilemap.GetCellCenterWorld(cell), half * 2f);

            if (!result.HasValue)
            {
                result = tileBounds;
            }
            else
            {
                Bounds merged = result.Value;
                merged.SetMinMax(
                    Vector3.Min(merged.min, tileBounds.min),
                    Vector3.Max(merged.max, tileBounds.max));
                result = merged;
            }
        }

        return result;
    }

    /// <summary>
    /// 水流方向 → 效果面片 Z 轴旋转角：向下 0°，向右 90°，向上 180°，向左 270°
    /// </summary>
    private static float GetEffectAngle(FlowDirection direction)
    {
        switch (direction)
        {
            case FlowDirection.Right: return 90f;
            case FlowDirection.Up:    return 180f;
            case FlowDirection.Left:  return 270f;
            default:                  return 0f;
        }
    }
}
