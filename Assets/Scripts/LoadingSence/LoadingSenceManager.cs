using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 加载场景管理器 —— 异步加载 StartSenece
/// 加载完成前显示当前加载场景，加载完成后切换到 StartSenece
/// 挂载到 LoadingSence 场景的 GameObject 上
/// </summary>
public class LoadingSenceManager : MonoBehaviour
{
    [Header("目标场景")]
    [SerializeField] private string targetSceneName = "StartSenece";

    [Header("进度显示（可选）")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text progressText;

    private void Start()
    {
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);
        if (operation == null)
        {
            Debug.LogError($"加载场景失败：场景 {targetSceneName} 不存在");
            yield break;
        }

        // 不自动切换，保持加载场景显示，等待加载完成后手动激活目标场景
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // operation.progress 范围 0 ~ 0.9，0.9 表示场景加载完成
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UpdateProgressUI(progress);

            if (operation.progress >= 0.9f)
            {
                UpdateProgressUI(1f);
                operation.allowSceneActivation = true; // 加载完成，切换到目标场景
            }

            yield return null;
        }
    }

    private void UpdateProgressUI(float progress)
    {
        if (progressSlider != null)
            progressSlider.value = progress;

        if (progressText != null)
            progressText.text = $"加载中 {Mathf.FloorToInt(progress * 100)}%";
    }
}
