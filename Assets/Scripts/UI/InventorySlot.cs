using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{

    public Image item_icon;
    public TMP_Text item_count;
    private ItemEntry item_entry;

    public Sprite default_item;         // Icon for item that has no sprite set

    public void SetItem(ItemEntry new_item)
    {
        // Set the item to the item in slot
        item_entry = new_item;
        
        // Check if item in slot is an empty slot item
        if(item_entry.item.ID != -1)
        {
            Functions.DebugLog("Item is not empty item <" + item_entry.item.name + ">");

            // Set the sprite to the item icon and enable it
            if(item_entry.item.icon == null)
            {
                item_icon.sprite = default_item;
            }
            else
            {
                item_icon.sprite = item_entry.item.icon;
            }

            // Enable the icon
            item_icon.enabled = true;

            // Set the item account
            if(item_entry.count > 1)
            {
                // Set the count label to the count if over 1
                item_count.text = item_entry.count.ToString();
            }
            else
            {
                // If count is one dont have a number
                item_count.text = "";
            }
        }
        else
        {
            item_icon.enabled = false;
            item_count.text = "";
        }
    }

    public void ClearSlot()
    {
        // Set the item entry to null
        item_entry = null;

        // set the sprite to null
        item_icon.sprite = null;
        
        // disable the icon and set the count text to empty
        item_icon.enabled = false;
        item_count.text = "";
    }
}
