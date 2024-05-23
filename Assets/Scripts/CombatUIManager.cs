using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CombatUIManager : MonoBehaviour
{
    public enum FightingPosition 
    { 
        Left, 
        Right 
    }

    // Canvas UI Shit
    public SpriteRenderer player1Renderer;
    public SpriteRenderer player2Renderer;

    // 0 = Player Name, 1 = Action Flavor Text, 2 = Player Current Phase
    public List<TextMeshProUGUI> player1StateTexts;
    public List<TextMeshProUGUI> player2StateTexts;

    // Action Order in the list: Up, Down, Left, Right
    public List<TextMeshProUGUI> player1ActionTexts;
    public List<TextMeshProUGUI> player2ActionTexts;

    public Animator player1Animator;
    public Animator player2Animator;

    private void Awake()
    {
        player1Renderer.color = CombatManager.Instance.player1.playerColor;
        player2Renderer.color = CombatManager.Instance.player2.playerColor;
    }

    public void UpdateActionText(EntityPiece ps, Action.PhaseTypes phase)
    {
        List<TextMeshProUGUI> stateTexts = null;
        List<TextMeshProUGUI> actionTexts = null;
        List<Action> actions = null;

        if (ps.fightingPosition == FightingPosition.Left)
        {
            // Change the text on the left side
            stateTexts = player1StateTexts;
            actionTexts = player1ActionTexts;
        }
        else
        {
            // Change the text on the right side
            stateTexts = player2StateTexts;
            actionTexts = player2ActionTexts;
        }

        stateTexts[0].text = ps.entityName;
        stateTexts[1].text = $"HP: {Mathf.Max(ps.health,0)}/{(ps.maxHealth * ps.currentStatsModifier.maxHealthMultModifier + ps.currentStatsModifier.maxHealthFlatModifier)}";
        

        if(phase == Action.PhaseTypes.Attack)
        {
            actions = ps.attackActions;
            stateTexts[2].text = "<color=#FF6476>[Attacking]</color>";
        }
        else
        {
            actions = ps.defendActions;
            stateTexts[2].text = "<color=#65BCFF>[Defending]</color>";
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
