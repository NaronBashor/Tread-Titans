using System.Collections.Generic;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    public static AudioPool Instance;

    [Header("Audio Source Pool")]
    public int poolSize = 10;
    public GameObject audioSourcePrefab; // A prefab with an AudioSource
    private Queue<AudioSource> audioPool = new Queue<AudioSource>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++) {
            var audioSourceObj = Instantiate(audioSourcePrefab, transform);
            var audioSource = audioSourceObj.GetComponent<AudioSource>();
            audioSourceObj.SetActive(false);
            audioPool.Enqueue(audioSource);
        }
    }

    public void PlaySound(AudioClip clip, Vector3 position)
    {
        if (audioPool.Count > 0) {
            AudioSource source = audioPool.Dequeue();

            // Activate the audio source
            source.gameObject.SetActive(true);

            // Set its position and clip
            source.transform.position = position;
            source.clip = clip;
            source.spatialBlend = 1f;
            source.volume = 1f;
            source.dopplerLevel = 0;
            source.minDistance = 3f;
            source.maxDistance = 10f;

            // Play the audio
            source.Play();

            // Return it to the pool after the clip's duration
            StartCoroutine(ReturnToPool(source, clip.length));
        }
    }


    private System.Collections.IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.gameObject.SetActive(false);
        audioPool.Enqueue(source);
    }
}
