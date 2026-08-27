using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 小鱼精灵控制器 —— 从 Resources/Images/SmallFish 按状态加载序列帧播放动画，按移动方向左右翻转（可配置）
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SmallFishSpriteController : MonoBehaviour
{
    private const string SwimAnimFolder = "Images/SmallFish/游走";
    private const string EatBubbleAnimFolder = "Images/SmallFish/吃泡泡";
    private const string DeadAnimFolder = "Images/SmallFish/死亡";

    private static readonly Dictionary<string, Sprite[]> FrameCache = new Dictionary<string, Sprite[]>();

    [FoldoutGroup("动画", expanded: true)]
    [LabelText("帧率（帧/秒）"), MinValue(0.1f)]
    [SerializeField] private float fps = 8f;

    [FoldoutGroup("翻转", expanded: true)]
    [LabelText("反转所有精灵朝向")]
    [SerializeField] private bool flipAllSprites = false;

    private SpriteRenderer spriteRenderer;
    private SmallFishController fish;
    private Sprite[] currentFrames;
    private int currentFrameIndex;
    private float frameTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fish = GetComponent<SmallFishController>();
    }

    private void Update()
    {
        if (fish == null) return;

        PlayStateAnimation(GetAnimFolder(fish.CurrentState));

        // 死亡动画播放期间不翻转
        if (fish.CurrentState != SmallFishState.Dead)
        {
            Vector2 dir = fish.MoveDirection;
            if (Mathf.Abs(dir.x) > 0.01f)
            {
                bool flip = dir.x < 0f;
                if (flipAllSprites) flip = !flip;
                spriteRenderer.flipX = flip;
            }
        }
    }

    /// <summary>状态 → 动画文件夹：巡逻/等待/追击共用游走，破坏用吃泡泡，死亡用死亡</summary>
    private static string GetAnimFolder(SmallFishState state)
    {
        return state switch
        {
            SmallFishState.Break => EatBubbleAnimFolder,
            SmallFishState.Dead => DeadAnimFolder,
            _ => SwimAnimFolder
        };
    }

    private void PlayStateAnimation(string folder)
    {
        Sprite[] frames = LoadFrames(folder);
        if (frames == null || frames.Length == 0) return;

        // 切换动画文件夹时重新开始播放
        if (!ReferenceEquals(frames, currentFrames))
        {
            currentFrames = frames;
            currentFrameIndex = 0;
            frameTimer = 0f;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / fps;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            if (currentFrameIndex < frames.Length - 1)
            {
                currentFrameIndex++;
            }
            else if (fish.CurrentState != SmallFishState.Dead)
            {
                currentFrameIndex = 0; // 循环播放
            }
            // 死亡动画停在最后一帧
        }

        spriteRenderer.sprite = frames[currentFrameIndex];
    }

    /// <summary>加载指定 Resources 文件夹下的所有精灵帧（按名称排序后缓存）</summary>
    private static Sprite[] LoadFrames(string folder)
    {
        if (FrameCache.TryGetValue(folder, out Sprite[] cached))
            return cached;

        Sprite[] sprites = Resources.LoadAll<Sprite>(folder);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[SmallFishSpriteController] Resources/{folder} 下未找到精灵帧");
            return null;
        }

        System.Array.Sort(sprites, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        FrameCache[folder] = sprites;
        return sprites;
    }
}
