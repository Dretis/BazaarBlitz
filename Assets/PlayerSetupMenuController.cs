using System.Collections;
using System.Collections.Generic;
using TMPro;
using Febucci.UI.Core;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSetupMenuController : MonoBehaviour
{
    private int playerIndex;
    //[SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private List<Color> playerPrimaryColors;
    [SerializeField] private TypewriterCore titleText;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private Button readyButton;

    private float ignoreInputTime = 1.5f;
    private bool inputEnabled = true;


    public void SetPlayerIndex(int pi)
    {
        playerIndex = pi;
        titleText.ShowText($"P{(pi+1).ToString()}");
        //PauseInputTimeFor(ignoreInputTime);
    }

    public void SetColor(int colorIndex)
    {
        if(!inputEnabled) return;

        PlayerConfigurationManager.instance.SetPlayerColor(playerIndex, playerPrimaryColors[colorIndex]);
        readyPanel.SetActive(true);
        readyButton.Select();
        menuPanel.SetActive(false);
    }

    public void SetColor(Color color)
    {
        if (!inputEnabled) return;

        PlayerConfigurationManager.instance.SetPlayerColor(playerIndex, color);
        readyPanel.SetActive(true);
        readyButton.Select();
        menuPanel.SetActive(false);
    }

    public void ReadyPlayer()
    {
        if (!inputEnabled) return;

        PlayerConfigurationManager.instance.ReadyPlayer(playerIndex);
        readyButton.gameObject.SetActive(false);
    }

    public IEnumerator PauseInputTimeFor(float delay)
    {
        yield return new WaitForSeconds(delay);
        inputEnabled = true;
        Debug.Log("you can input now");
        yield return null;
    }
}
