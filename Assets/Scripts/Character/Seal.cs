using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 主角（海豹）类
/// </summary>
public class Seal : MonoBehaviour, ICollisionEventPublisher
{
    #region 数据模型

    public SealModel Model { get; private set; }

    #endregion

    #region 控制器

    public SealOxygenController OxygenController { get; private set; }

    #endregion

    #region 状态机

    public LifeStateMachine LifeStateMachine { get; private set; }
    public OxygenStateMachine OxygenStateMachine { get; private set; }
    public EnvironmentStateMachine EnvironmentStateMachine { get; private set; }
    public ActionStateMachine ActionStateMachine { get; private set; }

    #endregion

    #region 碰撞检测

    public void PublishCollisionEvent(GameObject other)
    {
        GameEvents.Publish(EventType.COLLISION_EVENT_ON_ENTER,
            new CollisionEventArgs(this.gameObject, other));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PublishCollisionEvent(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameEvents.Publish(EventType.COLLISION_EVENT_ON_TRIGGER,
            new CollisionEventArgs(this.gameObject, other.gameObject));
    }

    private void OnCollisionEvent(IGameEvent evt)
    {
        var args = evt as CollisionEventArgs;
        if (args == null) return;
        // 海豹只响应与气泡的碰撞
        if (args.Source != this.gameObject && args.Target != this.gameObject) return;

        BubbleBase bubble = null;
        if (args.Source == this.gameObject)
            bubble = args.Target.GetComponent<BubbleBase>();
        else
            bubble = args.Source.GetComponent<BubbleBase>();

        if (bubble != null)
        {
            OxygenController.RecoverOxygen(bubble.oxygen);
        }
    }

    #endregion

    #region Unity 生命周期

    private void Awake()
    {
        Model = GetComponent<SealModel>();
        OxygenController = GetComponent<SealOxygenController>();
        LifeStateMachine = new LifeStateMachine(this);
        OxygenStateMachine = new OxygenStateMachine(this);
        EnvironmentStateMachine = new EnvironmentStateMachine(this);
        ActionStateMachine = new ActionStateMachine(this);
    }

    private void Start()
    {
        InformationPool.Set("Seal", this);
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_SPAWN,
            new PlayerEventArgs(this.gameObject));
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEvent);
        GameEvents.Listen(EventType.COLLISION_EVENT_ON_TRIGGER, OnCollisionEvent);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEvent);
        GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_TRIGGER, OnCollisionEvent);
    }

    private void Update()
    {
        LifeStateMachine.Update();
        OxygenStateMachine.Update();
        EnvironmentStateMachine.Update();
        ActionStateMachine.Update();
    }

    private void FixedUpdate()
    {
        LifeStateMachine.FixedUpdate();
        OxygenStateMachine.FixedUpdate();
        EnvironmentStateMachine.FixedUpdate();
        ActionStateMachine.FixedUpdate();
    }

    #endregion
}

