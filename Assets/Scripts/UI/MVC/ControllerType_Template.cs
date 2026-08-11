#region ========== 控制器类型枚举 ==========

/// <summary>
/// 控制器类型枚举
/// 每个枚举值对应一个具体的 Controller 类
/// 
/// 命名建议：与对应的 Controller 类名一致（去掉 "Controller" 后缀）
/// 如：MainMenu -> MainMenuController、GameHUD -> GameHUDController
/// 
/// 以下为常见游戏类型的示例，请根据项目实际需求修改
/// </summary>
public enum ControllerType
{
    // ===== 核心流程 =====
    MainMenu,       // 主菜单
    GameHUD,        // 游戏内 HUD
    Pause,          // 暂停菜单
    Settings,       // 设置面板
    GameOver,       // 结算界面
    SwitchSave,     // 切换存档
    OxygenBubble,   // 氧气气泡

    // ===== 功能模块 =====
    Inventory,      // 背包
    Shop,           // 商店
    Dialog,         // 对话系统
    QuestLog,       // 任务日志

    // ===== 提示/弹窗 =====
    Notification,   // 通知提示
    ConfirmDialog,  // 确认弹窗

    // 根据项目需要在此添加更多控制器类型
}

#endregion
