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

    [Header("Indexed")]
    [SerializeField] private Image selectedDevice;
    [SerializeField] private TypewriterCore titleText;

    [Header("Setup Visuals")]
    [SerializeField] private Image baggieVisual;
    [SerializeField] private TypewriterCore baggieVisualName;

    [Header("UI Elements")]
    [SerializeField] private CanvasGroup menuPanel;
    [SerializeField] private GameObject colorButtonContainer;
    [SerializeField] private List<Button> colorButtons = new List<Button>();
    private Button colorButtonSelected;

    [SerializeField] private CanvasGroup readyPanel;
    [SerializeField] private Button readyButton;
    [SerializeField] private TypewriterCore readyText;

    private float ignoreInputTime = 1.5f;
    private bool inputEnabled = true;

    private void Start()
    {
        foreach (Transform child in colorButtonContainer.transform)
        {
            colorButtons.Add(child.GetComponentInChildren<Button>());
        }
    }

    public void SetSelectedColorButton(Button b)
    {
        colorButtonSelected = b;
    }

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

        menuPanel.alpha = 0;
        menuPanel.interactable = false;

        //readyPanel.SetActive(true);

        readyButton.Select();
    }

    public void SetColor(Color color)
    {
        if (!inputEnabled) return;

        PlayerConfigurationManager.instance.SetPlayerColor(playerIndex, color);

        menuPanel.alpha = 0;
        menuPanel.interactable = false;

        readyPanel.alpha = 1;
        readyPanel.interactable = true;

        readyButton.Select();
    }

    public void SetPalette(List<Color> palette)
    {
        if (!inputEnabled) return;

        PlayerConfigurationManager.instance.SetPlayerPalette(playerIndex, palette);
    }

    public void SetName(string name)
    {
        if (!inputEnabled) return;

        PlayerConfigurationManager.instance.SetPlayerName(playerIndex, name);
    }

    public void SetSelectedColor(Color selectedColor)
    {
        baggieVisual.color = selectedColor;
    }

    public void SetSelectedName(string selectedName)
    {
        baggieVisualName.ShowText(selectedName);
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

        readyPanel.alpha = 0;
        readyPanel.interactable = false;

        readyText.ShowText("Ready!");
    }

    public void UnreadyPlayer()
    {
        PlayerConfigurationManager.instance.UnreadyPlayer(playerIndex);

        readyPanel.alpha = 0;
        readyPanel.interactable = false;

        menuPanel.alpha = 1;
        menuPanel.interactable = true;

        colorButtonSelected.Select();
        //colorButtons[0].Select();
    }

    public IEnumerator PauseInputTimeFor(float delay)
    {
        yield return new WaitForSeconds(delay);
        inputEnabled = true;
        Debug.Log("you can input now");
        yield return null;
    }
}
