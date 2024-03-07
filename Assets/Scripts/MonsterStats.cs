using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStats : MonoBehaviour
{
    public int id;
    public PlayerStats combatStats;

    // Start is called before the first frame update
    void Start()
    {
        combatStats = this.GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
