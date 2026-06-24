using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RollfaehrenFury.Prototype
{
    public sealed class ShopSceneCoordinator : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private SimpleFPSController playerController;
        [SerializeField, Min(0f)] private float doorTransitionDelay = 0.35f;

        private Vector3 returnPosition;
        private Quaternion returnRotation;
        private string loadedSceneName;
        private string returnSceneName;
        private GameObject exteriorDoorEmitter;
        private GameObject interiorDoorEmitter;

        public static ShopSceneCoordinator Instance { get; private set; }

        public bool IsTransitioning { get; private set; }
        public string CurrentShopId { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            gameManager ??= GameManager.Instance ?? FindFirstObjectByType<GameManager>();
            playerController ??= FindFirstObjectByType<SimpleFPSController>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool EnterShop(string sceneName, string shopId, GameObject doorEmitter)
        {
            if (IsTransitioning
                || string.IsNullOrWhiteSpace(sceneName)
                || gameManager == null
                || playerController == null
                || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                return false;
            }

            if (!gameManager.TryBeginShopVisit())
            {
                return false;
            }

            returnPosition = playerController.transform.position;
            returnRotation = playerController.transform.rotation;
            returnSceneName = playerController.gameObject.scene.name;
            exteriorDoorEmitter = doorEmitter;
            CurrentShopId = shopId;
            StartCoroutine(EnterShopRoutine(sceneName));
            return true;
        }

        public bool ExitShop(GameObject doorEmitter)
        {
            if (IsTransitioning
                || gameManager == null
                || playerController == null
                || !gameManager.IsInsideShop
                || gameManager.IsShopOverlayOpen)
            {
                return false;
            }

            interiorDoorEmitter = doorEmitter;
            StartCoroutine(ExitShopRoutine());
            return true;
        }

        private IEnumerator EnterShopRoutine(string sceneName)
        {
            IsTransitioning = true;
            PostDoor(WwiseAudioNames.PlayDoorOpen, exteriorDoorEmitter);
            yield return WaitForDoor();

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                RestoreAfterFailedEntry();
                yield break;
            }

            yield return loadOperation;

            Scene shopScene = SceneManager.GetSceneByName(sceneName);
            Transform spawn = FindSceneTransform(shopScene, "Shop Interior Spawn");
            if (!shopScene.IsValid() || !shopScene.isLoaded || spawn == null)
            {
                Debug.LogError($"Shop scene '{sceneName}' is missing 'Shop Interior Spawn'.", this);
                if (shopScene.IsValid() && shopScene.isLoaded)
                {
                    yield return SceneManager.UnloadSceneAsync(shopScene);
                }

                RestoreAfterFailedEntry();
                yield break;
            }

            loadedSceneName = sceneName;
            interiorDoorEmitter = FindSceneTransform(shopScene, "Shop Exit")?.gameObject;
            SceneManager.SetActiveScene(shopScene);
            playerController.Teleport(spawn.position, spawn.rotation);
            gameManager.CompleteShopEntry();
            yield return null;
            PostDoor(WwiseAudioNames.PlayDoorClose, interiorDoorEmitter);
            IsTransitioning = false;
        }

        private IEnumerator ExitShopRoutine()
        {
            IsTransitioning = true;
            PostDoor(WwiseAudioNames.PlayDoorOpen, interiorDoorEmitter);
            yield return WaitForDoor();

            gameManager.PrepareShopExit();

            Scene returnScene = SceneManager.GetSceneByName(returnSceneName);
            if (returnScene.IsValid() && returnScene.isLoaded)
            {
                SceneManager.SetActiveScene(returnScene);
            }

            playerController.Teleport(returnPosition, returnRotation);

            Scene shopScene = SceneManager.GetSceneByName(loadedSceneName);
            if (shopScene.IsValid() && shopScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(shopScene);
            }

            loadedSceneName = string.Empty;
            CurrentShopId = string.Empty;
            gameManager.CompleteShopExit();
            yield return null;
            PostDoor(WwiseAudioNames.PlayDoorClose, exteriorDoorEmitter);
            interiorDoorEmitter = null;
            exteriorDoorEmitter = null;
            IsTransitioning = false;
        }

        private void RestoreAfterFailedEntry()
        {
            Scene returnScene = SceneManager.GetSceneByName(returnSceneName);
            if (returnScene.IsValid() && returnScene.isLoaded)
            {
                SceneManager.SetActiveScene(returnScene);
            }

            playerController.Teleport(returnPosition, returnRotation);
            PostDoor(WwiseAudioNames.PlayDoorClose, exteriorDoorEmitter);
            loadedSceneName = string.Empty;
            CurrentShopId = string.Empty;
            interiorDoorEmitter = null;
            exteriorDoorEmitter = null;
            gameManager.CompleteShopExit();
            IsTransitioning = false;
        }

        private object WaitForDoor()
        {
            return doorTransitionDelay > 0f
                ? new WaitForSecondsRealtime(doorTransitionDelay)
                : null;
        }

        private static void PostDoor(string eventName, GameObject emitter)
        {
            if (emitter != null)
            {
                WwiseAudioRuntime.Post(eventName, emitter);
            }
        }

        private static Transform FindSceneTransform(Scene scene, string objectName)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return null;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.name == objectName)
                    {
                        return transform;
                    }
                }
            }

            return null;
        }
    }
}
