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
    private bool restorePending;

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
        ScheduleRestore();
    }

    private void OnPlayerDeath(IGameEvent evt)
    {
        ScheduleRestore();
    }

    private void OnPlayerSpawn(IGameEvent evt)
    {
        // 角色首次出生后延迟一帧记录（确保场景完全就绪），重生不重新记录
        if (hasCaptured || captureScheduled) return;
        captureScheduled = true;
        StartCoroutine(CaptureStateNextFrame());
    }

    /// <summary>安排下一帧恢复（同帧多次事件只安排一次）</summary>
    private void ScheduleRestore()
    {
        if (restorePending) return;
        restorePending = true;
        StartCoroutine(RestoreStateNextFrame());
    }

    private IEnumerator RestoreStateNextFrame()
    {
        yield return null;
        restorePending = false;
        RestoreState();
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

        // 瓦片与可重置对象都捕获到才算完成（对象快照缺失时允许下次重试，避免水雷等不恢复）
        hasCaptured = tilemapSnapshots.Count > 0 && objectSnapshots.Count > 0;
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
        foreach (var tm in FindObjectsOfType<Tilemap>(true))
        {
            tilemapSnapshots.Add(new TilemapSnapshot(tm));
        }
    }

    private void CaptureObjects()
    {
        objectSnapshots.Clear();
        // 含 inactive 对象：水雷爆炸后被禁用/销毁排队时也能被记录，保证恢复时不遗漏
        foreach (var mb in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (!(mb is ILevelResetable)) continue;
            objectSnapshots.Add(new ObjectSnapshot(mb.gameObject, FindPrefab(mb.gameObject.name)));
        }
    }

    private GameObject FindPrefab(string objectName)
    {
        if (resetablePrefabs == null) return null;
        // Unity 对重名实例自动加 " (1)" 后缀，匹配时忽略（Mine (1) -> Mine）
        string normalized = NormalizeInstanceName(objectName);
        foreach (var prefab in resetablePrefabs)
        {
            if (prefab != null && NormalizeInstanceName(prefab.name) == normalized) return prefab;
        }
        return null;
    }

    /// <summary>去掉 Unity 自动添加的 " (n)" 重名后缀</summary>
    private static string NormalizeInstanceName(string name)
    {
        int idx = name.IndexOf(" (");
        return idx > 0 ? name.Substring(0, idx) : name;
    }

    private void RestoreTilemaps()
    {
        // 已使用集合：多个同名 Tilemap（如 2 个 FloatingIce）按快照顺序一对一配对恢复
        var used = new HashSet<Tilemap>();
        foreach (var snapshot in tilemapSnapshots)
            snapshot.Restore(used);
    }

    private void RestoreObjects()
    {
        // 1. 有 prefab 的对象：按名称去重，统一清理所有旧实例（避免多实例互相误清）
        var cleanedNames = new HashSet<string>();
        foreach (var snapshot in objectSnapshots)
        {
            if (snapshot.Prefab == null) continue;
            if (cleanedNames.Add(snapshot.ObjectName))
                snapshot.CleanupSameName();
        }

        // 2. 逐个重建：每个快照从 prefab 重建一个实例（多实例各自恢复）
        foreach (var snapshot in objectSnapshots)
        {
            if (snapshot.Prefab != null)
                snapshot.RebuildFromPrefab();
        }

        // 3. 无 prefab 的对象：恢复位置
        foreach (var snapshot in objectSnapshots)
        {
            if (snapshot.Prefab == null)
                snapshot.RestorePosition();
        }

        // 恢复完成后回调（重算内部缓存）
        foreach (var mb in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (mb is ILevelResetable resetable)
                resetable.OnLevelRestore();
        }
    }

    /// <summary>单个 Tilemap 的瓦片快照（规范化名匹配，多实例按快照顺序一对一配对恢复）</summary>
    private class TilemapSnapshot
    {
        private readonly string tilemapName;
        private readonly Dictionary<Vector3Int, TileBase> tiles = new Dictionary<Vector3Int, TileBase>();

        public TilemapSnapshot(Tilemap tilemap)
        {
            tilemapName = NormalizeInstanceName(tilemap.gameObject.name);
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

        public void Restore(HashSet<Tilemap> used)
        {
            var tilemap = FindTilemap(tilemapName, used);
            if (tilemap == null) return;
            used.Add(tilemap);

            // 先清空快照记录的格子，再恢复瓦片（爆炸破坏的瓦片被重新铺上）
            foreach (var cell in tiles.Keys)
                tilemap.SetTile(cell, null);
            foreach (var kv in tiles)
                tilemap.SetTile(kv.Key, kv.Value);
        }

        private static Tilemap FindTilemap(string name, HashSet<Tilemap> used)
        {
            foreach (var tm in FindObjectsOfType<Tilemap>(true))
            {
                if (used.Contains(tm)) continue;
                if (NormalizeInstanceName(tm.gameObject.name) == name) return tm;
            }
            return null;
        }
    }

    /// <summary>
    /// 动态对象快照 —— 支持多实例：先统一清理同名旧对象，再逐个从 prefab 重建；
    /// 无 prefab 时退回恢复位置；重建时还原 Inspector 覆盖参数（缩放、collider、
    /// 水雷爆炸参数、小鱼行为/保护参数、浮冰移动参数、木箱浮力参数）与子物体状态
    /// </summary>
    private class ObjectSnapshot
    {
        private readonly GameObject gameObject;
        private readonly GameObject prefab;
        private readonly string objectName;
        private readonly string originalName;
        private readonly Vector3 position;
        private readonly Quaternion rotation;
        private readonly Transform parent;
        private readonly Vector3 localScale;
        private readonly Collider2DSnapshot colliderSnapshot;
        private readonly RendererSortingSnapshot rendererSorting;
        private readonly MineExplosionSettings? explosionSettings;
        private readonly SmallFishSettings? fishSettings;
        private readonly SmallFishProtectionSettings? fishProtectionSettings;
        private readonly FloatingIceMovingSettings? floatingIceSettings;
        private readonly WoodBoxBuoyancySettings? woodBoxSettings;
        private readonly List<ChildSnapshot> childSnapshots = new List<ChildSnapshot>();

        public GameObject Prefab => prefab;
        public string ObjectName => objectName;

        public ObjectSnapshot(GameObject gameObject, GameObject prefab)
        {
            this.gameObject = gameObject;
            this.prefab = prefab;
            objectName = NormalizeInstanceName(gameObject.name);
            originalName = gameObject.name;
            position = gameObject.transform.position;
            rotation = gameObject.transform.rotation;
            parent = gameObject.transform.parent;
            localScale = gameObject.transform.localScale;

            var collider = gameObject.GetComponent<Collider2D>();
            colliderSnapshot = collider != null ? new Collider2DSnapshot(collider) : null;

            // 记录顶层 Renderer 的图层排序（sorting layer / order in layer）
            var renderer = gameObject.GetComponent<Renderer>();
            rendererSorting = renderer != null ? new RendererSortingSnapshot(renderer) : null;

            var explosion = gameObject.GetComponent<MineExplosionController>();
            explosionSettings = explosion != null ? explosion.CaptureSettings() : (MineExplosionSettings?)null;

            var fish = gameObject.GetComponent<SmallFishController>();
            fishSettings = fish != null ? fish.CaptureSettings() : (SmallFishSettings?)null;

            var fishProtection = gameObject.GetComponent<SmallFishProtectionController>();
            fishProtectionSettings = fishProtection != null
                ? fishProtection.CaptureSettings() : (SmallFishProtectionSettings?)null;

            var floatingIce = gameObject.GetComponent<FloatingIceMovingController>();
            floatingIceSettings = floatingIce != null
                ? floatingIce.CaptureSettings() : (FloatingIceMovingSettings?)null;

            var woodBox = gameObject.GetComponent<WoodBoxBuoyancyController>();
            woodBoxSettings = woodBox != null
                ? woodBox.CaptureSettings() : (WoodBoxBuoyancySettings?)null;

            // 记录直接子物体状态（名称、顺序、本地变换、激活），恢复时还原子物体
            for (int i = 0; i < gameObject.transform.childCount; i++)
                childSnapshots.Add(new ChildSnapshot(gameObject.transform.GetChild(i), i));
        }

        /// <summary>清理所有同名旧对象（含 inactive 与排队销毁的，忽略重名后缀）</summary>
        public void CleanupSameName()
        {
            foreach (var go in FindObjectsOfType<GameObject>(true))
            {
                if (go != null && NormalizeInstanceName(go.name) == objectName)
                {
                    go.SetActive(false);
                    Destroy(go);
                }
            }
        }

        /// <summary>从 prefab 重建一个实例到记录位置，并还原 Inspector 覆盖参数</summary>
        public void RebuildFromPrefab()
        {
            GameObject instance = Instantiate(prefab, position, rotation);
            instance.name = originalName;
            if (parent != null)
                instance.transform.SetParent(parent, true);

            // 还原缩放与 collider 参数（prefab 重建会丢失场景覆盖值）
            instance.transform.localScale = localScale;
            if (colliderSnapshot != null)
            {
                var collider = instance.GetComponent<Collider2D>();
                if (collider != null)
                    colliderSnapshot.Restore(collider);
            }

            // 还原图层排序（sorting layer / order in layer）
            if (rendererSorting != null)
            {
                var renderer = instance.GetComponent<Renderer>();
                if (renderer != null)
                    rendererSorting.Restore(renderer);
            }

            // 还原水雷爆炸参数
            if (explosionSettings.HasValue)
            {
                var explosion = instance.GetComponent<MineExplosionController>();
                if (explosion != null)
                    explosion.RestoreSettings(explosionSettings.Value);
            }

            // 还原小鱼行为/保护参数
            if (fishSettings.HasValue)
            {
                var fish = instance.GetComponent<SmallFishController>();
                if (fish != null)
                    fish.RestoreSettings(fishSettings.Value);
            }
            if (fishProtectionSettings.HasValue)
            {
                var fishProtection = instance.GetComponent<SmallFishProtectionController>();
                if (fishProtection != null)
                    fishProtection.RestoreSettings(fishProtectionSettings.Value);
            }

            // 还原浮冰移动参数
            if (floatingIceSettings.HasValue)
            {
                var floatingIce = instance.GetComponent<FloatingIceMovingController>();
                if (floatingIce != null)
                    floatingIce.RestoreSettings(floatingIceSettings.Value);
            }

            // 还原木箱浮力参数
            if (woodBoxSettings.HasValue)
            {
                var woodBox = instance.GetComponent<WoodBoxBuoyancyController>();
                if (woodBox != null)
                    woodBox.RestoreSettings(woodBoxSettings.Value);
            }

            // 还原子物体（prefab 内定义的按名称恢复，场景动态添加的重新挂载）
            RestoreChildren(instance.transform);
        }

        /// <summary>无 prefab 时恢复位置（对象已销毁则跳过）</summary>
        public void RestorePosition()
        {
            if (gameObject == null) return;

            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
            var rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // 还原图层排序（运行时可能被修改）
            if (rendererSorting != null)
            {
                var renderer = gameObject.GetComponent<Renderer>();
                if (renderer != null)
                    rendererSorting.Restore(renderer);
            }

            RestoreChildren(gameObject.transform);
        }

        /// <summary>按快照还原子物体：恢复同名子物体的顺序、本地变换与激活状态</summary>
        private void RestoreChildren(Transform target)
        {
            foreach (var childSnapshot in childSnapshots)
                childSnapshot.Restore(target);
        }
    }

    /// <summary>
    /// 直接子物体快照 —— 记录名称、兄弟顺序、本地变换、激活状态与图层排序；
    /// 恢复时优先匹配父级下同名子物体，原始子物体仍存活（排队销毁中）则重新挂载
    /// </summary>
    private class ChildSnapshot
    {
        private readonly string name;
        private readonly int siblingIndex;
        private readonly Vector3 localPosition;
        private readonly Quaternion localRotation;
        private readonly Vector3 localScale;
        private readonly bool activeSelf;
        private readonly RendererSortingSnapshot rendererSorting;
        private readonly Transform originalChild;

        public ChildSnapshot(Transform child, int siblingIndex)
        {
            name = child.name;
            this.siblingIndex = siblingIndex;
            localPosition = child.localPosition;
            localRotation = child.localRotation;
            localScale = child.localScale;
            activeSelf = child.gameObject.activeSelf;
            originalChild = child;

            var renderer = child.GetComponent<Renderer>();
            rendererSorting = renderer != null ? new RendererSortingSnapshot(renderer) : null;
        }

        public void Restore(Transform parent)
        {
            Transform child = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).name == name)
                {
                    child = parent.GetChild(i);
                    break;
                }
            }

            // 父级下找不到（场景动态添加、prefab 中不存在的子物体）→ 原始子物体仍存活则挂回
            if (child == null && originalChild != null)
            {
                originalChild.SetParent(parent, true);
                child = originalChild;
            }
            if (child == null) return;

            child.SetSiblingIndex(Mathf.Min(siblingIndex, parent.childCount - 1));
            child.localPosition = localPosition;
            child.localRotation = localRotation;
            child.localScale = localScale;
            child.gameObject.SetActive(activeSelf);

            if (rendererSorting != null)
            {
                var renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                    rendererSorting.Restore(renderer);
            }
        }
    }

    /// <summary>Renderer 图层排序快照 —— 记录 sorting layer 与 order in layer</summary>
    private class RendererSortingSnapshot
    {
        private readonly int sortingLayerID;
        private readonly int sortingOrder;

        public RendererSortingSnapshot(Renderer renderer)
        {
            sortingLayerID = renderer.sortingLayerID;
            sortingOrder = renderer.sortingOrder;
        }

        public void Restore(Renderer renderer)
        {
            renderer.sortingLayerID = sortingLayerID;
            renderer.sortingOrder = sortingOrder;
        }
    }

    /// <summary>Collider2D 参数快照 —— 记录 Inspector 可覆盖的通用参数与形状参数</summary>
    private class Collider2DSnapshot
    {
        private readonly bool isTrigger;
        private readonly Vector2 offset;
        private readonly float radius; // 仅 CircleCollider2D 有效

        public Collider2DSnapshot(Collider2D collider)
        {
            isTrigger = collider.isTrigger;
            offset = collider.offset;
            radius = collider is CircleCollider2D circle ? circle.radius : 0f;
        }

        public void Restore(Collider2D collider)
        {
            collider.isTrigger = isTrigger;
            collider.offset = offset;
            if (collider is CircleCollider2D circle)
                circle.radius = radius;
        }
    }
}
