using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Deck-mirror interaction. While the player stands in front of the mirror, the shared
    /// <c>Player/Interact</c> key toggles a panel that lists every augment picked this run (with
    /// counts, e.g. "Tailwind ×2"). It only reads <see cref="AugmentSystem.AcquiredAugments"/> — it
    /// changes no game state, so it is safe to open mid-crossing. Walking away closes it.
    /// </summary>
    public sealed class MirrorInteractable : MonoBehaviour
    {
        [SerializeField] private AugmentSystem augmentSystem;
        [SerializeField] private GameManager gameManager;
        [SerializeField, Min(0.5f)] private float interactRange = 3.5f;
        [Tooltip("Shown while the player is in range (e.g. a 'Press E - Augments' label).")]
        [SerializeField] private GameObject promptObject;
        [Tooltip("Root of the augment-list panel; toggled by the interact key.")]
        [SerializeField] private GameObject panelObject;
        [Tooltip("Text element filled with the active-augment list.")]
        [SerializeField] private Text listText;

        private Transform player;
        private InputAction interactAction;
        private bool panelOpen;

        private void Awake()
        {
            if (augmentSystem == null)
            {
                augmentSystem = FindFirstObjectByType<AugmentSystem>();
            }

            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            SimpleFPSController controller = FindFirstObjectByType<SimpleFPSController>();
            if (controller != null)
            {
                player = controller.transform;
            }

            SetPanelOpen(false);
            if (promptObject != null)
            {
                promptObject.SetActive(false);
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

            SetPanelOpen(false);
        }

        private void Update()
        {
            bool inRange = IsAvailable() && IsPlayerInRange();

            // Walking away (or leaving normal gameplay, e.g. pause) closes the panel.
            if (panelOpen && !inRange)
            {
                SetPanelOpen(false);
            }

            if (promptObject != null)
            {
                promptObject.SetActive(inRange && !panelOpen);
            }
        }

        private void HandleInteract(InputAction.CallbackContext context)
        {
            if (panelOpen)
            {
                SetPanelOpen(false);
                return;
            }

            if (IsAvailable() && IsPlayerInRange())
            {
                RefreshList();
                SetPanelOpen(true);
            }
        }

        // Usable while roaming the deck (Playing or Preparation); never while paused or in a menu.
        private bool IsAvailable()
        {
            return gameManager == null || gameManager.AllowsGameplayInput;
        }

        private bool IsPlayerInRange()
        {
            if (player == null)
            {
                return false;
            }

            return (player.position - transform.position).sqrMagnitude <= interactRange * interactRange;
        }

        private void SetPanelOpen(bool open)
        {
            panelOpen = open;
            if (panelObject != null)
            {
                panelObject.SetActive(open);
            }
        }

        private void RefreshList()
        {
            if (listText == null)
            {
                return;
            }

            IReadOnlyList<AugmentDefinition> picked = augmentSystem != null ? augmentSystem.AcquiredAugments : null;
            if (picked == null || picked.Count == 0)
            {
                listText.text = "<b>Active Augments</b>\n\nNone yet — survive a crossing and pick one at the round-end draft.";
                return;
            }

            // Group by definition, preserving first-pick order, counting repeats.
            List<AugmentDefinition> order = new List<AugmentDefinition>();
            Dictionary<AugmentDefinition, int> counts = new Dictionary<AugmentDefinition, int>();
            foreach (AugmentDefinition augment in picked)
            {
                if (augment == null)
                {
                    continue;
                }

                if (!counts.ContainsKey(augment))
                {
                    counts.Add(augment, 0);
                    order.Add(augment);
                }

                counts[augment]++;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<b>Active Augments</b>");
            sb.AppendLine();
            foreach (AugmentDefinition augment in order)
            {
                sb.Append("• <b>").Append(augment.DisplayName).Append("</b>");
                if (counts[augment] > 1)
                {
                    sb.Append(" ×").Append(counts[augment]);
                }

                if (!string.IsNullOrEmpty(augment.Description))
                {
                    sb.Append("\n   <size=14><color=#bcc6d0>").Append(augment.Description).Append("</color></size>");
                }

                sb.AppendLine();
            }

            listText.text = sb.ToString();
        }
    }
}
