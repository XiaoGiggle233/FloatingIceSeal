using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 返回主菜单按钮 —— 仿照关卡切换逻辑，经加载场景返回开始界面
/// </summary>
public class ReturnMainMenuButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // 仿照 SwitchLevels：先记录目标场景，再进入加载场景
        InformationPool.Set("TargetSceneName", "StartSence");
        SceneManager.LoadScene("LoadingSence");
    }
}
