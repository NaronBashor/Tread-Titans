using Unity.Netcode;
using UnityEngine;

public class NetworkPrefabHashLogger : MonoBehaviour
{
    void Start()
    {
        var networkObject = GetComponent<NetworkObject>();
        if (networkObject != null) {
            //Debug.Log($"Prefab: {gameObject.name}, NetworkObjectId: {networkObject.GetHashCode()}");
        }
    }
}
