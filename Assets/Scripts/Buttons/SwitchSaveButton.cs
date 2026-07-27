using UnityEngine;
using UnityEngine.UI;

public class SwitchSaveButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GameEvents.Publish(EventType.UI_EVENT_ON_SWITCH_SAVE, new GameEventBase());
    }
}
