using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeGame : MonoBehaviour
{
    [SerializeField] MapNode spawnNode;

    [SerializeField] GameObject playerPrefab;
    // Start is called before the first frame update
    void Start()
    {
        var playerConfigs = PlayerConfigurationManager.instance.GetPlayerConfigs().ToArray();
        for(int i = 0; i < playerConfigs.Length; i++)
        {
            //var player = Instantiate(playerPrefab, spawn);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
