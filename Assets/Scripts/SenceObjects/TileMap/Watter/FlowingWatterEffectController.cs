using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 挂在 FlowingWatter(TileMap 水体)上：按流动方向在水体 tile 覆盖范围内自动生成水流效果，
/// 并让流速跟随推力大小。仿照 SteelWireMesh 的"tilemap 蒙板 + art 素材"思路：
/// 运行时创建只写模板缓冲的蒙板 tilemap（复制水体瓦片），水流效果仅在有瓦片的区域显示
/// </summary>
[RequireComponent(typeof(FlowingWatter))]
public class FlowingWatterEffectController : MonoBehaviour
{
    private static readonly int FlowSpeedId = Shader.PropertyToID("_FlowSpeed");
    private static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");

    // 水流专用模板值：与钢网等机制（Ref=8）区分，效果只在父物体水体的瓦片区域显示
    private const int StencilRefValue = 9;

    private const string MaskShaderName = "Custom/TilemapStencilMask";
    private const string MaskShaderResource = "Shaders/TilemapStencilMask";
    private const string EffectMaskedShaderName = "Custom/WindWall2DStencilMasked";
    private const string EffectMaskedShaderResource = "水流效果/WindWall2DStencilMasked";

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
    private FlowDirection currentDirection = FlowDirection.None;

    private Transform maskTransform;
    private Tilemap maskTilemap;
    private Material maskMaterial;

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

        // 方形面片按方向旋转，纹理流向跟随方向
        effectTransform.localRotation = Quaternion.Euler(0f, 0f, GetEffectAngle(currentDirection));
    }

    private void CreateEffect()
    {
        if (effectPrefab == null || tilemap == null) return;

        Bounds? bounds = CalculateTilesBounds();
        if (!bounds.HasValue) return;

        // 蒙板 tilemap 只写模板缓冲不显示颜色，效果仅在有水体瓦片的格子显示
        CreateMaskTilemap();

        // 在 tile 覆盖范围外各边延伸 extendDistance
        Bounds effectBounds = bounds.Value;
        effectBounds.Expand(extendDistance * 2f);

        GameObject effect = Instantiate(effectPrefab, transform);
        effectTransform = effect.transform;

        Vector3 localPos = transform.InverseTransformPoint(effectBounds.center);
        localPos.z = 0f;
        effectTransform.localPosition = localPos;
        // 面片放大到足够覆盖全部 tile，实际显示范围由模板蒙板裁剪
        effectTransform.localScale = new Vector3(100f, 100f, 1f);

        Renderer effectRenderer = effect.GetComponent<Renderer>();
        if (effectRenderer != null)
        {
            effectRenderer.sortingOrder = sortingOrder;

            // 保留原 shader 全部显示参数，仅叠加模板蒙板裁剪
            Material sourceMaterial = effectRenderer.material;
            Material masked = CreateMaskedMaterial(sourceMaterial);
            if (masked != null)
            {
                effectRenderer.material = masked;
                effectMaterial = masked;
                Destroy(sourceMaterial);
            }
            else
            {
                effectMaterial = sourceMaterial;
            }
        }
    }

    /// <summary>用原 shader 的全部参数创建带模板蒙板的水流材质（蒙板外不显示）</summary>
    private static Material CreateMaskedMaterial(Material source)
    {
        if (source == null) return null;

        Shader shader = Resources.Load<Shader>(EffectMaskedShaderResource);
        if (shader == null)
            shader = Shader.Find(EffectMaskedShaderName);
        if (shader == null) return null;

        Material masked = new Material(shader);
        masked.CopyPropertiesFromMaterial(source);
        // 强制与蒙板 tilemap 使用相同模板值（Copy 后设置，避免被源材质属性覆盖）
        masked.SetInt(StencilRefId, StencilRefValue);
        return masked;
    }

    /// <summary>创建只写模板缓冲的蒙板 tilemap：复制水体瓦片，保证效果仅在瓦片区域显示</summary>
    private void CreateMaskTilemap()
    {
        if (maskTransform != null || tilemap == null) return;

        Shader shader = Resources.Load<Shader>(MaskShaderResource);
        if (shader == null)
            shader = Shader.Find(MaskShaderName);
        if (shader == null) return;

        GameObject maskGo = new GameObject("FlowingWaterEffectMask");
        maskTransform = maskGo.transform;
        maskTransform.SetParent(transform, false);

        maskTilemap = maskGo.AddComponent<Tilemap>();
        maskTilemap.tileAnchor = tilemap.tileAnchor;

        TilemapRenderer maskRenderer = maskGo.AddComponent<TilemapRenderer>();
        maskMaterial = new Material(shader);
        maskMaterial.SetInt(StencilRefId, StencilRefValue);
        maskRenderer.material = maskMaterial;
        // 先于水流效果面片渲染，保证模板缓冲先写好
        maskRenderer.sortingOrder = sortingOrder - 1;

        // 复制水体瓦片：蒙板 shader 只写模板缓冲，不改变瓦片颜色显示
        BoundsInt bounds = tilemap.cellBounds;
        maskTilemap.SetTilesBlock(bounds, tilemap.GetTilesBlock(bounds));
    }

    private void DestroyEffect()
    {
        if (effectTransform != null)
            Destroy(effectTransform.gameObject);
        effectTransform = null;
        if (effectMaterial != null)
            Destroy(effectMaterial);
        effectMaterial = null;

        if (maskTransform != null)
            Destroy(maskTransform.gameObject);
        maskTransform = null;
        maskTilemap = null;
        if (maskMaterial != null)
            Destroy(maskMaterial);
        maskMaterial = null;
    }

    /// <summary>
    /// 遍历 tilemap 所有非空 tile，计算能完全覆盖它们的世界空间包围盒。
    /// 按瓦片精灵的实际绘制尺寸（而非格子尺寸）计算，避免波浪出血边盖不住
    /// </summary>
    private Bounds? CalculateTilesBounds()
    {
        Bounds? result = null;

        foreach (Vector3Int cell in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cell)) continue;

            // 瓦片渲染 quad 尺寸 = 精灵世界尺寸（textureRect / pixelsPerUnit），可能大于格子
            Vector2 tileSize = GetTileWorldSize(cell);

            Matrix4x4 matrix = tilemap.GetTransformMatrix(cell);
            Vector3 half = new Vector3(
                Mathf.Abs(matrix.m00) * tileSize.x + Mathf.Abs(matrix.m01) * tileSize.y,
                Mathf.Abs(matrix.m10) * tileSize.x + Mathf.Abs(matrix.m11) * tileSize.y,
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

    /// <summary>瓦片实际绘制的世界尺寸：优先取精灵尺寸，取不到则回退格子尺寸</summary>
    private Vector2 GetTileWorldSize(Vector3Int cell)
    {
        Sprite sprite = tilemap.GetSprite(cell);
        if (sprite != null)
            return sprite.bounds.size;

        Vector3 cellSize = tilemap.cellSize;
        return new Vector2(cellSize.x, cellSize.y);
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
