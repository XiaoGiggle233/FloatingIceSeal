using UnityEngine;

/// <summary>
/// 海豹吐泡泡控制器
/// 临时键位：J —— 长按蓄力吐泡泡（仅水中、氧气充足）
/// 长按越久，泡泡储存的氧气越高，同时持续消耗氧气
/// </summary>
public class SealBlowBubbleController : MonoBehaviour
{
    [Header("临时键位")]
    [SerializeField] private KeyCode blowBubbleKey = KeyCode.J;

    [Header("泡泡")]
    [SerializeField] private BubbleBase bubblePrefab;

    private Seal seal;
    private SealModel model;
    private SealOxygenController oxygenController;

    private bool isCharging;
    private BubbleBase currentBubble;
    private Rigidbody2D bubbleRb;
    private float chargedOxygen;
    private DirectionState lastHorizontalDirection = DirectionState.Right;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        model = GetComponent<SealModel>();
        oxygenController = GetComponent<SealOxygenController>();
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_DEATH, OnPlayerDeath);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_DEATH, OnPlayerDeath);
    }

    /// <summary>角色死亡/关卡重置时销毁未释放的蓄力泡泡</summary>
    private void OnPlayerDeath(IGameEvent evt)
    {
        if (currentBubble != null)
        {
            Destroy(currentBubble.gameObject);
            currentBubble = null;
            bubbleRb = null;
            isCharging = false;
            chargedOxygen = 0f;
        }
    }

    private void Update()
    {
        TrackLastHorizontalDirection();

        if (!seal.LifeStateMachine.IsAlive() || !seal.EnvironmentStateMachine.IsInWater())
        {
            if (isCharging) ReleaseBubble();
            return;
        }

        if (Input.GetKeyDown(blowBubbleKey))
        {
            StartCharging();
        }

        if (isCharging)
        {
            if (Input.GetKey(blowBubbleKey))
            {
                ChargeBubble();
            }
            else
            {
                ReleaseBubble();
            }
        }
    }

    private void TrackLastHorizontalDirection()
    {
        switch (seal.CurrentDirectionState)
        {
            case DirectionState.Left:
            case DirectionState.UpLeft:
            case DirectionState.DownLeft:
            case DirectionState.Right:
            case DirectionState.UpRight:
            case DirectionState.DownRight:
                lastHorizontalDirection = seal.CurrentDirectionState;
                break;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 offset;
        switch (lastHorizontalDirection)
        {
            case DirectionState.Left:
            case DirectionState.UpLeft:
            case DirectionState.DownLeft:
                offset = Vector3.left * model.BlowBubbleSpawnOffset;
                break;
            default:
                offset = Vector3.right * model.BlowBubbleSpawnOffset;
                break;
        }
        return transform.position + offset;
    }

    private void StartCharging()
    {
        if (bubblePrefab == null) return;
        if (!oxygenController.HasEnoughOxygen(model.BlowBubbleChargeOxygenRate * Time.deltaTime)) return;

        isCharging = true;
        chargedOxygen = 0f;
        Vector3 spawnPos = GetSpawnPosition();
        InformationPool.Set("BlowBubbleSpawnPos", spawnPos);
        currentBubble = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
        bubbleRb = currentBubble.GetComponent<Rigidbody2D>();
        if (bubbleRb != null) bubbleRb.simulated = false;
    }

    private void ChargeBubble()
    {
        if (currentBubble == null)
        {
            ReleaseBubble();
            return;
        }

        float consumeAmount = model.BlowBubbleChargeOxygenRate * Time.deltaTime;
        float actual = oxygenController.ConsumeOxygen(consumeAmount);
        if (actual <= 0f)
        {
            ReleaseBubble();
            return;
        }

        chargedOxygen += actual;
        currentBubble.oxygen = chargedOxygen;
        Vector3 spawnPos = GetSpawnPosition();
        InformationPool.Set("BlowBubbleSpawnPos", spawnPos);
        currentBubble.transform.position = spawnPos;
    }

    private void ReleaseBubble()
    {
        isCharging = false;

        if (currentBubble != null)
        {
            currentBubble.oxygen = chargedOxygen;

            GameEvents.Publish(EventType.PLAYER_EVENT_ON_BLOW_BUBBLE,
                new BubbleBlowEventArgs(this.gameObject, currentBubble.gameObject, chargedOxygen));

            GameEvents.Publish(EventType.BUBBLE_EVENT_ON_RELEASE,
                new BubbleBlowEventArgs(this.gameObject, currentBubble.gameObject, chargedOxygen));

            if (bubbleRb != null)
            {
                bubbleRb.simulated = true;
                bubbleRb.velocity = Vector2.up * currentBubble.speed;
            }
        }

        currentBubble = null;
        bubbleRb = null;
    }
}
