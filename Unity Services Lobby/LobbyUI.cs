using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static LobbyManager;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button startGameButton;

    [SerializeField] private GameObject exitButtonToHide;

    private Player player;

    private void Awake()
    {
        Instance = this;
        if (SceneManager.GetActiveScene().name == "Game") {
            Destroy(gameObject);
        }

        playerSingleTemplate.gameObject.SetActive(false);

        leaveLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
        });

        startGameButton.onClick.AddListener(() => {
            LobbyManager.Instance.StartGame();
        });
    }

    private void Start()
    {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLobbyGameModeChanged += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        Hide();
    }

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e)
    {
        ClearLobby();
        Hide();
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    private void UpdateLobby()
    {
        Lobby lobby = LobbyManager.Instance.GetJoinedLobby();
        if (lobby == null) return;

        ClearLobby();

        // Set the player reference for the local player
        player = lobby.Players.Find(p => p.Id == AuthenticationService.Instance.PlayerId);

        foreach (Player lobbyPlayer in lobby.Players) {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                lobbyPlayer.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            lobbyPlayerSingleUI.UpdatePlayer(lobbyPlayer);
        }

        lobbyNameText.text = lobby.Name;
        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

        Show();
    }

    private void ClearLobby()
    {
        if (container != null && container.childCount > 0) {
            foreach (Transform child in container) {
                if (child == playerSingleTemplate) continue;
                Destroy(child.gameObject);
            }
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        exitButtonToHide.SetActive(true);
    }

    private void Show()
    {
        gameObject.SetActive(true);
        exitButtonToHide.SetActive(false);
    }
}
