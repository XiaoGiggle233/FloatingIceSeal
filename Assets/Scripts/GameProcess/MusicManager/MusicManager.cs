using UnityEngine;

/// <summary>
/// 背景音乐管理器：默认循环播放背景音乐（挂载于 Managers 预制体）
/// </summary>
public class MusicManager : MonoBehaviour
{
    [SerializeField] private string musicPath = "Music/游戏内音乐"; // Resources 目录下的相对路径

    private static float _globalVolume = 1f;

    private AudioSource _audioSource;

    /// <summary>全局背景音乐音量（0~1）</summary>
    public static float GlobalVolume => _globalVolume;

    /// <summary>设置全局背景音乐音量（0~1），立即作用于所有 MusicManager</summary>
    public static void SetGlobalVolume(float volume)
    {
        _globalVolume = Mathf.Clamp01(volume);
        foreach (MusicManager manager in FindObjectsOfType<MusicManager>())
        {
            manager.ApplyVolume();
        }
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        AudioClip clip = Resources.Load<AudioClip>(musicPath);
        if (clip == null)
        {
            Debug.LogWarning($"[MusicManager] 未找到背景音乐：Resources/{musicPath}");
            return;
        }

        _audioSource.clip = clip;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f; // 背景音乐使用 2D 播放
        ApplyVolume();
        _audioSource.Play();
    }

    private void ApplyVolume()
    {
        if (_audioSource != null)
        {
            _audioSource.volume = _globalVolume;
        }
    }
}
