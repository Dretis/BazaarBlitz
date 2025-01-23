using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Febucci.UI.Core;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private EntityPiece currentPlayer;
    [SerializeField] private List<EntityPiece> players;

    [Header("Colors")]
    [SerializeField] private Color healthyColor;
    [SerializeField] private Color injuredColor;
    [SerializeField] private Color dangerColor;

    [SerializeField] private Color greenStampColor;
    [SerializeField] private Color redStampColor;
    [SerializeField] private Color blueStampColor;
    [SerializeField] private Color orangeStampColor;

    [Header("UI Elements")]
    [SerializeField] private Canvas scoreCanvas;
    [SerializeField] private List<TextMeshProUGUI> playerNames;
    [SerializeField] private List<TextMeshProUGUI> playerLevels;
    [SerializeField] private List<TextMeshProUGUI> playerExps;

    //[SerializeField] private List<TextMeshProUGUI> playerScores;
    [SerializeField] private List<TypewriterCore> playerScores;

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
        m_UndoPassByPawnShop.OnEventRaised += OnUndoPassByPawnShop;
    }

    private void OnDisable()
    {
        m_ChangeInScore.OnEventRaised -= UpdateScoreForPlayer;
        m_NextPlayerTurn.OnEventRaised -= ChangeCurrentPlayer;
        m_NextPlayerTurn.OnEventRaised -= UpdateHeldStamps;
        m_PassedByStamp.OnEventRaised -= UpdateHeldStamps;
        m_UndoPassByStamp.OnEventRaised -= HideObtainedStamps;

        m_PassByPawnShop.OnEventRaised -= ClearHeldStamps;
        m_UndoPassByPawnShop.OnEventRaised -= OnUndoPassByPawnShop;
    }

    void Start()
    {
        // Initial Setup
        for (int i = 0; i < players.Count; i++)
        {
            // Set name
            playerNames[i].text = "" + players[i].entityName; 

            // Set Color of Baggie in score
            playerImages[i].color = players[i].playerColor - new Color32(0, 0, 0, 0); // minus transparency

            UpdateScoreForPlayer(i);
        }

        for (int i = 0; i < players.Count; i++)
        {

            greenStamps[i].color -= new Color(0, 0, 0, 0.75f);
            redStamps[i].color -= new Color(0, 0, 0, 0.75f);
            blueStamps[i].color -= new Color(0, 0, 0, 0.75f);
            orangeStamps[i].color -= new Color(0, 0, 0, 0.75f);
            //greenStamps[i].enabled = false;
            //redStamps[i].enabled = false;
            //blueStamps[i].enabled = false;
            //orangeStamps[i].enabled = false;
        }
    }

    private void UpdateScoreForPlayer(int id)
    {
        // Debug.Log("updating player {id} score");
        // Update specific player score on the scoreboard based on their ID.
        playerLevels[id].text = "*\n" + players[id].RenownLevel;
        playerExps[id].text = "[" + (int)players[id].ReputationPoints + "/" + (int)players[id].levelThreshold + "]";

        UpdateMoneyForPlayer(id);

        UpdateHealthForPlayer(id);
    }

    private void UpdateMoneyForPlayer(int id)
    {
        if (players[id].heldPoints < 0)
        {
            // Red numbers when negative balance
            playerScores[id].ShowText($"<color=red> {players[id].heldPoints}</color>");
        }
        else
        {
            playerScores[id].ShowText($"{players[id].heldPoints}");

        }
    }

    private void UpdateHealthForPlayer(int id)
    {
        playerHPs[id].text = $"<color=#4DCF56>HP</color> {players[id].health}<size=18>/" +
            $"{players[id].maxHealth * players[id].currentStatsModifier.maxHealthMultModifier + players[id].currentStatsModifier.maxHealthFlatModifier}</size>";
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
                    //greenStamps[ps.id].enabled = true;
                    greenStamps[ps.id].color = greenStampColor;
                    break;
                case Stamp.StampType.Red:
                    //redStamps[ps.id].enabled = true;
                    redStamps[ps.id].color = redStampColor;
                    break;
                case Stamp.StampType.Blue:
                    //blueStamps[ps.id].enabled = true;
                    blueStamps[ps.id].color = blueStampColor;
                    break;
                case Stamp.StampType.Orange:
                    //orangeStamps[ps.id].enabled = true;
                    orangeStamps[ps.id].color = orangeStampColor;
                    break;
            }
        }
    }

    private void HideObtainedStamps(Stamp.StampType type)
    {
        switch (type)
        {
            case Stamp.StampType.Green:
                //greenStamps[currentPlayer.id].enabled = false;
                greenStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);

                break;
            case Stamp.StampType.Red:
                //redStamps[currentPlayer.id].enabled = false;
                redStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
                break;
            case Stamp.StampType.Blue:
                //blueStamps[currentPlayer.id].enabled = false;
                blueStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
                break;
            case Stamp.StampType.Orange:
                //orangeStamps[currentPlayer.id].enabled = false;
                orangeStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
                break;
        }
    }

    private void ClearHeldStamps()
    {
        foreach(Stamp.StampType s in currentPlayer.stamps)
        {
            HideObtainedStamps(s);
        }
        /*
        greenStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
        redStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
        blueStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
        orangeStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
        */
    }

    private void OnUndoPassByPawnShop(EntityPiece ps)
    {
        ClearHeldStamps();
        UpdateHeldStamps(ps);
    }
}
