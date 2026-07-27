using UnityEngine;

public class BackgroundAnimation : MonoBehaviour
{
    [Header("背景动画参数")]
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0.1f, 0.05f);
    [SerializeField] private Material backgroundMaterial;

    private Vector2 currentOffset;

    private void Awake()
    {
        if (backgroundMaterial == null)
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                backgroundMaterial = renderer.material;
            }
        }
    }

    private void Update()
    {
        if (backgroundMaterial == null) return;

        currentOffset += scrollSpeed * Time.deltaTime;
        backgroundMaterial.mainTextureOffset = currentOffset;
    }
}
