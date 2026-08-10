#region ========== 快速暂停菜单模型 ==========

/// <summary>
/// 快速暂停菜单模型 —— 持有暂停状态
/// </summary>
public class QuickPauseMenuModel : BaseModel
{
    /// <summary>当前是否处于暂停状态</summary>
    public bool IsPaused;

    public override void Init()
    {
        IsPaused = false;
    }
}

#endregion
