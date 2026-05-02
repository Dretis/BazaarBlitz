using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatBackgroundManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> backgrounds;
    [SerializeField] private GameplayTest.GameBoard currentBoard;

    // Start is called before the first frame update
    void Start()
    {
        currentBoard = CombatManager.Instance.sceneManager.overworldScene.board;
        backgrounds[(int)currentBoard].SetActive(true);
    }
}
