using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Start is called before the first frame update
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

    public int GetHealth()
    {
        return 0;
    }

    public int GetMana()
    {
        return 0;
    }

    public int GetSouls()
    {
        return 0;
    }

    public void ApplyHealth()
    {

    }

    public void ApplyMana()
    {

    }

    public void AddSouls()
    {

    }
}
