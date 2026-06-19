using UnityEngine;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    public sealed class ShopScenePortal : MonoBehaviour
    {
        [SerializeField] private string sceneName = SceneFlow.ShopInteriorSceneName;
        [SerializeField] private string shopId = "SharedShop";
        [SerializeField] private GameObject promptObject;

        private GameManager gameManager;
        private ShopSceneCoordinator coordinator;
        private SimpleFPSController playerInRange;
        private InputAction interactAction;

        private void Awake()
        {
            gameManager = GameManager.Instance ?? FindFirstObjectByType<GameManager>();
            coordinator = ShopSceneCoordinator.Instance ?? FindFirstObjectByType<ShopSceneCoordinator>();
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
            bool canEnter = playerInRange != null
                && gameManager != null
                && gameManager.AllowsShopSceneEntry
                && coordinator != null
                && !coordinator.IsTransitioning;
            SetPrompt(canEnter);
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
                || !gameManager.AllowsShopSceneEntry
                || coordinator == null)
            {
                return;
            }

            coordinator.EnterShop(sceneName, shopId);
        }

        private void SetPrompt(bool active)
        {
            if (promptObject != null)
            {
                promptObject.SetActive(active);
            }
        }
    }
}
