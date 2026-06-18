using UnityEngine;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(Collider))]
    public sealed class RoundStartConsole : MonoBehaviour
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

            SimpleFPSController controller = FindFirstObjectByType<SimpleFPSController>();
            if (controller != null)
            {
                player = controller.transform;
            }

            GetComponent<Collider>().isTrigger = true;
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
                interactAction.Disable();
            }

            SetPromptVisible(false);
        }

        private void Update()
        {
            SetPromptVisible(CanInteract());
        }

        private void HandleInteract(InputAction.CallbackContext context)
        {
            if (CanInteract() && gameManager.BeginCrossing())
            {
                SetPromptVisible(false);
            }
        }

        private bool CanInteract()
        {
            return gameManager != null
                && gameManager.State == PrototypeGameState.Preparation
                && !gameManager.IsShopOverlayOpen
                && IsPlayerInRange();
        }

        private bool IsPlayerInRange()
        {
            return player != null
                && (player.position - transform.position).sqrMagnitude <= interactRange * interactRange;
        }

        private void SetPromptVisible(bool visible)
        {
            if (promptObject != null && promptObject.activeSelf != visible)
            {
                promptObject.SetActive(visible);
            }
        }
    }
}
