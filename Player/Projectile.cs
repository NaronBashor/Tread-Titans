using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;    // Projectile speed
    private float damage;         // Damage dealt by this projectile

    private void Start()
    {
        Destroy(gameObject, 10f); // Destroy projectile after 5 seconds to prevent clutter
    }

    private void Update()
    {
        // Move the projectile along its own facing direction
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }


    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    public void SetDirection(Vector2 dir)
    {
        Vector2 direction = dir.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle); // No -90f adjustment
    }



    //[ServerCallback]
    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Enemy")) {
    //        // Only the server handles damage to the enemy
    //        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
    //        if (enemyHealth != null) {
    //            enemyHealth.TakeDamage(damage); // Apply damage on the server
    //        }

    //        NetworkServer.Destroy(gameObject); // Destroy the projectile on the server
    //    }
    //}
}
