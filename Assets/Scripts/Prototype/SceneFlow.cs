using UnityEngine;
using UnityEngine.SceneManagement;

namespace RollfaehrenFury.Prototype
{
    public static class SceneFlow
    {
        public const string BootstrapSceneName = "Bootstrap";
        public const string MenuSceneName = "Menu";
        public const string MainSceneName = "Main";

        public static void LoadBootstrap()
        {
            LoadScene(BootstrapSceneName);
        }

        public static void LoadMenu()
        {
            LoadScene(MenuSceneName);
        }

        public static void StartNewGame()
        {
            LoadScene(MainSceneName);
        }

        public static void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("Cannot load a scene without a scene name.");
                return;
            }

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(sceneName);
        }

        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
