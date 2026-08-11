/// <summary>
/// 小鱼状态 —— 巡逻 / 等待 / 追击 / 破坏
/// </summary>
public enum SmallFishState
{
    /// <summary>巡逻：自由移动，遇障碍转向，检测玩家释放的大气泡</summary>
    Patrol,
    /// <summary>等待：发现气泡后停止等待</summary>
    Wait,
    /// <summary>追击：向目标气泡移动</summary>
    Chase,
    /// <summary>破坏：接触气泡后计时，计时结束气泡破裂</summary>
    Break
}

/// <summary>小鱼状态机主控 —— 管理巡逻/等待/追击/破坏状态切换</summary>
public class SmallFishStateMachine
{
    public SmallFishState CurrentState { get; private set; } = SmallFishState.Patrol;

    public void SetState(SmallFishState state)
    {
        if (CurrentState == state) return;
        CurrentState = state;
    }

    public bool IsPatrol() => CurrentState == SmallFishState.Patrol;
    public bool IsWait() => CurrentState == SmallFishState.Wait;
    public bool IsChase() => CurrentState == SmallFishState.Chase;
    public bool IsBreak() => CurrentState == SmallFishState.Break;
}
