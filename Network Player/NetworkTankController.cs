using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class NetworkTankController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed;
    public float rotationSmoothTime = 0.1f;
    public float movementSmoothTime = 0.1f;

    public Rigidbody2D tankBodyRb;
    public GameObject bulletHitAnimation;

    private Vector2 currentVelocity;
    private float currentRotationVelocity;
    private Vector2 movementInput;

    [Header("Camera Settings")]
    public Camera mainCamera;

    [Header("Gun Settings")]
    public float damage = 10f;
    public float fireRate = 1f;  // 1 is once per second, 2 is 2 per second, 0.5 is once every 2 seconds
    [Range(0f, 100f)] public float accuracy = 100f;
    private float fireCooldown;

    [SerializeField] private GameObject bulletTrailPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;

    private Animator leftTrackAnimator;
    private Animator rightTrackAnimator;
    private Animator gunAnimator;

    private Vector2 minBounds;
    private Vector2 maxBounds;

    private Vector2 mousePosition;
    public Transform tankGun;
    private bool isMovementSoundPlaying = false;
    private AudioSource movementAudioSource;

    private bool canMove = true;

    public bool CanMove { get { return canMove; } set { canMove = value; } }

    public override void OnNetworkSpawn()
    {
        InitTankInfo();
    }

    private void Start()
    {
        InitTankInfo();
        FetchMapBounds();

        // Add an AudioSource for movement sounds if not already present
        if (movementAudioSource == null) {
            movementAudioSource = gameObject.AddComponent<AudioSource>();
            movementAudioSource.clip = AudioManager.Instance.GetSFXClipByName("Tank Move"); // Set movement clip
            movementAudioSource.spatialBlend = 1f; // Fully 3D
            movementAudioSource.minDistance = 3f; // Full volume within this range
            movementAudioSource.maxDistance = 8f; // Silent beyond this range
            movementAudioSource.rolloffMode = AudioRolloffMode.Logarithmic; // Default 3D roll-off
            movementAudioSource.dopplerLevel = 0f; // No Doppler effect
            movementAudioSource.volume = 0.75f;
        }

        canMove = false;
    }

    private void InitTankInfo()
    {
        if (!IsOwner) return;

        mainCamera = FindAnyObjectByType<Camera>();
        mainCamera.enabled = true;
        if (mainCamera == null) {
            //Debug.LogError("Main camera not found. Attempting to find it manually.");
            mainCamera = FindAnyObjectByType<Camera>();

            if (mainCamera != null) {
                mainCamera.tag = "MainCamera";
            } else {
                //Debug.LogError("Camera could not be found or assigned.");
            }
        }

        if (leftTrackAnimator == null || rightTrackAnimator == null || gunAnimator == null) {
            leftTrackAnimator = transform.Find("Left Track").GetComponent<Animator>();
            rightTrackAnimator = transform.Find("Right Track").GetComponent<Animator>();
            gunAnimator = firePoint.GetComponent<Animator>();
        }

        if (IsOwner) {
            Camera.main.GetComponent<AudioListener>().enabled = true;
        } else {
            Camera.main.GetComponent<AudioListener>().enabled = false;
        }

    }

    public void SetStats(float damage, float fireRate, float accuracy)
    {
        this.damage = damage;
        this.fireRate = fireRate;
        this.accuracy = Mathf.Clamp(accuracy, 0f, 100f);
    }

    public void OnMovePlayer(InputAction.CallbackContext context)
    {
        if (!IsOwner || !canMove) return;

        if (context.performed) {
            movementInput = context.ReadValue<Vector2>();
        } else if (context.canceled) {
            movementInput = Vector2.zero;
        }
    }

    public void OnTryFire(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (fireCooldown > 0) return;

        if (context.performed && SceneManager.GetActiveScene().name == "Game") {
            //Debug.Log("Host/client is attempting to fire");

            fireCooldown = 1f / fireRate;

            // Play the firing sound across all clients
            FireSoundServerRpc(firePoint.position);

            FireServerRpc();

            Vector2 origin = firePoint.position;
            Vector2 direction = tankGun.up.normalized;
            Vector2 finalDirection = ApplyInaccuracy(direction);
            float range = 50f;

            RaycastHit2D hitInfo = Physics2D.Raycast(origin, finalDirection, range, LayerMask.GetMask("Player", "Walls", "Barrel"));
            if (hitInfo) {
                Vector3 hitPosition = hitInfo.point;
                string hitLayerName = LayerMask.LayerToName(hitInfo.collider.gameObject.layer);

                if (hitLayerName == "Walls") {
                    BulletHitAnimationServerRpc(hitPosition, 0, this.transform.position);
                    StartCoroutine(HitSoundDelay(hitPosition));
                    //Debug.Log("Bullet hit a wall at " + hitPosition);
                } else if (hitLayerName == "Barrel") {
                    ExplosiveBarrel targetHealth = hitInfo.collider.GetComponent<ExplosiveBarrel>();
                    if (targetHealth != null) {
                        StartCoroutine(HitSoundDelay(hitPosition));
                        targetHealth.TakeDamage(damage);
                    }
                } else if (hitLayerName == "Player") {
                    NetworkTankHealth targetHealth = hitInfo.collider.GetComponent<NetworkTankHealth>();
                    if (targetHealth != null) {
                        StartCoroutine(HitSoundDelay(hitPosition));
                        //Debug.Log("Bullet hit a wall at " + hitPosition);
                        targetHealth.TakeDamageServerRpc(damage, this.GetComponent<NetworkTankHealth>().spawnSide.Value);
                        BulletHitAnimationServerRpc(hitPosition, hitInfo.collider.GetComponent<NetworkObject>().NetworkObjectId, this.transform.position);
                        //Debug.Log("Hit " + hitInfo.collider.name + " for " + damage + " damage.");
                    }
                }
            } else {
                //Debug.Log("No hit detected");
            }
        }
    }

    private IEnumerator HitSoundDelay(Vector3 hitPosition)
    {
        yield return new WaitForSeconds(0.125f);
        BulletHitSoundServerRpc(hitPosition);
    }

    [ServerRpc]
    private void FireSoundServerRpc(Vector3 firePosition)
    {
        AudioManager.Instance.PlaySoundClientRpc(firePosition, "Basic Tank Shot");
    }

    [ServerRpc(RequireOwnership = false)]
    private void BulletHitSoundServerRpc(Vector3 hitPosition)
    {
        AudioManager.Instance.PlaySoundClientRpc(hitPosition, "Impact Sound");
    }

    private void ShowBulletTrail(Vector2 start, Vector2 end)
    {
        firePoint.GetComponent<Animator>().SetTrigger("Shoot");

        GameObject bulletTrail = Instantiate(bulletTrailPrefab);
        LineRenderer lineRenderer = bulletTrail.GetComponent<LineRenderer>();

        if (lineRenderer != null) {
            // Set the width of the trail
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.004f;

            // Create a gradient with semi-transparent colors for a fading effect
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                new GradientColorKey(startColor, 0f),
                new GradientColorKey(endColor, 1.0f)
                },
                new GradientAlphaKey[] {
                new GradientAlphaKey(0.3f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            lineRenderer.colorGradient = gradient;

            // Set the start and end positions of the trail
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        bulletTrail.transform.position = start;

        Vector2 direction = (end - start).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bulletTrail.transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(bulletTrail, 0.1f);
    }

    private void FetchMapBounds()
    {
        // Retrieve the GameManager
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null || gameManager.currentMapConfig == null) {
            Debug.LogError("GameManager or currentMapConfig is missing!");
            return;
        }

        // Get min and max bounds from the map configuration
        minBounds = gameManager.currentMapConfig.minCameraBounds;
        maxBounds = gameManager.currentMapConfig.maxCameraBounds;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (mainCamera == null) {
            InitTankInfo();
        }
        // Move the camera with the tank but constrain it within the bounds
        Vector3 cameraPosition = new Vector3(transform.position.x, transform.position.y, -10f);
        cameraPosition.x = Mathf.Clamp(cameraPosition.x, minBounds.x, maxBounds.x);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, minBounds.y, maxBounds.y);

        mainCamera.transform.position = cameraPosition;
        mousePosition = Mouse.current.position.ReadValue();

        RotateGunToMouse();

        if (fireCooldown > 0) {
            fireCooldown -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !canMove) return;

        if (movementInput != Vector2.zero) {
            Vector2 targetVelocity = movementInput.normalized * moveSpeed;
            Vector2 smoothedVelocity = Vector2.SmoothDamp(tankBodyRb.linearVelocity, targetVelocity, ref currentVelocity, movementSmoothTime);
            tankBodyRb.linearVelocity = smoothedVelocity;

            if (!isMovementSoundPlaying) {
                StartMovementSoundServerRpc();
                isMovementSoundPlaying = true;
            }

            // Update the position of the movement sound source to follow the tank
            movementAudioSource.transform.position = transform.position;

            // Update the position of the tank movement sound source
            AudioManager.Instance.tankMoveSource.transform.position = transform.position;

            RotateTankTowardsMovement(smoothedVelocity);
        } else {
            tankBodyRb.linearVelocity = Vector2.zero;

            if (isMovementSoundPlaying) {
                StopMovementSoundServerRpc();
                isMovementSoundPlaying = false;
            }
        }

        UpdateTrackAnimation(tankBodyRb.linearVelocity != Vector2.zero);
    }

    [ServerRpc]
    private void StartMovementSoundServerRpc()
    {
        //Debug.Log("StartMovementSoundServerRpc called");
        if (movementAudioSource != null && !movementAudioSource.isPlaying) {
            //Debug.Log("Playing movement sound");
            movementAudioSource.Play();
        }
        PlayMovementSoundClientRpc();
    }

    [ServerRpc]
    private void StopMovementSoundServerRpc()
    {
        //Debug.Log("StopMovementSoundServerRpc called");
        if (movementAudioSource != null && movementAudioSource.isPlaying) {
            //Debug.Log("Stopping movement sound");
            movementAudioSource.Stop();
        }
        StopMovementSoundClientRpc();
    }


    [ClientRpc]
    private void PlayMovementSoundClientRpc()
    {
        //Debug.Log("PlayMovementSoundClientRpc called");
        if (movementAudioSource != null && !movementAudioSource.isPlaying) {
            //Debug.Log("Playing movement sound on client");
            movementAudioSource.Play();
        }
    }

    [ClientRpc]
    private void StopMovementSoundClientRpc()
    {
        //Debug.Log("StopMovementSoundClientRpc called");
        if (movementAudioSource != null && movementAudioSource.isPlaying) {
            //Debug.Log("Stopping movement sound on client");
            movementAudioSource.Stop();
        }
    }


    private void UpdateTrackAnimation(bool isMoving)
    {
        if (leftTrackAnimator != null && IsOwner) {
            leftTrackAnimator.SetBool("isMoving", isMoving);
        }

        if (rightTrackAnimator != null) {
            rightTrackAnimator.SetBool("isMoving", isMoving);
        }
    }

    private void RotateTankTowardsMovement(Vector2 smoothedVelocity)
    {
        if (smoothedVelocity != Vector2.zero) {
            float targetAngle = Mathf.Atan2(smoothedVelocity.y, smoothedVelocity.x) * Mathf.Rad2Deg - 90f;
            float smoothedAngle = Mathf.SmoothDampAngle(tankBodyRb.rotation, targetAngle, ref currentRotationVelocity, rotationSmoothTime);
            tankBodyRb.MoveRotation(smoothedAngle);
        }
    }

    private void RotateGunToMouse()
    {
        if (mainCamera == null || tankGun == null) return;

        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.nearClipPlane));

        worldMousePosition.z = tankGun.position.z;

        Vector2 direction = (worldMousePosition - tankGun.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        tankGun.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    [ServerRpc]
    private void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        if (tankGun == null || firePoint == null || bulletTrailPrefab == null) return;

        Vector2 origin = firePoint.position;
        Vector2 direction = tankGun.up.normalized;
        Vector2 finalDirection = ApplyInaccuracy(direction);
        float range = 50f;

        // Perform the raycast
        RaycastHit2D hitInfo = Physics2D.Raycast(origin, finalDirection, range, LayerMask.GetMask("Player", "Walls"));

        // If a hit is detected, set the target point to the hit position; otherwise, use the max range
        Vector2 targetPoint = hitInfo ? hitInfo.point : (Vector2)origin + finalDirection * range;

        // Trigger a ClientRpc to show the bullet trail on all clients
        ShowBulletTrailClientRpc(origin, targetPoint);
    }


    [ClientRpc]
    private void ShowBulletTrailClientRpc(Vector2 start, Vector2 end)
    {
        ShowBulletTrail(start, end);
    }


    [ServerRpc(RequireOwnership = false)]
    private void BulletHitAnimationServerRpc(Vector3 hitPosition, ulong networkObjectId, Vector3 shooterPosition)
    {
        if (bulletHitAnimation != null) {
            if (networkObjectId != 0 && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject parentNetObj)) {
                // Calculate the direction from the hit point to the shooter for animation rotation
                Vector3 directionToShooter = (hitPosition - shooterPosition).normalized;

                // Trigger the client-side hit animation exactly at the hit point
                ShowBulletHitAnimationClientRpc(hitPosition, directionToShooter);
            } else {
                // Wall hit case
                Vector3 directionToShooter = (hitPosition - shooterPosition).normalized;
                ShowBulletHitAnimationClientRpc(hitPosition, directionToShooter);
            }
        }
    }



    [ClientRpc]
    private void ShowBulletHitAnimationClientRpc(Vector3 hitPosition, Vector3 directionToShooter)
    {
        if (bulletHitAnimation == null) return;

        GameObject animationInstance = Instantiate(bulletHitAnimation, hitPosition, Quaternion.identity);
        float angle = Mathf.Atan2(directionToShooter.y, directionToShooter.x) * Mathf.Rad2Deg + 90f;
        animationInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        Destroy(animationInstance, 0.2f);
    }




    private Vector2 ApplyInaccuracy(Vector2 direction)
    {
        float inaccuracyFactor = 1f - (accuracy / 100f);
        float angleOffset = Random.Range(-inaccuracyFactor, inaccuracyFactor) * 10f;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angleOffset);
        return rotation * direction;
    }
}
