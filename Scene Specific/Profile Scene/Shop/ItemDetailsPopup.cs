using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailsPopup : MonoBehaviour
{
    public GameObject popupPanel; // Reference to the pop-up panel
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemStatsText;
    public Image itemImage;
    public Button closeButton;

    private void Start()
    {
        closeButton.onClick.AddListener(HidePopup); // Attach listener to close button
        HidePopup(); // Ensure the pop-up is hidden on start
    }

    // Show the pop-up with item details
    public void ShowPopup(string name, string stats, Sprite image)
    {
        itemNameText.text = name;
        itemStatsText.text = stats;
        itemImage.sprite = image;
        popupPanel.SetActive(true);
    }

    // Hide the pop-up
    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }
}
