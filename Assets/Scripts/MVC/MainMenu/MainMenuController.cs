using UnityEngine;
using UnityEngine.SceneManagement;

#region ========== 主菜单控制器 ==========

/// <summary>
/// 主菜单控制器 —— 处理开始游戏、切换存档、设置、制作人员表、退出等主菜单业务逻辑
/// </summary>
public class MainMenuController : BaseController
{
    private MainMenuView _view;
    private MainMenuModel _menuModel;

    public override void Init()
    {
        _menuModel = new MainMenuModel();
        SetModel(_menuModel);
        _menuModel.Init();
        LoadCurrentSlotInfo();
    }

    public override void OnLoadView(IBaseView view)
    {
        _view = view as MainMenuView;
        if (_view != null)
        {
            _view.StartGameClicked += OnStartGame;
            _view.SwitchSaveClicked += OnSwitchSave;
            _view.SettingsClicked += OnSettings;
            _view.CreditsClicked += OnCredits;
            _view.ExitGameClicked += OnExitGame;

            // View 加载后立即渲染当前存档信息
            _view.Render(_menuModel);
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        if (_view != null)
        {
            _view.StartGameClicked -= OnStartGame;
            _view.SwitchSaveClicked -= OnSwitchSave;
            _view.SettingsClicked -= OnSettings;
            _view.CreditsClicked -= OnCredits;
            _view.ExitGameClicked -= OnExitGame;
        }
    }

    /// <summary>读取当前槽位的关卡信息到模型</summary>
    private void LoadCurrentSlotInfo()
    {
        SaveData save = SaveManager.Load(_menuModel.CurrentSlotIndex);
        _menuModel.CurrentLevel = (save != null && !save.isEmpty) ? save.currentLevel : 1;
    }

    private void OnStartGame()
    {
        SaveData save = SaveManager.Load(_menuModel.CurrentSlotIndex);

        // 存档不存在或为空时，创建默认存档（关卡 = 1）
        if (save == null || save.isEmpty)
        {
            save = new SaveData { currentLevel = 1 };
            SaveManager.Save(_menuModel.CurrentSlotIndex, save);
        }

        string sceneName = $"Level{save.currentLevel}";
        InformationPool.Set("CurrentSlotIndex", _menuModel.CurrentSlotIndex);
        SceneManager.LoadScene(sceneName);
    }

    private void OnSwitchSave()
    {
        _menuModel.CurrentSlotIndex = (_menuModel.CurrentSlotIndex + 1) % SaveManager.MaxSaveSlots;
        LoadCurrentSlotInfo();
        _view?.Render(_menuModel);

        SaveData save = SaveManager.Load(_menuModel.CurrentSlotIndex);
        if (save != null && !save.isEmpty)
            Debug.Log($"切换到存档 {_menuModel.CurrentSlotIndex + 1}，当前关卡: {save.currentLevel}");
        else
            Debug.Log($"切换到存档 {_menuModel.CurrentSlotIndex + 1}（空）");
    }

    private void OnSettings()
    {
        GameEvents.Publish(EventType.UI_EVENT_ON_SETTINGS, new GameEventBase());
    }

    private void OnCredits()
    {
        GameEvents.Publish(EventType.UI_EVENT_ON_CREDITS, new GameEventBase());
    }

    private void OnExitGame()
    {
        GameEvents.Publish(EventType.UI_EVENT_ON_EXIT_GAME, new GameEventBase());
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

#endregion
