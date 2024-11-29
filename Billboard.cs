using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cameraTransform;
    public Transform target; // Reference to the tank or parent
    public Vector3 offset = new Vector3(0, 2.0f, 0); // Adjust the Y value as needed

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (target != null) {
            // Maintain a constant offset above the target's position
            transform.position = target.position + offset;
        }

        // Make the HP bar face the camera
        transform.LookAt(transform.position + cameraTransform.forward);
    }
}
