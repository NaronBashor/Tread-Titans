using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    public static TestRelay Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    public async Task<string> CreateRelay()
    {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("Join code: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            return joinCode;

        } catch (RelayServiceException e) { Debug.Log(e); return null; }
    }

    public async void JoinRelay(string joinCode, int retryCount = 0)
    {
        try {
            Debug.Log("Joining relay with code: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        } catch (RelayServiceException e) {
            Debug.LogError(e);

            if (retryCount < 3) // Retry up to 3 times
            {
                int delay = (retryCount + 1) * 1000; // Exponential backoff
                Debug.LogWarning($"Retrying join... Attempt {retryCount + 1}");
                await Task.Delay(delay);
                JoinRelay(joinCode, retryCount + 1);
            }
        }
    }

}
