using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 海豹精灵控制器
/// 监听状态机变化事件，根据氧气状态和生命状态切换对应的 Sprite
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SealSpriteController : MonoBehaviour
{
    #region 精灵序列化字段

    [FoldoutGroup("氧气状态精灵", expanded: true)]
    [PreviewField(60), LabelText("氧气充足")]
    [SerializeField] private Sprite oxygenFullSprite;

    [FoldoutGroup("氧气状态精灵")]
    [PreviewField(60), LabelText("氧气足够")]
    [SerializeField] private Sprite oxygenSufficientSprite;

    [FoldoutGroup("氧气状态精灵")]
    [PreviewField(60), LabelText("氧气不足")]
    [SerializeField] private Sprite oxygenInsufficientSprite;

    [FoldoutGroup("氧气状态精灵")]
    [PreviewField(60), LabelText("氧气危急")]
    [SerializeField] private Sprite oxygenCriticalSprite;

    [FoldoutGroup("氧气状态精灵")]
    [PreviewField(60), LabelText("窒息")]
    [SerializeField] private Sprite suffocatingSprite;

    [FoldoutGroup("生命状态精灵", expanded: true)]
    [PreviewField(60), LabelText("死亡")]
    [SerializeField] private Sprite deadSprite;

    #endregion

    #region 私有字段

    private SpriteRenderer spriteRenderer;
    private Seal seal;

    #endregion

    #region Unity 生命周期

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        seal = GetComponent<Seal>();
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_STATE_CHANGE, OnStateChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_STATE_CHANGE, OnStateChanged);
    }

    private void Start()
    {
        ApplySpriteByState();
        ApplyFlipByDirection(seal.DirectionStateMachine.CurrentState);
    }

    #endregion

    #region 事件处理

    private void OnStateChanged(IGameEvent evt)
    {
        var args = evt as GameStateEventArgs;
        if (args == null) return;

        // 方向变化：翻转 Sprite X 轴
        if (Enum.TryParse(args.StateName, out DirectionState dirState))
        {
            ApplyFlipByDirection(dirState);
            return;
        }

        ApplySpriteByState();
    }

    #endregion

    #region 精灵切换与翻转

    /// <summary>根据方向状态翻转 Sprite X 轴</summary>
    private void ApplyFlipByDirection(DirectionState state)
    {
        spriteRenderer.flipX = state switch
        {
            DirectionState.Right or DirectionState.UpRight or DirectionState.DownRight => true,
            DirectionState.Left or DirectionState.UpLeft or DirectionState.DownLeft => false,
            _ => spriteRenderer.flipX
        };
    }

    private void ApplySpriteByState()
    {
        // 死亡状态优先
        if (seal.LifeStateMachine.IsDead())
        {
            if (deadSprite != null)
                spriteRenderer.sprite = deadSprite;
            return;
        }

        // 根据氧气状态切换
        Sprite target = GetOxygenSprite(seal.OxygenStateMachine.CurrentState);
        if (target != null)
            spriteRenderer.sprite = target;
    }

    private Sprite GetOxygenSprite(OxygenState state)
    {
        return state switch
        {
            OxygenState.OxygenFull => oxygenFullSprite,
            OxygenState.OxygenSufficient => oxygenSufficientSprite,
            OxygenState.OxygenInsufficient => oxygenInsufficientSprite,
            OxygenState.OxygenCritical => oxygenCriticalSprite,
            OxygenState.Suffocating => suffocatingSprite,
            _ => null
        };
    }

    #endregion
}
