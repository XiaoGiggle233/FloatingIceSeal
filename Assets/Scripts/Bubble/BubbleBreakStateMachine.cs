/// <summary>
/// 大气泡破坏状态 —— 是否正在被小鱼破坏
/// </summary>
public enum BubbleBreakState
{
    /// <summary>未被破坏</summary>
    NotBreaking,
    /// <summary>正在被小鱼破坏（上浮速度大幅降低）</summary>
    Breaking
}

/// <summary>大气泡破坏状态机 —— 管理是否被小鱼破坏的状态切换</summary>
public class BubbleBreakStateMachine
{
    public BubbleBreakState CurrentState { get; private set; } = BubbleBreakState.NotBreaking;

    public void SetState(BubbleBreakState state)
    {
        CurrentState = state;
    }

    public bool IsBreaking() => CurrentState == BubbleBreakState.Breaking;
    public bool IsNotBreaking() => CurrentState == BubbleBreakState.NotBreaking;
}
