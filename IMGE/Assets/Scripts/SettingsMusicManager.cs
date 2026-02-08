using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;

    private void OnEnable()
    {
        if (AudioManager.Instance == null) return;
        musicSlider.SetValueWithoutNotify(AudioManager.Instance.GetMusicSliderValue());
    }

    public void OnMusicSliderChanged(float value)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.SetMusicVolume(value);
    }
}