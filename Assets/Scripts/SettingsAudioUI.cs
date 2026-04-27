using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsAudioUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text musicValueText;
    [SerializeField] private TMP_Text sfxValueText;

    [Header("Display")]
    [SerializeField] private string valueFormat = "{0:0}%";
    [SerializeField] private float sfxPreviewDelaySeconds = 0.12f;

    private bool isInitializing;
    private Coroutine sfxPreviewRoutine;

    private void OnEnable()
    {
        RegisterSliderListeners();
    }

    private void OnDisable()
    {
        UnregisterSliderListeners();
        StopSfxPreviewRoutine();
    }

    private void Start()
    {
        if (audioManager == null)
        {
            return;
        }

        isInitializing = true;

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.wholeNumbers = false;
            musicSlider.SetValueWithoutNotify(audioManager.GetMusicVolume());
            UpdateMusicValueLabel(musicSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.wholeNumbers = false;
            sfxSlider.SetValueWithoutNotify(audioManager.GetSfxVolume());
            UpdateSfxValueLabel(sfxSlider.value);
        }

        isInitializing = false;
    }

    public void OnMusicSliderChanged(float value)
    {
        if (audioManager == null)
        {
            return;
        }

        audioManager.SetMusicVolume(value);
        UpdateMusicValueLabel(value);
    }

    public void OnSfxSliderChanged(float value)
    {
        if (audioManager == null)
        {
            return;
        }

        audioManager.SetSfxVolume(value);
        UpdateSfxValueLabel(value);

        if (!isInitializing)
        {
            RestartSfxPreviewRoutine();
        }
    }

    private void UpdateMusicValueLabel(float value)
    {
        if (musicValueText != null)
        {
            musicValueText.text = string.Format(valueFormat, value * 100f);
        }
    }

    private void UpdateSfxValueLabel(float value)
    {
        if (sfxValueText != null)
        {
            sfxValueText.text = string.Format(valueFormat, value * 100f);
        }
    }

    private void RegisterSliderListeners()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
    }

    private void UnregisterSliderListeners()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
        }
    }

    private void RestartSfxPreviewRoutine()
    {
        StopSfxPreviewRoutine();
        sfxPreviewRoutine = StartCoroutine(PlaySfxPreviewAfterDelay());
    }

    private void StopSfxPreviewRoutine()
    {
        if (sfxPreviewRoutine != null)
        {
            StopCoroutine(sfxPreviewRoutine);
            sfxPreviewRoutine = null;
        }
    }

    private System.Collections.IEnumerator PlaySfxPreviewAfterDelay()
    {
        float delay = Mathf.Max(0.01f, sfxPreviewDelaySeconds);
        yield return new WaitForSecondsRealtime(delay);

        if (audioManager != null && !isInitializing)
        {
            audioManager.PlayPerfectSfx();
        }

        sfxPreviewRoutine = null;
    }
}
