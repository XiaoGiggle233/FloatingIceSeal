#region ========== 游戏日志事件类型 ==========

/// <summary>
/// 游戏日志动作类型枚举
/// 定义游戏过程中各种可记录的动作/事件类型
/// 
/// 适用于各类游戏：RPG、动作、策略、休闲等
/// 扩展方式：直接添加新的枚举值即可
/// </summary>
public enum GameLogAction
{
    // ===== 游戏流程 =====
    GameStart,      // 游戏开始
    GameEnd,        // 游戏结束
    LevelStart,     // 关卡开始
    LevelEnd,       // 关卡结束
    PhaseChange,    // 阶段切换

    // ===== 玩家行为 =====
    PlayerAction,   // 玩家操作
    ItemUse,        // 道具使用
    ItemPickup,     // 物品拾取
    Dialogue,       // 对话事件

    // ===== 系统 =====
    Achievement,    // 成就解锁
    Error,          // 错误/异常
    Warning,        // 警告
    Info,           // 一般信息

    // ===== 数值变化（可选，简单项目可直接用 Info）=====
    ScoreChange,    // 分数变化
    StatChange,     // 属性变化

    // 根据项目需要在此添加更多动作类型
}

/// <summary>
/// 游戏日志条目
/// 单条游戏日志的通用数据结构
/// 
/// 适用于战斗日志、任务日志、系统日志等场景
/// 所有字段为 public，方便序列化和快速访问
/// </summary>
public class GameLogEntry
{
    /// <summary>序号/索引</summary>
    public int index;

    /// <summary>阶段/分类标签（如 "Menu"/"Gameplay"/"Result"）</summary>
    public string phase;

    /// <summary>动作类型</summary>
    public GameLogAction action;

    /// <summary>事件来源名称（如玩家名、NPC名、系统模块名）</summary>
    public string sourceName;

    /// <summary>事件目标名称</summary>
    public string targetName;

    /// <summary>关联数值（分数、伤害量、数量等）</summary>
    public float value;

    /// <summary>附加标记（以字符串形式存储，可多个用逗号分隔）</summary>
    public string flags;

    /// <summary>扩展信息（可存放 JSON 或其他自定义数据）</summary>
    public string extra;
}

/// <summary>
/// 游戏日志事件参数
/// 用于通过 GameEvents 发布日志事件
/// </summary>
public class GameLogEventArgs : GameEventBase
{
    /// <summary>游戏日志条目</summary>
    public GameLogEntry Entry { get; }

    public GameLogEventArgs(GameLogEntry entry)
    {
        Entry = entry;
    }
}

#endregion
