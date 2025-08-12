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
    public VoidEventChannelSO m_AllPlayersReady;
    public PlayerEventChannelSO m_TransitionIntoCombat;

    void Awake()
    {
        uiImage = GetComponent<Image>();
        uiImage.material = new Material(uiImage.material);
    }

    private void OnEnable()
    {
        FadeOutInkTransition();

        m_EnteredCombatScene.OnEventRaised += OnEnteredCombatScene;
        m_EnteredOverworldScene.OnEventRaised += FadeInInkTransition;
        m_AllPlayersReady.OnEventRaised += FadeInInkTransition;
        m_TransitionIntoCombat.OnEventRaised += OnTransitionIntoCombat;
    }

    private void OnDisable()
    {
        m_EnteredCombatScene.OnEventRaised -= OnEnteredCombatScene;
        m_EnteredOverworldScene.OnEventRaised -= FadeInInkTransition;
        m_AllPlayersReady.OnEventRaised -= FadeInInkTransition;
        m_TransitionIntoCombat.OnEventRaised -= OnTransitionIntoCombat;
    }

    public void LoopTween(Material m, float delay, string attribute, float startVal, float endVal)
    {
        m.SetFloat(attribute, startVal);
        m.DOFloat(endVal, attribute, delay);
    }

    public void FadeOutInkTransition()
    {
        LoopTween(uiImage.material, 1f, "_FadeAmount", 0.1f, 1f);
    }

    public void FadeOutInkTransition(float delay)
    {
        LoopTween(uiImage.material, delay, "_FadeAmount", 0.1f, 1f);
    }

    public void FadeInInkTransition()
    {
        LoopTween(uiImage.material, 1f, "_FadeAmount", 1f, 0.1f);
    }

    public void FadeInInkTransition(float delay)
    {
        LoopTween(uiImage.material, delay, "_FadeAmount", 1f, 0.1f);
    }

    private void OnEnteredCombatScene()
    {
        FadeInInkTransition(.45f);
    }

    private void OnTransitionIntoCombat(EntityPiece player)
    {
        FadeInInkTransition(.5f);
    }
}
