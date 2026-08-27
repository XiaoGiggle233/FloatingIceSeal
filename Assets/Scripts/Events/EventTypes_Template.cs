using System;
using System.Collections.Generic;
using UnityEngine;

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
    PLAYER_EVENT_ON_DASH,           // 玩家冲刺
    PLAYER_EVENT_ON_BLOW_BUBBLE,    // 玩家吐泡泡
    PLAYER_EVENT_ON_OXYGEN_CONSUME, // 玩家氧气消耗
    PLAYER_EVENT_ON_OXYGEN_RECOVER, // 玩家氧气回复

    // ==================== UI 事件 ====================
    UI_EVENT_ON_PANEL_OPEN,         // UI 面板打开
    UI_EVENT_ON_PANEL_CLOSE,        // UI 面板关闭
    UI_EVENT_ON_BUTTON_CLICK,       // UI 按钮点击
    UI_EVENT_ON_SLIDER_CHANGE,      // UI 滑块值变化
    UI_EVENT_SHOW_MESSAGE,          // UI 显示提示消息
    UI_EVENT_SHOW_DIALOG,           // UI 显示对话框
    UI_EVENT_ON_START_GAME,         // 开始游戏按钮
    UI_EVENT_ON_SWITCH_SAVE,        // 切换存档按钮
    UI_EVENT_ON_SETTINGS,           // 设置按钮
    UI_EVENT_ON_CREDITS,            // 制作人员表按钮
    UI_EVENT_ON_EXIT_GAME,          // 退出游戏按钮
    UI_EVENT_ON_RESET_LEVEL,        // 重置关卡按钮

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

    // ==================== 碰撞事件 ====================
    COLLISION_EVENT_ON_ENTER,       // 物理碰撞事件
    COLLISION_EVENT_ON_TRIGGER,     // 触发器碰撞事件

    // ==================== 泡泡事件 ====================
    BUBBLE_EVENT_ON_SPAWN,          // 泡泡生成
    BUBBLE_EVENT_ON_BURST,          // 泡泡破裂
    BUBBLE_EVENT_ON_RELEASE,        // 泡泡释放（蓄力完成）

    // ==================== 小鱼事件 ====================
    SMALLFISH_EVENT_ON_STATE_CHANGE, // 小鱼状态变化（巡逻/等待/追击/破坏/死亡）

    // ==================== 水雷事件 ====================
    MINE_EVENT_ON_EXPLODE,          // 水雷爆炸

    // ==================== 信息池事件 ====================
    INFO_POOL_EVENT_ON_CHANGE,      // 信息池数据变更

    // ==================== 动画事件 ====================
    ANIMATION_EVENT_ON_STATE_CHANGE, // 动画状态切换
    ANIMATION_EVENT_ON_START,        // 动画开始播放
    ANIMATION_EVENT_ON_END,          // 动画播放完毕
    ANIMATION_EVENT_ON_KEYFRAME,     // 动画关键帧
    ANIMATION_EVENT_ON_CUSTOM,       // 自定义动画事件

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

    /// <summary>是否由手动重置触发（手动重置链发布的死亡事件，用于区分重生时机）</summary>
    public bool IsManualReset { get; }

    public PlayerEventArgs(object player, bool isManualReset = false)
    {
        Player = player;
        IsManualReset = isManualReset;
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

/// <summary>UI 按钮点击事件参数</summary>
public class ButtonClickEventArgs : GameEventBase
{
    /// <summary>按钮标识名</summary>
    public string ButtonName { get; }

    public ButtonClickEventArgs(string buttonName)
    {
        ButtonName = buttonName;
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

// ==================== 碰撞事件参数 ====================

/// <summary>触发器碰撞事件参数</summary>
public class CollisionEventArgs : GameEventBase
{
    /// <summary>碰撞源对象</summary>
    public GameObject Source { get; }
    /// <summary>被碰撞对象</summary>
    public GameObject Target { get; }
    /// <summary>碰撞法线</summary>
    public Vector2 Normal { get; }

    public CollisionEventArgs(GameObject source, GameObject target, Vector2 normal)
    {
        Source = source;
        Target = target;
        Normal = normal;
    }
}

// ==================== 泡泡事件参数 ====================

/// <summary>泡泡破裂事件参数</summary>
public class BubbleBurstEventArgs : GameEventBase
{
    /// <summary>破裂的泡泡对象</summary>
    public GameObject Bubble { get; }

    public BubbleBurstEventArgs(GameObject bubble)
    {
        Bubble = bubble;
    }
}

/// <summary>吐泡泡事件参数（蓄力完成后释放）</summary>
public class BubbleBlowEventArgs : GameEventBase
{
    /// <summary>海豹对象</summary>
    public GameObject Seal { get; }
    /// <summary>生成的泡泡对象</summary>
    public GameObject Bubble { get; }
    /// <summary>泡泡储存的氧气量</summary>
    public float OxygenValue { get; }

    public BubbleBlowEventArgs(GameObject seal, GameObject bubble, float oxygenValue)
    {
        Seal = seal;
        Bubble = bubble;
        OxygenValue = oxygenValue;
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

// ==================== 动画事件参数 ====================

/// <summary>动画状态变更事件参数</summary>
public class AnimationStateEventArgs : GameEventBase
{
    /// <summary>动画所属的 GameObject</summary>
    public GameObject AnimatorOwner { get; }
    /// <summary>上一个动画状态名（可为空）</summary>
    public string PreviousState { get; }
    /// <summary>当前动画状态名（可为空）</summary>
    public string CurrentState { get; }

    public AnimationStateEventArgs(GameObject owner, string previousState, string currentState)
    {
        AnimatorOwner = owner;
        PreviousState = previousState;
        CurrentState = currentState;
    }
}

/// <summary>自定义动画事件参数（支持多种数据类型）</summary>
public class AnimationCustomEventArgs : GameEventBase
{
    /// <summary>动画所属的 GameObject</summary>
    public GameObject AnimatorOwner { get; }
    /// <summary>事件键（用于区分不同类型的动画事件）</summary>
    public string EventKey { get; }
    /// <summary>字符串参数值</summary>
    public string StringValue { get; }
    /// <summary>整数参数值</summary>
    public int IntValue { get; }
    /// <summary>浮点参数值</summary>
    public float FloatValue { get; }
    /// <summary>参数数据类型</summary>
    public enum ValueType { None, String, Int, Float }
    /// <summary>当前携带的数据类型</summary>
    public ValueType DataType { get; }

    /// <summary>无参构造函数</summary>
    public AnimationCustomEventArgs(GameObject owner, string eventKey)
    {
        AnimatorOwner = owner;
        EventKey = eventKey;
        DataType = ValueType.None;
    }

    /// <summary>字符串参数构造函数</summary>
    public AnimationCustomEventArgs(GameObject owner, string eventKey, string value)
    {
        AnimatorOwner = owner;
        EventKey = eventKey;
        StringValue = value;
        DataType = ValueType.String;
    }

    /// <summary>整数参数构造函数</summary>
    public AnimationCustomEventArgs(GameObject owner, string eventKey, int value)
    {
        AnimatorOwner = owner;
        EventKey = eventKey;
        IntValue = value;
        DataType = ValueType.Int;
    }

    /// <summary>浮点参数构造函数</summary>
    public AnimationCustomEventArgs(GameObject owner, string eventKey, float value)
    {
        AnimatorOwner = owner;
        EventKey = eventKey;
        FloatValue = value;
        DataType = ValueType.Float;
    }
}

#endregion
