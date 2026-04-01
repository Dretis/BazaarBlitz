using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorStateTransitionDefaultParamSetter
{
    [InitializeOnLoadMethod]
    private static void HookToObjectChangeEventStream()
    {
        ObjectChangeEvents.changesPublished += OnObjectChangeEventsPublished;
    }

    private static void OnObjectChangeEventsPublished(ref ObjectChangeEventStream stream)
    {
        for (var i = 0; i < stream.length; i++)
        {
            if (stream.GetEventType(i) is not ObjectChangeKind.CreateAssetObject) continue;

            stream.GetCreateAssetObjectEvent(i, out var createAssetEventData);
            if (Resources.InstanceIDToObject(createAssetEventData.instanceId) is not AnimatorStateTransition transition) continue;

            // write your default parameter values you want to set here...
            transition.hasExitTime = false;
            transition.duration = 0;
            transition.offset = 0;

            Debug.Log("AnimatorStateTransitionDefaultParamSetter | Default Values Changed.");
        }
    }
}

