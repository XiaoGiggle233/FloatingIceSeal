using UnityEngine;

public abstract class BubbleControllerBase : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected BubbleBase bubbleBase;
    protected float speed;

    protected GameObject seal;
    protected GameObject spike;

    protected virtual void Start()
    {
        bubbleBase = GetComponent<BubbleBase>();
        rb = GetComponent<Rigidbody2D>();
        speed = bubbleBase.speed;
        rb.velocity = Vector2.up * speed;

        seal = ResolveGameObject("Seal");
        spike = ResolveGameObject("Spike");
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEvent);
        GameEvents.Listen(EventType.COLLISION_EVENT_ON_TRIGGER, OnCollisionEvent);
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_DEATH, OnPlayerDeath);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEvent);
        GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_TRIGGER, OnCollisionEvent);
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_DEATH, OnPlayerDeath);
    }

    protected virtual void Update()
    {
        if (!WatterUtils.HasWaterBelow(transform.position, bubbleBase.outOfWaterBurstDistance))
        {
            bubbleBase.Burst();
            return;
        }

        TryAbsorbBySeal();
    }

    /// <summary>
    /// 检测海豹是否在吸收范围内（随泡泡大小）——持续吸收：角色按速率恢复氧气，
    /// 泡泡按速率减少氧气；泡泡含氧量降到角色最大氧气量的六分之一以下时破裂
    /// </summary>
    private void TryAbsorbBySeal()
    {
        // 未释放的大泡泡不可被吸收
        if (bubbleBase is BigBubble bigBubble && bigBubble.State == BigBubbleState.BeforeRelease)
            return;

        var seal = InformationPool.Get<Seal>("Seal", null);
        if (seal == null || !seal.LifeStateMachine.IsAlive()) return;

        float absorbRadius = GetAbsorbRadius();
        if (absorbRadius <= 0f) return;

        bool inRange = Vector2.Distance(transform.position, seal.transform.position) <= absorbRadius;

        // 通知大气泡是否正在被吸收（进入/退出吸收状态，离开范围自动恢复）
        if (bubbleBase is BigBubble absorbable)
            absorbable.SetAbsorbing(inRange);

        if (!inRange) return;

        float absorbAmount = seal.Model.BubbleAbsorbRate * Time.deltaTime;
        seal.OxygenController.RecoverOxygen(absorbAmount);
        bubbleBase.oxygen -= absorbAmount;

        if (bubbleBase.oxygen <= seal.Model.OxygenMaxValue / 6f)
        {
            bubbleBase.Burst();
        }
    }

    /// <summary>吸收范围 = CircleCollider2D 世界半径（随泡泡缩放变化）</summary>
    private float GetAbsorbRadius()
    {
        var circle = GetComponent<CircleCollider2D>();
        return circle != null ? circle.bounds.extents.x : 0f;
    }

    private void OnCollisionEvent(IGameEvent evt)
    {
        var args = evt as CollisionEventArgs;
        if (args == null) return;
        if (args.Source != this.gameObject && args.Target != this.gameObject) return;

        GameObject other = args.Source == this.gameObject ? args.Target : args.Source;

        // 动态从信息池获取 Seal，避免缓存已销毁的旧引用
        var currentSeal = ResolveGameObject("Seal");
        var currentSpike = ResolveGameObject("Spike");

        if (other == currentSeal || other == currentSpike)
        {
            bubbleBase.Burst();
        }
    }

    private void OnPlayerDeath(IGameEvent evt)
    {
        bubbleBase.Burst();
    }

    protected static GameObject ResolveGameObject(string key)
    {
        if (InformationPool.TryGet(key, out object obj))
        {
            if (obj is GameObject go) return go;
            if (obj is Component comp) return comp.gameObject;
        }
        return null;
    }
}
