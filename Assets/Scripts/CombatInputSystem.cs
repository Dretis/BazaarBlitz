using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      bool currentPlayer = CombatInstance.isAggressorTurn;
      if (Input.GetKeyDown(KeyCode.A)) {
          CombatInstance.chooseAction(1, currentPlayer);
          CombatInstance.passTurn();
      }
      if (Input.GetKeyDown(KeyCode.W)) {
          CombatInstance.chooseAction(2, currentPlayer);
          CombatInstance.passTurn();
      }
      if (Input.GetKeyDown(KeyCode.D)) {
          CombatInstance.chooseAction(3, currentPlayer);
          CombatInstance.passTurn();
      }


      if (Input.GetKeyDown(KeyCode.Alpha1)) {
          CombatManager.activateItem(1, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha2)) {
          CombatManager.activateItem(2, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha3)) {
          CombatManager.activateItem(3, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha4)) {
          CombatManager.activateItem(4, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha5)) {
          CombatManager.activateItem(5, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha6)) {
          CombatManager.activateItem(6, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha7)) {
          CombatManager.activateItem(7, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha8)) {
          CombatManager.activateItem(8, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha9)) {
          CombatManager.activateItem(9, currentPlayer);
      }

    }
}
