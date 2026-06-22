using System.Collections;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(AkGameObj))]
    public sealed class WwiseAudioRuntime : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private string mainBankName = "MainSoundBank";
        [SerializeField] private string outdoorBankName = "OutdoorSoundBank";

        private bool mainBankLoaded;
        private bool outdoorBankLoaded;
        private bool backgroundMusicPlaying;
        private bool defeatMusicPlaying;
        private string activeGameState;
        private string activeCombatIntensity;

        public static WwiseAudioRuntime Instance { get; private set; }
        public static bool IsReady => Instance != null && Instance.isReady;

        private bool isReady;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            gameManager ??= GameManager.Instance ?? FindFirstObjectByType<GameManager>();
        }

        private IEnumerator Start()
        {
            while (!AkUnitySoundEngine.IsInitialized())
            {
                yield return null;
            }

            mainBankLoaded = LoadBank(mainBankName);
            outdoorBankLoaded = LoadBank(outdoorBankName);
            isReady = mainBankLoaded && outdoorBankLoaded;

            if (!isReady)
            {
                Debug.LogWarning(
                    "Wwise gameplay banks were not fully loaded. Generate MainSoundBank and OutdoorSoundBank locally.",
                    this);
                yield break;
            }

            SetMusicSwitch("GameState", "Docked");
            SetMusicSwitch("CombatIntensity", "Mid");
            StartBackgroundMusic();
        }

        private void Update()
        {
            if (!isReady)
            {
                return;
            }

            gameManager ??= GameManager.Instance ?? FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                return;
            }

            if (gameManager.State == PrototypeGameState.GameOver)
            {
                EnterGameOverAudio();
                return;
            }

            if (defeatMusicPlaying)
            {
                PostEvent(WwiseAudioNames.StopDefeatMusic, gameObject);
                defeatMusicPlaying = false;
                StartBackgroundMusic();
            }

            string gameState = gameManager.IsInsideShop
                ? "Shop"
                : gameManager.State == PrototypeGameState.Playing
                    ? "Moving"
                    : "Docked";
            SetMusicSwitch("GameState", gameState);

            string intensity = gameManager.State == PrototypeGameState.Playing
                               && gameManager.CrossingProgress >= 0.5f
                ? "Intense"
                : "Mid";
            SetMusicSwitch("CombatIntensity", intensity);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (!AkUnitySoundEngine.IsInitialized())
            {
                return;
            }

            PostEvent(WwiseAudioNames.StopBackgroundMusic, gameObject);
            PostEvent(WwiseAudioNames.StopDefeatMusic, gameObject);

            if (outdoorBankLoaded)
            {
                AkBankManager.UnloadBank(outdoorBankName);
            }

            if (mainBankLoaded)
            {
                AkBankManager.UnloadBank(mainBankName);
            }
        }

        public static uint Post(string eventName, GameObject emitter)
        {
            if (!IsReady || string.IsNullOrWhiteSpace(eventName) || emitter == null)
            {
                return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
            }

            return PostEvent(eventName, emitter);
        }

        public static bool SetSwitch(string groupName, string valueName, GameObject emitter)
        {
            if (!IsReady
                || string.IsNullOrWhiteSpace(groupName)
                || string.IsNullOrWhiteSpace(valueName)
                || emitter == null)
            {
                return false;
            }

            return AkUnitySoundEngine.SetSwitch(groupName, valueName, emitter) == AKRESULT.AK_Success;
        }

        public static bool SetRtpc(string rtpcName, float value, GameObject emitter)
        {
            if (!IsReady || string.IsNullOrWhiteSpace(rtpcName) || emitter == null)
            {
                return false;
            }

            return AkUnitySoundEngine.SetRTPCValue(rtpcName, value, emitter) == AKRESULT.AK_Success;
        }

        private static bool LoadBank(string bankName)
        {
            if (string.IsNullOrWhiteSpace(bankName))
            {
                return false;
            }

            // AK_INVALID_UNIQUE_ID also means AkBankManager already had the bank
            // loaded and incremented its reference count.
            AkBankManager.LoadBank(bankName, false, false);
            return true;
        }

        private static uint PostEvent(string eventName, GameObject emitter)
        {
            if (!AkUnitySoundEngine.IsInitialized()
                || string.IsNullOrWhiteSpace(eventName)
                || emitter == null)
            {
                return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
            }

            return AkUnitySoundEngine.PostEvent(eventName, emitter);
        }

        private void SetMusicSwitch(string groupName, string valueName)
        {
            string cachedValue = groupName == "GameState"
                ? activeGameState
                : activeCombatIntensity;
            if (cachedValue == valueName)
            {
                return;
            }

            if (SetSwitch(groupName, valueName, gameObject))
            {
                if (groupName == "GameState")
                {
                    activeGameState = valueName;
                }
                else
                {
                    activeCombatIntensity = valueName;
                }
            }
        }

        private void StartBackgroundMusic()
        {
            if (backgroundMusicPlaying)
            {
                return;
            }

            backgroundMusicPlaying =
                PostEvent(WwiseAudioNames.PlayBackgroundMusic, gameObject)
                != AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        }

        private void EnterGameOverAudio()
        {
            if (defeatMusicPlaying)
            {
                return;
            }

            if (backgroundMusicPlaying)
            {
                PostEvent(WwiseAudioNames.StopBackgroundMusic, gameObject);
                backgroundMusicPlaying = false;
            }

            defeatMusicPlaying =
                PostEvent(WwiseAudioNames.PlayDefeatMusic, gameObject)
                != AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        }
    }

    public static class WwiseAudioNames
    {
        public const string PlayBackgroundMusic = "Play_BackgroundMusic";
        public const string StopBackgroundMusic = "Stop_BackgroundMusic";
        public const string PlayDefeatMusic = "Play_DefeatMusic";
        public const string StopDefeatMusic = "Stop_DefeatMusic";

        public const string PlayBoatEngine = "Play_BoatEngine";
        public const string StopBoatEngine = "Stop_BoatEngine";
        public const string PlayBoatStanding = "Play_BoatWaveStanding";
        public const string StopBoatStanding = "Stop_BoatWaveStanding";
        public const string PlayBoatMoving = "Play_BoatWaveMoving";
        public const string StopBoatMoving = "Stop_BoatWaveMoving";
        public const string PlayBoatSteering = "Play_BoatSteeringScreech";
    }
}
