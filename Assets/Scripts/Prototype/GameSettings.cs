using System;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public static class GameSettings
    {
        private const string MasterVolumeKey = "settings.masterVolume";
        private const string MusicVolumeKey = "settings.musicVolume";
        private const string SfxVolumeKey = "settings.sfxVolume";
        private const string MouseSensitivityKey = "settings.mouseSensitivity";

        public const float DefaultVolume = 1f;
        public const float DefaultMouseSensitivity = 0.12f;
        public const float MinMouseSensitivity = 0.05f;
        public const float MaxMouseSensitivity = 0.30f;

        public static event Action Changed;

        public static float MasterVolume => Load01(MasterVolumeKey, DefaultVolume);
        public static float MusicVolume => Load01(MusicVolumeKey, DefaultVolume);
        public static float SfxVolume => Load01(SfxVolumeKey, DefaultVolume);
        public static float MouseSensitivity => Mathf.Clamp(
            PlayerPrefs.GetFloat(MouseSensitivityKey, DefaultMouseSensitivity),
            MinMouseSensitivity,
            MaxMouseSensitivity);

        public static void SetMasterVolume(float value)
        {
            Save(MasterVolumeKey, Mathf.Clamp01(value));
        }

        public static void SetMusicVolume(float value)
        {
            Save(MusicVolumeKey, Mathf.Clamp01(value));
        }

        public static void SetSfxVolume(float value)
        {
            Save(SfxVolumeKey, Mathf.Clamp01(value));
        }

        public static void SetMouseSensitivity(float value)
        {
            Save(MouseSensitivityKey, Mathf.Clamp(value, MinMouseSensitivity, MaxMouseSensitivity));
            ApplyMouseSensitivity();
        }

        public static void ApplyAll()
        {
            ApplyAudio();
            ApplyMouseSensitivity();
        }

        public static void ApplyAudio()
        {
            if (!AkUnitySoundEngine.IsInitialized())
            {
                return;
            }

            AkUnitySoundEngine.SetRTPCValue("MasterVolume", MasterVolume * 100f);
            AkUnitySoundEngine.SetRTPCValue("MusicVolume", MusicVolume * 100f);
            AkUnitySoundEngine.SetRTPCValue("SFXVolume", SfxVolume * 100f);
        }

        public static void ApplyMouseSensitivity()
        {
            foreach (SimpleFPSController controller in
                     UnityEngine.Object.FindObjectsByType<SimpleFPSController>(FindObjectsSortMode.None))
            {
                controller.SetMouseSensitivity(MouseSensitivity);
            }
        }

        private static float Load01(string key, float fallback)
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(key, fallback));
        }

        private static void Save(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            ApplyAudio();
            Changed?.Invoke();
        }
    }
}
