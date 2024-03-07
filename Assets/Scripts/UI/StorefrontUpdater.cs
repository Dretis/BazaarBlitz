using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorefrontUpdater : MonoBehaviour, IObserver
{
    [SerializeField] Subject _uiSubject;

    public void OnNotify()
    {
        Debug.Log("I HAVE BEEN NOTIFIED!");
    }

    private void OnEnable()
    {
        // add itself to subject's list of observers
        _uiSubject.AddObserver(this);
    }

    private void OnDisable()
    {
        // remove itself to subject's list of observers
        _uiSubject.RemoveObserver(this);
    }

}
