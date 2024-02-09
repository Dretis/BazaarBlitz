using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatInputSystem : MonoBehaviour
{

    public List<ItemStats> itemKeyMap;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      bool currentPlayer = CombatManager.Instance.isAggressorTurn;
      if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
          CombatManager.Instance.chooseAction(3, currentPlayer);
          CombatManager.Instance.passTurn();
      }
      if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.DownArrow)) {
          CombatManager.Instance.chooseAction(2, currentPlayer);
          CombatManager.Instance.passTurn();
      }
      if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
          CombatManager.Instance.chooseAction(4, currentPlayer);
          CombatManager.Instance.passTurn();
      }
      if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
          CombatManager.Instance.chooseAction(1, currentPlayer);
          CombatManager.Instance.passTurn();
      }


      if (Input.GetKeyDown(KeyCode.Alpha1)) {
          CombatManager.Instance.addItem(itemKeyMap[1], currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha2)) {
          CombatManager.Instance.addItem(itemKeyMap[2], currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha3)) {
          CombatManager.Instance.addItem(itemKeyMap[3], currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha4)) {
          CombatManager.Instance.addItem(itemKeyMap[4], currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha5)) {
          CombatManager.Instance.addItem(itemKeyMap[5], currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha6)) {
          CombatManager.Instance.addItem(itemKeyMap[6], currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha7)) {
          CombatManager.Instance.addItem(itemKeyMap[7], currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha8)) {
          CombatManager.Instance.addItem(itemKeyMap[8], currentPlayer);
      }
      if (Input.GetKeyDown(KeyCode.Alpha9)) {
          CombatManager.Instance.addItem(itemKeyMap[9], currentPlayer);
      }
      // 1 potato
      // 2 mutated potato
      // 3 cloth
      // 4 ice dice
      // 5 milk
      // 6 chicken nuggest
      // 7 metal
      // 8 lightbulb
      // 9 the whole street lamp

    }
}
