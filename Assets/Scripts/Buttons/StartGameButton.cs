using UnityEngine;
using UnityEngine.UI;

public class StartGameButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GameEvents.Publish(EventType.UI_EVENT_ON_START_GAME, new GameEventBase());
    }
}
