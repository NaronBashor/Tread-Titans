using Lovatto.Countdown;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private bl_Countdown timer;
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] public MapConfig currentMapConfig; // Holds map-specific settings
    [SerializeField] private TextMeshProUGUI leftScoreText;
    [SerializeField] private TextMeshProUGUI rightScoreText;
    [SerializeField] private TextMeshProUGUI waitingForOtherPlayersText;
    [SerializeField] private Texture2D crosshairTexture;

    private int expectedPlayers = 1;

    private bool hostTankSpawned = false;
    private bool spawnOnLeftSide = true; // Toggle for alternating team spawns

    private int leftSideScore = 0;
    private int rightSideScore = 0;

    private void Start()
    {
        UpdateScoreUIClientRpc(0, 0); // Initialize score UI
        Cursor.SetCursor(crosshairTexture, new Vector2((crosshairTexture.width / 2), (crosshairTexture.height / 2)), CursorMode.ForceSoftware);
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            if (NetworkManager.Singleton.ConnectedClients.Count == expectedPlayers) {
                Debug.Log("All players connected. Starting countdown...");
                waitingForOtherPlayersText.enabled = false;
                timer.StartCountdown();
            } else {
                Debug.Log("Waiting for more players to connect...");
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            SetupMapConfig();
            RegisterHostAndClientConnections();
        }
    }

    private void SetupMapConfig()
    {
        if (currentMapConfig == null) {
            Debug.LogError("MapConfig is not assigned!");
            return;
        }

        Debug.Log($"Loaded Map: {currentMapConfig.mapName}");
        Debug.Log($"Left spawn: {currentMapConfig.leftSideSpawnPosition}, Right spawn: {currentMapConfig.rightSideSpawnPosition}");
    }

    private void RegisterHostAndClientConnections()
    {
        // Spawn host tank and map the client ID
        LobbyManager.Instance.MapClientIdToPlayerId(NetworkManager.Singleton.LocalClientId);

        if (!hostTankSpawned) {
            SpawnTankForClient(NetworkManager.Singleton.LocalClientId);
            hostTankSpawned = true;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        //if (clientId == NetworkManager.Singleton.LocalClientId && hostTankSpawned) {
        //    Debug.Log("Host tank already spawned.");
        //    return;
        //}

        // Check if all expected players are connected (including the host)
        if (NetworkManager.Singleton.ConnectedClients.Count == expectedPlayers) {
            Debug.Log("All players connected. Starting countdown...");
            timer.StartCountdown();
        } else {
            Debug.Log("Waiting for more players to connect...");
        }

        LobbyManager.Instance.MapClientIdToPlayerId(clientId);
        SpawnTankForClient(clientId);
    }

    private void SpawnTankForClient(ulong clientId)
    {
        if (!IsServer) return;

        Vector3 spawnPosition = spawnOnLeftSide ? currentMapConfig.leftSideSpawnPosition : currentMapConfig.rightSideSpawnPosition;
        GameObject tank = Instantiate(tankPrefab, spawnPosition, Quaternion.identity);

        NetworkTankHealth tankHealth = tank.GetComponent<NetworkTankHealth>();
        if (tankHealth != null) {
            tankHealth.spawnSide.Value = spawnOnLeftSide ? NetworkTankHealth.SpawnSide.left : NetworkTankHealth.SpawnSide.right;
        }

        var networkObject = tank.GetComponent<NetworkObject>();
        if (networkObject != null) {
            networkObject.SpawnWithOwnership(clientId);
        }

        spawnOnLeftSide = !spawnOnLeftSide;
    }

    public void AddScore(NetworkTankHealth.SpawnSide deadTankSide)
    {
        if (deadTankSide == NetworkTankHealth.SpawnSide.left) {
            rightSideScore++; // Right side scores if left side dies
        } else {
            leftSideScore++; // Left side scores if right side dies
        }

        UpdateScoreUIClientRpc(leftSideScore, rightSideScore);
    }

    [ClientRpc]
    private void UpdateScoreUIClientRpc(int leftScore, int rightScore)
    {
        leftScoreText.text = leftScore.ToString();
        rightScoreText.text = rightScore.ToString();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    public void StartRespawnCoroutine(NetworkTankHealth tank)
    {
        AddScore(tank.spawnSide.Value); // Use .Value to ensure we get the synchronized side

        // Trigger explosion effect at the tank's position for all clients
        TriggerExplosionClientRpc(tank.transform.position);

        StartCoroutine(RespawnCoroutine(tank));
    }

    [ClientRpc]
    private void TriggerExplosionClientRpc(Vector3 position)
    {
        if (explosionPrefab == null) return;

        // Instantiate the explosion at the specified position
        GameObject explosionInstance = Instantiate(explosionPrefab, position, Quaternion.identity);

        // Optionally, destroy the explosion after the animation completes
        Destroy(explosionInstance, 0.5f); // Adjust the time to match your animation length
    }

    private IEnumerator RespawnCoroutine(NetworkTankHealth tank)
    {
        yield return new WaitForSeconds(currentMapConfig.respawnTime);
        tank.RespawnTank();
    }

    private void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private async void OnApplicationQuit()
    {
        if (AuthenticationService.Instance.IsSignedIn) {
            AuthenticationService.Instance.SignOut();
        }
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
