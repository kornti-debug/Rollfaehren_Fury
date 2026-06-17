using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Round-end augment draft. Offers a few random augments from the pool on draft
    /// buttons; picking one applies it (via <see cref="AugmentContext"/>) and starts
    /// the next round through <see cref="GameManager.StartNextRound"/>.
    /// </summary>
    public sealed class AugmentSystem : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private List<AugmentDefinition> pool = new List<AugmentDefinition>();
        [SerializeField] private List<Button> draftButtons = new List<Button>();

        private readonly List<AugmentDefinition> offered = new List<AugmentDefinition>();

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            if (spawner == null)
            {
                spawner = FindFirstObjectByType<EnemySpawner>();
            }
        }

        /// <summary>Called by GameManager at round end.</summary>
        public void OpenDraft()
        {
            offered.Clear();

            List<AugmentDefinition> available = new List<AugmentDefinition>();
            foreach (AugmentDefinition augment in pool)
            {
                if (augment != null)
                {
                    available.Add(augment);
                }
            }

            for (int i = 0; i < draftButtons.Count; i++)
            {
                Button button = draftButtons[i];

                if (button == null || available.Count == 0)
                {
                    offered.Add(null);
                    if (button != null)
                    {
                        button.gameObject.SetActive(false);
                    }

                    continue;
                }

                int pick = Random.Range(0, available.Count);
                AugmentDefinition chosen = available[pick];
                available.RemoveAt(pick);
                offered.Add(chosen);

                button.gameObject.SetActive(true);
                button.interactable = true;

                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = string.IsNullOrEmpty(chosen.Description)
                        ? $"<b>{chosen.DisplayName}</b>"
                        : $"<b>{chosen.DisplayName}</b>\n<size=15><color=#bcc6d0>{chosen.Description}</color></size>";
                }
            }
        }

        /// <summary>Bound to draft button i (UnityEvent int listener) by the scene builder.</summary>
        public void Pick(int index)
        {
            if (index >= 0 && index < offered.Count && offered[index] != null)
            {
                offered[index].Apply(new AugmentContext(gameManager, spawner));
            }

            gameManager?.StartNextRound();
        }
    }
}
