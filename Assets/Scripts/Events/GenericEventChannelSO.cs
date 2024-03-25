using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class GenericEventChannelSO<T> : ScriptableObject
{
    [Tooltip("The action to perform; Listeners subscribe to this UnityAction")]
    public UnityAction<T> OnEventRaised;

    public void RaiseEvent(T parameter)
    {

        if (OnEventRaised == null)
            return;

        OnEventRaised.Invoke(parameter);

    }
}

public abstract class GenericEventChannelSO<T0, T1> : ScriptableObject
{
    [Tooltip("The action to perform; Listeners subscribe to this UnityAction")]
    public UnityAction<T0, T1> OnEventRaised;

    public void RaiseEvent(T0 parameter1, T1 parameter2)
    {

        if (OnEventRaised == null)
            return;

        OnEventRaised.Invoke(parameter1, parameter2);

    }
}

// To create addition event channels, simply derive a class from GenericEventChannelSO
// filling in the type T. Leave the concrete implementation blank. This is a quick way
// to create new event channels.

// For example:
//[CreateAssetMenu(menuName = "Events/Float EventChannel", fileName = "FloatEventChannel")]
//public class FloatEventChannelSO : GenericEventChannelSO<float> {}

// Define additional GenericEventChannels if you need more than one parameter in the payload.

