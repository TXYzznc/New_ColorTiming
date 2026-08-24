using UnityEngine;
using UnityEngine.UI;
using System;
using ColorTiming.Presentation.UI;

public class U_HeroHit : MonoBehaviour, IPlayerDamageSignalConsumer
{
    Animation Animation;
    Animator animator;
    IPlayerDamageSignal damageSignal;

    public void BindPlayerDamageSignal(IPlayerDamageSignal signal)
    {
        if (damageSignal != null) damageSignal.Damaged -= ShowHit;
        damageSignal = signal ?? throw new ArgumentNullException(nameof(signal));
        damageSignal.Damaged += ShowHit;
    }
    void Start()
    {
        Animation = GetComponent<Animation>();
        animator = GetComponent<Animator>();
        Invoke("Fake",0.5f);
    }

    private void ShowHit()
    {
        //Animation?.Play();
        animator.SetTrigger("hit");
    }

    void Fake()
    {
        Image image = GetComponent<Image>();
        if (image)
        {
            image.color = Color.white;
        }
    }

    private void OnDestroy()
    {
        if (damageSignal != null) damageSignal.Damaged -= ShowHit;
        damageSignal = null;
    }
}
