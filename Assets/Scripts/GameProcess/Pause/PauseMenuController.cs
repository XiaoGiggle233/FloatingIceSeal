using UnityEngine;
using UnityEngine.UI;

namespace GameProcess.Pause
{
    /// <summary>
    /// 暂停菜单控制器
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private Button backButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform scrollContent;
        [SerializeField] private RectTransform scrollViewport;
        [SerializeField] private Scrollbar scrollbarVertical;

        private PauseController _pauseController;

        private void Awake()
        {
            if (menuRoot != null)
                menuRoot.SetActive(false);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            SetupScrollView();
        }

        private void SetupScrollView()
        {
            if (scrollRect != null)
            {
                scrollRect.content = scrollContent;
                scrollRect.viewport = scrollViewport;
                scrollRect.verticalScrollbar = scrollbarVertical;
                scrollRect.horizontal = false;
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
            }
        }

        public void Init(PauseController pauseController)
        {
            _pauseController = pauseController;
        }

        public void Show()
        {
            if (menuRoot != null)
                menuRoot.SetActive(true);
        }

        public void Hide()
        {
            if (menuRoot != null)
                menuRoot.SetActive(false);
        }

        private void OnBackClicked()
        {
            _pauseController?.Resume();
        }
    }
}
