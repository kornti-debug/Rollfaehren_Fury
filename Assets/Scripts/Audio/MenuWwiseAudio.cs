using System.Collections;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public sealed class MenuWwiseAudio : MonoBehaviour
    {
        [SerializeField] private string mainBankName = "MainSoundBank";

        private bool bankLoaded;
        private bool ready;
        private uint titleMusicPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;

        public static MenuWwiseAudio Instance { get; private set; }
        public static bool IsReady => Instance != null && Instance.ready;

        private void Awake()
        {
            WwiseInitializerRuntime.Ensure();

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private IEnumerator Start()
        {
            while (!AkUnitySoundEngine.IsInitialized())
            {
                yield return null;
            }

            GameObject emitter = WwiseInitializerRuntime.Emitter;
            while (emitter == null || !WwiseInitializerRuntime.IsEmitterRegistered())
            {
                yield return null;
                emitter = WwiseInitializerRuntime.Emitter;
            }

            AkBankManager.LoadBank(mainBankName, false, false);
            bankLoaded = true;
            yield return null;

            ready = true;
            GameSettings.ApplyAudio();
            titleMusicPlayingId = Post(WwiseAudioNames.PlayTitleMusic, emitter);
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            StopTitleMusic();
            ready = false;
            if (AkUnitySoundEngine.IsInitialized() && bankLoaded)
            {
                AkBankManager.UnloadBank(mainBankName);
            }

            Instance = null;
        }

        public void StopTitleMusic()
        {
            if (!AkUnitySoundEngine.IsInitialized())
            {
                titleMusicPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
                return;
            }

            if (titleMusicPlayingId != AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
            {
                AkUnitySoundEngine.StopPlayingID(titleMusicPlayingId, 150);
                titleMusicPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
            }

            GameObject emitter = WwiseInitializerRuntime.Emitter;
            if (emitter != null && WwiseInitializerRuntime.IsEmitterRegistered())
            {
                AkUnitySoundEngine.PostEvent(WwiseAudioNames.StopTitleMusic, emitter);
            }
        }

        public static uint Post(string eventName, GameObject emitter)
        {
            if (!IsReady
                || string.IsNullOrWhiteSpace(eventName)
                || emitter == null
                || !AkUnitySoundEngine.IsInitialized())
            {
                return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
            }

            GameObject persistentEmitter = WwiseInitializerRuntime.Emitter;
            return persistentEmitter != null && WwiseInitializerRuntime.IsEmitterRegistered()
                ? AkUnitySoundEngine.PostEvent(eventName, persistentEmitter)
                : AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        }
    }
}
