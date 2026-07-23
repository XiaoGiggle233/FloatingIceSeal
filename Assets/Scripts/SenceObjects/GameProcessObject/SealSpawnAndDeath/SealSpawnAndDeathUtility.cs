using UnityEngine;

/// <summary>
/// 角色出生与死亡工具集
/// 提供 Seal 的获取、销毁、重生等静态方法
/// </summary>
public static class SealSpawnAndDeathUtility
{
    private const string SEAL_POOL_KEY = "Seal";
    private const string SPAWN_POINT_KEY = "SpawnPoint";

    /// <summary>从信息池获取当前 Seal</summary>
    public static Seal GetSeal()
    {
        return InformationPool.Get<Seal>(SEAL_POOL_KEY, null);
    }

    /// <summary>从信息池获取当前出生点</summary>
    public static SpawnPointController GetSpawnPoint()
    {
        return InformationPool.Get<SpawnPointController>(SPAWN_POINT_KEY, null);
    }

    /// <summary>销毁当前 Seal（从信息池移除并销毁 GameObject）</summary>
    public static void DestroySeal()
    {
        var seal = GetSeal();
        if (seal != null)
        {
            InformationPool.Remove(SEAL_POOL_KEY);
            Object.Destroy(seal.gameObject);
        }
    }

    /// <summary>在指定位置重生 Seal</summary>
    /// <param name="sealPrefab">Seal 预制体</param>
    /// <param name="position">出生位置</param>
    /// <returns>新生成的 Seal 实例</returns>
    public static Seal RespawnSeal(Seal sealPrefab, Vector3 position)
    {
        DestroySeal();

        if (sealPrefab == null)
        {
            Debug.LogError("[SealSpawnAndDeathUtility] RespawnSeal: sealPrefab 为空");
            return null;
        }

        var newSeal = Object.Instantiate(sealPrefab, position, Quaternion.identity);
        Debug.Log($"[SealSpawnAndDeathUtility] Seal 已在 {position} 重生");
        return newSeal;
    }

    /// <summary>在出生点位置重生 Seal</summary>
    /// <param name="sealPrefab">Seal 预制体</param>
    /// <returns>新生成的 Seal 实例</returns>
    public static Seal RespawnAtSpawnPoint(Seal sealPrefab)
    {
        var spawnPoint = GetSpawnPoint();
        Vector3 pos = spawnPoint != null ? spawnPoint.transform.position : Vector3.zero;
        return RespawnSeal(sealPrefab, pos);
    }
}
