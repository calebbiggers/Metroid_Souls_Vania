using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class Weapon : Item
{
    public float damage_multiplier = 0;
    public float attack_speed = 1f;
    public float crit_chance = 0f;
}
