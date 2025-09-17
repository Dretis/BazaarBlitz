using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using static UnityEngine.Rendering.DebugUI;

public class UIResultsManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup resultGroup;
    [SerializeField] private List<Image> playerImages;
    [SerializeField] private List<TextMeshProUGUI> playerNames;
    [SerializeField] private List<TextMeshProUGUI> playerScores;

    [Header("Listen On Event Channels")]
    public PlayerListEventChannelSO m_ResultFinalScores;
    private void OnEnable()
    {
        m_ResultFinalScores.OnEventRaised += OnResultFinalScores;
    }

    private void OnDisable()
    {
        m_ResultFinalScores.OnEventRaised -= OnResultFinalScores;
    }

    private void Start()
    {
        resultGroup.alpha = 0f;
    }

    private void OnResultFinalScores(List<EntityPiece> players)
    {
        StartCoroutine(DelayShowResults(5f));

        //int i = players.Count - 1; i >= 0; i--
        for (int i = 0; i < players.Count; i++)
        {
            playerNames[i].text = "" + players[i].entityName;
            playerImages[i].color = players[i].playerColor - new Color32(0, 0, 0, 0); // minus transparency
            playerScores[i].text = "" + players[i].heldPoints;
        }
    }

    private IEnumerator DelayShowResults(float delay)
    {
        yield return new WaitForSeconds(delay);
        LMotion.Create(resultGroup.alpha, 1, 1f)
            .Bind(x => resultGroup.alpha = x);
        yield return null;
    }
}
