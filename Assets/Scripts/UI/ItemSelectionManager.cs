using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSelectionManager : MonoBehaviour
{
    public static ItemSelectionManager instance;
    public GameObject[] items;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void OnEnable()
    {
        SetSelectedAfterOneFrame();
        //EventSystem.current.SetSelectedGameObject(items[0]);
    }

    private IEnumerator SetSelectedAfterOneFrame()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(items[0]);
    }
}
