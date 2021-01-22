using UnityEngine;

public class ItemPickups_SO{
    public ItemType item_type;
}


public class ItemPickup : Interactable
{
    [Space]
    [Header("Item Pickup")]
    [SerializeField] private PlayerInventory player_inventory;

    public Item item;

    public override void Interact()
    {
        base.Interact();

        PickUp();
    }
    public void PickUp()
    {
        Functions.DebugLog("Picking up <" + item.name + ">");

        if (PlayerInventory.instance.PickUpItem(item))
        {
            // TODO: add animation of item being picked up
            Destroy(gameObject);
        }
    }

    public void UseItem()
    {
        switch (item.item_type) {
            case ItemType.HEALTH:
                break;
            case ItemType.MANA:
                break;
            case ItemType.SOULS:
                break;
            case ItemType.STATUS:
                break;
            case ItemType.OTHER:
                break;
        }
    }
}
