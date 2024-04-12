using UnityEngine;

public class CombatInputSystem : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        bool currentPlayer = CombatManager.Instance.isInitiatorTurn;


        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            CombatManager.Instance.chooseAction(3, currentPlayer);
            CombatManager.Instance.passTurn();
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) 
        {
            CombatManager.Instance.chooseAction(2, currentPlayer);
            CombatManager.Instance.passTurn();
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            CombatManager.Instance.chooseAction(4, currentPlayer);
            CombatManager.Instance.passTurn();
        }
    }
}
