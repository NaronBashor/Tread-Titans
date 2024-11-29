using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour
{
    [Header("UI Components")]
    public Image itemImage;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemCostText;
    public Button unlockButton;
    public Button infoButton; // The new info button

    private string itemName;
    private string itemType;
    private int unlockCost;
    private string itemStats;
    private ShopManager shopManager;
    private ItemDetailsPopup detailsPopup;

    public void Initialize(ShopManager manager, string name, string type, int cost, Sprite image, bool isLocked, string stats, ItemDetailsPopup popup)
    {
        shopManager = manager;
        itemName = name;
        itemType = type;
        unlockCost = cost;
        itemStats = stats;
        detailsPopup = popup;

        // Set up UI components
        itemNameText.text = name;
        itemCostText.text = isLocked ? $"Cost: {cost}" : "Unlocked";
        itemImage.sprite = image;

        // Set button interactivity based on lock status
        unlockButton.interactable = isLocked;
        unlockButton.onClick.AddListener(OnUnlockButtonClick);

        // Add a listener for the info button to show item details
        infoButton.onClick.AddListener(OnInfoButtonClick);
    }

    private void OnUnlockButtonClick()
    {
        bool isUnlocked = shopManager.UnlockItem(itemName, itemType, unlockCost);
        if (isUnlocked) {
            unlockButton.interactable = false;
            itemCostText.text = "Unlocked";
        }
    }

    private void OnInfoButtonClick()
    {
        detailsPopup.ShowPopup(itemName, itemStats, itemImage.sprite);
    }
}
