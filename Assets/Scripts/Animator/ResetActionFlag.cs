using UnityEngine;

public class ResetActionFlag : StateMachineBehaviour
{
    CharacterManager character;
    AIEarthGuardianBodyCombatManager earthGuardian;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(character == null)
        {
            character = animator.GetComponent<CharacterManager>();
            earthGuardian = animator.GetComponent<AIEarthGuardianBodyCombatManager>();
        }

        if(character.isDead.Value)
        {
            return;
        }

        character.applyRootMotion = false;
        character.isPerformingAction = false;
        character.canRotate = true;
        character.canMove = true;
        character.characterLocomotionManager.isRolling = false;
        character.characterAnimatorManager.DisableCanDoCombo();

        if (character.IsOwner)
        {
            character.characterNetworkManager.isJumping.Value = false;
            character.characterNetworkManager.isInvulnerable.Value = false;

        }




    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
