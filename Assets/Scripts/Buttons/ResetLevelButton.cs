using UnityEngine;
using UnityEngine.UI;

public class ResetLevelButton : MonoBehaviour
{
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    [SerializeField] private float holdDuration = 0.5f;

    private float holdTimer;
    private bool hasTriggered;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void Update()
    {
        if (Input.GetKey(resetKey))
        {
            if (hasTriggered)
            {
                return;
            }

            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration)
            {
                hasTriggered = true;
                OnClick();
            }
        }
        else
        {
            holdTimer = 0f;
            hasTriggered = false;
        }
    }

    private void OnClick()
    {
        GameEvents.Publish(EventType.UI_EVENT_ON_RESET_LEVEL, new GameEventBase());
    }
}
