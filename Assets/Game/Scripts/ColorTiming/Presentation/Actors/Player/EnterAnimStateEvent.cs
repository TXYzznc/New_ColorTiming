// 文件职责：兼容既有 AnimatorController 中序列化的动画状态回调，并转发给玩家表现层。
// 所属模块：ColorTiming / Presentation / Actors / Player。

using UnityEngine;

/// <summary>
/// 保留原项目类型名和脚本 GUID，避免既有 AnimatorController 中的状态行为引用失效。
/// </summary>
public sealed class EnterAnimStateEvent : StateMachineBehaviour
{
    private PlayerActorView _player;

    // 动画状态开始参与评估时，将进入通知转交给玩家表现对象。
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ResolvePlayer(animator)?.NotifyAnimationState(stateInfo, true);
    }

    // 动画状态结束评估时，将退出通知转交给玩家表现对象。
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ResolvePlayer(animator)?.NotifyAnimationState(stateInfo, false);
    }

    // 每个状态行为实例只解析一次父级玩家组件，避免状态切换时重复遍历层级。
    private PlayerActorView ResolvePlayer(Animator animator)
    {
        if (_player == null && animator != null)
        {
            _player = animator.GetComponentInParent<PlayerActorView>();
        }

        return _player;
    }
}
