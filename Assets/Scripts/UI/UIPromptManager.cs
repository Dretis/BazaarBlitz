using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPromptManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI inputPrompt;
    [SerializeField] private TextMeshProUGUI rolledNumber;
    [SerializeField] private TextMeshProUGUI movementRoll;

    [Header("Listen on Event Channels")]
    public IntEventChannelSO m_RollForMovement;
    public PlayerEventChannelSO m_NextPlayerTurn;
    public PlayerEventChannelSO m_EncounterDecisions;

    private void Start()
    {
        rolledNumber.text = "";
        movementRoll.text = "-";
    }
    private void OnEnable()
    {
        m_RollForMovement.OnEventRaised += RolledDice;
        m_NextPlayerTurn.OnEventRaised += DisplayRollPrompt;
        m_EncounterDecisions.OnEventRaised += DisplayEncounterChoices;
    }

    private void OnDisable()
    {
        m_RollForMovement.OnEventRaised -= RolledDice;
        m_NextPlayerTurn.OnEventRaised -= DisplayRollPrompt;
        m_EncounterDecisions.OnEventRaised -= DisplayEncounterChoices;
    }

    private void RolledDice(int diceRoll)
    {
        inputPrompt.text = "";

        StartCoroutine(DisplayDiceRoll(diceRoll));

        //Debug.Log("Rolled");
    }

    private IEnumerator DisplayDiceRoll(int diceRoll)
    {
        rolledNumber.enabled = true;
        yield return new WaitForSeconds(0.25f);

        rolledNumber.text = "" + diceRoll;

        yield return new WaitForSeconds(0.75f);

        /*
        Color lowerAlpha;
        lowerAlpha = Color.Lerp(Color.white, new Color(rolledNumber.color.r, rolledNumber.color.g, rolledNumber.color.b, 0), Mathf.PingPong(Time.time, 2));
        rolledNumber.color = lowerAlpha;
        */
        rolledNumber.text = "";
        rolledNumber.enabled = false;

        yield return null;
        movementRoll.text = "" + diceRoll;
    }

    private void DisplayRollPrompt(PlayerStats ps)
    {
        // This will get swapped out with a menu selection
        inputPrompt.text = "<color=white>[SPACE]</color> to roll!";
        movementRoll.text = "-";
    }

    private void DisplayEncounterChoices(PlayerStats ps)
    {
        inputPrompt.text = "<color=white>[ENTER]</color> to encounter an enemy.";
        inputPrompt.text += "\n<color=white>[R-SHIFT]</color> to build a store.";
    }

    private void ClearInputText()
    {
        inputPrompt.text = "";
    }
}
