using UnityEngine;

public class LevelResetManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.Listen(EventType.UI_EVENT_ON_RESET_LEVEL, OnResetLevel);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.UI_EVENT_ON_RESET_LEVEL, OnResetLevel);
    }

    private void OnResetLevel(IGameEvent e)
    {
        var seal = SealSpawnAndDeathUtility.GetSeal();
        var sealGo = seal != null ? seal.gameObject : null;

        SealSpawnAndDeathUtility.DestroySeal();

        // 手动重置链的死亡事件带标记：角色立即重生，不走死亡延迟
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_DEATH,
            new PlayerEventArgs(sealGo, true));
    }
}
