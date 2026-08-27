using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 海豹精灵控制器
/// 监听状态机变化事件，根据氧气状态和生命状态切换对应的游泳动画（Resources/Images/Character/角色动画），
/// 播放速度 = k × 角色当前速度；并根据方向状态翻转/旋转子物体 Sprite（只旋转子物体，不影响碰撞体）
/// </summary>
public class SealSpriteController : MonoBehaviour
{
    private const string AnimRoot = "Images/Character/角色动画";
    private const string OxygenFullFolder = AnimRoot + "/满气泡";
    private const string OxygenSufficientFolder = AnimRoot + "/三分之四";
    private const string OxygenInsufficientFolder = AnimRoot + "/二分之一";
    private const string OxygenCriticalFolder = AnimRoot + "/五分之一";
    private const string SuffocatingFolder = AnimRoot + "/零";
    private const string DeadFolder = AnimRoot + "/死亡";

    private static readonly Dictionary<string, Sprite[]> FrameCache = new Dictionary<string, Sprite[]>();

    #region 序列化字段

    [FoldoutGroup("动画", expanded: true)]
    [LabelText("播放速度系数 k（帧/米）"), MinValue(0)]
    [SerializeField] private float framesPerMeter = 2f;

    [FoldoutGroup("动画")]
    [LabelText("死亡动画帧率（帧/秒）"), MinValue(0.1f)]
    [SerializeField] private float deathFps = 8f;

    #endregion

    #region 私有字段

    private SpriteRenderer spriteRenderer;
    private Seal seal;
    private Rigidbody2D rb;
    private Sprite[] currentFrames;
    private int currentFrameIndex;
    private float frameProgress;

    #endregion

    #region Unity 生命周期

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        seal = GetComponent<Seal>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_STATE_CHANGE, OnStateChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_STATE_CHANGE, OnStateChanged);
    }

    private void Start()
    {
        SelectAnimation();
        ApplyFlipAndRotation(seal.DirectionStateMachine.CurrentState);
    }

    private void Update()
    {
        AdvanceAnimation();
    }

    #endregion

    #region 事件处理

    private void OnStateChanged(IGameEvent evt)
    {
        var args = evt as GameStateEventArgs;
        if (args == null) return;

        // 方向变化：翻转 Sprite X 轴并旋转
        if (Enum.TryParse(args.StateName, out DirectionState dirState))
        {
            ApplyFlipAndRotation(dirState);
            return;
        }

        // 氧气/生命等状态变化：切换动画
        SelectAnimation();
    }

    #endregion

    #region 动画播放

    /// <summary>根据生命/氧气状态选择动画文件夹（切换时重新开始播放）</summary>
    private void SelectAnimation()
    {
        string folder = seal.LifeStateMachine.IsDead()
            ? DeadFolder
            : GetOxygenFolder(seal.OxygenStateMachine.CurrentState);

        Sprite[] frames = LoadFrames(folder);
        if (frames == null || frames.Length == 0) return;

        if (!ReferenceEquals(frames, currentFrames))
        {
            currentFrames = frames;
            currentFrameIndex = 0;
            frameProgress = 0f;
        }

        spriteRenderer.sprite = frames[currentFrameIndex];
    }

    private void AdvanceAnimation()
    {
        if (currentFrames == null || currentFrames.Length == 0) return;

        if (seal.LifeStateMachine.IsDead())
        {
            // 死亡动画：固定帧率播放一次，停在最后一帧
            frameProgress += Time.deltaTime * deathFps;
            while (frameProgress >= 1f && currentFrameIndex < currentFrames.Length - 1)
            {
                frameProgress -= 1f;
                currentFrameIndex++;
            }
            if (currentFrameIndex >= currentFrames.Length - 1)
                frameProgress = 0f;
        }
        else
        {
            // 游泳动画：播放速度 = k × 角色当前速度（停止时定格）
            float speed = rb != null ? rb.velocity.magnitude : 0f;
            frameProgress += Time.deltaTime * speed * framesPerMeter;
            while (frameProgress >= 1f)
            {
                frameProgress -= 1f;
                currentFrameIndex = (currentFrameIndex + 1) % currentFrames.Length;
            }
        }

        spriteRenderer.sprite = currentFrames[currentFrameIndex];
    }

    private static string GetOxygenFolder(OxygenState state)
    {
        return state switch
        {
            OxygenState.OxygenFull => OxygenFullFolder,
            OxygenState.OxygenSufficient => OxygenSufficientFolder,
            OxygenState.OxygenInsufficient => OxygenInsufficientFolder,
            OxygenState.OxygenCritical => OxygenCriticalFolder,
            OxygenState.Suffocating => SuffocatingFolder,
            _ => null
        };
    }

    /// <summary>加载指定 Resources 文件夹下的所有精灵帧（按文件名升序排序后缓存）</summary>
    private static Sprite[] LoadFrames(string folder)
    {
        if (FrameCache.TryGetValue(folder, out Sprite[] cached))
            return cached;

        Sprite[] sprites = Resources.LoadAll<Sprite>(folder);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[SealSpriteController] Resources/{folder} 下未找到精灵帧");
            return null;
        }

        Array.Sort(sprites, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        FrameCache[folder] = sprites;
        return sprites;
    }

    #endregion

    #region 翻转与旋转

    /// <summary>根据方向状态翻转 Sprite X 轴并旋转子物体 Sprite</summary>
    private void ApplyFlipAndRotation(DirectionState state)
    {
        // 纯上/下状态没有左右信息：沿用当前 flipX 保存的最后一次水平朝向（flipX=true 朝右）
        bool facingLeft = !spriteRenderer.flipX;

        switch (state)
        {
            case DirectionState.Left:
            case DirectionState.UpLeft:
            case DirectionState.DownLeft:
                facingLeft = true;
                break;
            case DirectionState.Right:
            case DirectionState.UpRight:
            case DirectionState.DownRight:
                facingLeft = false;
                break;
        }

        spriteRenderer.flipX = !facingLeft;

        // 上：朝左顺时针90°、朝右逆时针90°；下：朝左逆时针90°、朝右顺时针90°
        // 左上顺时针45°、右上逆时针45°、左下逆时针45°、右下顺时针45°；左/右不旋转
        float zAngle = state switch
        {
            DirectionState.Up => facingLeft ? -90f : 90f,
            DirectionState.Down => facingLeft ? 90f : -90f,
            DirectionState.UpLeft => -45f,
            DirectionState.UpRight => 45f,
            DirectionState.DownLeft => 45f,
            DirectionState.DownRight => -45f,
            _ => 0f
        };

        spriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, zAngle);
    }

    #endregion
}
