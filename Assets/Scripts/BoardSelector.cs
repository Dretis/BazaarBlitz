using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BoardSelector : MonoBehaviour
{
    [SerializeField] private string selectedBoard;

    [Header("UI")]
    [SerializeField] private Canvas mainLayout;
    [SerializeField] private CanvasGroup boardSelectGroup;
    [SerializeField] private GameObject boardSelectMenuObject;
    [SerializeField] private CanvasGroup numberOfPlayersSelectGroup;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI bottomText;


    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_BoardSelected;
    public VoidEventChannelSO m_ReturnToMainMenu;
    
    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_AllPlayersReady;
    public VoidEventChannelSO m_NumberOfPlayersSelected;

    private void OnEnable()
    {
        m_AllPlayersReady.OnEventRaised += OnAllPlayersReady;
        m_NumberOfPlayersSelected.OnEventRaised += OnNumberOfPlayersSelected;


        m_BoardSelected.OnEventRaised += OnBoardSelected;
        m_ReturnToMainMenu.OnEventRaised += OnReturnToMainMenu;
    }


    private void OnDisable()
    {
        m_AllPlayersReady.OnEventRaised -= OnAllPlayersReady;
        m_NumberOfPlayersSelected.OnEventRaised -= OnNumberOfPlayersSelected;


        m_BoardSelected.OnEventRaised -= OnBoardSelected;
        m_ReturnToMainMenu.OnEventRaised -= OnReturnToMainMenu;
    }

    void Start()
    {
        //boardSelectGroup.alpha = 0;
        boardSelectGroup.gameObject.SetActive(false);

        headerText.text = "Game Setup";
        bottomText.text = "How many people are playing?";
    }

    private void Update()
    {
        if (Input.GetButtonDown("Back"))
        {
            Debug.Log("'Back' button pressed");
            m_ReturnToMainMenu.RaiseEvent();
            //SceneManager.LoadScene("MainMenu");
        }
    }

    private void OnReturnToMainMenu()
    {
        //SceneManager.LoadScene("MainMenu");
        StartCoroutine(DelayedEnterMatch("MainMenu", 1f));
    }

    private void OnAllPlayersReady()
    {
        mainLayout.enabled = false;
        mainLayout.gameObject.SetActive(false);

        boardSelectGroup.gameObject.SetActive(true);

        headerText.text = "Board Select";
        bottomText.text = "Choose which board to play on!";

        EventSystem.current.SetSelectedGameObject(boardSelectMenuObject);
    }

    private void OnNumberOfPlayersSelected()
    {
        numberOfPlayersSelectGroup.gameObject.SetActive(false);

        headerText.text = $"{PlayerConfigurationManager.instance.ruleset.numberOfPlayers}-Player Setup";
        bottomText.text = "Press START or [ENTER] to join!";
        //numberOfPlayersSelectGroup
    }

    public void BoardSelected(string board)
    {
        selectedBoard = board;
        m_BoardSelected.RaiseEvent();
    }

    public void OnBoardSelected()
    {
        StartCoroutine(DelayedEnterMatch(selectedBoard, 1.5f));
    }

    public IEnumerator DelayedEnterMatch(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
        yield return null;
    }
}
