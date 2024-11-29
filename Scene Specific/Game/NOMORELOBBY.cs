using UnityEngine;

public class NOMORELOBBY : MonoBehaviour
{
    private void OnEnable()
    {
        if (LobbyManager.Instance != null) {
            LobbyManager.Instance.transform.SetParent(null); // Detach from parent if needed
            Destroy(LobbyManager.Instance.gameObject);
        }
    }
}
