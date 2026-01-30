using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCameraManager : MonoBehaviour
{
    [SerializeField] Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        //var currentBoard = CombatManager.Instance.sceneManager.overworldScene.board;

        // change sky color
        //if (GameplayTest.instance.board == GameplayTest.GameBoard.CoconutCanal)
        //    cam.backgroundColor = new Color(142, 180, 217); //light orange

        //cam.backgroundColor = new Color(142, 180, 217); light blue
    }
}
