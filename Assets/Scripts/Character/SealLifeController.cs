using System.Collections;
using UnityEngine;

public class SealLifeController : MonoBehaviour
{
    private Seal seal;
    private SealModel model;
    private Coroutine deathCountdown;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        model = GetComponent<SealModel>();
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.PLAYER_EVENT_ON_STATE_CHANGE, OnStateChanged);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.PLAYER_EVENT_ON_STATE_CHANGE, OnStateChanged);
        StopDeathCountdown();
    }

    private void OnStateChanged(IGameEvent evt)
    {
        // 已死亡的角色（尸体保留期间）不再启动死亡倒计时
        if (!seal.LifeStateMachine.IsAlive())
        {
            StopDeathCountdown();
            return;
        }

        if (seal.OxygenStateMachine.IsSuffocating())
        {
            if (deathCountdown == null)
                deathCountdown = StartCoroutine(DeathCountdownRoutine());
        }
        else
        {
            StopDeathCountdown();
        }
    }

    private void StopDeathCountdown()
    {
        if (deathCountdown != null)
        {
            StopCoroutine(deathCountdown);
            deathCountdown = null;
        }
    }

    private IEnumerator DeathCountdownRoutine()
    {
        yield return new WaitForSeconds(model.SuffocatingDeathTimer);
        deathCountdown = null;
        GameEvents.Publish(EventType.PLAYER_EVENT_ON_DEATH,
            new PlayerEventArgs(gameObject));
    }
}
