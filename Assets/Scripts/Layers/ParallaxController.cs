using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 视差滚动控制器 —— 管理所有视差图层，根据相机位移统一更新各层位置
/// 
/// 使用方式：
///   1. 将此组件挂载到场景中的任意GameObject上（或由ParallaxLayer自动创建）
///   2. 指定Reference Transform（默认为主相机）
///   3. 各图层挂载ParallaxLayer组件即可自动注册
/// </summary>
public class ParallaxController : MonoBehaviour
{
    public static ParallaxController Instance { get; private set; }
    [Header("参考对象")]
    [SerializeField, Tooltip("视差计算的参考Transform，通常为主相机")]
    private Transform referenceTransform;

    [Header("调试")]
    [SerializeField, Tooltip("在Scene视图中绘制视差信息")]
    private bool showDebugInfo;

    /// <summary>所有已注册的视差图层</summary>
    private readonly List<ParallaxLayer> layers = new List<ParallaxLayer>();

    /// <summary>参考对象的初始位置（用于计算累计位移）</summary>
    private Vector3 referenceStartPosition;

    /// <summary>相机视口在世界空间中的宽度</summary>
    private float cameraViewWidth;

    /// <summary>公开当前累计的相机位移量</summary>
    public Vector3 CurrentDelta { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (referenceTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                referenceTransform = mainCam.transform;
        }
    }

    private void Start()
    {
        if (referenceTransform != null)
            referenceStartPosition = referenceTransform.position;

        Camera cam = referenceTransform != null ? referenceTransform.GetComponent<Camera>() : null;
        if (cam != null && cam.orthographic)
        {
            cameraViewWidth = cam.orthographicSize * cam.aspect * 2f;
        }
        else
        {
            cameraViewWidth = 20f; // 默认值
            Debug.LogWarning("[ParallaxController] 无法获取正交相机，使用默认视口宽度20");
        }
    }

    private void LateUpdate()
    {
        if (referenceTransform == null) return;

        Vector3 delta = referenceTransform.position - referenceStartPosition;
        CurrentDelta = delta;

        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i] != null)
                layers[i].UpdateParallax(delta);
        }
    }

    #region 图层注册/注销

    /// <summary>注册一个视差图层</summary>
    public void Register(ParallaxLayer layer)
    {
        if (layer == null || layers.Contains(layer)) return;
        layers.Add(layer);
    }

    /// <summary>注销一个视差图层</summary>
    public void Unregister(ParallaxLayer layer)
    {
        layers.Remove(layer);
    }

    #endregion

    #region 公开接口

    /// <summary>获取相机视口宽度（世界单位）</summary>
    public float GetCameraViewWidth() => cameraViewWidth;

    /// <summary>重新校准参考起始位置（场景切换/传送后调用）</summary>
    public void RecalibrateReference()
    {
        if (referenceTransform != null)
            referenceStartPosition = referenceTransform.position;
    }

    /// <summary>手动设置参考Transform（用于运行时切换跟踪目标）</summary>
    public void SetReferenceTransform(Transform newRef)
    {
        referenceTransform = newRef;
        if (newRef != null)
            referenceStartPosition = newRef.position;
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugInfo || referenceTransform == null) return;

        Gizmos.color = Color.yellow;
        Vector3 center = referenceTransform.position;
        float height = cameraViewWidth > 0 ? cameraViewWidth * 0.5f : 10f;
        Gizmos.DrawWireCube(center, new Vector3(cameraViewWidth, height, 0.1f));
    }
#endif
}
