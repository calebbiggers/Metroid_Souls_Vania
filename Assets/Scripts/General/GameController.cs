using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    #region Singleton
    public static GameController instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Game Controller found");
            return;
        }
        instance = this;
    }
    #endregion 

    public bool game_is_paused = false;
    //AudioSource.ignoreListenerPause=true; <- used to not pause audio source

    public void PauseGame()
    {
        // Set time scale to zero to pause
        Time.timeScale = 0;
        game_is_paused = true;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        // Reset time scale to resume
        Time.timeScale = 1;
        game_is_paused = false;
        AudioListener.pause = false;
    }
}
