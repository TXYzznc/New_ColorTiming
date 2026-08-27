using System;
using System.Collections;
using ColorTiming.Input;
using ColorTiming.Presentation.UI;
using UnityEngine;

/// <summary>
/// Scene-side bridge for explicitly authored battle actors. It does not own UI instances.
/// </summary>
public sealed class ColorTimingBattleHudBootstrap : MonoBehaviour, IColorTimingUiConsumer
{
    [SerializeField] private HeroController hero;
    [SerializeField] private Boss1_Controller boss1;
    [SerializeField] private Boss2_Controller boss2;

    private IColorTimingUiService uiService;

    public void BindUiService(IColorTimingUiService service)
    {
        uiService = service ?? throw new ArgumentNullException(nameof(service));
    }

    private IEnumerator Start()
    {
        // Actor Start callbacks establish their HP state before the GF.UI form reads it.
        yield return null;

        if (uiService == null)
        {
            Debug.LogError("Battle HUD context was not bound to the UI service.", this);
            yield break;
        }

        try
        {
            uiService.ShowBattleHud(new BattleHudPresentation(hero, boss1, boss2));
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }
}
