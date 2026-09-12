using TMPro;
using UnityEngine;

/// <summary>
/// 水雷爆炸提示文本控制器 —— 水雷爆炸时显示文本，关卡重置时关闭；
/// 文本全局只显示一次，关卡重置后即使再次爆炸也不再显示。
/// </summary>
public class MineExplodeTextController : MonoBehaviour
{
    [Tooltip("提示文本组件（留空则自动获取同物体上的 TextMeshProUGUI）")]
    [SerializeField] private TextMeshProUGUI targetText;

    /// <summary>是否已显示过（全局只显示一次）</summary>
    private bool hasShown;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>();

        // 初始状态：文本隐藏，等待水雷爆炸事件
        if (targetText != null)
            targetText.enabled = false;
        hasShown = false;
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.MINE_EVENT_ON_EXPLODE, OnMineExplode);
        GameEvents.Listen(EventType.GAME_EVENT_ON_LEVEL_RESET, OnLevelReset);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.MINE_EVENT_ON_EXPLODE, OnMineExplode);
        GameEvents.Unlisten(EventType.GAME_EVENT_ON_LEVEL_RESET, OnLevelReset);
    }

    /// <summary>水雷爆炸：显示文本（仅第一次生效）</summary>
    private void OnMineExplode(IGameEvent evt)
    {
        if (hasShown) return;
        hasShown = true;

        if (targetText != null)
            targetText.enabled = true;
    }

    /// <summary>关卡重置：关闭文本（不复位显示控制，文本全局只显示一次）</summary>
    private void OnLevelReset(IGameEvent evt)
    {
        if (targetText != null)
            targetText.enabled = false;
    }
}
