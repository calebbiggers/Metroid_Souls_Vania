using UnityEngine;

// Enum to represent the type of item
public enum ItemType { HEALTH, MANA, SOULS, STATUS, WEAPON, CONSUMABLE, SPELL, QUEST, OTHER }

[CreateAssetMenu(fileName = "New Basic Item", menuName = "Inventory/Basic Item")]
public class Item : ScriptableObject
{
    public int ID = -1;                                         // Numerical ID of item
    new public string name = "New Item";                        // Name of the item
    [TextArea(15, 20)] public string item_description = "";     // Item description
    public Sprite icon = null;                                  // Icon for the item
    public ItemType item_type = ItemType.OTHER;                 // Item type
    public int base_value = -1;                                 // Items base value for selling/buying
    public bool is_default = false;                             // If it is a default item
    public int max_carried = 20;                                // How many of this item can the player carry
    public int max_stashed = 999;                               // How many can the player have in their stash

    // Function to use the item. This will do different things depending on the type of item
    public virtual void Use()
    {
        Functions.DebugLog("Using <" + this.name + ">");
    }

    public virtual bool Equip()
    {
        Functions.DebugLog("Equiped <" + this.name + ">");
        return true;
    }

    public virtual bool Dequip()
    {
        Functions.DebugLog("Dequiped <" + this.name + ">");
        return true;
    }
}
