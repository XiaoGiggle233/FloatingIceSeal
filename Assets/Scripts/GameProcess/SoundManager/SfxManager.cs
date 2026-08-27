using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音效管理器 —— 通过事件系统与信息池通信，管理 Resources/Music/音效文件夹 下音效的播放（挂载于 Managers 预制体）
/// 小鱼警觉：小鱼进入追击状态；气泡音效：气泡释放；水流音效：场景存在流动水体时循环；水雷爆炸：水雷爆炸时
/// </summary>
public class SfxManager : MonoBehaviour
{
    private const string SfxRoot = "Music/音效文件夹";
    private const string FishAlertClip = "小鱼警觉音效1";
    private const string BubbleClip = "气泡音效";
    private const string WaterFlowClip = "水流音效";
    private const string MineExplosionClip = "水雷爆炸音效";

    private const string FlowingWaterListKey = "FlowingWatterList";

    private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    private AudioSource oneShotSource;
    private AudioSource loopSource;

    private void Awake()
    {
        foreach (string name in new[] { FishAlertClip, BubbleClip, WaterFlowClip, MineExplosionClip })
        {
            AudioClip clip = Resources.Load<AudioClip>($"{SfxRoot}/{name}");
            if (clip != null)
                clips[name] = clip;
            else
                Debug.LogWarning($"[SfxManager] 未找到音效：Resources/{SfxRoot}/{name}");
        }

        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.playOnAwake = false;
        oneShotSource.spatialBlend = 0f;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = true;
        loopSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.SMALLFISH_EVENT_ON_STATE_CHANGE, OnSmallFishStateChanged);
        GameEvents.Listen(EventType.BUBBLE_EVENT_ON_RELEASE, OnBubbleReleased);
        GameEvents.Listen(EventType.MINE_EVENT_ON_EXPLODE, OnMineExploded);
        GameEvents.Listen(EventType.INFO_POOL_EVENT_ON_CHANGE, OnInfoPoolChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.SMALLFISH_EVENT_ON_STATE_CHANGE, OnSmallFishStateChanged);
        GameEvents.Unlisten(EventType.BUBBLE_EVENT_ON_RELEASE, OnBubbleReleased);
        GameEvents.Unlisten(EventType.MINE_EVENT_ON_EXPLODE, OnMineExploded);
        GameEvents.Unlisten(EventType.INFO_POOL_EVENT_ON_CHANGE, OnInfoPoolChanged);
    }

    private void Start()
    {
        // 初始检查：水体可能先于本组件注册到信息池
        UpdateWaterLoop();
    }

    private void OnSmallFishStateChanged(IGameEvent evt)
    {
        var args = evt as GameStateEventArgs;
        if (args == null) return;

        if (args.StateName == nameof(SmallFishState.Chase))
            PlayOneShot(FishAlertClip);
    }

    private void OnBubbleReleased(IGameEvent evt)
    {
        PlayOneShot(BubbleClip);
    }

    private void OnMineExploded(IGameEvent evt)
    {
        PlayOneShot(MineExplosionClip);
    }

    private void OnInfoPoolChanged(IGameEvent evt)
    {
        var args = evt as InfoPoolEventArgs;
        if (args == null || args.Key != FlowingWaterListKey) return;

        UpdateWaterLoop();
    }

    /// <summary>场景中存在流动水体（FlowDirection != None）时循环播放水流音效，否则停止</summary>
    private void UpdateWaterLoop()
    {
        bool hasFlowingWater = false;
        if (InformationPool.TryGet(FlowingWaterListKey, out List<FlowingWatter> waters) && waters != null)
        {
            foreach (var water in waters)
            {
                if (water != null && water.FlowDirection != FlowDirection.None)
                {
                    hasFlowingWater = true;
                    break;
                }
            }
        }

        if (hasFlowingWater)
        {
            if (!loopSource.isPlaying)
            {
                loopSource.clip = GetClip(WaterFlowClip);
                if (loopSource.clip != null)
                    loopSource.Play();
            }
        }
        else if (loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    private void PlayOneShot(string clipName)
    {
        AudioClip clip = GetClip(clipName);
        if (clip != null)
            oneShotSource.PlayOneShot(clip);
    }

    private AudioClip GetClip(string clipName) => clips.TryGetValue(clipName, out var clip) ? clip : null;
}
