using UnityEngine;

/// <summary>
/// 泡泡大小控制器 —— 根据自身含氧量自动控制大小
/// 公式：大小 = 参数 * 含氧量 + 最小体积
/// </summary>
[RequireComponent(typeof(BubbleBase))]
public class BubbleSizeController : MonoBehaviour
{
    [Header("大小公式：大小 = 参数 * 含氧量 + 最小体积")]
    [SerializeField] private float sizeParameter = 0.1f;
    [SerializeField] private float minVolume = 0.5f;

    private BubbleBase bubbleBase;
    private float lastSize = -1f;

    private void Awake()
    {
        bubbleBase = GetComponent<BubbleBase>();
    }

    private void Start()
    {
        ApplySize();
    }

    private void Update()
    {
        ApplySize();
    }

    private void ApplySize()
    {
        if (bubbleBase == null)
            bubbleBase = GetComponent<BubbleBase>();
        if (bubbleBase == null) return;

        float size = sizeParameter * bubbleBase.oxygen + minVolume;
        if (Mathf.Abs(size - lastSize) < 0.0001f) return;
        lastSize = size;
        transform.localScale = new Vector3(size, size, 1f);
    }
}
