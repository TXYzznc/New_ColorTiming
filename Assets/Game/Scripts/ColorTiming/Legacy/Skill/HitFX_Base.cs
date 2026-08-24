using System;
using ColorTiming.Presentation.Entities;
using UnityEngine;

public class HitFX_Base : MonoBehaviour, IFrameworkEntityParticipant
{
    Action frameworkRelease;

    void OnFXEnd()
    {
        if (frameworkRelease != null)
        {
            frameworkRelease.Invoke();
        }
        else
        {
            Destroy(transform.parent.gameObject);
        }
    }

    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    public void OnFrameworkEntitySpawned() { }
    public void OnFrameworkEntityDespawned() { }
}
