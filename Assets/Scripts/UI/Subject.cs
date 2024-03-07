using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Subject : MonoBehaviour
{
    private List<IObserver> _observers = new List<IObserver>();

    
    // add an observer to subject's collection
    public void AddObserver(IObserver observer)
    {
        _observers.Add(observer);
    }

    // remove an observer to subject's collection
    public void RemoveObserver(IObserver observer)
    {
        _observers.Remove(observer);
    }

    // notify each observer that an event has occured
    protected void NotifyObservers()
    {
        _observers.ForEach((_observer) =>
        {
            _observer.OnNotify();
        });
    }
}
