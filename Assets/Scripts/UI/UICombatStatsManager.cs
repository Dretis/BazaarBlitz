using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICombatStatsManager : MonoBehaviour
{
    [SerializeField] private Image hp;
    [SerializeField] private float hpRatio;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hp.fillAmount = hpRatio;
    }
}
