using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum STAT { VITALITY, STRENGTH, DEXTERITY, INTELLIGENCE, LUCK }

public class PlayerStats : MonoBehaviour
{
    #region Singleton
    public static PlayerStats instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Player Stats found");
            return;
        }
        instance = this;
    }
    #endregion  

    [Space]
    [Header("Health")]
    [SerializeField] public float base_max_health = 100f;
    [SerializeField] public float health_rate = 5f;
    [SerializeField] public float current_max_health;
    [SerializeField] public float current_health;

    [Space]
    [Header("Mana")]
    [SerializeField] public float base_max_mana = 20;
    [SerializeField] public float mana_rate = 2;
    [SerializeField] public float current_max_mana;
    [SerializeField] public float current_mana;

    [Space]
    [Header("Souls")]
    [SerializeField] public int current_souls;

    [Space]
    [Header("Atributes")]
    [SerializeField] public int level = 0;
    [SerializeField] public int vitality = 0;
    [SerializeField] public int strength = 0;
    [SerializeField] public int dexterity = 0;
    [SerializeField] public int intelligence = 0;
    [SerializeField] public int luck = 0;

    [Space]
    [Header("Damage")]
    [SerializeField] public float base_damage;
    [SerializeField] public float base_crit_chance = .05f;
    [SerializeField] public float current_crit_chance;

    [Space]
    [Header("Drop Rates")]
    [SerializeField] public float base_drop_chance = .10f;
    [SerializeField] public float current_drop_chance;

    public void Start()
    {
        // Set health values
        current_max_health = base_max_health;
        current_health = current_max_health;

        // Set mana values
        current_max_mana = base_max_mana;
        current_mana = current_max_mana;

        // Set damage stats
        current_crit_chance = base_crit_chance;

        // Set drop rate stats
        current_drop_chance = base_drop_chance;
    }

    public void ApplyHealth(int health_to_add)
    {
        current_health = Mathf.Clamp(current_health + health_to_add, 0, current_max_health);
    }

    public void ApplyMana(int mana_to_add)
    {
        current_mana = Mathf.Clamp(current_mana + mana_to_add, 0, current_max_mana);
    }

    public void AddSouls(int souls_to_add)
    {
        current_souls += souls_to_add;
    }

    public void UpdateStat(STAT to_update, int amount)
    {
        // Add amount to correct stat
        switch (to_update)
        {
            case STAT.VITALITY:
                vitality += amount;
                break;
            case STAT.STRENGTH:
                strength += amount;
                break;
            case STAT.DEXTERITY:
                dexterity += amount;
                break;
            case STAT.INTELLIGENCE:
                intelligence += amount;
                break;
            case STAT.LUCK:
                luck += amount;
                break;
            default:
                Functions.DebugLogError("Attempted to update unrecognized stat");
                break;
        }

        // Add the amount to the players level
        level += amount;

        UpdateValuesFromStats();
    }

    public void UpdateValuesFromStats()
    {
        // Update the amount of health based on vitality
        float percentage = current_health / current_max_health;
        current_max_health = vitality * health_rate + base_max_health;
        current_health = percentage * current_max_health;

        // Update the amount of mana based on intelligence
        percentage = current_mana / current_max_mana;
        current_max_mana = intelligence * mana_rate + base_max_mana;
        current_mana = percentage * current_max_mana;

        //
    }
}
