/// <summary>信息池变更事件参数</summary>
public class InfoPoolEventArgs : GameEventBase
{
    public enum ChangeType { Set, Remove, Clear }

    /// <summary>变更的键</summary>
    public string Key { get; }
    /// <summary>变更前的值（新增时为 null）</summary>
    public object OldValue { get; }
    /// <summary>变更后的值（删除时为 null）</summary>
    public object NewValue { get; }
    /// <summary>变更类型</summary>
    public ChangeType Type { get; }

    public InfoPoolEventArgs(string key, object oldValue, object newValue, ChangeType type)
    {
        Key = key;
        OldValue = oldValue;
        NewValue = newValue;
        Type = type;
    }

    /// <summary>Clear 操作专用构造</summary>
    public InfoPoolEventArgs(ChangeType type)
    {
        Type = type;
    }

    public override string ToString()
    {
        return Type == ChangeType.Clear
            ? $"[InfoPool] Clear"
            : $"[InfoPool] {Type}: {Key} = {NewValue ?? "null"} (was: {OldValue ?? "null"})";
    }
}
