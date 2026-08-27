using System.Collections;
using UnityEngine;

public class SealSpawnAndDeathManager : MonoBehaviour
{
    [SerializeField] private Seal sealPrefab;

    [Header("死亡后重生延迟（与 LevelResetSystem 的死亡重置延迟保持一致）")]
    [SerializeField] private float respawnDelay = 1.5f;

    private bool isRespawning;
    private Coroutine respawnCoroutine;

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_DEATH, OnSealDeath);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_DEATH, OnSealDeath);
        respawnCoroutine = null;
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

        // 手动重置链：立即重生（关卡同步立即恢复），并取消挂起的死亡延迟重生
        if (args.IsManualReset)
        {
            CancelRespawn();
            SealSpawnAndDeathUtility.RespawnAtSpawnPoint(sealPrefab);
            return;
        }

        // 同一帧多次死亡事件（如多水雷连锁爆炸）或延迟重生挂起时只处理一次
        if (isRespawning || respawnCoroutine != null) return;
        isRespawning = true;
        StartCoroutine(ResetRespawnFlag());

        // 不销毁尸体：进入 Dead 状态停止角色行为，延迟后重生时由 RespawnSeal 销毁
        var seal = SealSpawnAndDeathUtility.GetSeal();
        if (seal != null)
            seal.LifeStateMachine.SetState(LifeState.Dead);

        respawnCoroutine = StartCoroutine(RespawnCoroutine());
    }

    private void CancelRespawn()
    {
        if (respawnCoroutine == null) return;
        StopCoroutine(respawnCoroutine);
        respawnCoroutine = null;
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        respawnCoroutine = null;
        SealSpawnAndDeathUtility.RespawnAtSpawnPoint(sealPrefab);
    }

    private IEnumerator ResetRespawnFlag()
    {
        yield return null;
        isRespawning = false;
    }
}
