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

    public List<GameObject> overworldSceneGameObjects;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        players.AddRange(FindObjectsOfType<EntityPiece>());
    }

    public void LoadCombatScene()
    {
        overworldSceneGameObjects = new List<GameObject>(FindObjectsOfType<GameObject>());
        SceneManager.LoadScene("CombatTest", LoadSceneMode.Additive);
    }

    public void UnloadCombatScene()
    {
        SceneManager.UnloadSceneAsync("CombatTest");
    }
}
