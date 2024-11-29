using Unity.Netcode;
using UnityEngine;

public class NetworkSoundPlayer : NetworkBehaviour
{
    [SerializeField] private AudioSource audioSource; // Optional: Set in Inspector

    [ClientRpc]
    public void PlaySoundClientRpc(Vector3 position, string clipName)
    {
        // Find or load the sound clip
        AudioClip clip = Resources.Load<AudioClip>(clipName);
        if (clip == null) {
            Debug.LogWarning($"Sound {clipName} not found!");
            return;
        }

        // Play the sound at the specified position
        AudioSource.PlayClipAtPoint(clip, position);
    }
}
