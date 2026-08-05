using System.Collections.Generic;

#region ========== 切换存档模型 ==========

/// <summary>
/// 切换存档模型 —— 持有各存档槽位数据与当前选中的槽位
/// </summary>
public class SwitchSaveModel : BaseModel
{
    /// <summary>所有槽位的存档数据（索引即槽位号）</summary>
    public List<SaveData> SaveSlots;

    /// <summary>当前选中的槽位索引，-1 表示未选择</summary>
    public int SelectedSlotIndex;

    /// <summary>存档槽位总数</summary>
    public int MaxSaveSlots;

    public override void Init()
    {
        SelectedSlotIndex = -1;
        MaxSaveSlots = SaveManager.MaxSaveSlots;
        RefreshSaves();
    }

    /// <summary>重新读取所有槽位数据</summary>
    public void RefreshSaves()
    {
        SaveSlots = SaveManager.GetAllSaves();
    }
}

#endregion
