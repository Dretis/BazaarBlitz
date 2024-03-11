using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ScoreManager : MonoBehaviour
{
    [SerializeField] private PlayerStats currentPlayer;
    [SerializeField] private List<EntityPiece> players;

    [Header("UI Elements")]
    [SerializeField] private Canvas scoreCanvas;
    [SerializeField] private List<TextMeshProUGUI> playerNames;
    [SerializeField] private List<TextMeshProUGUI> playerPlacements;
    [SerializeField] private List<TextMeshProUGUI> playerScores;
    [SerializeField] private List<Image> playerImages;

    void Start()
    {
        for (int i = 0; i < players.Count; i++)
        {
            playerNames[i].text = "" + players[i].nickname;
            playerScores[i].text = "@ " + players[i].heldPoints;
            playerImages[i].color = players[i].playerColor - new Color32(0, 0, 0, 125);

            // Placeholder for now
            playerPlacements[i].text = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
