using System;
using System.Collections.Generic;

#region ========== 事件类型枚举定义 ==========

/// <summary>
/// 事件类型枚举（通用模板）
/// 
/// 覆盖了常见游戏类型的通用事件，按模块分组：
///   游戏流程 / 场景 / 玩家 / UI / 输入 / 道具 / 资源 / 音频
/// 
/// 使用建议：
///   - 删除不需要的类别，添加项目特有的类别
///   - 用注释分隔各组，保持可读性
///   - 大型项目可拆分为多个文件
/// </summary>
public enum EventType
{
    // ==================== 游戏流程事件 ====================
    GAME_EVENT_ON_START,            // 游戏开始
    GAME_EVENT_ON_PAUSE,            // 游戏暂停
    GAME_EVENT_ON_RESUME,           // 游戏恢复
    GAME_EVENT_ON_GAMEOVER,         // 游戏结束
    GAME_EVENT_ON_RESTART,          // 重新开始

    // ==================== 场景/关卡事件 ====================
    SCENE_EVENT_ON_LOADED,          // 场景加载完成
    SCENE_EVENT_ON_UNLOADED,        // 场景卸载
    SCENE_EVENT_BEFORE_TRANSITION,  // 场景切换前

    // ==================== 玩家相关事件 ====================
    PLAYER_EVENT_ON_SPAWN,          // 玩家生成
    PLAYER_EVENT_ON_DEATH,          // 玩家死亡
    PLAYER_EVENT_ON_STATE_CHANGE,   // 玩家状态变化（如加速、无敌等）

    // ==================== UI 事件 ====================
    UI_EVENT_ON_PANEL_OPEN,         // UI 面板打开
    UI_EVENT_ON_PANEL_CLOSE,        // UI 面板关闭
    UI_EVENT_ON_BUTTON_CLICK,       // UI 按钮点击
    UI_EVENT_ON_SLIDER_CHANGE,      // UI 滑块值变化
    UI_EVENT_SHOW_MESSAGE,          // UI 显示提示消息
    UI_EVENT_SHOW_DIALOG,           // UI 显示对话框

    // ==================== 输入事件 ====================
    INPUT_EVENT_ON_SELECT,          // 选中目标
    INPUT_EVENT_ON_UNSELECT,        // 取消选中
    INPUT_EVENT_ON_CLICK,           // 点击
    INPUT_EVENT_ON_DRAG,            // 拖拽
    INPUT_EVENT_ON_HOVER,           // 悬停

    // ==================== 道具/物品事件 ====================
    ITEM_EVENT_ON_PICKUP,           // 拾取物品
    ITEM_EVENT_ON_USE,              // 使用物品
    ITEM_EVENT_ON_DROP,             // 丢弃物品
    ITEM_EVENT_ON_EQUIP,            // 装备物品
    ITEM_EVENT_ON_UNEQUIP,          // 卸下物品
    ITEM_EVENT_ON_BUY,              // 购买物品
    ITEM_EVENT_ON_SELL,             // 出售物品

    // ==================== 资源/货币事件 ====================
    RESOURCE_EVENT_ON_CHANGE,       // 资源数量变化（货币、材料等）

    // ==================== 音频事件 ====================
    AUDIO_EVENT_ON_PLAY_BGM,        // 播放背景音乐
    AUDIO_EVENT_ON_PLAY_SFX,        // 播放音效
    AUDIO_EVENT_ON_STOP_BGM,        // 停止背景音乐
    AUDIO_EVENT_ON_VOLUME_CHANGE,   // 音量改变

    // ==================== 日志事件 ====================
    LOG_EVENT_ENTRY,                // 日志条目（配合 GameLogTypes 使用）

    // ==================== 联网事件（可选） ====================
    // NETWORK_EVENT_ON_CONNECTED,
    // NETWORK_EVENT_ON_DISCONNECTED,
    // NETWORK_EVENT_ON_DATA_RECEIVED,
}

#endregion

#region ========== 事件参数类定义 ==========

/// <summary>
/// 事件参数示例类
/// 继承 GameEventBase 会自动获得 Timestamp 属性
/// 
/// 命名建议：以 "EventArgs" 结尾，如 GameStateEventArgs、PlayerEventArgs
/// 
/// 所有项目特定类型使用 object，接入项目后替换为实际类型
/// </summary>

// ==================== 游戏流程事件参数 ====================

/// <summary>游戏状态变更事件参数</summary>
public class GameStateEventArgs : GameEventBase
{
    /// <summary>状态名（如 "Paused"/"Playing"/"GameOver"）</summary>
    public string StateName { get; }

    public GameStateEventArgs(string stateName)
    {
        StateName = stateName;
    }
}

/// <summary>场景事件参数</summary>
public class SceneEventArgs : GameEventBase
{
    /// <summary>场景名称</summary>
    public string SceneName { get; }

    public SceneEventArgs(string sceneName)
    {
        SceneName = sceneName;
    }
}

// ==================== 玩家事件参数 ====================

/// <summary>玩家事件基类</summary>
public class PlayerEventArgs : GameEventBase
{
    /// <summary>玩家对象</summary>
    public object Player { get; }

    public PlayerEventArgs(object player)
    {
        Player = player;
    }
}

/// <summary>玩家数值变化事件参数（伤害、治疗、升级等）</summary>
public class PlayerValueEventArgs : GameEventBase
{
    /// <summary>玩家对象</summary>
    public object Player { get; }
    /// <summary>变化数值</summary>
    public float Value { get; }

    public PlayerValueEventArgs(object player, float value)
    {
        Player = player;
        Value = value;
    }
}

// ==================== UI 事件参数 ====================

/// <summary>UI 面板事件参数</summary>
public class UIPanelEventArgs : GameEventBase
{
    /// <summary>面板标识（名称或 ID）</summary>
    public string PanelName { get; }

    public UIPanelEventArgs(string panelName)
    {
        PanelName = panelName;
    }
}

/// <summary>UI 消息事件参数</summary>
public class UIMessageEventArgs : GameEventBase
{
    /// <summary>消息文本</summary>
    public string Message { get; }
    /// <summary>消息类型（Info/Warning/Error）</summary>
    public string MessageType { get; }

    public UIMessageEventArgs(string message, string messageType = "Info")
    {
        Message = message;
        MessageType = messageType;
    }
}

// ==================== 输入事件参数 ====================

/// <summary>选中事件参数</summary>
public class SelectEventArgs : GameEventBase
{
    /// <summary>选中的目标对象</summary>
    public object SelectedObject { get; }

    public SelectEventArgs(object selectedObject)
    {
        SelectedObject = selectedObject;
    }
}

/// <summary>取消选中事件参数</summary>
public class UnselectEventArgs : GameEventBase
{
}

// ==================== 道具事件参数 ====================

/// <summary>道具事件参数</summary>
public class ItemEventArgs : GameEventBase
{
    /// <summary>道具对象</summary>
    public object Item { get; }
    /// <summary>数量</summary>
    public int Amount { get; }

    public ItemEventArgs(object item, int amount = 1)
    {
        Item = item;
        Amount = amount;
    }
}

// ==================== 资源事件参数 ====================

/// <summary>资源数量变化事件参数</summary>
public class ResourceChangeEventArgs : GameEventBase
{
    /// <summary>资源类型标识（名称或 ID）</summary>
    public string ResourceType { get; }
    /// <summary>变更后的新值</summary>
    public float NewValue { get; }
    /// <summary>变化量（正=增加，负=减少）</summary>
    public float Delta { get; }

    public ResourceChangeEventArgs(string resourceType, float newValue, float delta)
    {
        ResourceType = resourceType;
        NewValue = newValue;
        Delta = delta;
    }
}

// ==================== 音频事件参数 ====================

/// <summary>音频事件参数</summary>
public class AudioEventArgs : GameEventBase
{
    /// <summary>音频资源名或路径</summary>
    public string AudioName { get; }
    /// <summary>音量（0~1）</summary>
    public float Volume { get; }

    public AudioEventArgs(string audioName, float volume = 1f)
    {
        AudioName = audioName;
        Volume = volume;
    }
}

// ==================== 通用事件参数 ====================

/// <summary>通用键值对事件参数（适合简单数据传递）</summary>
public class GenericEventArgs : GameEventBase
{
    /// <summary>数据字典</summary>
    public Dictionary<string, object> Data { get; }

    public GenericEventArgs(Dictionary<string, object> data = null)
    {
        Data = data ?? new Dictionary<string, object>();
    }
}

#endregion
