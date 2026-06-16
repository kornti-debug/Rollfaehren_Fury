using UnityEngine;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    public sealed class GameplayMenuInput : MonoBehaviour
    {
        [SerializeField] private bool returnToMenuOnCancel = true;

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

        private void OnDisable()
        {
            if (cancelAction != null)
            {
                cancelAction.performed -= HandleCancel;
                cancelAction.Disable();
            }
        }

        public void ReturnToMenu()
        {
            SceneFlow.LoadMenu();
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            if (returnToMenuOnCancel)
            {
                ReturnToMenu();
            }
        }
    }
}
