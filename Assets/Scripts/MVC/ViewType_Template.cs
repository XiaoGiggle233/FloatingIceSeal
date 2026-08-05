#region ========== 视图类型枚举 ==========

/// <summary>
/// 视图类型枚举
/// 每个枚举值对应一个具体的 View 类
/// 
/// 注意：枚举名需与 View 脚本类名完全一致，
/// 因为 ViewManager 会通过枚举名来 AddComponent
/// 如果使用自定义的 ViewTypeName，则可以不保持一致
/// 
/// 以下为常见游戏类型的示例，请根据项目实际需求修改
/// </summary>
public enum ViewType
{
    // ===== 核心流程 =====
    MainMenuView,       // 主菜单界面
    GameHUDView,        // 游戏内 HUD
    PauseView,          // 暂停菜单界面
    SettingsView,       // 设置面板界面
    GameOverView,       // 结算界面
    SwitchSaveView,     // 切换存档界面

    // ===== 功能模块 =====
    InventoryView,      // 背包界面
    ShopView,           // 商店界面
    DialogView,         // 对话界面
    QuestLogView,       // 任务日志界面

    // ===== 提示/弹窗 =====
    NotificationView,   // 通知提示界面
    ConfirmDialogView,  // 确认弹窗界面

    // 根据项目需要在此添加更多视图类型
}

#endregion
