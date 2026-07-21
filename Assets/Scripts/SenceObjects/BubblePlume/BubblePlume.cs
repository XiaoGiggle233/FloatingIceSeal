using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BubblePlume : MonoBehaviour
{
    [Title("小气泡预制体")]
    [LabelText("小气泡预制体")]
    [SerializeField, Required, InfoBox("请拖入小气泡预制体")]
    private GameObject smallBubblePrefab;

    [Title("生成设置")]
    [LabelText("生成间隔"), SuffixLabel("秒", true)]
    [SerializeField, MinValue(0.1f)]
    private float spawnInterval = 2f;

    [Title("小气泡参数")]
    [LabelText("氧气量")]
    [SerializeField, MinValue(0)]
    private int oxygen = 1;

    [LabelText("上浮速度")]
    [SerializeField, MinValue(0.1f)]
    private float speed = 1f;

    [LabelText("出水破裂距离")]
    [SerializeField, MinValue(0f)]
    private float outOfWaterBurstDistance = 0.5f;

    [LabelText("上升破裂距离")]
    [SerializeField, MinValue(0f)]
    private float riseBurstDistance = 10f;

    [Title("信息池")]
    [LabelText("信息池键名")]
    [SerializeField] private string infoPoolKey = "BubblePlume";

    private float timer;
    private Tilemap tilemap;
    private List<Vector3> tileWorldPositions = new List<Vector3>();

    private void OnEnable()
    {
        InformationPool.Set(infoPoolKey, transform);
    }

    private void OnDisable()
    {
        InformationPool.Remove(infoPoolKey);
    }

    private void Start()
    {
        timer = 0f;
        CacheTilePositions();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnSmallBubbles();
        }
    }

    private void CacheTilePositions()
    {
        tileWorldPositions.Clear();
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null) return;

        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(cellPos))
                {
                    Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
                    tileWorldPositions.Add(worldPos);
                }
            }
        }
    }

    private void SpawnSmallBubbles()
    {
        if (smallBubblePrefab == null) return;

        foreach (Vector3 pos in tileWorldPositions)
        {
            GameObject instance = Instantiate(smallBubblePrefab, pos, Quaternion.identity);

            if (instance.TryGetComponent<SmallBubble>(out var bubble))
            {
                bubble.oxygen = oxygen;
                bubble.speed = speed;
                bubble.outOfWaterBurstDistance = outOfWaterBurstDistance;
                bubble.riseBurstDistance = riseBurstDistance;
            }
        }
    }
}
