using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGameManager : MonoBehaviour
{
    public List<EntityPiece> players = new List<EntityPiece>();
    public List<MonsterStats> enemies = new List<MonsterStats>();

    public int player1ID;
    public int player2ID;

    public List<CombatManager> combatManagers = new List<CombatManager>();

    private GameplayTest overworldScene;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        players.AddRange(FindObjectsOfType<EntityPiece>());
        enemies.AddRange(FindObjectsOfType<MonsterStats>());
        overworldScene = GameObject.Find("Input Manager").GetComponent<GameplayTest>();
    }

    public void LoadCombatScene()
    {
        // Disable overworld.
        DisableScene(0);
        SceneManager.LoadScene("CombatTest", LoadSceneMode.Additive);
        
        //SetActiveSceneAfterWait();
    }

    public void UnloadCombatScene(Scene scene, int sceneIndex)
    {
        Debug.Log("unload scene" + sceneIndex);
        // Remove combat manager of finished combat scene.
        combatManagers.Remove(combatManagers[sceneIndex-1]);
        SceneManager.UnloadSceneAsync(scene);

        // Update all remaining combat managers scene indices.
        foreach(var combatManager in combatManagers)
        {
            combatManager.combatSceneIndex = combatManagers.IndexOf(combatManager)+1;
            combatManager.player1.combatSceneIndex = combatManager.combatSceneIndex;
            combatManager.player2.combatSceneIndex = combatManager.combatSceneIndex;
        }

        //SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));
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
        Debug.Log(scene.name + " " + sceneIndex + " enabled!");
       // SceneManager.SetActiveScene(SceneManager.GetSceneAt(sceneIndex));
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
        Debug.Log(scene.name + " " + sceneIndex + " disabled!");
    }

    public void ChangeGamePhase(GameplayTest.GamePhase phase)
    {
        overworldScene.phase = phase;
    }
    IEnumerator SetActiveSceneAfterWait()
    {
        yield return new WaitForSeconds(1.0f);

        //SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount-1));
        Debug.Log(SceneManager.GetActiveScene().name);
    }

    IEnumerator WaitOneSecondToEnable(Scene scene, int sceneIndex)
    {
        yield return new WaitForSeconds(1.0f);

        print("Hi");
    }

    IEnumerator WaitOneSecondToDisable(Scene scene, int sceneIndex)
    {
        yield return new WaitForSeconds(1.0f);

        print("Hi");
    }
}
