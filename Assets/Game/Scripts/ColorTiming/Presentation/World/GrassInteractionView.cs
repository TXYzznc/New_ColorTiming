// 文件职责：负责 GrassInteraction 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / World。

using System.Collections.Generic;
using UnityEngine;
using System;
using ColorTiming.Presentation.Audio;

public sealed class GrassInteractionView : MonoBehaviour, IColorTimingSoundConsumer
{

    Animator animator;
    bool hasTriggerParameter;
    IColorTimingSoundService soundService;

    // 绑定音效Service依赖或事件监听。
    public void BindSoundService(IColorTimingSoundService service)
    {
        soundService = service ?? throw new ArgumentNullException(nameof(service));
    }

    public List<AudioClip> audioClips;

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.name == "Trigger"
                    && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    hasTriggerParameter = true;
                    break;
                }
            }
        }
    }

    // 响应TriggerEnter2D回调，并更新本对象状态。
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if ("Player" == collision.gameObject.tag )
        //{
        //    animator?.SetTrigger("Trigger");
        //}
        if (hasTriggerParameter)
        {
            animator.SetTrigger("Trigger");
        }

        if ("Player" == collision.gameObject.tag)
        {
            PlayerSoundView heroSoundManager = collision.gameObject.GetComponentInChildren<PlayerSoundView>();
            heroSoundManager?.AddOverwriteMoveCase(true, gameObject.name);

            if (audioClips.Count > 0)
            {
                List<AudioClip> clips = FunctionLibrary.RandomSort(audioClips);
                soundService?.Play(clips[0], ColorTimingSoundChannel.Environment, transform.position);
            }



            //animator?.SetTrigger("Trigger"); 之前是 只有玩家进入草丛才会触发晃动
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        if ("Player" == collision.gameObject.tag)
        {
            PlayerSoundView heroSoundManager = collision.gameObject.GetComponentInChildren<PlayerSoundView>();
            heroSoundManager?.AddOverwriteMoveCase(false, gameObject.name);
            //animator?.SetTrigger("Trigger"); 之前是 只有玩家进入草丛才会触发晃动
        }
    }
}
