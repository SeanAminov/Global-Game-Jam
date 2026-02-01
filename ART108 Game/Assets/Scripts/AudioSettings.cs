using UnityEngine;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance { get; private set; }

    [Header("Master Volumes")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    public static float SfxVolumeMultiplier => Instance != null ? Instance.masterVolume * Instance.sfxVolume : 1f;
    public static float MusicVolumeMultiplier => Instance != null ? Instance.masterVolume * Instance.musicVolume : 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
