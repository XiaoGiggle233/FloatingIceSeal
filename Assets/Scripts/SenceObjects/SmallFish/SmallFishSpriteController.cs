using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 小鱼精灵控制器 —— 按状态机状态切换自定义 Sprite，可按移动方向左右翻转（可配置）
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SmallFishSpriteController : MonoBehaviour
{
    [FoldoutGroup("状态精灵", expanded: true)]
    [PreviewField(60), LabelText("巡逻")]
    [SerializeField] private Sprite patrolSprite;

    [FoldoutGroup("状态精灵")]
    [PreviewField(60), LabelText("等待")]
    [SerializeField] private Sprite waitSprite;

    [FoldoutGroup("状态精灵")]
    [PreviewField(60), LabelText("追击")]
    [SerializeField] private Sprite chaseSprite;

    [FoldoutGroup("状态精灵")]
    [PreviewField(60), LabelText("破坏")]
    [SerializeField] private Sprite breakSprite;

    [FoldoutGroup("翻转", expanded: true)]
    [LabelText("反转所有精灵朝向")]
    [SerializeField] private bool flipAllSprites = false;

    private SpriteRenderer spriteRenderer;
    private SmallFishController fish;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fish = GetComponent<SmallFishController>();
    }

    private void Update()
    {
        if (fish == null) return;

        // 按状态机状态切换 sprite
        Sprite target = GetStateSprite(fish.CurrentState);
        if (target != null)
            spriteRenderer.sprite = target;

        // 按移动方向左右翻转（始终生效）；勾选"反转所有精灵朝向"时统一取反
        Vector2 dir = fish.MoveDirection;
        if (Mathf.Abs(dir.x) > 0.01f)
        {
            bool flip = dir.x < 0f;
            if (flipAllSprites)
                flip = !flip;
            spriteRenderer.flipX = flip;
        }
    }

    private Sprite GetStateSprite(SmallFishState state)
    {
        return state switch
        {
            SmallFishState.Patrol => patrolSprite,
            SmallFishState.Wait => waitSprite,
            SmallFishState.Chase => chaseSprite,
            SmallFishState.Break => breakSprite,
            _ => null
        };
    }
}
