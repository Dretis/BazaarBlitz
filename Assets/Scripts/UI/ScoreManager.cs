using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private EntityPiece currentPlayer;
    [SerializeField] private List<EntityPiece> players;

    [Header("Colors")]
    [SerializeField] private Color healthyColor;
    [SerializeField] private Color injuredColor;
    [SerializeField] private Color dangerColor;

    [Header("UI Elements")]
    [SerializeField] private Canvas scoreCanvas;
    [SerializeField] private List<TextMeshProUGUI> playerNames;
    [SerializeField] private List<TextMeshProUGUI> playerPlacements;
    [SerializeField] private List<TextMeshProUGUI> playerScores;
    [SerializeField] private List<TextMeshProUGUI> playerHPs;
    [SerializeField] private List<Image> playerImages;

    [Header("Stamp Elements")]
    [SerializeField] private List<Image> greenStamps;
    [SerializeField] private List<Image> redStamps;
    [SerializeField] private List<Image> blueStamps;
    [SerializeField] private List<Image> orangeStamps;

    [Header("Listen On Event Channels")]
    public IntEventChannelSO m_ChangeInScore;
    public PlayerEventChannelSO m_NextPlayerTurn;

    public PlayerEventChannelSO m_PassedByStamp;
    public StampEventChannelSO m_UndoPassByStamp;

    public VoidEventChannelSO m_PassByPawnShop;
    public PlayerEventChannelSO m_UndoPassByPawnShop;

    private void OnEnable()
    {
        m_ChangeInScore.OnEventRaised += UpdateScoreForPlayer;
        m_NextPlayerTurn.OnEventRaised += ChangeCurrentPlayer;
        m_NextPlayerTurn.OnEventRaised += UpdateHeldStamps;
        m_PassedByStamp.OnEventRaised += UpdateHeldStamps;
        m_UndoPassByStamp.OnEventRaised += HideObtainedStamps;

        m_PassByPawnShop.OnEventRaised += ClearHeldStamps;
        m_UndoPassByPawnShop.OnEventRaised += UpdateHeldStamps;
    }

    private void OnDisable()
    {
        m_ChangeInScore.OnEventRaised -= UpdateScoreForPlayer;
        m_NextPlayerTurn.OnEventRaised -= ChangeCurrentPlayer;
        m_NextPlayerTurn.OnEventRaised -= UpdateHeldStamps;
        m_PassedByStamp.OnEventRaised -= UpdateHeldStamps;
        m_UndoPassByStamp.OnEventRaised -= HideObtainedStamps;

        m_PassByPawnShop.OnEventRaised -= ClearHeldStamps;
        m_UndoPassByPawnShop.OnEventRaised -= UpdateHeldStamps;
    }

    void Start()
    {
        for (int i = 0; i < players.Count; i++)
        {
            UpdateScoreForPlayer(i);
        }

        for (int i = 0; i < players.Count; i++)
        {
            greenStamps[i].enabled = false;
            redStamps[i].enabled = false;
            blueStamps[i].enabled = false;
            orangeStamps[i].enabled = false;
        }
    }

    private void UpdateScoreForPlayer(int id)
    {
        // Update specific player score on the scoreboard based on their ID.

        // playerNames[id].text = "" + players[id].nickname;
        playerNames[id].text = "" + players[id].entityName;

        if(players[id].heldPoints < 0)
        {
            // Red numbers when negative balance
            playerScores[id].text = $"<color=yellow>@</color> <color=red>{players[id].heldPoints}</color>";
        }
        else
        {
            playerScores[id].text = $"<color=yellow>@</color>{players[id].heldPoints}";
            
        }
        playerHPs[id].text = "<color=red>HP</color> " + players[id].health + "/" + 
            (players[id].maxHealth * players[id].currentStatsModifier.maxHealthMultModifier + 
            players[id].currentStatsModifier.maxHealthFlatModifier);
        playerImages[id].color = players[id].playerColor - new Color32(0, 0, 0, 125);

        // Placeholder for now
        playerPlacements[id].text = "";
    }

    private void ChangeCurrentPlayer(EntityPiece ps)
    {
        currentPlayer = ps;
    }

    private void UpdateHeldStamps(EntityPiece ps)
    {
        foreach(Stamp.StampType s in ps.stamps)
        {
            Debug.Log(s);
            switch (s)
            {
                case Stamp.StampType.Green:
                    greenStamps[ps.id].enabled = true;
                    break;
                case Stamp.StampType.Red:
                    redStamps[ps.id].enabled = true;
                    break;
                case Stamp.StampType.Blue:
                    blueStamps[ps.id].enabled = true;
                    break;
                case Stamp.StampType.Orange:
                    orangeStamps[ps.id].enabled = true;
                    break;
            }
        }
    }

    private void HideObtainedStamps(Stamp.StampType type)
    {
        switch (type)
        {
            case Stamp.StampType.Green:
                greenStamps[currentPlayer.id].enabled = false;
                break;
            case Stamp.StampType.Red:
                redStamps[currentPlayer.id].enabled = false;
                break;
            case Stamp.StampType.Blue:
                blueStamps[currentPlayer.id].enabled = false;
                break;
            case Stamp.StampType.Orange:
                orangeStamps[currentPlayer.id].enabled = false;
                break;
        }
    }

    private void ClearHeldStamps()
    {
        greenStamps[currentPlayer.id].enabled = false;
        redStamps[currentPlayer.id].enabled = false;
        blueStamps[currentPlayer.id].enabled = false;
        orangeStamps[currentPlayer.id].enabled = false;
    }
}
