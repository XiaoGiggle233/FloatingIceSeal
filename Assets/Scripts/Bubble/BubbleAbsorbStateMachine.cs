/// <summary>
/// 大气泡吸收状态 —— 是否正在被海豹吸收
/// </summary>
public enum BubbleAbsorbState
{
    /// <summary>未被吸收</summary>
    NotAbsorbing,
    /// <summary>正在被海豹吸收（上浮力大幅降低）</summary>
    Absorbing
}

/// <summary>大气泡吸收状态机 —— 管理是否被海豹吸收的状态切换</summary>
public class BubbleAbsorbStateMachine
{
    public BubbleAbsorbState CurrentState { get; private set; } = BubbleAbsorbState.NotAbsorbing;

    public void SetState(BubbleAbsorbState state)
    {
        CurrentState = state;
    }

    public bool IsAbsorbing() => CurrentState == BubbleAbsorbState.Absorbing;
    public bool IsNotAbsorbing() => CurrentState == BubbleAbsorbState.NotAbsorbing;
}
