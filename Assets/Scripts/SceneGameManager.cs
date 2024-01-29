using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGameManager : MonoBehaviour
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

    public void LoadCombatScene()
    {
        SceneManager.LoadScene("CombatTest", LoadSceneMode.Additive);
    }
}
