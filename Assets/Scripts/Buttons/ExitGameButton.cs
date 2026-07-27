using UnityEngine;
using UnityEngine.UI;

public class ExitGameButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GameEvents.Publish(EventType.UI_EVENT_ON_EXIT_GAME, new GameEventBase());
    }
}
