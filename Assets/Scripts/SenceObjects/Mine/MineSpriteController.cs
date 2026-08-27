using UnityEngine;

/// <summary>
/// 水雷精灵控制器 —— 负责水雷精灵的显示与爆炸特效切换
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class MineSpriteController : MonoBehaviour
{
    private const string ExplosionSpritePath = "Images/爆炸特效";

    private static Sprite explosionSprite;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (explosionSprite == null)
        {
            explosionSprite = Resources.Load<Sprite>(ExplosionSpritePath);
            if (explosionSprite == null)
                Debug.LogWarning($"[MineSpriteController] 未找到爆炸特效：Resources/{ExplosionSpritePath}");
        }
    }

    /// <summary>显示爆炸特效精灵</summary>
    public void ShowExplosion()
    {
        if (explosionSprite != null)
            spriteRenderer.sprite = explosionSprite;
    }
}
