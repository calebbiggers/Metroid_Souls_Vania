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
    [Header("Player Config")]
    [SerializeField] public static int base_health_max;
    [SerializeField] public static int base_mana;
    [SerializeField] public static int base_damage;

    [Space]
    [Header("Player Data")]
    [SerializeField] public static int health;
    [SerializeField] public static int bmana;
    [SerializeField] public static int souls;

    public void ApplyHealth(int health_to_add)
    {

    }

    public void ApplyMana()
    {

    }

    public void AddSouls()
    {

    }
}
