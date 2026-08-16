using UnityEngine;

#region ========== 切换存档控制器 ==========

/// <summary>
/// 切换存档控制器 —— 处理存档槽位的创建/选择/复制/删除逻辑
/// </summary>
public class SwitchSaveController : BaseController
{
    private SwitchSaveView _view;
    private SwitchSaveModel _saveModel;

    public override void Init()
    {
        _saveModel = new SwitchSaveModel();
        SetModel(_saveModel);
        _saveModel.Init();
        RegisterFunc("OpenPanel", args => OpenPanel());
        RegisterFunc("ClosePanel", args => ClosePanel());
    }

    public override void OnLoadView(IBaseView view)
    {
        _view = view as SwitchSaveView;
        if (_view != null)
        {
            _view.SaveSlotClicked += OnSaveSlotClicked;
            _view.CopyClicked += OnCopyClicked;
            _view.DeleteClicked += OnDeleteClicked;
            _view.BackClicked += OnBackClicked;
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        UnRegisterFunc("OpenPanel");
        UnRegisterFunc("ClosePanel");
        if (_view != null)
        {
            _view.SaveSlotClicked -= OnSaveSlotClicked;
            _view.CopyClicked -= OnCopyClicked;
            _view.DeleteClicked -= OnDeleteClicked;
            _view.BackClicked -= OnBackClicked;
        }
    }

    /// <summary>打开存档切换面板（由主菜单调用）</summary>
    public void OpenPanel()
    {
        if (_view == null) return;
        _saveModel.RefreshSaves();
        _view.SetMenuVisible(true);
        _view.Render(_saveModel);
    }

    /// <summary>关闭存档切换面板</summary>
    public void ClosePanel()
    {
        _view?.SetMenuVisible(false);
    }

    private void OnSaveSlotClicked(int index)
    {
        SaveData save = SaveManager.Load(index);

        // 槽位未创建 → 创建默认存档并选中
        if (save == null || save.isEmpty)
        {
            SaveManager.Save(index, new SaveData { currentLevel = 1 });
            Debug.Log($"创建存档 {index + 1}");
        }

        _saveModel.SelectedSlotIndex = index;
        _saveModel.RefreshSaves();
        _view?.Render(_saveModel);

        // 同步选中槽位到主菜单，保证"开始游戏"使用该槽位
        MVCManager.ControllerManager.ApplyFunc(ControllerType.MainMenu, "SetCurrentSlotIndex", index);
    }

    private void OnCopyClicked()
    {
        if (_saveModel.SelectedSlotIndex < 0)
        {
            Debug.Log("请先选择一个存档");
            return;
        }

        int source = _saveModel.SelectedSlotIndex;
        int target = SaveManager.Copy(source);
        if (target < 0)
        {
            Debug.Log("没有空余的存档槽位可复制");
            return;
        }

        _saveModel.SelectedSlotIndex = target;
        _saveModel.RefreshSaves();
        _view?.Render(_saveModel);
        Debug.Log($"已将存档 {source + 1} 复制到槽位 {target + 1}");

        // 复制后选中目标槽位，同步到主菜单
        MVCManager.ControllerManager.ApplyFunc(ControllerType.MainMenu, "SetCurrentSlotIndex", target);
    }

    private void OnDeleteClicked()
    {
        if (_saveModel.SelectedSlotIndex < 0)
        {
            Debug.Log("请先选择一个存档");
            return;
        }

        SaveManager.Delete(_saveModel.SelectedSlotIndex);
        _saveModel.SelectedSlotIndex = -1;
        _saveModel.RefreshSaves();
        _view?.Render(_saveModel);
        Debug.Log($"已删除存档");
    }

    private void OnBackClicked()
    {
        ClosePanel();
    }
}

#endregion
