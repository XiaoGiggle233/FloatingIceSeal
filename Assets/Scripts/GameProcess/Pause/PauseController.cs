using UnityEngine;

namespace GameProcess.Pause
{
    /// <summary>
    /// 暂停系统控制器 —— 按 ESC 切换暂停/恢复，发布对应事件，控制暂停菜单显隐
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private PauseMenuController pauseMenu;

        private bool _isPaused;

        private void Start()
        {
            if (pauseMenu != null)
                pauseMenu.Init(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                if (_isPaused)
                    Resume();
                else
                    Pause();
            }
        }

        private void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;

            if (pauseMenu != null)
                pauseMenu.Show();

            GameEvents.Publish(EventType.GAME_EVENT_ON_PAUSE, new GameEventBase());
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;

            if (pauseMenu != null)
                pauseMenu.Hide();

            GameEvents.Publish(EventType.GAME_EVENT_ON_RESUME, new GameEventBase());
        }
    }
}
