using UnityEngine;

/// <summary>
/// 小鱼锁定范围光源 —— 小鱼发现/锁定/破坏气泡期间显示检测范围光圈，
/// 尺寸自动跟随 SmallFishController 的 detectRadius，漫游时隐藏
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SmallFishLockRangeController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private SmallFishController fish;
    private float halfSpriteSize;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fish = GetComponentInParent<SmallFishController>();
        halfSpriteSize = spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds.extents.x : 1f;
        spriteRenderer.enabled = false;
    }

    private void Update()
    {
        if (fish == null) return;

        bool locked = fish.CurrentState != SmallFishState.Patrol;
        if (spriteRenderer.enabled != locked)
            spriteRenderer.enabled = locked;

        if (locked)
        {
            float scale = fish.DetectRadius / Mathf.Max(halfSpriteSize, 0.001f);
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
