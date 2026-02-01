using UnityEngine;
using UnityEngine.UI;

public class GlobalAudioSlider : MonoBehaviour
{
    [Header("UI")]
    public Slider volumeSlider;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float defaultVolume = 0.8f;

    private void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = defaultVolume;
            SetVolume(defaultVolume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            SetVolume(defaultVolume);
        }
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }
}
