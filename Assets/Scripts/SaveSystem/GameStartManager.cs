using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏启动管理器 —— 处理存档选择与开始游戏逻辑
/// 挂载到 StartSenece 场景的 GameObject 上
/// </summary>
public class GameStartManager : MonoBehaviour
{
    private int _currentSlotIndex = 0;

    private void OnEnable()
    {
        GameEvents.Listen(EventType.UI_EVENT_ON_START_GAME, OnStartGame);
        GameEvents.Listen(EventType.UI_EVENT_ON_SWITCH_SAVE, OnSwitchSave);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.UI_EVENT_ON_START_GAME, OnStartGame);
        GameEvents.Unlisten(EventType.UI_EVENT_ON_SWITCH_SAVE, OnSwitchSave);
    }

    private void OnStartGame(IGameEvent e)
    {
        SaveData save = SaveManager.Load(_currentSlotIndex);

        // 如果存档不存在或为空，创建默认存档（关卡 = 1）
        if (save == null || save.isEmpty)
        {
            save = new SaveData { currentLevel = 1 };
            SaveManager.Save(_currentSlotIndex, save);
        }

        string sceneName = $"Level{save.currentLevel}";
        InformationPool.Set("CurrentSlotIndex", _currentSlotIndex);
        SceneManager.LoadScene(sceneName);
    }

    private void OnSwitchSave(IGameEvent e)
    {
        _currentSlotIndex = (_currentSlotIndex + 1) % SaveManager.MaxSaveSlots;
        // TODO: 可在此处更新 UI，显示当前选中的存档槽位

        SaveData save = SaveManager.Load(_currentSlotIndex);
        if (save != null && !save.isEmpty)
            Debug.Log($"切换到存档 {_currentSlotIndex + 1}，当前关卡: {save.currentLevel}");
        else
            Debug.Log($"切换到存档 {_currentSlotIndex + 1}（空）");
    }
}
