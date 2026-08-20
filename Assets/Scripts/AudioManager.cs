using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    [Header("Sources")]
    [SerializeField] private AudioSource menuMusicSource;
    [SerializeField] private AudioSource gameplayMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip gameplayMusicClip;
    [SerializeField] private AudioClip perfectSfxClip;
    [SerializeField] private AudioClip gameOverSfxClip;
    [SerializeField] private AudioClip specialOrderStartClip;
    [SerializeField] private AudioClip specialOrderSuccessClip;
    [SerializeField] private AudioClip timeUpAlarmSfxClip;

    [Header("Default Volumes")]
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.9f;

    [Header("Output Caps")]
    [SerializeField, Range(0f, 1f)] private float maxMusicOutput = 0.25f;
    [SerializeField, Range(0f, 1f)] private float maxSfxOutput = 0.5f;

    private float musicVolume = 0.45f;
    private float sfxVolume = 0.9f;

    private void Awake()
    {
        ConfigureSource(menuMusicSource, true, false);
        ConfigureSource(gameplayMusicSource, true, false);
        ConfigureSource(sfxSource, false, false);

        if (menuMusicSource != null) menuMusicSource.clip = menuMusicClip;
        if (gameplayMusicSource != null) gameplayMusicSource.clip = gameplayMusicClip;

        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume);
        ApplyMusicVolume(musicVolume);
        ApplySfxVolume(sfxVolume);
    }

    public void PlayMenuMusic()
    {
        StopSource(gameplayMusicSource);
        StopSource(sfxSource);
        PlayLoop(menuMusicSource, menuMusicClip);
    }

    public void PlayGameplayMusic()
    {
        StopSource(menuMusicSource);
        PlayLoop(gameplayMusicSource, gameplayMusicClip);
    }

    public void PlayGameOverSfx()
    {
        if (sfxSource == null || gameOverSfxClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(gameOverSfxClip);
    }

    public void PlayPerfectSfx()
    {
        if (sfxSource == null || perfectSfxClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(perfectSfxClip);
    }

    public void PlaySpecialOrderStart()
    {
        if (sfxSource == null || specialOrderStartClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(specialOrderStartClip);
    }

    public void PlaySpecialOrderSuccess()
    {
        if (sfxSource == null || specialOrderSuccessClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(specialOrderSuccessClip);
    }

    public void PlayTimeUpAlarmSfx()
    {
        if (sfxSource == null || timeUpAlarmSfxClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(timeUpAlarmSfxClip);
    }

    public void SetMusicVolume(float volume, bool persistImmediately = false)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume(musicVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        if (persistImmediately)
        {
            PlayerPrefs.Save();
        }
    }

    public void SetSfxVolume(float volume, bool persistImmediately = false)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplySfxVolume(sfxVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        if (persistImmediately)
        {
            PlayerPrefs.Save();
        }
    }

    public void PersistVolumes()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PersistVolumes();
        }
    }

    private void OnApplicationQuit()
    {
        PersistVolumes();
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSfxVolume()
    {
        return sfxVolume;
    }

    private void ConfigureSource(AudioSource source, bool loop, bool playOnAwake)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = playOnAwake;
        source.loop = loop;
        source.spatialBlend = 0f;
    }

    private void PlayLoop(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null)
        {
            return;
        }

        if (source.clip != clip)
        {
            source.clip = clip;
        }

        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    private void StopSource(AudioSource source)
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    private void ApplyMusicVolume(float volume)
    {
        float outputVolume = Mathf.Clamp01(volume) * Mathf.Clamp01(maxMusicOutput);
        if (menuMusicSource != null) menuMusicSource.volume = outputVolume;
        if (gameplayMusicSource != null) gameplayMusicSource.volume = outputVolume;
    }

    private void ApplySfxVolume(float volume)
    {
        float outputVolume = Mathf.Clamp01(volume) * Mathf.Clamp01(maxSfxOutput);
        if (sfxSource != null) sfxSource.volume = outputVolume;
    }
}
