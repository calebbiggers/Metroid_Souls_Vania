using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : Interactable
{
    [Space]
    [Header("Item Pickup")]
    [SerializeField] private List<ItemEntry> items = new List<ItemEntry>();

    public void Awake()
    {
        // Set item pickup to ignore player and enemy collisions
        Physics2D.IgnoreLayerCollision(7, 9);
        Physics2D.IgnoreLayerCollision(7, 11);
    }

    public override void Interact()
    {
        base.Interact();
        PickUp();
    }

    public void PickUp()
    {
        int amount_picked_up = 0;

        for(int i = 0; i < items.Count; i++)
        {
            // Pick up the items and get the amount picked up
            amount_picked_up = PlayerInventory.instance.PickUpItem(items[i]);

            Functions.DebugLog("Picked up " + amount_picked_up.ToString() + "/" + items[i].count.ToString() + " of item " + items[i].item.name);

            // Subtract the amount picked up from the pickup inventory
            items[i].count -= amount_picked_up;
        }

        // Check if container is empty
        CheckEmpty();
    }

    public void CheckEmpty()
    {
        bool is_empty = true;

        for (int i = 0; i < items.Count; i++)
        {
            // Check if item in inventory is at zero
            if (items[i].count == 0)
            {
                Functions.DebugLog("Removing empty item entry for " + items[i].item.name);
                items.RemoveAt(i);
            }
            else
            {
                is_empty = false;
            }
        }

        if (is_empty)
        {
            // If the inventory is empty. destroy this object
            Destroy(gameObject);
        }
    }
}
