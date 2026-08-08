using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 关卡重置系统 —— 手动记录关卡初始状态（瓦片 + 动态对象），
/// 角色死亡（PLAYER_EVENT_ON_DEATH）或重置关卡（UI_EVENT_ON_RESET_LEVEL）时恢复到初始状态
/// </summary>
public class LevelResetSystem : MonoBehaviour
{
    [Header("可重建对象 prefab（被销毁后重新生成，按名称匹配场景对象）")]
    [SerializeField] private GameObject[] resetablePrefabs;

    private readonly List<TilemapSnapshot> tilemapSnapshots = new List<TilemapSnapshot>();
    private readonly List<ObjectSnapshot> objectSnapshots = new List<ObjectSnapshot>();
    private bool hasCaptured;
    private bool captureScheduled;

    private void OnEnable()
    {
        GameEvents.Listen(EventType.UI_EVENT_ON_RESET_LEVEL, OnResetLevel);
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_DEATH, OnPlayerDeath);
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_SPAWN, OnPlayerSpawn);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.UI_EVENT_ON_RESET_LEVEL, OnResetLevel);
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_DEATH, OnPlayerDeath);
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_SPAWN, OnPlayerSpawn);
    }

    private void OnResetLevel(IGameEvent evt)
    {
        RestoreState();
    }

    private void OnPlayerDeath(IGameEvent evt)
    {
        RestoreState();
    }

    private void OnPlayerSpawn(IGameEvent evt)
    {
        // 角色首次出生后延迟一帧记录（确保场景完全就绪），重生不重新记录
        if (hasCaptured || captureScheduled) return;
        captureScheduled = true;
        StartCoroutine(CaptureStateNextFrame());
    }

    private IEnumerator CaptureStateNextFrame()
    {
        yield return null;
        captureScheduled = false;
        CaptureState();
    }

    /// <summary>记录当前关卡状态为初始状态（关卡开始时手动调用）</summary>
    public void CaptureState()
    {
        CaptureTilemaps();
        CaptureObjects();

        // 场景未就绪（未抓到任何快照）时不算记录完成，等待下次触发
        hasCaptured = tilemapSnapshots.Count > 0 || objectSnapshots.Count > 0;
    }

    /// <summary>将关卡恢复到记录的初始状态</summary>
    public void RestoreState()
    {
        if (!hasCaptured) CaptureState();

        // 先重建/恢复对象，再铺瓦片（重建的 Tilemap 机制可被重新铺上瓦片）
        RestoreObjects();
        RestoreTilemaps();
    }

    private void CaptureTilemaps()
    {
        tilemapSnapshots.Clear();
        foreach (var tm in FindObjectsOfType<Tilemap>())
        {
            tilemapSnapshots.Add(new TilemapSnapshot(tm));
        }
    }

    private void CaptureObjects()
    {
        objectSnapshots.Clear();
        foreach (var mb in FindObjectsOfType<MonoBehaviour>())
        {
            if (!(mb is ILevelResetable)) continue;
            objectSnapshots.Add(new ObjectSnapshot(mb.gameObject, FindPrefab(mb.gameObject.name)));
        }
    }

    private GameObject FindPrefab(string objectName)
    {
        if (resetablePrefabs == null) return null;
        foreach (var prefab in resetablePrefabs)
        {
            if (prefab != null && prefab.name == objectName) return prefab;
        }
        return null;
    }

    private void RestoreTilemaps()
    {
        foreach (var snapshot in tilemapSnapshots)
            snapshot.Restore();
    }

    private void RestoreObjects()
    {
        foreach (var snapshot in objectSnapshots)
            snapshot.Restore();

        // 恢复完成后回调（重算内部缓存）
        foreach (var mb in FindObjectsOfType<MonoBehaviour>())
        {
            if (mb is ILevelResetable resetable)
                resetable.OnLevelRestore();
        }
    }

    /// <summary>单个 Tilemap 的瓦片快照（按名称匹配恢复，支持对象销毁重建）</summary>
    private class TilemapSnapshot
    {
        private readonly string tilemapName;
        private readonly Dictionary<Vector3Int, TileBase> tiles = new Dictionary<Vector3Int, TileBase>();

        public TilemapSnapshot(Tilemap tilemap)
        {
            tilemapName = tilemap.gameObject.name;
            BoundsInt bounds = tilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                    if (tilemap.HasTile(cell))
                        tiles[cell] = tilemap.GetTile(cell);
                }
            }
        }

        public void Restore()
        {
            var tilemap = FindTilemap(tilemapName);
            if (tilemap == null) return;

            // 先清空快照记录的格子，再恢复瓦片（爆炸破坏的瓦片被重新铺上）
            foreach (var cell in tiles.Keys)
                tilemap.SetTile(cell, null);
            foreach (var kv in tiles)
                tilemap.SetTile(kv.Key, kv.Value);
        }

        private static Tilemap FindTilemap(string name)
        {
            foreach (var tm in FindObjectsOfType<Tilemap>())
            {
                if (tm.gameObject.name == name) return tm;
            }
            return null;
        }
    }

    /// <summary>
    /// 动态对象快照 —— 恢复时总是从 prefab 重建（含排队销毁的对象），
    /// 无 prefab 时退回恢复位置
    /// </summary>
    private class ObjectSnapshot
    {
        private readonly GameObject gameObject;
        private readonly GameObject prefab;
        private readonly string objectName;
        private readonly Vector3 position;
        private readonly Quaternion rotation;
        private readonly Transform parent;

        public ObjectSnapshot(GameObject gameObject, GameObject prefab)
        {
            this.gameObject = gameObject;
            this.prefab = prefab;
            objectName = gameObject.name;
            position = gameObject.transform.position;
            rotation = gameObject.transform.rotation;
            parent = gameObject.transform.parent;
        }

        public void Restore()
        {
            if (prefab != null)
            {
                // 清理所有同名旧对象（含 inactive 与排队销毁的，避免多次重建残留），再从 prefab 重建
                foreach (var go in FindObjectsOfType<GameObject>(true))
                {
                    if (go != null && go.name == objectName)
                    {
                        go.SetActive(false);
                        Destroy(go);
                    }
                }

                GameObject instance = Instantiate(prefab, position, rotation);
                instance.name = objectName;
                if (parent != null)
                    instance.transform.SetParent(parent, true);
            }
            else if (gameObject != null)
            {
                // 无 prefab 时退回恢复位置
                gameObject.transform.position = position;
                gameObject.transform.rotation = rotation;
                var rb = gameObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }
        }
    }
}
