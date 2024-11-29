using Unity.Netcode;
using UnityEngine;

public class UniversalBomb : NetworkBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f; // Radius of AoE damage
    public float explosionDamage = 50f; // Damage dealt to each target

    [Header("Detonation Settings")]
    public bool useTimer = true; // If true, detonates after a timer
    public float detonationTime = 5f; // Time in seconds before the bomb explodes

    public bool useProximity = true; // If true, detonates on proximity
    public string triggerTag = "Player"; // Tag for objects that trigger the bomb

    Animator anim;

    private bool hasExploded = false; // Ensures the bomb only explodes once

    private void Start()
    {
        anim = GetComponent<Animator>();

        if (useTimer) {
            // Start timer-based detonation
            Invoke(nameof(Explode), detonationTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (useProximity && !hasExploded && other.CompareTag(triggerTag)) {
            Invoke("Explode", 0.5f);
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        anim.SetTrigger("Explode");
    }

    public void ExplodeAndPlaySound()
    {
        // Find all objects within the explosion radius
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius, LayerMask.GetMask("Player"));

        // Apply damage to each object
        foreach (Collider2D obj in hitObjects) {
            // Example: Assuming tanks have a script with a TakeDamage method
            var tank = obj.GetComponent<NetworkTankHealth>();
            if (tank != null) {
                tank.TakeDamageFromAnythingServerRpc(explosionDamage);
            }
        }

        AudioManager.Instance.PlaySoundClientRpc(transform.position, "Bomb Sound");
    }

    public void DestroyAfterAnimation()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the explosion radius in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
