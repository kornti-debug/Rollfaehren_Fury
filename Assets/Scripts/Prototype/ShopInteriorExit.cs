using UnityEngine;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    public sealed class ShopInteriorExit : MonoBehaviour
    {
        [SerializeField] private string promptObjectName = "Shop Scene Prompt";

        private GameManager gameManager;
        private ShopSceneCoordinator coordinator;
        private GameObject promptObject;
        private SimpleFPSController playerInRange;
        private InputAction interactAction;

        private void Awake()
        {
            gameManager = GameManager.Instance ?? FindFirstObjectByType<GameManager>();
            coordinator = ShopSceneCoordinator.Instance ?? FindFirstObjectByType<ShopSceneCoordinator>();
            promptObject = FindSceneObject(promptObjectName);
        }

        private void OnEnable()
        {
            interactAction ??= PrototypeInputActions.Find("Player/Interact");
            if (interactAction != null)
            {
                interactAction.performed += HandleInteract;
                interactAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (interactAction != null)
            {
                interactAction.performed -= HandleInteract;
            }

            SetPrompt(false);
        }

        private void Update()
        {
            bool canExit = playerInRange != null
                && gameManager != null
                && gameManager.IsInsideShop
                && !gameManager.IsPaused
                && !gameManager.IsShopOverlayOpen
                && coordinator != null
                && !coordinator.IsTransitioning;
            SetPrompt(canExit);
        }

        private void OnTriggerEnter(Collider other)
        {
            SimpleFPSController controller = other.GetComponentInParent<SimpleFPSController>();
            if (controller != null)
            {
                playerInRange = controller;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            SimpleFPSController controller = other.GetComponentInParent<SimpleFPSController>();
            if (controller != null && controller == playerInRange)
            {
                playerInRange = null;
                SetPrompt(false);
            }
        }

        private void HandleInteract(InputAction.CallbackContext context)
        {
            if (playerInRange == null
                || gameManager == null
                || gameManager.IsShopOverlayOpen
                || coordinator == null)
            {
                return;
            }

            coordinator.ExitShop(gameObject);
        }

        private void SetPrompt(bool active)
        {
            if (promptObject == null)
            {
                promptObject = FindSceneObject(promptObjectName);
            }

            if (promptObject == null)
            {
                return;
            }

            TextPrompt.Set(promptObject, "Press E - Leave shop", active);
        }

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (transform.gameObject.scene.IsValid() && transform.name == objectName)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }
    }
}
