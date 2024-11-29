using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ProfileSceneController : MonoBehaviour
{
    [SerializeField] private GameObject partsPanel;
    [SerializeField] private GameObject hangarPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject playerNamePanel;

    [SerializeField] private TextMeshProUGUI currencyTotalText;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TMP_InputField playerNameInput;

    PlayerData playerData;

    private void OnEnable()
    {
        // Remove the DontDestroyOnLoad in specific cases
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.transform.SetParent(null); // Detach from parent if needed
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (LobbyManager.Instance != null) {
            LobbyManager.Instance.transform.SetParent(null); // Detach from parent if needed
            Destroy(LobbyManager.Instance.gameObject);
        }

        playerData = FindAnyObjectByType<PlayerData>();
    }

    private void Start()
    {
        if (playerData.needPlayerName) {
            playerNamePanel.SetActive(true);
        } else {
            playerNamePanel.SetActive(false);
        }
        ShowPartsPanel();
        playerName.text = playerData.playerName;
    }

    public void OnAssignPlayerName(string name)
    {
        playerData.playerName = name;
        playerData.SaveProfile();
    }

    public void OnClosePlayerNamePanel()
    {
        if (playerNamePanel.activeSelf) {
            OnAssignPlayerName(playerNameInput.text);
            AudioManager.Instance.PlaySFX("Button Click");
            playerNamePanel.SetActive(false);
        }
    }

    private void Update()
    {
        playerName.text = playerData.playerName;

        currencyTotalText.text = playerData.GetCurrency().ToString();
    }

    // These methods can be linked directly to button clicks via the Inspector
    public void ShowPartsPanel()
    {
        ActivatePanel(partsPanel);
    }

    public void ShowHangarPanel()
    {
        ActivatePanel(hangarPanel);
    }

    public void ShowShopPanel()
    {
        ActivatePanel(shopPanel);
    }

    public void ShowUpgradePanel()
    {
        ActivatePanel(upgradePanel);
    }

    // This method enables the selected panel and disables all others
    private void ActivatePanel(GameObject activePanel)
    {
        AudioManager.Instance.PlaySFX("Button Click");
        partsPanel.SetActive(activePanel == partsPanel);
        hangarPanel.SetActive(activePanel == hangarPanel);
        shopPanel.SetActive(activePanel == shopPanel);
        upgradePanel.SetActive(activePanel == upgradePanel);
    }
}
