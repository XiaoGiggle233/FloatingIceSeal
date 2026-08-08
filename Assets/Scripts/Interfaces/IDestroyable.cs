using UnityEngine;

/// <summary>
/// 可摧毁接口
/// 实现此接口的物体具备被爆炸摧毁的能力
/// </summary>
public interface IDestroyable
{
    /// <summary>被爆炸摧毁（按爆炸中心与半径破坏自身）</summary>
    /// <param name="explosionCenter">爆炸中心</param>
    /// <param name="explosionRadius">爆炸半径</param>
    void DestroyByExplosion(Vector2 explosionCenter, float explosionRadius);
}
