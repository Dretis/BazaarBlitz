using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEditor.Experimental.GraphView;

public class CombatUIManager : MonoBehaviour
{
    public enum FightingPosition 
    { 
        Left, 
        Right 
    }

    // Canvas UI Shit
    public CombatAnimationManager player1CombatAnimManager;
    public CombatAnimationManager player2CombatAnimManager;

    public SpriteRenderer player1Renderer;
    public SpriteRenderer player2Renderer;

    // 0 = Player Name, 1 = Action Flavor Text, 2 = Player Current Phase
    public List<TextMeshProUGUI> player1StateTexts;
    public List<TextMeshProUGUI> player2StateTexts;

    // Action Order in the list: Up, Down, Left, Right
    public List<TextMeshProUGUI> player1ActionTexts;
    public List<TextMeshProUGUI> player2ActionTexts;

    [SerializeField] private Image player1HPFill;
    [SerializeField] private Image player2HPFill;

    public Animator player1Animator;
    public Animator player2Animator;

    public AnimatorOverrideController player1OverrideController;
    public AnimatorOverrideController player2OverrideController;

    private void Awake()
    {
        var player1 = FindObjectOfType<CombatManager>().player1;
        var player2 = FindObjectOfType<CombatManager>().player2;
        /*
        if (player1.isEnemy)
            player1Renderer.color = player1.playerColor;
        else
        {
            player1Renderer.gameObject.GetComponent<PlayerPaletteLoader>().
                SetInspectorPalette(player1.GetComponent<PlayerPaletteLoader>().GetInspectorPalette());
        }
        */
        if (player1.TryGetComponent<PlayerPaletteLoader>(out PlayerPaletteLoader pal1))
        {
            player1Renderer.gameObject.GetComponent<PlayerPaletteLoader>().
                SetInspectorPalette(pal1.GetInspectorPalette());

            if (!player1.isEnemy)
            {
                //var p1AnimManager = player1Renderer.GetComponent<CombatAnimationManager>();
                player1CombatAnimManager.SetCombatBoatPalette(player1); 
            }
        }
        else
        {
            Debug.Log("Player/Enemy1 with no palette");
            player1Renderer.color = player2.playerColor;
        }

        if (player2.TryGetComponent<PlayerPaletteLoader>(out PlayerPaletteLoader pal2))
        {
            player2Renderer.gameObject.GetComponent<PlayerPaletteLoader>().
                SetInspectorPalette(pal2.GetInspectorPalette());

            if (!player2.isEnemy)
            {
                //var p2AnimManager = player2Renderer.GetComponent<CombatAnimationManager>();
                player2CombatAnimManager.SetCombatBoatPalette(player2);
            }
        }
        else
        {
            Debug.Log("Player/Enemy2 with no palette");
            player2Renderer.color = player2.playerColor;
        }

        player1OverrideController = new AnimatorOverrideController(FindObjectOfType<CombatManager>().player1.combatAnimatorController);
        player1Animator.runtimeAnimatorController = player1OverrideController;

        player2OverrideController = new AnimatorOverrideController(FindObjectOfType<CombatManager>().player2.combatAnimatorController);
        player2Animator.runtimeAnimatorController = player2OverrideController;

        //player1Animator.runtimeAnimatorController = FindObjectOfType<CombatManager>().player1.combatAnimatorController;
        //player2Animator.runtimeAnimatorController = FindObjectOfType<CombatManager>().player2.combatAnimatorController;

        //player1Renderer.color = CombatManager.Instance.player1.playerColor;
        //player2Renderer.color = CombatManager.Instance.player2.playerColor;
    }

    public void UpdateActionText(EntityPiece ps, Action.PhaseTypes phase)
    {
        List<TextMeshProUGUI> stateTexts = null;
        List<TextMeshProUGUI> actionTexts = null;
        List<Action> actions = null;
        Image hpFill = null;

        if (ps.fightingPosition == FightingPosition.Left)
        {
            // Change the text on the left side
            stateTexts = player1StateTexts;
            actionTexts = player1ActionTexts;
            hpFill = player1HPFill;
        }
        else
        {
            // Change the text on the right side
            stateTexts = player2StateTexts;
            actionTexts = player2ActionTexts;
            hpFill = player2HPFill;
        }

        stateTexts[0].text = ps.entityName;
        stateTexts[1].text = $"{Mathf.Max(ps.health,0)}";
        stateTexts[2].text = $"/{(ps.maxHealth * ps.currentStatsModifier.maxHealthMultModifier + ps.currentStatsModifier.maxHealthFlatModifier)}";

        float hpPercent = (float)(Mathf.Max(ps.health, 0)) / ((float)(ps.maxHealth * ps.currentStatsModifier.maxHealthMultModifier + ps.currentStatsModifier.maxHealthFlatModifier));
        Debug.Log($"HP% = {hpPercent}");
        hpFill.fillAmount = hpPercent;

        if (phase == Action.PhaseTypes.Attack)
        {
            actions = ps.attackActions;
            //stateTexts[2].text = "<color=#FF6476>[Attacking]</color>";
        }
        else
        {
            actions = ps.defendActions;
            //stateTexts[2].text = "<color=#65BCFF>[Defending]</color>";
        }

        var count = 0;
        foreach (Action a in actions)
        {
            var typeIconIndex = 0; // the tag to use based off the TMPro sprite asset
            Color32 actionColor = Color.white;
            // Change text color and icon based off action type
            switch (a.type)
            {
                case Action.WeaponTypes.Melee:
                    typeIconIndex = 0;
                    actionColor = new Color32(242, 108, 124, 255);
                    break;
                case Action.WeaponTypes.Gun:
                    typeIconIndex = 1;
                    actionColor = new Color32(109, 159, 242, 255);
                    break;
                case Action.WeaponTypes.Magic:
                    typeIconIndex = 2;
                    actionColor = new Color32(225, 142, 255, 255);
                    break;
                default: // Temporary Special
                    actionColor = Color.yellow;
                    break;
            }

            actionTexts[count].color = actionColor;
            actionTexts[count].text = "<sprite=" + typeIconIndex + ">" + a.actionName;
            count++;
        }
    }

    public void UpdateActionAnimation(int actionID, FightingPosition fp)
    {
        Animator animator = null;
        if (fp == FightingPosition.Left)
        {
            animator = player1Animator;
        }
        else
        {
            animator = player2Animator;
        }

        animator.SetInteger("ActionState", actionID);
    }
}
