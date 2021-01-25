using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipSlot { NOT_EQUIPED, WEAPON_1, WEAPON_2, RING_1, RING_2, RING_3, RING_4, HOTBAR_1, HOTBAR_2, HOTBAR_3, HOTBAR_4, HOTBAR_5, HOTBAR_6 }

[System.Serializable]
public class ItemEntry
{
    public Item item;               // Item entry is storing
    public int count;               // Amount of item this entry contains
    public EquipSlot equip_slot;    // Slot where item is currently equipped if at all

    public ItemEntry(Item item, int count)
    {
        this.item = item;
        this.count = count;
        this.equip_slot = EquipSlot.NOT_EQUIPED;
    }

    public void Add(int amount)
    {
        this.count += amount;
    }

    public override bool Equals(object other)
    {
        if(other == null || GetType() != other.GetType())
        {
            return false;
        }

        ItemEntry other_item_entry = (ItemEntry)other;

        return (other_item_entry.item.name == item.name);
    }

    public override int GetHashCode()
    {
        return item.name.GetHashCode();
    }
}



public class PlayerInventory : MonoBehaviour
{
    #region Singleton
    public static PlayerInventory instance;

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("More than one instance of Player Inventory found");
            return;
        }
        instance = this;
    }
    #endregion  

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<ItemEntry> player_items = new List<ItemEntry>();  // Player's inventory
    public List<ItemEntry> stash_items = new List<ItemEntry>();   // Stash inventory

    public int PickUpItem(ItemEntry item_entry)
    {
        if(item_entry == null)
        {
            Functions.DebugLogError("Attemped to pick up null Item Entry");
            return 0;
        }

        Item item = item_entry.item;
        int amount_taken = 0;
        int index = 0;

        if (!item.is_default)
        {
            // Check if player inv has item already
            if (!player_items.Contains(item_entry))
            {
                // If this is a new item, add it to the inventory
                player_items.Add(new ItemEntry(item_entry.item, item_entry.count));

                // Get the index of the item in the inventory
                index = player_items.IndexOf(item_entry);
            }
            else
            {
                // Get the index of the item in the inventory
                index = player_items.IndexOf(item_entry);

                // If players inventory already has the item. Add the amount given
                player_items[index].count += item_entry.count;
            }

            // Add the total amount picked up to amount taken. Will be adjusted later
            amount_taken = item_entry.count;

            // Check if players has gone over the carry amount for the item
            if (player_items[index].count > player_items[index].item.max_carried)
            {
                // Subtract the difference from the amount taken
                amount_taken -= player_items[index].count - player_items[index].item.max_carried;

                // Set the current amount in inventory to the max
                player_items[index].count = player_items[index].item.max_carried;

                // Stash the difference
                return amount_taken + StashItem(item_entry);
            }

            // Call the update inventory callback
            if (onItemChangedCallback != null)
            {
                onItemChangedCallback.Invoke();
            }

            // return amount taken
            return amount_taken;
        }
        
        // If the item is a default item, dont add it
        return amount_taken;
        
    }

    public int DiscardItem(ItemEntry item_entry, int amount)
    {
        Item item = item_entry.item;
        int amount_taken = 0;
        int index = 0;

        if (!item.is_default)
        {
            // Make sure player inventory contains the item
            if (player_items.Contains(item_entry))
            {
                // Get the index of the item
                index = player_items.IndexOf(item_entry);


            }
            player_items.Remove(item_entry);

            if (onItemChangedCallback != null)
            {
                onItemChangedCallback.Invoke();
            }

            return amount_taken;
        }
        return amount_taken;
    }

    public int StashItem(ItemEntry item_entry)
    {
        Item item = item_entry.item;
        int amount_taken = 0;
        int index = 0;

        stash_items.Add(item_entry);

        if (onItemChangedCallback != null)
        {
            onItemChangedCallback.Invoke();
        }

        return amount_taken;
    }

    public int DiscardFromStash(ItemEntry item_entry, int amount)
    {
        Item item = item_entry.item;
        int amount_taken = 0;
        int index = 0;

        stash_items.Remove(item_entry);

        if (onItemChangedCallback != null)
        {
            onItemChangedCallback.Invoke();
        }

        return amount_taken;
    } 
}
