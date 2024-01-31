using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGameManager : MonoBehaviour
{
    public List<EntityPiece> players = new List<EntityPiece>();

    public int player1ID;
    public int player2ID;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        players.AddRange(FindObjectsOfType<EntityPiece>());
    }

    public void LoadCombatScene()
    {
        SceneManager.LoadScene("CombatTest", LoadSceneMode.Additive);
    }

    public void UnloadCombatScene()
    {
        SceneManager.UnloadSceneAsync("CombatTest");
    }
}
