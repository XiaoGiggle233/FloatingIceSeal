#region ========== 主菜单模型 ==========

/// <summary>
/// 主菜单模型 —— 持有当前选中的存档槽位信息
/// </summary>
public class MainMenuModel : BaseModel
{
    /// <summary>当前选中的存档槽位索引（0 ~ MaxSaveSlots-1）</summary>
    public int CurrentSlotIndex;

    /// <summary>当前槽位对应的关卡</summary>
    public int CurrentLevel;

    /// <summary>存档槽位总数</summary>
    public int MaxSaveSlots;

    public override void Init()
    {
        CurrentSlotIndex = 0;
        CurrentLevel = 1;
        MaxSaveSlots = SaveManager.MaxSaveSlots;
    }
}

#endregion
