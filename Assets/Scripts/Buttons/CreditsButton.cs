using UnityEngine;
using UnityEngine.UI;

public class CreditsButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GameEvents.Publish(EventType.UI_EVENT_ON_CREDITS, new GameEventBase());
    }
}
