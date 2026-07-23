using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 视差图层组件 —— 挂载到每个背景层GameObject上
/// 
/// 视差系数说明：
///   parallaxFactor ＜ 0 : 朝相机反方向移动（超远景，比跟随镜头更"远"）
///   parallaxFactor = 0 : 远景（完全跟随镜头，屏幕上位置不变，如天空、远山）
///   parallaxFactor = 1 : 近景（固定在世界中，镜头移动时产生完整相对位移，如前景物体）
///   parallaxFactor ＞ 1 : 比相机移动更快（超近景，产生强烈的透视速度感）
/// 
/// 平铺模式：
///   启用 enableTiling 后，图层会无限循环平铺，适用于需要重复的地面、水面等。
///   需确保 Sprite 的导入设置中 Wrap Mode 设为 "Repeat"。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxLayer : MonoBehaviour
{
    [Header("视差设置")]
    [SerializeField, Tooltip("视差系数：0=跟随镜头, 1=固定在世界, <0=反向移动, >1=比镜头更快")]
    private float parallaxFactor = 0.5f;

    [Header("平铺设置")]
    [SerializeField, Tooltip("是否启用无限平铺（背景需要循环重复时启用）")]
    private bool enableTiling;

    [SerializeField, ShowIf(nameof(enableTiling)), Min(0.01f), Tooltip("单块Tile的世界宽度（0=自动从Sprite尺寸获取）")]
    private float overrideTileWidth;

    // 初始世界位置
    private Vector3 initialPosition;

    // 组件缓存
    private SpriteRenderer spriteRenderer;
    private ParallaxController controller;

    // 平铺相关
    private float tileWidth;
    private float tiledAreaWidth;

    // 防止重复初始化
    private bool isInitialized;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (Application.isPlaying && !isInitialized)
            Initialize();
    }

    private void OnEnable()
    {
        if (Application.isPlaying && !isInitialized)
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            Initialize();
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
            UnregisterFromController();
    }

    private void OnDestroy()
    {
        UnregisterFromController();
    }

    #region 初始化

    private void Initialize()
    {
        isInitialized = true;
        initialPosition = transform.position;
        controller = FindOrCreateController();

        if (controller != null)
            controller.Register(this);

        if (enableTiling)
            SetupTiling();
    }

    private void UnregisterFromController()
    {
        if (controller != null)
            controller.Unregister(this);

        isInitialized = false;
    }

    private void SetupTiling()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"[ParallaxLayer] {gameObject.name}: 无法设置平铺，Sprite为空");
            return;
        }

        // 设置绘制模式为Tiled
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;

        // 确定tile宽度
        if (overrideTileWidth > 0.01f)
        {
            tileWidth = overrideTileWidth;
        }
        else
        {
            tileWidth = spriteRenderer.sprite.bounds.size.x;
        }

        // 计算平铺区域：屏幕宽度 + 两侧各一个tile的缓冲
        float viewWidth = controller != null ? controller.GetCameraViewWidth() : 20f;
        tiledAreaWidth = viewWidth + tileWidth * 2f;

        // 设置SpriteRenderer的平铺尺寸
        Vector2 newSize = spriteRenderer.size;
        newSize.x = tiledAreaWidth;
        spriteRenderer.size = newSize;

        // 将平铺区域居中（从Transform位置向两侧延伸）
        // SpriteRenderer在Tiled模式下以pivot为中心平铺，无需额外偏移
    }

    private ParallaxController FindOrCreateController()
    {
        ParallaxController ctrl = FindObjectOfType<ParallaxController>();
        if (ctrl == null)
        {
            GameObject go = new GameObject("[Auto] ParallaxController");
            ctrl = go.AddComponent<ParallaxController>();
        }
        return ctrl;
    }

    #endregion

    #region 视差更新

    /// <summary>
    /// 由ParallaxController在LateUpdate中调用，更新本层的视差位置
    /// </summary>
    /// <param name="referenceDelta">参考对象（相机）自初始位置以来的累计位移</param>
    public void UpdateParallax(Vector3 referenceDelta)
    {
        float offsetX = referenceDelta.x * parallaxFactor;

        Vector3 newPos = initialPosition;
        newPos.x = initialPosition.x + offsetX;

        if (enableTiling && tileWidth > 0f)
        {
            // 对平铺层做偏移值取模：利用平铺图案的周期性实现无缝循环
            // Mathf.Repeat保证偏移值始终在 [0, tileWidth) 范围内
            newPos.x = initialPosition.x + Mathf.Repeat(offsetX, tileWidth);
        }

        transform.position = newPos;
    }

    #endregion

    #region 公开接口

    /// <summary>获取/设置视差系数（无范围限制，支持负值和>1的值）</summary>
    public float ParallaxFactor
    {
        get => parallaxFactor;
        set => parallaxFactor = value;
    }

    /// <summary>是否启用平铺</summary>
    public bool EnableTiling
    {
        get => enableTiling;
        set
        {
            enableTiling = value;
            if (value && Application.isPlaying)
                SetupTiling();
        }
    }

    /// <summary>获取当前Tile宽度</summary>
    public float GetTileWidth() => tileWidth;

    /// <summary>重新记录初始位置（手动调整图层位置后调用）</summary>
    public void RecalibratePosition()
    {
        initialPosition = transform.position;
    }

    /// <summary>获取初始世界位置</summary>
    public Vector3 GetInitialPosition() => initialPosition;

    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (enableTiling && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            if (spriteRenderer.sprite.texture != null &&
                spriteRenderer.sprite.texture.wrapMode != TextureWrapMode.Repeat)
            {
                Debug.LogWarning(
                    $"[ParallaxLayer] {gameObject.name}: 已启用平铺，但Sprite '{spriteRenderer.sprite.name}' " +
                    $"的WrapMode不是Repeat，平铺可能显示异常。请在Sprite导入设置中将Wrap Mode改为Repeat。",
                    this);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!enableTiling || spriteRenderer == null || spriteRenderer.sprite == null) return;

        Gizmos.color = Color.cyan;
        Vector3 pos = transform.position;
        float halfW = tiledAreaWidth > 0 ? tiledAreaWidth * 0.5f : 10f;
        float h = spriteRenderer.sprite.bounds.size.y * 0.5f;

        Gizmos.DrawWireCube(pos, new Vector3(tiledAreaWidth, h * 2f, 0.01f));
    }
#endif
}
