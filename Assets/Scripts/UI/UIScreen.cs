using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;

public class UIScreen : MonoBehaviour
{
    public GameObject defaultSelectedObject;

    [Header("Main Parent UI")]
    public RectTransform rect;
    public Canvas canvas;

    [Header("Transitions")]
    public bool hideOnAwake;
    public bool useTransition;
    public float transitionDelay;

    public MotionHandle currentMotion;

    // Start is called before the first frame update
    void Awake()
    {
        Initialize();

        if (hideOnAwake) HideImmediately();
    }

    public virtual void Initialize()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponent<Canvas>();
    }

    public virtual void Show() 
    {
        // Does something when it needs to appear

        if (currentMotion.IsPlaying()) currentMotion.Cancel();

        canvas.enabled = true;
        // currentMotion = do some animation here
    }

    public virtual void Hide()
    {
        // Does something when it needs to go away

        if (currentMotion.IsPlaying()) currentMotion.Complete();
            
        canvas.enabled = false;
        // currentMotion = do some animation here
    }

    public void HideImmediately()
    {
        canvas.enabled = false;
    }
}
