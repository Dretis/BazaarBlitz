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

    private GameplayTest overworldScene;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        players.AddRange(FindObjectsOfType<EntityPiece>());
        overworldScene = GameObject.Find("Input Manager").GetComponent<GameplayTest>();
    }

    public void LoadCombatScene()
    {
        SceneManager.LoadScene("CombatTest", LoadSceneMode.Additive);
        SetActiveSceneAfterWait();
    }

    public void UnloadCombatScene(int sceneIndex)
    {
        SceneManager.UnloadSceneAsync(sceneIndex);
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));
        overworldScene.encounterStarted = false;
    }

    public void EnableScene(int sceneIndex)
    {
        Scene scene = SceneManager.GetSceneAt(sceneIndex);
        List<GameObject> sceneObjects = new List<GameObject>();
        scene.GetRootGameObjects(sceneObjects);

        foreach (GameObject obj in sceneObjects) 
        {
            obj.SetActive(true);
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneAt(sceneIndex));
    }

    public void DisableScene(int sceneIndex)
    {
        Scene scene = SceneManager.GetSceneAt(sceneIndex);
        List<GameObject> sceneObjects = new List<GameObject>();
        scene.GetRootGameObjects(sceneObjects);

        foreach (GameObject obj in sceneObjects)
        {
            obj.SetActive(false);
        }      
    }

    public void ChangeGamePhase(GameplayTest.GamePhase phase)
    {
        overworldScene.phase = phase;
    }
    IEnumerator SetActiveSceneAfterWait()
    {
        yield return new WaitForSeconds(1.0f);

        SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount-1));
        Debug.Log(SceneManager.GetActiveScene().name);
    }
}
