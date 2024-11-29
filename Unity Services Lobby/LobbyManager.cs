using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_GAME_MODE = "GameMode";

    public event EventHandler OnLeftLobby;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnLobbyGameModeChanged;

    PlayerData playerData;

    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public enum GameMode
    {
        Deathmatch
    }

    private float heartbeatTimer;
    private float lobbyPollTimer;
    private float refreshLobbyListTimer = 5f;
    private Lobby joinedLobby;
    private string playerName;

    public Dictionary<ulong, string> clientToPlayerIdMap = new Dictionary<ulong, string>();

    public void MapClientIdToPlayerId(ulong clientId)
    {
        string playerId = AuthenticationService.Instance.PlayerId;
        if (!clientToPlayerIdMap.ContainsKey(clientId)) {
            clientToPlayerIdMap[clientId] = playerId;
            Debug.Log($"Mapped clientId {clientId} to playerId {playerId}");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    private async void Start()
    {
        ClearLobbyState();
        playerData = FindAnyObjectByType<PlayerData>();

        bool isAuthenticated = await Authenticate();
        if (!isAuthenticated) {
            Debug.LogError("Authentication failed. Unable to proceed.");
            return; // Prevent further execution
        }

        Debug.Log("Player authenticated successfully.");
    }

    private void ClearLobbyState()
    {
        joinedLobby = null;
        heartbeatTimer = 0;
        lobbyPollTimer = 0;
        refreshLobbyListTimer = 5f;
    }

    private void Update()
    {
        //HandleRefreshLobbyList(); // Disabled Auto Refresh for testing with multiple builds
        if (SceneManager.GetActiveScene().name == "Game") { return; }
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    public async Task<bool> Authenticate()
    {
        playerName = playerData.playerName;

        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        try {
            if (UnityServices.State != ServicesInitializationState.Initialized) {
                await UnityServices.InitializeAsync(initializationOptions);
            }

            if (!AuthenticationService.Instance.IsSignedIn) {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in with Player ID: " + AuthenticationService.Instance.PlayerId);
                return true; // Return true if authentication is successful
            }
        } catch (Exception e) {
            Debug.LogError("Authentication failed: " + e.Message);
        }

        return false; // Return false if authentication fails
    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost()) {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f) {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                Debug.Log("Heartbeat");
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (joinedLobby != null) {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f) {
                float lobbyPollTimerMax = 1.1f;
                lobbyPollTimer = lobbyPollTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                if (!IsPlayerInLobby()) {
                    // Player was kicked out of this lobby
                    Debug.Log("Kicked from Lobby!");

                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    joinedLobby = null;
                }

                // Check if the game has started (relay code is non-zero)
                if (joinedLobby.Data["Start Game"].Value != "0") {
                    string relayCode = joinedLobby.Data["Start Game"].Value;
                    if (!IsLobbyHost()) {
                        // Clients join the relay, but they do not load the scene themselves
                        TestRelay.Instance.JoinRelay(relayCode);
                    }

                    joinedLobby = null;
                }
            }
        }
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null) {
            foreach (Player player in joinedLobby.Players) {
                if (player.Id == AuthenticationService.Instance.PlayerId) {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>
        {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerData.playerName) }
        });
    }

    public void ChangeGameMode()
    {
        if (IsLobbyHost()) {
            GameMode gameMode =
                Enum.Parse<GameMode>(joinedLobby.Data[KEY_GAME_MODE].Value);

            switch (gameMode) {
                default:
                case GameMode.Deathmatch:
                    gameMode = GameMode.Deathmatch;
                    break;
            }

            UpdateLobbyGameMode(gameMode);
        }
    }

    public async Task CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, GameMode gameMode)
    {
        if (!AuthenticationService.Instance.IsSignedIn) {
            Debug.LogError("User is not signed in. Please authenticate first.");
            return;
        }

        Debug.Log("Creating Lobby.");

        try {
            CreateLobbyOptions options = new CreateLobbyOptions {
                Player = GetPlayer(),
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>
                {
                { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) },
                { "Start Game", new DataObject(DataObject.VisibilityOptions.Member, "0") }
            }
            };

            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log("Lobby created with ID: " + joinedLobby.Id);
        } catch (LobbyServiceException e) {
            Debug.LogError("Failed to create lobby: " + e);
        }
    }

    public async void RefreshLobbyList()
    {
        try {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        Player player = GetPlayer();

        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions {
            Player = player
        });

        joinedLobby = lobby;

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    public async void JoinLobby(Lobby lobby)
    {
        Player player = GetPlayer();

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions {
            Player = player
        });

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    //public async void UpdatePlayerName(string playerName)
    //{
    //    this.playerName = playerData.playerName;

    //    if (joinedLobby != null) {
    //        try {
    //            UpdatePlayerOptions options = new UpdatePlayerOptions();

    //            options.Data = new Dictionary<string, PlayerDataObject>() {
    //                {
    //                    KEY_PLAYER_NAME, new PlayerDataObject(
    //                        visibility: PlayerDataObject.VisibilityOptions.Public,
    //                        value: playerName)
    //                }
    //            };

    //            string playerId = AuthenticationService.Instance.PlayerId;

    //            Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
    //            joinedLobby = lobby;

    //            OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
    //        } catch (LobbyServiceException e) {
    //            Debug.Log(e);
    //        }
    //    }
    //}

    public async void UpdatePlayerCharacter()
    {
        if (joinedLobby != null) {
            try {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void QuickJoinLobby()
    {
        try {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobby = lobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private void OnApplicationQuit()
    {
        LeaveLobby();
        SignOutUser();
        ClearLobbyState();
    }

    public void SignOutUser()
    {
        if (AuthenticationService.Instance.IsSignedIn) {
            AuthenticationService.Instance.SignOut();
            Debug.Log("User signed out successfully.");
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;

                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost()) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void UpdateLobbyGameMode(GameMode gameMode)
    {
        try {
            Debug.Log("UpdateLobbyGameMode " + gameMode);

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) }
                }
            });

            joinedLobby = lobby;

            OnLobbyGameModeChanged?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void StartGame()
    {
        if (!IsLobbyHost()) {
            Debug.LogWarning("Only the host can start the game!");
            return;
        }

        try {
            Debug.Log("Starting game...");
            string relayCode = await TestRelay.Instance.CreateRelay();

            if (string.IsNullOrEmpty(relayCode)) {
                Debug.LogError("Relay creation failed. Cannot start game.");
                return; // Exit if relay creation fails
            }

            // Update the lobby with the relay code for clients to join
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject>
                {
                { "Start Game", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
            }
            });

            joinedLobby = lobby;

            if (NetworkManager.Singleton.IsServer) {
                NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            }
        } catch (RelayServiceException e) {
            Debug.LogError("Failed to start game: " + e);
        }
    }

    public void LeaveLobbyToProfile()
    {
        SceneManager.LoadScene("Profile");
    }
}