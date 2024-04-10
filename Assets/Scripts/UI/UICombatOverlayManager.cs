using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICombatOverlayManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject versusHeader;

    [SerializeField] private GameObject leftActionPrompt;
    [SerializeField] private GameObject rightActionPrompt;

    [SerializeField] private GameObject leftDiceStat;
    [SerializeField] private GameObject rightDiceStat;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI leftHealthPoints;
    [SerializeField] private TextMeshProUGUI rightHealthPoints;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
