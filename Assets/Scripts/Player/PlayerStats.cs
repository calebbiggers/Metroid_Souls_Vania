using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    #region Singleton
    public static PlayerStats instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Player Stat found");
            return;
        }
        instance = this;
    }
    #endregion  

    [Space]
    [Header("Health")]
    [SerializeField] public static int base_health_max;
    [SerializeField] public static int max_health;
    [SerializeField] public static int current_health;

    [Space]
    [Header("Mana")]
    [SerializeField] public static int base_mana;
    [SerializeField] public static int max_mana;
    [SerializeField] public static int current_mana;

    [Space]
    [Header("Souls")]
    [SerializeField] public static int current_souls;

    [Space]
    [Header("Atributes")]
    [SerializeField] public static int level = 0;
    [SerializeField] public static int vitality = 0;
    [SerializeField] public static int strength = 0;
    [SerializeField] public static int dexterity = 0;
    [SerializeField] public static int intelligence = 0;
    [SerializeField] public static int luck = 0;

    [Space]
    [Header("Damage")]
    [SerializeField] public static int base_damage;
    [SerializeField] public static float crit_chance;


    public void ApplyHealth(int health_to_add)
    {
        current_health += health_to_add;
        if(current_health > max_health)
        {
            current_health = max_health;
        }
    }

    public void ApplyMana(int mana_to_add)
    {
        current_mana += mana_to_add;
        if (current_mana > max_mana)
        {
            current_mana = max_mana;
        }
    }

    public void AddSouls(int souls_to_add)
    {
        current_souls += souls_to_add;
    }
}
