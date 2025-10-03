using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using static UnityEngine.Rendering.DebugUI;

public class UIResultsManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup resultGroup;
    [SerializeField] private List<Image> playerImages;
    [SerializeField] private List<TextMeshProUGUI> playerNames;
    [SerializeField] private List<TextMeshProUGUI> playerScores;

    private EntityPiece theWinner;

    [Header("Listen On Event Channels")]
    public PlayerListEventChannelSO m_ResultFinalScores;
    public PlayerEventChannelSO m_PlayerWon;
    private void OnEnable()
    {
        m_ResultFinalScores.OnEventRaised += OnResultFinalScores;
        m_PlayerWon.OnEventRaised += OnPlayerWon;
    }

    private void OnDisable()
    {
        m_ResultFinalScores.OnEventRaised -= OnResultFinalScores;
        m_PlayerWon.OnEventRaised -= OnPlayerWon;
    }

    private void Start()
    {
        resultGroup.alpha = 0f;
    }

    private void OnPlayerWon(EntityPiece winner)
    {
        theWinner = winner;
    }

    private void OnResultFinalScores(List<EntityPiece> players)
    {
        StartCoroutine(DelayShowResults(5f));

        //int i = players.Count - 1; i >= 0; i--
        if(players.Contains(theWinner))
        {
            Debug.Log($"theWinner: {theWinner}");
            playerNames[playerNames.Count - 1].text = "" + theWinner.entityName;
            playerImages[playerImages.Count - 1].color = theWinner.playerColor - new Color32(0, 0, 0, 0); // minus transparency
            playerScores[playerScores.Count - 1].text = "" + theWinner.heldPoints;

            players.Remove(theWinner);
        }

        for (int i = 0; i < players.Count; i++)
        {
            // this goes from last place up to the top
            Debug.Log($"Player at Placement-{i}: {players[i].entityName}");
            playerNames[i].text = "" + players[i].entityName;
            playerImages[i].color = players[i].playerColor - new Color32(0, 0, 0, 0); // minus transparency
            playerScores[i].text = "" + players[i].heldPoints;
        }
    }

    private IEnumerator DelayShowResults(float delay)
    {
        yield return new WaitForSeconds(delay);
        LMotion.Create(resultGroup.alpha, 1, 1f)
            .Bind(x => resultGroup.alpha = x);
        yield return null;
    }
}
