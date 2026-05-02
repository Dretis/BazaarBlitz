using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGameManager : MonoBehaviour
{
    public MapNode spawnPoint;

    public List<EntityPiece> entities = new List<EntityPiece>();

    public int player1ID;
    public int player2ID;

    public List<CombatManager> combatManagers = new List<CombatManager>();

    // TODO: Change back to private.
    public GameplayTest overworldScene;
    private string overworldSceneName;

    [Header("Listen On Event Channels")]
    public VoidEventChannelSO m_ReturnToMainMenu;
    private void OnEnable()
    {
        m_ReturnToMainMenu.OnEventRaised += OnReturnToMainMenu;
    }

    private void OnDisable()
    {
        m_ReturnToMainMenu.OnEventRaised -= OnReturnToMainMenu;
    }


    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        entities.AddRange(FindObjectsOfType<EntityPiece>());
        overworldScene = GameObject.Find("Game Manager").GetComponent<GameplayTest>();
        overworldSceneName = overworldScene.gameObject.scene.name;
    }

    public void LoadCombatScene()
    {
        // Disable overworld.
        DisableScene(overworldSceneName);
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
            //combatManager.player1.combatSceneIndex = combatManager.combatSceneIndex;
            //combatManager.player2.combatSceneIndex = combatManager.combatSceneIndex;
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

    public void DisableScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        List<GameObject> sceneObjects = new List<GameObject>();
        scene.GetRootGameObjects(sceneObjects);

        foreach (GameObject obj in sceneObjects)
        {
            obj.SetActive(false);
        }
        Debug.Log(scene.name + " " + sceneName + " disabled!");
    }

    public void ChangeGamePhase(GameplayTest.GamePhase phase)
    {
        overworldScene.phase = phase;
    }

    private void OnReturnToMainMenu()
    {
        StartCoroutine(DelayReturnToMainMenu(1f));
    }

    private IEnumerator DelayReturnToMainMenu(float delay)
    {
        yield return new WaitForSeconds(delay);

        var operation = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        operation.allowSceneActivation = true;

        yield return operation.isDone;

        Destroy(this);
    }
}
