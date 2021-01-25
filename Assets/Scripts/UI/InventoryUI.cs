using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    private PlayerInventory inventory;  // Cached reference to player inventory
    public GameObject inventory_UI;     // Reference to Inventory UI canvas

    public Transform items_parent;      // Transform holding the inventory slots
    public Item empty_item;             // Empty item to display
    InventorySlot[] slots;              // List of inventory slots

    // Start is called before the first frame update
    void Start()
    {
        // Get the player inventory
        inventory = PlayerInventory.instance;

        // Set the callback for update UI
        inventory.onItemChangedCallback += UpdateUI;

        // Get list of item slots in inventory list
        slots = items_parent.GetComponentsInChildren<InventorySlot>();

        // Set inventory to be full of empty items
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetItem(new ItemEntry(empty_item, 0));
        }
    }

    // Method to update the UI
    void UpdateUI()
    {
        Functions.DebugLog("Updating Inventory UI");

        // For each item in the players inventory list
        for (int i = 0; i < inventory.player_items.Count; i++)
        {
            if (i >= slots.Length)
            {
                // If the item index is higher than the amount able to be displayed. Dont display it and warn
                Functions.DebugLogWarning("Player inventory is greater than the array for item inventory display");
            }
            else
            {
                // Set the item to the inventory UI slot
                slots[i].SetItem(inventory.player_items[i]);
            }
        }
    }
}
