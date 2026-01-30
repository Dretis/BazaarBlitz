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
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI bottomText;


    [Header("Boardcast on Event Channels")]
    public VoidEventChannelSO m_BoardSelected;
    public VoidEventChannelSO m_ReturnToMainMenu;
    
    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_AllPlayersReady;

    private void OnEnable()
    {
        m_AllPlayersReady.OnEventRaised += OnAllPlayersReady;
        m_BoardSelected.OnEventRaised += OnBoardSelected;
        m_ReturnToMainMenu.OnEventRaised += OnReturnToMainMenu;
    }


    private void OnDisable()
    {
        m_AllPlayersReady.OnEventRaised -= OnAllPlayersReady;
        m_BoardSelected.OnEventRaised -= OnBoardSelected;
        m_ReturnToMainMenu.OnEventRaised -= OnReturnToMainMenu;
    }

    void Start()
    {
        //boardSelectGroup.alpha = 0;
        boardSelectGroup.gameObject.SetActive(false);
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

        headerText.text = "[Board Select]";
        bottomText.text = "Choose which board to play on!";

        EventSystem.current.SetSelectedGameObject(boardSelectGroup.transform.GetChild(0).gameObject);
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
