using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 气泡柱动画控制器 —— 循环播放指定 Resources 文件夹中的序列帧（按名称 1→2→3→4→5 顺序）
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BubblePlumeSpriteController : MonoBehaviour
{
    private static readonly Dictionary<string, Sprite[]> FrameCache = new Dictionary<string, Sprite[]>();

    [FoldoutGroup("动画", expanded: true)]
    [LabelText("动画帧文件夹（Resources 相对路径）")]
    [SerializeField] private string animFolder = "Images/气泡动画（四帧";

    [FoldoutGroup("动画")]
    [LabelText("帧率（帧/秒）"), MinValue(0.1f)]
    [SerializeField] private float fps = 4f;

    private SpriteRenderer spriteRenderer;
    private Sprite[] currentFrames;
    private int currentFrameIndex;
    private float frameTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentFrames = LoadFrames(animFolder);
    }

    private void Start()
    {
        ApplyFrame();
    }

    private void Update()
    {
        if (currentFrames == null || currentFrames.Length == 0) return;

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / fps;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrameIndex = (currentFrameIndex + 1) % currentFrames.Length;
        }

        ApplyFrame();
    }

    private void ApplyFrame()
    {
        if (currentFrames == null || currentFrames.Length == 0) return;
        spriteRenderer.sprite = currentFrames[currentFrameIndex];
    }

    private static Sprite[] LoadFrames(string folder)
    {
        if (FrameCache.TryGetValue(folder, out Sprite[] cached))
            return cached;

        Sprite[] sprites = Resources.LoadAll<Sprite>(folder);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[BubblePlumeSpriteController] Resources/{folder} 下未找到精灵帧，请检查文件夹路径");
            return null;
        }

        System.Array.Sort(sprites, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        FrameCache[folder] = sprites;
        return sprites;
    }
}
