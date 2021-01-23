using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public List<Item> player_items = new List<Item>();  // Player's inventory
    public List<Item> stash_items = new List<Item>();   // Player's stash inventory

    public bool PickUpItem(Item item)
    {
        if (!item.is_default)
        {
            player_items.Add(item);

            if(onItemChangedCallback != null)
            {
                onItemChangedCallback.Invoke();
            }

            return true;
        }
        return false;
        
    }

    public bool DiscardItem(Item item)
    {
        if (!item.is_default)
        {
            player_items.Remove(item);

            if (onItemChangedCallback != null)
            {
                onItemChangedCallback.Invoke();
            }

            return true;
        }
        return false;
    }

    public bool StashItem(Item item)
    {
        stash_items.Add(item);

        if (onItemChangedCallback != null)
        {
            onItemChangedCallback.Invoke();
        }

        return true;
    }

    public bool DiscardFromStash(Item item)
    {
        stash_items.Remove(item);

        if (onItemChangedCallback != null)
        {
            onItemChangedCallback.Invoke();
        }

        return true;
    } 
}
