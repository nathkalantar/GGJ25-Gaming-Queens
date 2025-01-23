using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStates{MainMenu, Pause, Gameplay}
public class GameManager : MonoBehaviour
{
    #region Variables Declaration
    public static GameManager Instance { get; private set; }
    public GameStates _gameState;
    #endregion

    #region Running Instances
    private void Awake() 
    { 
        if (Instance != null) 
        { 
            Debug.Log("Found more than one GameManager. Destroying the newest one.");
            Destroy(this);
            return; 
        } 
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        
    }
    #endregion
}
