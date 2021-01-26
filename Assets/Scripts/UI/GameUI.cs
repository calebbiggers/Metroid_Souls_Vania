using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum CurrentTab { INVENTORY, EQUIPMENT, STATS, INDEX }

public class GameUI : MonoBehaviour
{
    private PlayerInventory inventory;  // Cached reference to player inventory
    private PlayerStats stats;          // Cached reference to player stats
    
    public GameObject game_UI;          // Reference to whole game UI
    public GameObject stats_UI;         // Reference to stats UI container
    public GameObject inventory_UI;     // Reference to inventory UI container
    public GameObject equipment_UI;     // Reference to equipment UI container
    public GameObject index_UI;         // Reference to index UI container

    public GameObject health_container;                             // Health bar container
    public GameObject health_bar;                                   // Health bar
    [SerializeField] private float health_container_base_size;      // Base size the health container is
    [SerializeField] private float health_container_current_size;   // Current size of health container
    [SerializeField] private float container_offset = 4f;           // How much bigger the health/mana bars are than their containers

    public GameObject mana_container;                           // Mana bar container
    public GameObject mana_bar;                                 // Mana bar 
    [SerializeField] private float mana_container_base_size;    // Base size the mana container is
    [SerializeField] private float mana_container_current_size; // Current size of mana container

    public Transform items_parent;      // Transform holding the inventory slots
    public Item empty_item;             // Empty item to display
    private InventorySlot[] slots;      // List of inventory slots

    public CurrentTab current_tab = CurrentTab.INVENTORY;   // The current tab the inventory menu is on

    // Start is called before the first frame update
    public void Start()
    {
        // Get the player inventory
        inventory = PlayerInventory.instance;

        // Set the callback for update UI
        inventory.onItemChangedCallback += UpdateGameUI;

        // Get list of item slots in inventory list
        slots = items_parent.GetComponentsInChildren<InventorySlot>();

        // Set inventory to be full of empty items
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetItem(new ItemEntry(empty_item, 0));
        }

        // Get the player stats
        stats = PlayerStats.instance;

        health_container_base_size = health_container.GetComponent<RectTransform>().sizeDelta.x;
        mana_container_base_size = mana_container.GetComponent<RectTransform>().sizeDelta.x;
    }

    public void Update()
    {
        UpdateHUD();
    }

    public void UpdateHUD()
    {
        // Health bar container size
        float ratio = stats.current_max_health / stats.base_max_health;
        health_container_current_size = health_container_base_size * ratio;
        health_container.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, health_container_current_size) ;

        // Health bar size
        ratio = stats.current_health / stats.current_max_health;
        float new_size = (health_container_current_size * ratio) - container_offset;
        health_bar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, new_size);

        // Mana bar container size
        ratio = stats.current_max_mana / stats.base_max_mana;
        mana_container_current_size = mana_container_base_size * ratio;
        mana_container.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mana_container_current_size);

        // Mana bar size
        ratio = stats.current_mana / stats.current_max_mana;
        new_size = (mana_container_current_size * ratio) - container_offset;
        mana_bar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, new_size);
    }

    // Method to update the UI
    public void UpdateGameUI()
    {
        SetActiveWindow();

        Functions.DebugLog("Updating Game UI");

        if(current_tab == CurrentTab.STATS)
        {
            Functions.DebugLog("Stats menu last up");
        }
        else if(current_tab == CurrentTab.EQUIPMENT)
        {
            Functions.DebugLog("Equipment menu last up");
        }
        else if(current_tab == CurrentTab.INVENTORY)
        {
            Functions.DebugLog("Inventory menu last up");

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
        else if(current_tab == CurrentTab.INDEX)
        {
            Functions.DebugLog("Index menu last up");
        }
        else
        {
            Functions.DebugLogError("Current tab is an unknown value");
        }
    }

    public void SetActiveWindow()
    {

        if (current_tab == CurrentTab.STATS)
        {
            stats_UI.SetActive(true);
            equipment_UI.SetActive(false);
            inventory_UI.SetActive(false);
            index_UI.SetActive(false);
        }
        else if (current_tab == CurrentTab.EQUIPMENT)
        {
            stats_UI.SetActive(false);
            equipment_UI.SetActive(true);
            inventory_UI.SetActive(false);
            index_UI.SetActive(false);
        }
        else if (current_tab == CurrentTab.INVENTORY)
        {
            stats_UI.SetActive(false);
            equipment_UI.SetActive(false);
            inventory_UI.SetActive(true);
            index_UI.SetActive(false);
        }
        else if (current_tab == CurrentTab.INDEX)
        {
            stats_UI.SetActive(false);
            equipment_UI.SetActive(false);
            inventory_UI.SetActive(false);
            index_UI.SetActive(true);
        }
    }

    public void ToggleGameUI(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Functions.DebugLog("Toggle game UI called");
            if (!game_UI.activeSelf)
            {
                PlayerController2D.instance.control_enabled = false;
                UpdateGameUI();
            }
            else
            {
                PlayerController2D.instance.control_enabled = true;
            }

            game_UI.SetActive(!game_UI.activeSelf);
        }
    }

    public void NextGameUI(InputAction.CallbackContext context)
    {
        if (game_UI.activeSelf)
        {
            if (context.started)
            {
                current_tab = NextTab(current_tab);

                UpdateGameUI();
            }
        }
    }

    public void LastGameUI(InputAction.CallbackContext context)
    {
        if (game_UI.activeSelf)
        {
            if (context.started)
            {
                current_tab = LastTab(current_tab);

                UpdateGameUI();
            }
        }
    }

    public CurrentTab NextTab(CurrentTab current)
    {
        switch (current)
        {
            case CurrentTab.STATS:
                return CurrentTab.EQUIPMENT;
            case CurrentTab.EQUIPMENT:
                return CurrentTab.INVENTORY;
            case CurrentTab.INVENTORY:
                return CurrentTab.INDEX;
            case CurrentTab.INDEX:
                return CurrentTab.INDEX;
            default:
                return CurrentTab.STATS;
        }
    }

    public CurrentTab LastTab(CurrentTab current)
    {
        switch (current)
        {
            case CurrentTab.STATS:
                return CurrentTab.STATS;
            case CurrentTab.INDEX:
                return CurrentTab.INVENTORY;
            case CurrentTab.INVENTORY:
                return CurrentTab.EQUIPMENT;
            case CurrentTab.EQUIPMENT:
                return CurrentTab.STATS;
            default:
                return CurrentTab.STATS;
        }
    }
}
