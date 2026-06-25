using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    public sealed class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Text masterValueText;
        [SerializeField] private Text musicValueText;
        [SerializeField] private Text sfxValueText;
        [SerializeField] private Text sensitivityValueText;

        private bool isRefreshing;

        private void Awake()
        {
            ConfigureRanges();
            BindListeners();
            Refresh();
        }

        private void OnEnable()
        {
            GameSettings.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            GameSettings.Changed -= Refresh;
        }

        private void ConfigureRanges()
        {
            ConfigureSlider(masterVolumeSlider, 0f, 1f);
            ConfigureSlider(musicVolumeSlider, 0f, 1f);
            ConfigureSlider(sfxVolumeSlider, 0f, 1f);
            ConfigureSlider(
                mouseSensitivitySlider,
                GameSettings.MinMouseSensitivity,
                GameSettings.MaxMouseSensitivity);
        }

        private static void ConfigureSlider(Slider slider, float min, float max)
        {
            if (slider == null)
            {
                return;
            }

            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
        }

        private void BindListeners()
        {
            masterVolumeSlider?.onValueChanged.AddListener(HandleMasterChanged);
            musicVolumeSlider?.onValueChanged.AddListener(HandleMusicChanged);
            sfxVolumeSlider?.onValueChanged.AddListener(HandleSfxChanged);
            mouseSensitivitySlider?.onValueChanged.AddListener(HandleSensitivityChanged);
        }

        private void HandleMasterChanged(float value)
        {
            if (!isRefreshing)
            {
                GameSettings.SetMasterVolume(value);
            }
        }

        private void HandleMusicChanged(float value)
        {
            if (!isRefreshing)
            {
                GameSettings.SetMusicVolume(value);
            }
        }

        private void HandleSfxChanged(float value)
        {
            if (!isRefreshing)
            {
                GameSettings.SetSfxVolume(value);
            }
        }

        private void HandleSensitivityChanged(float value)
        {
            if (!isRefreshing)
            {
                GameSettings.SetMouseSensitivity(value);
            }
        }

        public void Refresh()
        {
            isRefreshing = true;
            SetSlider(masterVolumeSlider, GameSettings.MasterVolume);
            SetSlider(musicVolumeSlider, GameSettings.MusicVolume);
            SetSlider(sfxVolumeSlider, GameSettings.SfxVolume);
            SetSlider(mouseSensitivitySlider, GameSettings.MouseSensitivity);

            SetText(masterValueText, $"{Mathf.RoundToInt(GameSettings.MasterVolume * 100f)}%");
            SetText(musicValueText, $"{Mathf.RoundToInt(GameSettings.MusicVolume * 100f)}%");
            SetText(sfxValueText, $"{Mathf.RoundToInt(GameSettings.SfxVolume * 100f)}%");
            SetText(sensitivityValueText, GameSettings.MouseSensitivity.ToString("0.00"));
            isRefreshing = false;
        }

        private static void SetSlider(Slider slider, float value)
        {
            if (slider != null)
            {
                slider.SetValueWithoutNotify(value);
            }
        }

        private static void SetText(Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }
    }
}
