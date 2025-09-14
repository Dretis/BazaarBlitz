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
    [SerializeField] private List<Sprite> deviceSprites; // Keyboard=0,Xbox=1,PS=2,Switch=3
    [SerializeField] private List<Color> playerPrimaryColors;

    [SerializeField] private Image selectedDevice;
    [SerializeField] private TypewriterCore titleText;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private Button readyButton;
    [SerializeField] private TypewriterCore readyText;

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

    public void SetName(string name)
    {
        if (!inputEnabled) return;

        PlayerConfigurationManager.instance.SetPlayerName(playerIndex, name);
    }

    public void SetDevice(PlayerInputController.CurrentDevice device)
    {
        if (!inputEnabled) return;

        selectedDevice.sprite = deviceSprites[(int)device];
        //PlayerConfigurationManager.instance.SetPlayerName(playerIndex, name);
    }

    public void ReadyPlayer()
    {
        if (!inputEnabled) return;

        PlayerConfigurationManager.instance.ReadyPlayer(playerIndex);
        readyButton.gameObject.SetActive(false);
        readyText.ShowText("Ready!");
    }

    public IEnumerator PauseInputTimeFor(float delay)
    {
        yield return new WaitForSeconds(delay);
        inputEnabled = true;
        Debug.Log("you can input now");
        yield return null;
    }
}
