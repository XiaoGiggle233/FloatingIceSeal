using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局信息池
/// 
/// 提供模块间共享数据的中心化存储，配合 GameEvents 通知数据变更。
/// 
/// 使用方式：
///   InformationPool.Set("PlayerHP", 100);
///   int hp = InformationPool.Get("PlayerHP", 0);
///   InformationPool.Remove("PlayerHP");
/// </summary>
public static class InformationPool
{
    private static readonly Dictionary<string, object> _pool = new Dictionary<string, object>();

    #region 存入

    /// <summary>存入或更新一个键值对，并发布变更事件</summary>
    public static void Set(string key, object value)
    {
        if (string.IsNullOrEmpty(key)) return;

        object oldValue = null;
        bool existed = _pool.TryGetValue(key, out oldValue);

        _pool[key] = value;

        if (existed)
            Debug.Log($"InformationPool: Set {key} = {value} (was: {oldValue})");
        else
            Debug.Log($"InformationPool: Set {key} = {value}");

        GameEvents.Publish(EventType.INFO_POOL_EVENT_ON_CHANGE,
            new InfoPoolEventArgs(key, oldValue, value, InfoPoolEventArgs.ChangeType.Set));
    }

    #endregion

    #region 获取

    /// <summary>获取指定键的值，不存在时返回默认值</summary>
    public static T Get<T>(string key, T defaultValue = default)
    {
        if (string.IsNullOrEmpty(key)) return defaultValue;

        if (_pool.TryGetValue(key, out object value) && value is T typedValue)
            return typedValue;

        return defaultValue;
    }

    /// <summary>尝试获取值，返回是否成功</summary>
    public static bool TryGet<T>(string key, out T value)
    {
        value = default;
        if (string.IsNullOrEmpty(key)) return false;

        if (_pool.TryGetValue(key, out object obj) && obj is T typed)
        {
            value = typed;
            return true;
        }
        return false;
    }

    #endregion

    #region 检查

    /// <summary>检查键是否存在</summary>
    public static bool Has(string key)
    {
        return !string.IsNullOrEmpty(key) && _pool.ContainsKey(key);
    }

    #endregion

    #region 删除

    /// <summary>移除指定键，返回是否成功</summary>
    public static bool Remove(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;

        if (_pool.TryGetValue(key, out object oldValue))
        {
            _pool.Remove(key);
            Debug.Log($"InformationPool: Remove {key} (was: {oldValue})");
            GameEvents.Publish(EventType.INFO_POOL_EVENT_ON_CHANGE,
                new InfoPoolEventArgs(key, oldValue, null, InfoPoolEventArgs.ChangeType.Remove));
            return true;
        }
        return false;
    }

    /// <summary>清空所有信息</summary>
    public static void Clear()
    {
        Debug.Log("InformationPool: Clear");
        _pool.Clear();
        GameEvents.Publish(EventType.INFO_POOL_EVENT_ON_CHANGE,
            new InfoPoolEventArgs(InfoPoolEventArgs.ChangeType.Clear));
    }

    #endregion

    #region 调试

    /// <summary>获取所有键名（调试用）</summary>
    public static string[] GetAllKeys()
    {
        var keys = new string[_pool.Count];
        _pool.Keys.CopyTo(keys, 0);
        return keys;
    }

    /// <summary>获取当前池中条目数</summary>
    public static int Count => _pool.Count;
    #endregion
}
