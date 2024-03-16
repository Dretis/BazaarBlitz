using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ScoreManager : MonoBehaviour
{
    [SerializeField] private PlayerData currentPlayer;
    [SerializeField] private List<EntityPiece> players;

    [Header("UI Elements")]
    [SerializeField] private Canvas scoreCanvas;
    [SerializeField] private List<TextMeshProUGUI> playerNames;
    [SerializeField] private List<TextMeshProUGUI> playerPlacements;
    [SerializeField] private List<TextMeshProUGUI> playerScores;
    [SerializeField] private List<TextMeshProUGUI> playerHPs;
    [SerializeField] private List<Image> playerImages;

    [Header("Listen On Event Channels")]
    public IntEventChannelSO m_ChangeInScore;

    private void OnEnable()
    {
        m_ChangeInScore.OnEventRaised += UpdateScoreForPlayer;
    }

    private void OnDisable()
    {
        m_ChangeInScore.OnEventRaised -= UpdateScoreForPlayer;
    }

    void Start()
    {
        for (int i = 0; i < players.Count; i++)
        {
            UpdateScoreForPlayer(i);
        }
    }

    private void UpdateScoreForPlayer(int id)
    {
        // Update specific player score on the scoreboard based on their ID.

        // playerNames[id].text = "" + players[id].nickname;
        playerNames[id].text = "" + players[id].nickname;
        playerScores[id].text = "<color=yellow>@</color> " + players[id].heldPoints;
        playerHPs[id].text = "HP " + players[id].combatStats.health;
        playerImages[id].color = players[id].playerColor - new Color32(0, 0, 0, 125);

        // Placeholder for now
        playerPlacements[id].text = "";
    }

}
