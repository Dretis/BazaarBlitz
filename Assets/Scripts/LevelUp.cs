using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUp : MonoBehaviour
{
    int pointsLeft;
    bool levelingUp = false;
    EntityPiece curPlayer;
    int attSelected = 0;
    int diceSelected = 0;

    // Uncomment everything when the level up ui is back in

    //public Canvas levelUpScreen;
    //public TextMeshProUGUI remainingSP;
    //public TextMeshProUGUI upgradeTooltip;
    //public List<TextMeshProUGUI> playerDiceNumbers = new List<TextMeshProUGUI>();
    //public List<TextMeshProUGUI> playerDiceNumbers = new List<TextMeshProUGUI>();


    // Start is called by the initializing script to assign what player to level up. This script should be part of an object, 
    // prob in a new scene or just disabled in an existing one, that has also has the ui elements. controls should be disabled during the levelup.
    void StartLevelUp(EntityPiece player)
    {
        pointsLeft = 5;
        curPlayer.maxHealth += 10;
        curPlayer.health += 10;
        curPlayer.RenownLevel += 1;
        curPlayer = player;
        levelingUp = true;
        
        //levelUpScreen.enabled = true;
        //remainingSP.text = $"{pointsLeft} SP left.";
        //upgradeTooltip.text = "Use [WASD] or [Arrows] to select dice faces.";
        //UpdatePlayerDiceStats(p, diceStats);


    }

    // Update is called once per frame
    void Update()
    {
        if (levelingUp == false) { // ensure curPlayer is updated and we're actually in the level up screen
            return;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) // Replace all these with proper OnButtonPress (whatever the name was) functions
        {
            controllerUp();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            controllerDown();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            controllerRight();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            controllerLeft();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) // E to leave till I find a good exit method that doesn't get you stuck.
        {
            controllerSelect();
        }
        if (pointsLeft <= 0 || Input.GetKeyDown(KeyCode.Escape)) // Esc to leave
        {
            finishLevelUp();
        }
    }

    void controllerUp()
    {
        if (attSelected > 1)
        {
            attSelected -= 1;
        }
        printIndex(attSelected, diceSelected, curPlayer);
    }

    void controllerDown()
    {
        if (attSelected < 3)
        {
            attSelected += 1;
        }
        printIndex(attSelected, diceSelected, curPlayer);
    }

    void controllerLeft()
    {
        if (diceSelected < 6)
        {
            diceSelected += 1;
        }
        printIndex(attSelected, diceSelected, curPlayer);
    }

    void controllerRight()
    {

        if (diceSelected > 1)
        {
            diceSelected -= 1;
        }
        printIndex(attSelected, diceSelected, curPlayer);
    }

    void printIndex(int row, int col, EntityPiece curPlayer) {
        int[] costArray = { -1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 5, 999 };
        if (row == 1) {
            int diceValue = (int)(curPlayer.strDie.dieFaces[diceSelected-1]);
            int diceUpgradeCost = costArray[diceValue];
            Debug.Log("Strength dice face " + col + " selected. Current value: " + diceValue + ". Upgrade cost " + diceUpgradeCost);
            //upgradeTooltip.text = $"<sprite=0> <color=red>[{diceValue}]</color> costs {diceUpgradeCost} SP to upgrade.";
        } else if (row == 2) {
            int diceValue = (int)(curPlayer.dexDie.dieFaces[diceSelected-1]);
            int diceUpgradeCost = costArray[diceValue];
            Debug.Log("Dex dice face " + col + " selected. Current value: " + diceValue + ". Upgrade cost " + diceUpgradeCost);
            //upgradeTooltip.text = $"<sprite=1> <color=blue>[{diceValue}]</color> costs {diceUpgradeCost} SP to upgrade.";

        } else {
            int diceValue = (int)(curPlayer.intDie.dieFaces[diceSelected-1]);
            int diceUpgradeCost = costArray[diceValue];
            Debug.Log("Magic dice face " + col + " selected. Current value: " + diceValue + ". Upgrade cost " + diceUpgradeCost);
            //upgradeTooltip.text = $"<sprite=2> <color=purple>[{diceValue}]</color> costs {diceUpgradeCost} SP to upgrade.";

        }
    }

    
    // I ripped this from another script, delete this later
    public void UpdatePlayerDiceStats(EntityPiece entity, GameObject diceStats)
    {
        /*
        // Visually updates the dice stats ui based on the entity and side
        playerDiceNumbers.Clear();

        // Goes through the diceStats UI List and finds the text components
        foreach (Transform child in diceStats.transform)
        {
            playerDiceNumbers.Add(child.GetComponentInChildren<TextMeshProUGUI>());
        }

        // Updates each individual dice from the text list based on the type
        // the following code is ABSOLUTELY DISGUSTING
        var faceIndex = 0;

        for (int i = 0; i < 6; i++)
        {
            playerDiceNumbers[i].text = $"{entity.strDie[faceIndex]}";
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 6; i < 12; i++)
        {
            playerDiceNumbers[i].text = $"{entity.dexDie[faceIndex]}";
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 12; i < 18; i++)
        {
            playerDiceNumbers[i].text = $"{entity.intDie[faceIndex]}";
            faceIndex++;
        }*/
    }

    void controllerSelect() {
        
        int[] costArray = { -1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 5, 999 }; // 0-1 (0 indexing issues), 1-2, 2-3, etc. 10-11 isnt possible.
        if (attSelected == 1)
        {
            int diceUpgradeCost = costArray[(int)(curPlayer.strDie.dieFaces[diceSelected - 1])];
            if (pointsLeft >= diceUpgradeCost)
            {
                curPlayer.strDie.dieFaces[diceSelected - 1] += 1;
                pointsLeft -= diceUpgradeCost;
                Debug.Log("Strength Die upgraded on face " + diceSelected + " making it " + curPlayer.strDie.dieFaces[diceSelected - 1]);
                Debug.Log(pointsLeft + " points left after paying " + diceUpgradeCost);
                //remainingSP.text = $"{pointsLeft} SP left.";
                //UpdatePlayerDiceStats(curPlayer, diceStats);
            }
            else
            {
                //upgradeToolticurPlayer.text = "Not enough points. Costs " + diceUpgradeCost + " SP but you have only " + pointsLeft;
                Debug.Log("Not enough points. Costs " + diceUpgradeCost + ", but you have only " + pointsLeft);
            }
        }
        else if (attSelected == 2)
        {
            int diceUpgradeCost = costArray[(int)(curPlayer.dexDie.dieFaces[diceSelected - 1])];
            if (pointsLeft >= diceUpgradeCost)
            {
                curPlayer.dexDie.dieFaces[diceSelected - 1] += 1;
                pointsLeft -= diceUpgradeCost;
                Debug.Log("Dex Die upgraded on face " + diceSelected + " making it " + curPlayer.dexDie.dieFaces[diceSelected - 1]);
                Debug.Log(pointsLeft + " points left after paying " + diceUpgradeCost);
                //remainingSP.text = $"{pointsLeft} SP left.";
                //UpdatePlayerDiceStats(curPlayer, diceStats);
            }
            else
            {
                //upgradeToolticurPlayer.text = "Not enough points. Costs " + diceUpgradeCost + " SP but you have only " + pointsLeft;
                Debug.Log("Not enough points. Costs " + diceUpgradeCost + ", but you have only " + pointsLeft);
            }
        }
        else if (attSelected == 3)
        {
            int diceUpgradeCost = costArray[(int)(curPlayer.intDie.dieFaces[diceSelected - 1])];
            if (pointsLeft >= diceUpgradeCost)
            {
                curPlayer.intDie.dieFaces[diceSelected - 1] += 1;
                pointsLeft -= diceUpgradeCost;
                Debug.Log("Magic Die upgraded on face " + diceSelected + " making it " + curPlayer.intDie.dieFaces[diceSelected - 1]);
                Debug.Log(pointsLeft + " points left after paying " + diceUpgradeCost);
                //remainingSP.text = $"{pointsLeft} SP left.";
                //UpdatePlayerDiceStats(curPlayer, diceStats);
            }
            else
            {
                //upgradeToolticurPlayer.text = "Not enough points. Costs " + diceUpgradeCost + " SP but you have only " + pointsLeft;
                Debug.Log("Not enough points. Costs " + diceUpgradeCost + ", but you have only " + pointsLeft);
            }
        }
        else
        {
            Debug.Log("Uh oh, you just tried to upgrade a dice you dont have.");
        }
        
    }



    void finishLevelUp() {
        levelingUp = false;
        Debug.Log("Out of points, level up done");
        //levelUpScreen.enabled = false;
        Destroy(this);
    }
}
