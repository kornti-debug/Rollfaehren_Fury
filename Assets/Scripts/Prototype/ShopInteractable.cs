using UnityEngine;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Walk-up vending-machine shop. When the player is within range during gameplay,
    /// pressing B opens the shop overlay; pressing B again (or the Close button) closes it.
    /// Open/closed state lives on <see cref="GameManager"/> so B and the Close button stay in sync.
    /// </summary>
    public sealed class ShopInteractable : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private float interactRange = 3.5f;
        [SerializeField] private GameObject promptObject;

        private Transform player;

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
        }

        private void Update()
        {
            if (gameManager == null)
            {
                return;
            }

            bool open = gameManager.IsShopOverlayOpen;
            bool inRange = gameManager.AllowsGameplayInput && IsPlayerInRange();

            if (promptObject != null)
            {
                promptObject.SetActive(inRange && !open);
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.bKey.wasPressedThisFrame)
            {
                return;
            }

            if (open)
            {
                gameManager.CloseShopOverlay();
            }
            else if (inRange)
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
    }
}
