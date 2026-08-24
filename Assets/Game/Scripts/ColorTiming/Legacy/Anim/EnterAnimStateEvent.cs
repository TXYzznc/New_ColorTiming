using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterAnimStateEvent : StateMachineBehaviour
{
    // OnStateEnter在转换开始并且状态机开始评估此状态时调用
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HeroController heroController = animator.GetComponentInParent<HeroController>();
        if (heroController != null)
        {
            //animator.GetCurrentAnimatorStateInfo()
            heroController.OnAnimStateEnter?.Invoke(stateInfo,true);
        }
    }

    // OnStateUpdate在OnStateEnter和OnStateExit回调之间的每个更新帧上调用
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // 当转换结束并且状态机完成对此状态的评估时，调用OnStateExit
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HeroController heroController = animator.GetComponentInParent<HeroController>();
        if (heroController != null)
        {
            //animator.GetCurrentAnimatorStateInfo()
            heroController.OnAnimStateEnter?.Invoke(stateInfo, false);
        }
    }

    // OnStateMove在Animator之后被调用。OnAnimatorMove（）
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK在Animator之后被调用。OnAnimatorIK（）
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
