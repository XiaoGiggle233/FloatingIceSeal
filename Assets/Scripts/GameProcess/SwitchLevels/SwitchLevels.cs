using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡切换控制器 —— 监听角色与 WiningArea 的碰撞，完成关卡切换流程
/// </summary>
public class SwitchLevels : MonoBehaviour
{
    [SerializeField] private int _currentLevel = 1;

    private bool _isSwitching = false;

    private void OnEnable()
    {
        GameEvents.Listen(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEvent);
        GameEvents.Listen(EventType.COLLISION_EVENT_ON_TRIGGER, OnCollisionEvent);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEvent);
        GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_TRIGGER, OnCollisionEvent);
    }

    private void OnCollisionEvent(IGameEvent evt)
    {
        if (_isSwitching) return;

        var args = evt as CollisionEventArgs;
        if (args == null) return;

        // 从信息池获取 WiningArea 和 Seal
        var winingArea = InformationPool.Get<WiningAreaController>("WiningArea");
        var seal = InformationPool.Get<Seal>("Seal");
        if (winingArea == null || seal == null) return;

        // 判断是否为 Seal 与 WiningArea 之间的碰撞
        bool isMatch = (args.Source == seal.gameObject && args.Target == winingArea.gameObject)
                    || (args.Target == seal.gameObject && args.Source == winingArea.gameObject);
        if (!isMatch) return;

        // 检查角色是否存活
        if (!seal.LifeStateMachine.IsAlive()) return;

        _isSwitching = true;

        // 发布关卡完成事件
        GameEvents.Publish(EventType.GAME_EVENT_ON_START,
            new GameStateEventArgs("LevelComplete"));

        // 获取当前存档槽位
        int slotIndex = InformationPool.Get<int>("CurrentSlotIndex", 0);

        // 根据场景中存储的当前关卡数计算下一关
        int nextLevel = _currentLevel + 1;

        // 更新存档
        SaveManager.SetCurrentLevel(slotIndex, nextLevel);

        // 切换至下一关卡：先记录目标场景，再进入加载场景
        string nextSceneName = $"Level{nextLevel}";
        InformationPool.Set("TargetSceneName", nextSceneName);
        SceneManager.LoadScene("LoadingSence");
    }
}
