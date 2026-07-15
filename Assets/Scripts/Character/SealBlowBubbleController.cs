using UnityEngine;

/// <summary>
/// 海豹吐泡泡控制器
/// 临时键位：J —— 吐泡泡（仅水中、氧气充足）
/// </summary>
public class SealBlowBubbleController : MonoBehaviour
{
    [Header("临时键位")]
    [SerializeField] private KeyCode blowBubbleKey = KeyCode.J;

    [Header("泡泡")]
    [SerializeField] private BubbleBase bubblePrefab;

    private Seal seal;
    private SealModel model;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        model = GetComponent<SealModel>();
    }

    private void Update()
    {
        if (!IsAlive() || !IsInWater()) return;

        if (Input.GetKeyDown(blowBubbleKey))
        {
            if (model.OxygenValue < model.BlowBubbleOxygenCost) return;
            if (bubblePrefab == null) return;

            model.OxygenValue -= model.BlowBubbleOxygenCost;
            Instantiate(bubblePrefab, transform.position, Quaternion.identity);
            GameEvents.Publish(EventType.PLAYER_EVENT_ON_BLOW_BUBBLE,
                new PlayerEventArgs(this.gameObject));
        }
    }

    private bool IsAlive()
    {
        return seal.LifeStateMachine.CurrentState == LifeState.Alive;
    }

    private bool IsInWater()
    {
        return seal.EnvironmentStateMachine.CurrentState == EnvironmentState.InWater;
    }
}
