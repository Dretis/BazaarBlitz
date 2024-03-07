using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class UnityGameEventListener : MonoBehaviour, IGameEventListener
{
    [Tooltip("Event to register with.")]
    [SerializeField] private GameEvent gameEvent;

    [Tooltip("Unity Events to perform when Game Event is Raised.")]
    [SerializeField] private UnityEvent response;

    [SerializeField] Sprite itemSprite;

    void OnEnable()
    {

        if(gameEvent != null) gameEvent.RegisterListener(this);
    }

    void OnDisable()
    {
        gameEvent.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        response?.Invoke();
    }
}
