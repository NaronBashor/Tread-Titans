using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    Animator anim;

    [Header("Explosion Settings")]
    public float explosionRadius = 5f; // Radius of the explosion
    public float explosionDamage = 50f; // Damage dealt to nearby objects
    public LayerMask damageableLayers; // Layers that can take damage

    [Header("Barrel Settings")]
    public float health = 1f; // Barrel's health
    public bool isExploded = false;

    [Header("Sound Effects")]
    public string explosionSound = "Explosion";

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(float damage)
    {
        if (isExploded) return;

        health -= damage;

        if (health <= 0) {
            Explode();
        }
    }

    private void Explode()
    {
        if (isExploded) return;
        isExploded = true;

        anim.SetTrigger("Explode");

        // Play explosion sound
        AudioManager.Instance.PlaySFX(explosionSound);

        // Deal damage to nearby objects
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);

        foreach (Collider2D hitObject in hitObjects) {
            // Apply damage if the object has a health component
            NetworkTankHealth healthComponent = hitObject.GetComponent<NetworkTankHealth>();
            if (healthComponent != null) {
                healthComponent.TakeDamageFromAnythingServerRpc(explosionDamage); // Pass spawn side if necessary
            }
        }
    }

    public void DestroyObjectAfterAnimation()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the explosion radius in the editor for visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
