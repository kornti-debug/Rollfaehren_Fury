using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private uint backgroundMusicPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        private uint defeatMusicPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
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

        private void OnEnable()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded += HandleSceneLoaded;
            }
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

            RefreshSceneContext();
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
                StopMusicPlayback();
                return;
            }

            if (gameManager.State == PrototypeGameState.GameOver)
            {
                EnterGameOverAudio();
                return;
            }

            if (IsPlaying(defeatMusicPlayingId))
            {
                StopPlaying(ref defeatMusicPlayingId, 150);
                StartBackgroundMusic();
            }

            string gameState = gameManager.IsInsideShop
                ? "Shop"
                : gameManager.State == PrototypeGameState.Playing
                    ? "Moving"
                    : "Docked";
            bool gameStateChanged = SetMusicSwitch("GameState", gameState);

            string intensity = gameManager.State == PrototypeGameState.Playing
                               && gameManager.CrossingProgress >= 0.5f
                ? "Intense"
                : "Mid";
            bool intensityChanged = SetMusicSwitch("CombatIntensity", intensity);
            if ((gameStateChanged || intensityChanged) && IsPlaying(backgroundMusicPlayingId))
            {
                RestartBackgroundMusic();
            }
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
            StopMusicPlayback();

            if (AkUnitySoundEngine.IsInitialized() && outdoorBankLoaded)
            {
                AkBankManager.UnloadBank(outdoorBankName);
            }

            if (AkUnitySoundEngine.IsInitialized() && mainBankLoaded)
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

        public static void StopPlaying(ref uint playingId, int fadeMilliseconds = 100)
        {
            if (!IsPlaying(playingId))
            {
                playingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
                return;
            }

            AkUnitySoundEngine.StopPlayingID(playingId, Mathf.Max(0, fadeMilliseconds));
            playingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
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

        private bool SetMusicSwitch(string groupName, string valueName)
        {
            string cachedValue = groupName == "GameState"
                ? activeGameState
                : activeCombatIntensity;
            if (cachedValue == valueName)
            {
                return false;
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

                return true;
            }

            return false;
        }

        private void StartBackgroundMusic()
        {
            if (IsPlaying(backgroundMusicPlayingId))
            {
                return;
            }

            backgroundMusicPlayingId = PostEvent(WwiseAudioNames.PlayBackgroundMusic, gameObject);
        }

        private void RestartBackgroundMusic()
        {
            StopPlaying(ref backgroundMusicPlayingId, 150);
            StartBackgroundMusic();
        }

        private void EnterGameOverAudio()
        {
            if (IsPlaying(defeatMusicPlayingId))
            {
                return;
            }

            StopPlaying(ref backgroundMusicPlayingId, 200);
            defeatMusicPlayingId = PostEvent(WwiseAudioNames.PlayDefeatMusic, gameObject);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshSceneContext();
        }

        private void RefreshSceneContext()
        {
            gameManager = GameManager.Instance ?? FindFirstObjectByType<GameManager>();
            activeGameState = null;
            activeCombatIntensity = null;

            if (!isReady || gameManager == null)
            {
                StopMusicPlayback();
                return;
            }

            SetMusicSwitch("GameState", "Docked");
            SetMusicSwitch("CombatIntensity", "Mid");
            StopPlaying(ref defeatMusicPlayingId, 100);
            StartBackgroundMusic();
        }

        private void StopMusicPlayback()
        {
            StopPlaying(ref backgroundMusicPlayingId, 100);
            StopPlaying(ref defeatMusicPlayingId, 100);
        }

        private static bool IsPlaying(uint playingId)
        {
            return AkUnitySoundEngine.IsInitialized()
                   && playingId != AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
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

        public const string PlaySteps = "Play_Steps";
        public const string PlayHarpoon = "Play_HarpoonFired";
        public const string PlayPistol = "Play_PistolFired";
        public const string PlayShotgun = "Play_ShotgunFiredAndReload";
        public const string PlayAssaultRifle = "Play_AK47Fired";
        public const string PlayFishMovement = "Play_FishSwimming";
        public const string StopFishMovement = "Stop_FishSwimming";
        public const string PlayPigeonMovement = "Play_BirdFlap";
        public const string StopPigeonMovement = "Stop_BirdFlap";
        public const string PlayFishHit = "Play_EnemyFishHit";
        public const string PlayPigeonHit = "Play_EnemyBirdHit";
        public const string PlayFishContact = "Play_EnemyFishReachFerry";
        public const string PlayPigeonContact = "Play_EnemyBirdReachFerry";
        public const string PlayHarald = "Play_HaraldKrullSpeaking";
        public const string PlayUiHover = "Play_RC_UI_Hover";
        public const string PlayUiClick = "Play_RC_UI_Click";
        public const string PlayDoorOpen = "Play_RC_Door_Open";
    }
}
