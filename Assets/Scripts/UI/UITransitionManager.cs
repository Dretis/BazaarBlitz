using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using AllIn1SpriteShader;

public class UITransitionManager : MonoBehaviour
{
    Image uiImage;

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_EnteredCombatScene;
    public VoidEventChannelSO m_EnteredOverworldScene;

    void Awake()
    {
        uiImage = GetComponent<Image>();
        uiImage.material = new Material(uiImage.material);
    }

    private void OnEnable()
    {
        FadeOutInkTransition();

        m_EnteredCombatScene.OnEventRaised += FadeInInkTransition;
        m_EnteredOverworldScene.OnEventRaised += FadeInInkTransition;
    }

    private void OnDisable()
    {
        m_EnteredCombatScene.OnEventRaised -= FadeInInkTransition;
        m_EnteredOverworldScene.OnEventRaised -= FadeInInkTransition;
    }

    public void LoopTween(Material m, float delay, string attribute, float startVal, float endVal)
    {
        m.SetFloat(attribute, startVal);
        m.DOFloat(endVal, attribute, delay);
    }

    public void FadeOutInkTransition()
    {
        LoopTween(uiImage.material, 1f, "_FadeAmount", 0.2f, 1f);
    }

    public void FadeInInkTransition()
    {
        LoopTween(uiImage.material, 1f, "_FadeAmount", 1f, 0.2f);
    }
}
