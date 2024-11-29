using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    [Header("Tank Body Settings")]
    public float moveSpeed;
    public float rotationSmoothTime = 0.1f; // Time for smooth rotation
    public float movementSmoothTime = 0.1f; // Time for smooth movement

    [Header("References")]
    public Rigidbody2D tankBodyRb; // Rigidbody2D component for tank body movement
    public Transform tankGun;      // Reference to the tank gun for rotation

    private PlayerControls controls; // Reference to the generated input class
    private Camera mainCamera;
    private Vector2 movementInput; // Stores movement input
    private Vector2 mousePosition; // Stores mouse position
    private Vector2 rightStickInput; // Stores right stick input
    private Vector2 currentVelocity; // For SmoothDamp velocity

    private Animator leftTrackAnimator;
    private Animator rightTrackAnimator;

    private float currentRotationVelocity; // For SmoothDamp rotation

    private void Awake()
    {
        // Initialize controls and set up input callbacks
        controls = new PlayerControls();

        // Bind actions to movement and mouse position
        controls.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => movementInput = Vector2.zero;
        controls.Player.Look.performed += ctx => mousePosition = ctx.ReadValue<Vector2>();
        controls.Player.Aim.performed += ctx => rightStickInput = ctx.ReadValue<Vector2>();
        controls.Player.Aim.canceled += ctx => rightStickInput = Vector2.zero;
        controls.Player.Shoot.performed += PlayerTryFire;

        mainCamera = Camera.main;

        // Find the left and right track animators
        leftTrackAnimator = transform.Find("Left Track").GetComponent<Animator>();
        rightTrackAnimator = transform.Find("Right Track").GetComponent<Animator>();
    }

    private void OnEnable()
    {
        controls.Enable(); // Enable input actions when the object is active
    }

    private void OnDisable()
    {
        controls.Disable(); // Disable input actions when the object is inactive
    }

    private void Update()
    {
        MoveTank();
        if (rightStickInput != Vector2.zero) {
            RotateGunWithStick();
        } else {
            RotateGunToMouse();
        }

        bool isMoving = tankBodyRb.linearVelocity.sqrMagnitude > 0.1f; // Checks if the tank is moving
        UpdateTrackAnimation(isMoving);

        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    private void UpdateTrackAnimation(bool isMoving)
    {
        if (leftTrackAnimator != null) {
            leftTrackAnimator.SetBool("isMoving", isMoving);
        }

        if (rightTrackAnimator != null) {
            rightTrackAnimator.SetBool("isMoving", isMoving);
        }
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
        Debug.Log("Setting speed to: " + moveSpeed);
    }

    private void MoveTank()
    {
        // Smoothly calculate target velocity from movement input
        Vector2 targetVelocity = movementInput.normalized * moveSpeed;
        Vector2 smoothedVelocity = Vector2.SmoothDamp(tankBodyRb.linearVelocity, targetVelocity, ref currentVelocity, movementSmoothTime);
        tankBodyRb.linearVelocity = smoothedVelocity;

        // If the tank is moving, smoothly rotate it in the direction of movement
        if (smoothedVelocity != Vector2.zero) {
            float targetAngle = Mathf.Atan2(smoothedVelocity.y, smoothedVelocity.x) * Mathf.Rad2Deg - 90f;
            float smoothedAngle = Mathf.SmoothDampAngle(tankBodyRb.rotation, targetAngle, ref currentRotationVelocity, rotationSmoothTime);
            tankBodyRb.rotation = smoothedAngle;
        }
    }

    private void RotateGunToMouse()
    {
        // Convert mouse position to world space
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        worldMousePosition.z = 0f; // Ensure we work in 2D

        // Calculate direction and angle to face the mouse
        Vector2 direction = (worldMousePosition - tankGun.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // Rotate the gun to face the mouse
        tankGun.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void RotateGunWithStick()
    {
        if (rightStickInput.sqrMagnitude > 0.1f) // Only rotate if there's significant input
        {
            float angle = Mathf.Atan2(rightStickInput.y, rightStickInput.x) * Mathf.Rad2Deg - 90f;
            tankGun.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    public void PlayerTryFire(InputAction.CallbackContext context)
    {
        if (context.performed) {
            GetComponent<PlayerGun>().TryFire();
        }
    }
}
