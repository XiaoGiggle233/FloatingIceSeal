using UnityEngine;
using UnityEngine.UI;

namespace GameProcess.Pause
{
    /// <summary>
    /// 通用设置按钮 —— 点击时发布 UI_EVENT_ON_BUTTON_CLICK 事件
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SettingsButton : MonoBehaviour
    {
        public enum ButtonAction
        {
            Volume,
            Resolution,
            BGM,
            Fullscreen,
            KeySettings
        }

        [SerializeField] private ButtonAction action;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            GameEvents.Publish(EventType.UI_EVENT_ON_BUTTON_CLICK,
                new ButtonClickEventArgs(action.ToString()));
        }
    }
}
