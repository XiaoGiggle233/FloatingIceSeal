using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 海豹精灵控制器
/// 监听状态机变化事件，根据氧气状态和生命状态切换对应的 Sprite，
/// 并根据方向状态翻转/旋转子物体 Sprite（只旋转子物体，不影响碰撞体）
/// </summary>
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
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        ApplyFlipAndRotation(seal.DirectionStateMachine.CurrentState);
    }

    #endregion

    #region 事件处理

    private void OnStateChanged(IGameEvent evt)
    {
        var args = evt as GameStateEventArgs;
        if (args == null) return;

        // 方向变化：翻转 Sprite X 轴并旋转
        if (Enum.TryParse(args.StateName, out DirectionState dirState))
        {
            ApplyFlipAndRotation(dirState);
            return;
        }

        ApplySpriteByState();
    }

    #endregion

    #region 精灵切换与翻转旋转

    /// <summary>根据方向状态翻转 Sprite X 轴并旋转子物体 Sprite</summary>
    private void ApplyFlipAndRotation(DirectionState state)
    {
        // 纯上/下状态没有左右信息：沿用当前 flipX 保存的最后一次水平朝向（flipX=true 朝右）
        bool facingLeft = !spriteRenderer.flipX;

        switch (state)
        {
            case DirectionState.Left:
            case DirectionState.UpLeft:
            case DirectionState.DownLeft:
                facingLeft = true;
                break;
            case DirectionState.Right:
            case DirectionState.UpRight:
            case DirectionState.DownRight:
                facingLeft = false;
                break;
        }

        spriteRenderer.flipX = !facingLeft;

        // 上：朝左顺时针90°、朝右逆时针90°；下：朝左逆时针90°、朝右顺时针90°
        // 左上顺时针45°、右上逆时针45°、左下逆时针45°、右下顺时针45°；左/右不旋转
        float zAngle = state switch
        {
            DirectionState.Up => facingLeft ? -90f : 90f,
            DirectionState.Down => facingLeft ? 90f : -90f,
            DirectionState.UpLeft => -45f,
            DirectionState.UpRight => 45f,
            DirectionState.DownLeft => 45f,
            DirectionState.DownRight => -45f,
            _ => 0f
        };

        spriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, zAngle);
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
