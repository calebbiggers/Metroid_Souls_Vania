using UnityEngine;

public enum ItemType { HEALTH, MANA, SOULS, STATUS, WEAPON, CONSUMABLE, USEABLE, OTHER }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public int item_ID = -1;                    // Numerical ID of item
    new public string name = "New Item";        // Name of the item
    public Sprite icon = null;                  // Icon for the item
    public int amount_carried = 0;              // Amount of item player has in inventory
    public ItemType item_type = ItemType.OTHER; // Item type
    public int effect_amount = -1;              // Effect if item. Used for different thing depending on the item
    public int base_value = -1;                 // Items base value for selling/buying
    public bool is_default = false;             // If it is a default item
    public int max_carried = 20;                // How many of this item can the player carry
    public int max_stashed = 999;               // How many can the player have in their stash

    // Item description. (Possibly do in json?)
    public string item_description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In eleifend iaculis urna eu pharetra. Sed eget augue ornare, sagittis mi et, sollicitudin dui. \nUt massa mauris, porttitor sed ipsum in.";
}
