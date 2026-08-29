using System.Collections;
using UnityEngine;

/// <summary>
/// 重置过场控制器 —— 监听关卡重置事件，显示 ResetImage 指定时长后自动隐藏
/// </summary>
public class ResetTransitionController : MonoBehaviour
{
    [Header("过场设置")]
    [Tooltip("关卡重置时显示的图片对象")]
    [SerializeField] private GameObject resetImage;

    [Tooltip("图片显示时长（秒）")]
    [SerializeField] private float displayDuration = 1f;

    private Coroutine hideCoroutine;

    private void OnEnable()
    {
        // 关卡重置统一事件：手动重置与角色死亡后的恢复都会发布
        GameEvents.Listen(EventType.GAME_EVENT_ON_LEVEL_RESET, OnLevelResetTriggered);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.GAME_EVENT_ON_LEVEL_RESET, OnLevelResetTriggered);
    }

    private void OnLevelResetTriggered(IGameEvent evt)
    {
        if (resetImage == null) return;

        resetImage.SetActive(true);
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (resetImage != null)
            resetImage.SetActive(false);
        hideCoroutine = null;
    }
}
