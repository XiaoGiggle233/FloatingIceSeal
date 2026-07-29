using UnityEngine;

public class SealSpawnAndDeathManager : MonoBehaviour
{
    [SerializeField] private Seal sealPrefab;

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_DEATH, OnSealDeath);

        // 启用时在出生点生成角色
        if (SealSpawnAndDeathUtility.GetSeal() == null)
        {
            SealSpawnAndDeathUtility.RespawnAtSpawnPoint(sealPrefab);
        }
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_DEATH, OnSealDeath);
    }

    private void OnSealDeath(IGameEvent evt)
    {
        var args = evt as PlayerEventArgs;
        if (args == null) return;

        Debug.Log("[SealSpawnAndDeathManager] 收到角色死亡事件，准备重生");

        // 销毁信息池中的 Seal 并在出生点重新生成
        SealSpawnAndDeathUtility.RespawnAtSpawnPoint(sealPrefab);
    }
}
