using UnityEngine;

public enum ItemType { HEALTH, MANA, SOULS, STATUS, WEAPON, CONSUMABLE, USEABLE, OTHER }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    new public string name = "New Item";        // Name of the item
    public Sprite icon = null;                  // Icon for the item
    public ItemType item_type = ItemType.OTHER; // Item type
    public int value = 0;                       // Value of item. Used for different thing depening on the item
    public int base_value = 0;                  // Items base value for selling/buying
    public bool is_default = false;             // If it is a default item
    public int max_carried = 10;                // How many of this item can the player carry
    public int max_stashed = 999;               // How many can the player have in their stash
}
