using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkTankGunController : NetworkBehaviour
{
    public Transform tankGun;
    private Camera mainCamera;
    private Vector2 mousePosition;
    private Vector2 rightStickInput;

    private void Start()
    {
        if (!IsOwner) return;

        mainCamera = Camera.main;
    }

    public void SetMousePosition(Vector2 position)
    {
        mousePosition = position;
    }

    public void SetRightStickInput(Vector2 input)
    {
        rightStickInput = input;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (rightStickInput != Vector2.zero) {
            RotateGunWithStick();
        } else {
            RotateGunToMouse();
        }
    }

    private void RotateGunToMouse()
    {
        //if (mainCamera == null || tankGun == null) return;

        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        worldMousePosition.z = 0f;
        Vector2 direction = (worldMousePosition - tankGun.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        tankGun.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void RotateGunWithStick()
    {
        if (rightStickInput.sqrMagnitude > 0.1f) {
            float angle = Mathf.Atan2(rightStickInput.y, rightStickInput.x) * Mathf.Rad2Deg - 90f;
            tankGun.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    public void PlayerTryFire(InputAction.CallbackContext context)
    {
        if (context.performed) {
            //GetComponent<NetworkPlayerGun>().TryFire();
        }
    }
}
