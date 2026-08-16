using System.Collections;
using UnityEngine;

public class SealSpawnAndDeathManager : MonoBehaviour
{
    [SerializeField] private Seal sealPrefab;

    private bool isRespawning;

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_DEATH, OnSealDeath);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_DEATH, OnSealDeath);
    }

    private void Start()
    {
        // 出生点在场景加载的 OnEnable 阶段注册到信息池；Start 晚于所有 OnEnable，
        // 避免打包后初始化顺序不同导致读不到出生点而在 (0,0) 生成
        if (SealSpawnAndDeathUtility.GetSeal() == null)
        {
            SealSpawnAndDeathUtility.RespawnAtSpawnPoint(sealPrefab);
        }
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
