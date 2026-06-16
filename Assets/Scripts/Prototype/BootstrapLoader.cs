using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public sealed class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string menuSceneName = SceneFlow.MenuSceneName;

        private void Start()
        {
            SceneFlow.LoadScene(menuSceneName);
        }
    }
}
