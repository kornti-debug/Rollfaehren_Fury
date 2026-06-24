using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(Collider))]
    public sealed class RoundStartConsole : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private float interactRange = 3.5f;
        [SerializeField] private GameObject promptObject;

        [Header("Floating Label")]
        [SerializeField] private string floatingLabelText = "Start Crossing";
        [SerializeField] private float floatingLabelHeight = 2f;

        private Transform player;
        private InputAction interactAction;
        private GameObject floatingLabel;
        private Camera labelCamera;
        private float labelBaseHeight;

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
            CreateFloatingLabel();
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

        private void LateUpdate()
        {
            if (floatingLabel == null)
            {
                return;
            }

            bool show = gameManager != null
                && gameManager.State == PrototypeGameState.Preparation
                && !gameManager.IsShopOverlayOpen;

            if (floatingLabel.activeSelf != show)
            {
                floatingLabel.SetActive(show);
            }

            if (!show)
            {
                return;
            }

            // 1. Calculate World Position
            // Using the collider's bounds ensures the label centers over the actual geometry, 
            // ignoring weird mesh pivot points.
            Collider col = GetComponent<Collider>();
            Vector3 centerPos = col != null ? col.bounds.center : transform.position;

            // 2. Apply gentle hover bob
            float bob = Mathf.Sin(Time.time * 2f) * 0.15f;

            // 3. Set absolute WORLD position to ignore parent scale and rotation completely
            floatingLabel.transform.position = centerPos + Vector3.up * (labelBaseHeight + bob);

            // 4. Billboard toward the camera
            if (labelCamera == null)
            {
                labelCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            }

            if (labelCamera != null)
            {
                floatingLabel.transform.rotation = Quaternion.LookRotation(
                    floatingLabel.transform.position - labelCamera.transform.position,
                    labelCamera.transform.up);
            }
        }

        private void CreateFloatingLabel()
        {
            labelBaseHeight = floatingLabelHeight;

            floatingLabel = new GameObject("Start Crossing Label");
            floatingLabel.transform.SetParent(transform, false);
            
            // 1. Counteract the parent's scale to guarantee a consistent world size.
            // Note: 0.01f on a 320px width canvas creates a 3.2-meter wide label. 
            // 0.0025f reduces it to a much more standard 0.8-meter UI prompt.
            Vector3 targetWorldScale = Vector3.one * 0.0025f; 
            Vector3 parentScale = transform.lossyScale;
            floatingLabel.transform.localScale = new Vector3(
                targetWorldScale.x / parentScale.x,
                targetWorldScale.y / parentScale.y,
                targetWorldScale.z / parentScale.z
            );

            Canvas canvas = floatingLabel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(320f, 90f);

            GameObject background = new GameObject("Background", typeof(RectTransform));
            background.transform.SetParent(floatingLabel.transform, false);
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.04f, 0.06f, 0.09f, 0.78f);
            RectTransform backgroundRect = backgroundImage.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            GameObject textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(floatingLabel.transform, false);
            Text label = textObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = floatingLabelText;
            label.fontSize = 44;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            RectTransform textRect = label.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Material overlayMaterial = new Material(Shader.Find("UI/Default"));
            overlayMaterial.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
            backgroundImage.material = overlayMaterial;
            label.material = overlayMaterial;
            canvas.sortingOrder = 100;

            floatingLabel.SetActive(false);
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
            if (promptObject == null)
            {
                return;
            }

            if (visible)
            {
                TextPrompt.Set(promptObject, "Press E", true);
            }
            else if (promptObject.activeSelf)
            {
                promptObject.SetActive(false);
            }
        }
    }
}
