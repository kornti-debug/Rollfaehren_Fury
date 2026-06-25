using UnityEngine;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    public sealed class GameplayMenuInput : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;

        private InputAction cancelAction;
        private bool isPaused;
        private bool isChangingScene;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            SetPanelActive(pausePanel, false);
            SetPanelActive(settingsPanel, false);
        }

        private void OnEnable()
        {
            cancelAction ??= PrototypeInputActions.Find("UI/Cancel");
            if (cancelAction != null)
            {
                cancelAction.performed += HandleCancel;
                cancelAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (cancelAction != null)
            {
                cancelAction.performed -= HandleCancel;
                cancelAction.Disable();
            }

            if (isPaused && !isChangingScene)
            {
                Time.timeScale = 1f;
                gameManager?.SetPaused(false);
                isPaused = false;
            }
        }

        public void ReturnToMenu()
        {
            BeginSceneChange();
            SceneFlow.LoadMenu();
        }

        public void Resume()
        {
            if (!isPaused)
            {
                return;
            }

            isPaused = false;
            Time.timeScale = 1f;
            SetPanelActive(pausePanel, false);
            SetPanelActive(settingsPanel, false);
            gameManager?.SetPaused(false);
        }

        public void RestartRun()
        {
            BeginSceneChange();
            SceneFlow.StartNewGame();
        }

        public void ShowSettings()
        {
            if (!isPaused)
            {
                return;
            }

            SetPanelActive(pausePanel, false);
            SetPanelActive(settingsPanel, true);
        }

        public void BackToPause()
        {
            if (!isPaused)
            {
                return;
            }

            SetPanelActive(settingsPanel, false);
            SetPanelActive(pausePanel, true);
        }

        public void QuitGame()
        {
            BeginSceneChange();
            SceneFlow.QuitGame();
        }

        private void BeginSceneChange()
        {
            isChangingScene = true;
            isPaused = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            if (gameManager != null && gameManager.IsShopOverlayOpen)
            {
                gameManager.CloseShopOverlay();
                return;
            }

            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                BackToPause();
                return;
            }

            if (isPaused)
            {
                Resume();
                return;
            }

            if (gameManager == null
                || (gameManager.State != PrototypeGameState.Preparation
                    && gameManager.State != PrototypeGameState.Playing))
            {
                return;
            }

            if (pausePanel == null)
            {
                Debug.LogWarning("Gameplay pause panel is not assigned.", this);
                return;
            }

            isPaused = true;
            gameManager.SetPaused(true);
            Time.timeScale = 0f;
            SetPanelActive(settingsPanel, false);
            SetPanelActive(pausePanel, true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
    }
}
