using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditPlayerName : MonoBehaviour
{
    //public static EditPlayerName Instance { get; private set; }


    //public event EventHandler OnNameChanged;


    //[SerializeField] private TextMeshProUGUI playerNameText;


    //private string playerName = "PlayerName";


    //private void Awake()
    //{
    //    Instance = this;

    //    GetComponent<Button>().onClick.AddListener(() => {
    //        UI_InputWindow.Show_Static("PlayerName", playerName, "abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ", 16,
    //        () => {
    //            // Cancel
    //        },
    //        (string newName) => {
    //            playerName = newName;

    //            playerNameText.text = playerName;

    //            OnNameChanged?.Invoke(this, EventArgs.Empty);
    //        });
    //    });

    //    playerNameText.text = playerName;
    //}

    //private void Start()
    //{
    //    OnNameChanged += EditPlayerName_OnNameChanged;
    //}

    //private void EditPlayerName_OnNameChanged(object sender, EventArgs e)
    //{
    //    LobbyManager.Instance.UpdatePlayerName(GetPlayerName());
    //}

    //public string GetPlayerName()
    //{
    //    return playerName;
    //}
}
