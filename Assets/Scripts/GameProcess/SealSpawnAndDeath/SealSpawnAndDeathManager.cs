using System.Collections;
using UnityEngine;

public class SealSpawnAndDeathManager : MonoBehaviour
{
    [SerializeField] private Seal sealPrefab;

    private bool isRespawning;

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

        // 同一帧多次死亡事件（如多水雷连锁爆炸）只重生一次，避免生成多个 Seal
        if (isRespawning) return;
        isRespawning = true;
        StartCoroutine(ResetRespawnFlag());

        SealSpawnAndDeathUtility.RespawnAtSpawnPoint(sealPrefab);
    }

    private IEnumerator ResetRespawnFlag()
    {
        yield return null;
        isRespawning = false;
    }
}
