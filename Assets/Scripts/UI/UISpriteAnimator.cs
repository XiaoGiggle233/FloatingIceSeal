using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIImage 序列帧动画 —— 按顺序循环切换 Image 的 sprite，可自定义切换间隔
/// </summary>
[RequireComponent(typeof(Image))]
public class UISpriteAnimator : MonoBehaviour
{
    [Tooltip("按播放顺序排列的帧序列")]
    [SerializeField] private Sprite[] frames;

    [Tooltip("每帧切换间隔（秒）")]
    [SerializeField] private float frameInterval = 0.1f;

    [Tooltip("是否循环播放")]
    [SerializeField] private bool loop = true;

    [Tooltip("启用时是否自动从头播放")]
    [SerializeField] private bool playOnEnable = true;

    private Image image;
    private int currentFrame;
    private float timer;

    /// <summary>是否正在播放</summary>
    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    private void Update()
    {
        if (!IsPlaying) return;
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameInterval)
        {
            timer = 0f;
            AdvanceFrame();
        }
    }

    private void AdvanceFrame()
    {
        currentFrame++;
        if (currentFrame >= frames.Length)
        {
            if (!loop)
            {
                currentFrame = frames.Length - 1;
                IsPlaying = false;
            }
            else
            {
                currentFrame = 0;
            }
        }
        image.sprite = frames[currentFrame];
    }

    /// <summary>从头开始播放</summary>
    public void Play()
    {
        if (frames == null || frames.Length == 0) return;
        currentFrame = 0;
        timer = 0f;
        IsPlaying = true;
        image.sprite = frames[0];
    }

    /// <summary>暂停（保持当前帧）</summary>
    public void Pause() => IsPlaying = false;

    /// <summary>停止并回到第一帧</summary>
    public void Stop()
    {
        IsPlaying = false;
        if (frames != null && frames.Length > 0)
            image.sprite = frames[0];
    }
}
