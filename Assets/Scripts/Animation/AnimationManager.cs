using System.Collections;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] private Animator currentAnimator;
    [SerializeField] public EntityPiece ep; //test variable delete later

    [Header("Listen On Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    public PlayerEventChannelSO m_DiceRollPrep;
    public IntEventChannelSO m_DiceThrown;
    public PlayerEventChannelSO m_ResetToIdle;

    private void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += SetCurrentPlayerAnimator;
        m_DiceRollPrep.OnEventRaised += DiceIsRolling;
        m_DiceThrown.OnEventRaised += DiceIsThrown;
        m_ResetToIdle.OnEventRaised += ResetToIdleAnim;
    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= SetCurrentPlayerAnimator;
        m_DiceRollPrep.OnEventRaised -= DiceIsRolling;
        m_DiceThrown.OnEventRaised -= DiceIsThrown;
        m_ResetToIdle.OnEventRaised -= ResetToIdleAnim;
    }

    private void SetCurrentPlayerAnimator(EntityPiece entity)
    {
        if (currentAnimator != null)
            ResetToIdleAnim(null);
        currentAnimator = entity.GetComponentInChildren<Animator>();
    }

    private void DiceIsRolling(EntityPiece entity)
    {
        // currentAnimator = entity.GetComponentInChildren<Animator>();

        if (currentAnimator != null)
            currentAnimator.SetBool("Dice Rolling", true);
    }

    private void DiceIsThrown(int roll)
    {
        // currentAnimator = entity.GetComponentInChildren<Animator>();

        if (currentAnimator != null)
            currentAnimator.SetBool("Dice Thrown", true);

        // Get current animation 
        var state = currentAnimator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"Current Animation State Length:{state.length}");

        // Send in the duration of the dice throw animation to eventually reset to idle animation
        StartCoroutine(ToBoingAnimAfter(state.length)); // placeholder, change later
    }

    private IEnumerator ToBoingAnimAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        ResetToAnim(null, "ToBoing");
    }

    private void ResetToAnim(EntityPiece entity, string trigger)
    {
        // currentAnimator = entity.GetComponentInChildren<Animator>();

        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger(trigger);
            currentAnimator.SetBool("Dice Rolling", false);
            currentAnimator.SetBool("Dice Thrown", false);
        }
    }

    private void ResetToIdleAnim(EntityPiece entity)
    {
        // currentAnimator = entity.GetComponentInChildren<Animator>();

        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger("ToIdle");
            currentAnimator.SetBool("Dice Rolling", false);
            currentAnimator.SetBool("Dice Thrown", false);
        }
    }

    // Testing Functions
    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.I))
        {
            m_DiceRolling.RaiseEvent(ep);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            m_DiceThrown.RaiseEvent(ep);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            m_ResetToIdle.RaiseEvent(ep);
        }
        */
    }
}
