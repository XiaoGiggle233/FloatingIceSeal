using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
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
    [ShowInInspector]
    public LifeState CurrentLifeState => LifeStateMachine?.CurrentState ?? LifeState.Alive;

    public OxygenStateMachine OxygenStateMachine { get; private set; }
    [ShowInInspector]
    public OxygenState CurrentOxygenState => OxygenStateMachine?.CurrentState ?? OxygenState.OxygenFull;

    public EnvironmentStateMachine EnvironmentStateMachine { get; private set; }
    [ShowInInspector]
    public EnvironmentState CurrentEnvironmentState => EnvironmentStateMachine?.CurrentState ?? EnvironmentState.InAir;

    public ActionStateMachine ActionStateMachine { get; private set; }
    [ShowInInspector]
    public ActionState CurrentActionState => ActionStateMachine?.CurrentState ?? ActionState.Idle;

    public DirectionStateMachine DirectionStateMachine { get; private set; }
    [ShowInInspector]
    public DirectionState CurrentDirectionState => DirectionStateMachine?.CurrentState ?? DirectionState.Right;

    #endregion

    #region 碰撞检测

    public void PublishCollisionEvent(GameObject other, Vector2 normal)
    {
        GameEvents.Publish(EventType.COLLISION_EVENT_ON_ENTER,
            new CollisionEventArgs(this.gameObject, other, normal));
    }

    public void PublishTriggerEvent(GameObject other)
    {
        GameEvents.Publish(EventType.COLLISION_EVENT_ON_TRIGGER,
            new CollisionEventArgs(this.gameObject, other, Vector2.zero));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 normal = collision.contactCount > 0 ? collision.contacts[0].normal : Vector2.zero;
        PublishCollisionEvent(collision.gameObject, normal);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PublishTriggerEvent(other.gameObject);
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
            OxygenController.RecoverOxygen(bubble.oxygen * Model.BubbleOxygenRecoverRatio);
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
        DirectionStateMachine = new DirectionStateMachine(this);
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

    private void OnDestroy()
    {
        // 仅当信息池中仍是自身时才移除，避免误删重生后的新实例
        if (InformationPool.TryGet("Seal", out Seal current) && current == this)
        {
            InformationPool.Remove("Seal");
        }
    }

    private void Update()
    {
        LifeStateMachine.Update();
        OxygenStateMachine.Update();
        EnvironmentStateMachine.Update();
        ActionStateMachine.Update();
        DirectionStateMachine.Update();
    }

    private void FixedUpdate()
    {
        LifeStateMachine.FixedUpdate();
        OxygenStateMachine.FixedUpdate();
        EnvironmentStateMachine.FixedUpdate();
        ActionStateMachine.FixedUpdate();
        DirectionStateMachine.FixedUpdate();
    }

    #endregion
}

