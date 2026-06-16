using UnityEngine;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    internal static class PrototypeInputActions
    {
        public static InputAction Find(string actionPath)
        {
            InputActionAsset actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogWarning($"Project-wide input actions are not assigned. Missing '{actionPath}'.");
                return null;
            }

            InputAction action = actions.FindAction(actionPath, false);
            if (action == null)
            {
                Debug.LogWarning($"Input action '{actionPath}' was not found in the project-wide input actions.");
            }

            return action;
        }

        public static void SetEnabled(InputAction action, bool enabled)
        {
            if (action == null)
            {
                return;
            }

            if (enabled)
            {
                action.Enable();
            }
            else
            {
                action.Disable();
            }
        }
    }
}
