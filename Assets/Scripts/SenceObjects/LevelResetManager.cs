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

        GameEvents.Publish(EventType.PLAYER_EVENT_ON_DEATH,
            new PlayerEventArgs(sealGo));
    }
}
