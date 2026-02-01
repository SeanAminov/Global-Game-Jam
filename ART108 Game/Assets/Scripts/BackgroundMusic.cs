using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Music")]
    public AudioClip musicClip;
    public float volume = 0.7f;
    
    private AudioSource audioSource;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (musicClip != null)
        {
            audioSource.clip = musicClip;
            audioSource.volume = volume;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
