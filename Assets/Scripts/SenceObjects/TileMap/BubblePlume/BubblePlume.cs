using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 气泡柱 —— 触发器区域，角色进入时按空气中速率恢复氧气
/// 需挂载 TilemapCollider2D + Rigidbody2D + CompositeCollider2D（IsTrigger）作为触发器区域
/// </summary>
public class BubblePlume : MonoBehaviour
{
    [Title("信息池")]
    [LabelText("信息池键名")]
    [SerializeField] private string infoPoolKey = "BubblePlume";

    private void OnEnable()
    {
        InformationPool.Set(infoPoolKey, transform);
    }

    private void OnDisable()
    {
        InformationPool.Remove(infoPoolKey);
    }
}
