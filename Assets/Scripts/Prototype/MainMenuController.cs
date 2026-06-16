using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject firstSelectedButton;
        [SerializeField] private GameObject settingsFirstSelectedButton;

        private InputAction cancelAction;

        private void OnEnable()
        {
            cancelAction ??= PrototypeInputActions.Find("UI/Cancel");
            if (cancelAction != null)
            {
                cancelAction.performed += HandleCancel;
                cancelAction.Enable();
            }
        }

        private void Start()
        {
            ShowMain();
        }

        private void OnDisable()
        {
            if (cancelAction != null)
            {
                cancelAction.performed -= HandleCancel;
                cancelAction.Disable();
            }
        }

        public void NewGame()
        {
            SceneFlow.StartNewGame();
        }

        public void ShowSettings()
        {
            SetActive(mainPanel, false);
            SetActive(settingsPanel, true);
            SetSelected(settingsFirstSelectedButton);
        }

        public void ShowMain()
        {
            SetActive(mainPanel, true);
            SetActive(settingsPanel, false);
            SetSelected(firstSelectedButton);
        }

        public void QuitGame()
        {
            SceneFlow.QuitGame();
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                ShowMain();
            }
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }

        private static void SetSelected(GameObject target)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(target);
            }
        }
    }
}
