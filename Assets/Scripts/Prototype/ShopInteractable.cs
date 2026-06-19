using UnityEngine;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Walk-up vending-machine shop using the shared Player/Interact action.
    /// </summary>
    public sealed class ShopInteractable : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private float interactRange = 3.5f;
        [SerializeField] private GameObject promptObject;

        private Transform player;
        private InputAction interactAction;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            if (promptObject == null)
            {
                promptObject = FindSceneObject("Shop Prompt");
            }

            SimpleFPSController controller = FindFirstObjectByType<SimpleFPSController>();
            if (controller != null)
            {
                player = controller.transform;
            }
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

            if (promptObject != null)
            {
                promptObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (gameManager == null)
            {
                return;
            }

            bool open = gameManager.IsShopOverlayOpen;
            bool inRange = gameManager.AllowsShopInteraction && IsPlayerInRange();

            if (promptObject != null)
            {
                promptObject.SetActive(inRange && !open);
            }
        }

        private void HandleInteract(InputAction.CallbackContext context)
        {
            if (gameManager == null)
            {
                return;
            }

            bool open = gameManager.IsShopOverlayOpen;
            if (open)
            {
                gameManager.CloseShopOverlay();
            }
            else if (gameManager.AllowsShopInteraction && IsPlayerInRange())
            {
                gameManager.OpenShopOverlay();
            }
        }

        private bool IsPlayerInRange()
        {
            if (player == null)
            {
                return false;
            }

            return (player.position - transform.position).sqrMagnitude <= interactRange * interactRange;
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
