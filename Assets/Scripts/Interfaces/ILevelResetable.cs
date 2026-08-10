/// <summary>
/// 关卡重置接口 —— 实现此接口的对象参与关卡初始状态记录与恢复（用于动态机制）
/// </summary>
public interface ILevelResetable
{
    /// <summary>关卡状态恢复完成后回调（用于重算内部缓存等）</summary>
    void OnLevelRestore();
}
