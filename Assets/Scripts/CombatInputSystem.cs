using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatInputSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      bool currentPlayer = CombatManager.Instance.isAggressorTurn;
      if (Input.GetKeyDown(KeyCode.A)) {
          CombatManager.Instance.chooseAction(1, currentPlayer);
          CombatManager.Instance.passTurn();
      }
      if (Input.GetKeyDown(KeyCode.W)) {
          CombatManager.Instance.chooseAction(2, currentPlayer);
          CombatManager.Instance.passTurn();
      }
      if (Input.GetKeyDown(KeyCode.D)) {
          CombatManager.Instance.chooseAction(3, currentPlayer);
          CombatManager.Instance.passTurn();
      }


      if (Input.GetKeyDown(KeyCode.Alpha1)) {
          CombatManager.Instance.addItem(1, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha2)) {
          CombatManager.Instance.addItem(2, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha3)) {
          CombatManager.Instance.addItem(3, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha4)) {
          CombatManager.Instance.addItem(4, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha5)) {
          CombatManager.Instance.addItem(5, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha6)) {
          CombatManager.Instance.addItem(6, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha7)) {
          CombatManager.Instance.addItem(7, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha8)) {
          CombatManager.Instance.addItem(8, currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha9)) {
          CombatManager.Instance.addItem(9, currentPlayer);
      }

    }
}
