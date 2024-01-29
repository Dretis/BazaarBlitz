using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public PlayerStats player1;
    public PlayerStats player2;
    public PlayerStats player3;
    public PlayerStats player4;

    public int player1ID;
    public int player2ID;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadCombatScene(PlayerStats player1, PlayerStats player2)
    { 
        SceneManager.LoadScene(combatSceneName, LoadSceneMode.Additive);
    }
}
