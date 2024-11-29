using UnityEngine;
using Unity.Netcode;

public class NetworkProjectile : NetworkBehaviour
{
    public float speed = 80f; // Adjust as needed
    private float damage;     // Damage dealt by this projectile
    public Vector2 MoveDirection { get; set; }
    public Rigidbody2D rb;

    public override void OnNetworkSpawn()
    {
        if (IsServer || IsOwner) {
            // Set the projectile to move as soon as it spawns
            rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = MoveDirection * speed;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = MoveDirection * speed;
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    [ServerRpc]
    private void OnTriggerEnter2DServerRpc(ulong otherNetworkObjectId)
    {
        NetworkObject otherObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[otherNetworkObjectId];
        if (otherObj != null && otherObj.CompareTag("Player")) {
            // Only the server handles damage to the enemy
            //enemyHealth.TakeDamage(damage);

            // Destroy the projectile on the server, synced to all clients
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer) {
            NetworkObject networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null) {
                OnTriggerEnter2DServerRpc(networkObject.NetworkObjectId);
            }
        }
    }
}
