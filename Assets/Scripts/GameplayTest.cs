using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameplayTest : MonoBehaviour
{
    // Tile Data and Shit IGNORE THIS SECTION FOR NOW
    [SerializeField]
    private List<EntityPiece> playerUnits = new List<EntityPiece>();
    [SerializeField]
    private List<EntityPiece> nextPlayers = new List<EntityPiece>();
    public EntityPiece currentPlayer;
    public MapNode currentPlayerInitialNode;

    public Dictionary<Vector2Int, GameObject> map = new Dictionary<Vector2Int, GameObject>();
    public Dictionary<Vector2Int, GameObject> unitPos = new Dictionary<Vector2Int, GameObject>();

    public int turn = 1;
    public int phase = 1;
    public int diceRoll;
    private int yoinkRoll;

    public TextMeshProUGUI rollText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI gameInfo;

    public GameObject encounterScreen;
    public TextMeshProUGUI p1fight;
    public TextMeshProUGUI p2fight;
    public TextMeshProUGUI resultInfo;
    public bool encounterOver = false;

    public MapNode wantedNode;

    ///private Scene currentCombat;

    //public PlayerInput playerInput;
    //public PlayerControls input;

    // Start is called before the first frame update
    void Start()
    {
        playerUnits.AddRange(FindObjectsOfType<EntityPiece>());
        //nextPlayers = playerUnits;
        encounterScreen.SetActive(false);

        gameInfo.text = "[Scoreboard]";
        foreach (var player in playerUnits)
        {
            nextPlayers.Add(player);
            gameInfo.text += "\n" + player.nickname + ": " + player.heldPoints + " | " + player.finalPoints;
        }

        currentPlayer = nextPlayers[playerUnits.Count - 1];
        currentPlayerInitialNode = currentPlayer.occupiedNode;
        turnText.text = currentPlayer.nickname + "'s Turn!";
        turnText.color = currentPlayer.playerColor;
    }

    // Update is called once per frame
    void Update()
    {
        // Roll Phase (1)
        RollDice(currentPlayer);

        // Pick Direction to Go Phase (2)
        PickDirection(currentPlayer);

        // Move-to Node (3)
        MoveAround(currentPlayer);

        // Battle-Event Phase (4)
        EncounterTime(currentPlayer, currentPlayer.occupiedNode);

        // Confirmation Portion (5)
        ConfirmContinue(currentPlayer);

        // End of turn, next player! (5)
        EndOfTurn(currentPlayer);
    }

    void RollDice(EntityPiece p)
    {
        if (phase == 1 && Input.GetKeyDown(KeyCode.Space))
        {
            diceRoll = Random.Range(1, 7); // Roll from 1 to 6
            rollText.text = "" + diceRoll;
            p.movementTotal = p.movementLeft = diceRoll;

            phase++;
        }
    }

    void PickDirection(EntityPiece p)
    {
        if (phase != 2)
            return;
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                wantedNode = p.occupiedNode.north;

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                wantedNode = p.occupiedNode.east;

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                wantedNode = p.occupiedNode.south;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                wantedNode = p.occupiedNode.west;

            if (wantedNode != null)
            {
                phase++;
            }
        }
    }

    void MoveAround(EntityPiece p)
    {
        if (phase != 3)
            return;
        else
        {
            var lastEle = p.traveledNodes.Count;
            var lastNode = p.traveledNodes[lastEle - 1];

            if (wantedNode == lastNode) // The direction you picked was the node you just came from (Redo)
            {
                p.traveledNodes.Remove(lastNode);
                p.occupiedNode = lastNode;

                p.movementLeft++;
                rollText.text = "" + p.movementLeft;

                p.transform.DOMove(lastNode.transform.position, .25f)
                    .SetEase(Ease.OutQuint);

                wantedNode = null;
                phase--; // Go back to picking direction
            }
            else if(wantedNode != null) // Go to that new node and occupy it
            {
                p.traveledNodes.Add(p.occupiedNode);
                p.occupiedNode = wantedNode;

                p.movementLeft--;
                rollText.text = "" + p.movementLeft;

                p.transform.DOMove(wantedNode.transform.position, .25f)
                    .SetEase(Ease.OutQuint);

                wantedNode = null;
                if (p.movementLeft <= 0)
                {
                    p.traveledNodes.Clear(); // Forget all the nodes traveled to
                    p.traveledNodes.Add(p.occupiedNode); //i need this i guess

                    phase++; // next phase
                }
                else
                    phase--; // Go back to picking direction
            }
        }
    }

    void EncounterTime(EntityPiece p, MapNode m)
    {
        if (phase != 4)
            return;

        var otherPlayer = p.occupiedNode.playerOccupied;

        if (m.CompareTag("Castle")) //Stash your points
        {
            encounterScreen.SetActive(true);
            p1fight.text = "";
            p2fight.text = "";
            resultInfo.text = "<size=45>[STASHING]</size>\n" + p.heldPoints + " point(s) have been stashed. \n<size=30> You are now at " + p.finalPoints + ". </size>";

            if (p.heldPoints != 0)
                p.finalPoints += p.heldPoints;
            p.heldPoints = 0;

            encounterOver = true;
            phase++;
        }
        else if (m.CompareTag("Encounter") && otherPlayer != null && otherPlayer != currentPlayer) // Player Fight
        {
            ///
            encounterScreen.SetActive(true);
            int yoinkValue = 4;

            if (yoinkRoll <= 0)
            {
                encounterScreen.SetActive(true);
                p1fight.text = "" + p.heldPoints;
                p2fight.text = "" + otherPlayer.heldPoints;

                resultInfo.text = "<size=45>[PLAYER ENCOUNTER]</size>\nRoll higher than " + yoinkValue + "!!! \n<size=30> [SPACE] to proceed the roll. </size>";

                if (Input.GetKeyDown(KeyCode.Space))
                    yoinkRoll = Random.Range(1, 7); // Roll from 1 to 6
            }
            else
            {
                if (yoinkRoll > yoinkValue)
                {
                    resultInfo.text = "<size=45>[SUCCESS]</size><size=30>\nYou rolled a... </size>\n" + yoinkRoll + "! \n<size=30> Their points are now yours. </size>";
                    p.heldPoints += otherPlayer.heldPoints;
                    otherPlayer.heldPoints = 0;
                }
                else
                {
                    resultInfo.text = "<size=45>[FAIL]</size><size=30>\nYou rolled a... </size>\n" + yoinkRoll + ". \n<size=30> Half your points dropped due to carelessness. </size>";
                    p.heldPoints /= 2;
                }
                yoinkRoll = 0;
                encounterOver = true;
                phase++;
            }
            ///

            // instance combat scene
            // Combat scene gets a reference to GameplayTest
            // Combat scene calls function in gameplay test when someone wins, with winner
            //   and player objects (so items stay used)
            // That returns the winner, and points are stolen accordingly w/ phase++;encounterOver



        }
        else if (m.CompareTag("Encounter")) // Regular Encounter
        {
            encounterScreen.SetActive(true);
            p1fight.text = "";
            p2fight.text = "";
            resultInfo.text = "<size=45>[ENCOUNTER]</size>\nRock, Paper, Scissors!!! \n<size=30> Rock = 1 or J | Paper = 2 or K | Scissors = 3 or L </size>";

            var playerPick = 0;
            /// monsterPick = Random.Range(1, 10);





            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.J))
            { // Rock
                playerPick = 1;
                p1fight.text = "ROCK";
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.K))
            {// Paper
                playerPick = 2;
                p1fight.text = "PAPER";
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.L))
            {// Scissors
                playerPick = 3;
                p1fight.text = "SCISSORS";
            }
            if (playerPick != 0)
            {
                var monsterPick = Random.Range(1, 4); // Rock = 1, Paper = 2, Scissors = 3

                switch (monsterPick)
                {
                    case 1:
                        p2fight.text = "ROCK";
                        break;
                    case 2:
                        p2fight.text = "PAPER";
                        break;
                    case 3:
                        p2fight.text = "SCISSORS";
                        break;

                }
                if (playerPick == monsterPick) // Tie
                {
                    resultInfo.text = "TIE.";
                }
                else if ((playerPick == 1 && monsterPick == 2) ||
                         (playerPick == 2 && monsterPick == 3) ||
                         (playerPick == 3 && monsterPick == 1))
                {
                    resultInfo.text = "YOU LOSE...";
                    p.heldPoints -= 1;
                }
                else
                {
                    resultInfo.text = "YOU WIN!!!";
                    p.heldPoints += 2;
                }


                //
                encounterOver = true;
                phase++;
            }
        }
    }

    void ConfirmContinue(EntityPiece p)
    {
        if (phase != 5)
            return;

        if (encounterOver && Input.GetKeyDown(KeyCode.Space))
        {
            phase++;
            encounterOver = false;
            encounterScreen.SetActive(false);
            UpdatePoints();
        }
    }

    void EndOfTurn(EntityPiece p)
    {
        if (phase != 6)
            return;

        Debug.Log("initial node player: " + currentPlayerInitialNode.playerOccupied);
        Debug.Log("initial node player: " + currentPlayer);

        if (currentPlayerInitialNode.playerOccupied == currentPlayer)
        {
            currentPlayerInitialNode.playerOccupied = null;
        }

        // Change to the next player in the list
        nextPlayers.Remove(currentPlayer); // remove current player from the turn order
        p.occupiedNode.playerOccupied = p; // update to have that player on that node now

        if (nextPlayers.Count != 0)
        {
            currentPlayer = nextPlayers[nextPlayers.Count - 1];
            turnText.text = currentPlayer.nickname + "'s Turn!";
            turnText.color = currentPlayer.playerColor;

            currentPlayerInitialNode = currentPlayer.occupiedNode;
            phase = 1;
        }
        else
        {
            foreach(var players in playerUnits)
                nextPlayers.Add(players); // Refill the list with all the players again

            currentPlayer = nextPlayers[nextPlayers.Count - 1]; //Player at end of the ist goes again
            currentPlayer = nextPlayers[nextPlayers.Count - 1];
            turnText.text = currentPlayer.nickname + "'s Turn!";
            turnText.color = currentPlayer.playerColor;

            currentPlayerInitialNode = currentPlayer.occupiedNode;
            phase = 1;
        }
    }

    void UpdatePoints()
    {
        gameInfo.text = "[Scoreboard]";
        foreach (var player in playerUnits)
        {
            gameInfo.text += "\n" + player.nickname + ": " + player.heldPoints + " | " + player.finalPoints;
        }
    }
}
