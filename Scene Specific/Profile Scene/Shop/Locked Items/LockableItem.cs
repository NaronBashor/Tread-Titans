using UnityEngine;

[CreateAssetMenu(fileName = "NewLockableItem", menuName = "Shop/LockableItem")]
public class LockableItem : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public int unlockCost; // Cost in currency or points
    public bool isLocked = true; // Initially locked by default
}
