using Unity.Netcode;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource; // For background music
    public AudioSource tankMoveSource; // For background music
    public AudioSource sfxSource;  // For sound effects
    public AudioSource ambientSource; // For ambient sounds

    [Header("Audio Clips")]
    public AudioClip[] soundEffects; // Array of sound effects (assign in Inspector)
    public AudioClip[] musicClips; // Array of sound effects (assign in Inspector)

    private float soundVolume = 1f; // Default music volume

    private void Awake()
    {
        // Singleton pattern to ensure only one AudioManager exists
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = volume;
        if (musicSource != null) {
            musicSource.volume = soundVolume;
        }
    }

    public void PlaySFX(string clipName)
    {
        AudioClip clip = GetSFXClipByName(clipName);
        if (clip != null) {
            sfxSource.PlayOneShot(clip);
            sfxSource.volume = 1f;
        }
    }

    public void PlayMusic(string clipName, bool loop = true)
    {
        AudioClip clip = GetMusicClipByName(clipName);
        musicSource.loop = loop;
        musicSource.volume = soundVolume;
        musicSource.Play();
    }

    public void PlayAmbient(AudioClip ambient, bool loop = true)
    {
        ambientSource.clip = ambient;
        ambientSource.loop = loop;
        ambientSource.volume = soundVolume;
        ambientSource.Play();
    }

    public AudioClip GetSFXClipByName(string name)
    {
        foreach (var clip in soundEffects) {
            if (clip.name == name)
                return clip;
        }
        Debug.LogWarning($"Audio clip {name} not found!");
        return null;
    }

    private AudioClip GetMusicClipByName(string name)
    {
        foreach (var clip in musicClips) {
            if (clip.name == name)
                return clip;
        }
        Debug.LogWarning($"Audio clip {name} not found!");
        return null;
    }

    [ClientRpc]
    public void PlaySoundClientRpc(Vector3 position, string clipName)
    {
        // Find or load the sound clip
        AudioClip clip = GetSFXClipByName(clipName);
        if (clip == null) {
            Debug.LogWarning($"Sound {clipName} not found!");
            return;
        }

        // Play the sound at the specified position
        AudioSource.PlayClipAtPoint(clip, position);
    }

    [ClientRpc]
    public void StartTankMoveSoundClientRpc()
    {
        tankMoveSource.Play();
        tankMoveSource.spatialBlend = 3f;
        tankMoveSource.minDistance = 1f;
        tankMoveSource.maxDistance = 10f;
        tankMoveSource.dopplerLevel = 0f;
        tankMoveSource.volume = 1f;
        tankMoveSource.loop = true;
    }

    [ClientRpc]
    public void StopTankMoveSoundClientRpc()
    {
        if (tankMoveSource.isPlaying) {
            tankMoveSource.Stop();
        }
    }
}
