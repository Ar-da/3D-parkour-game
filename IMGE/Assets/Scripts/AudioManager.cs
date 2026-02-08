using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixer mixer;

    private const string MUSIC_KEY = "MusicVolume";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplySavedVolumes();
    }

    public float GetMusicSliderValue()
    {
        return PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
    }

    public void SetMusicVolume(float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        mixer.SetFloat("MusicVolume", dB);

        PlayerPrefs.SetFloat(MUSIC_KEY, sliderValue);
        PlayerPrefs.Save();
    }

    public void ApplySavedVolumes()
    {
        SetMusicVolume(GetMusicSliderValue());
    }
}