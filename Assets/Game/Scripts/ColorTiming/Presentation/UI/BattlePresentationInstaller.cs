using System;
using System.Collections;
using System.Linq;
using ColorTiming.Bootstrap.Flow;
using UnityEngine;

namespace ColorTiming.Presentation.UI
{
    /// <summary>Runtime-only scene bridge that composes battle presentation after actor startup.</summary>
    public sealed class BattlePresentationInstaller : MonoBehaviour, IBattleResultSink
    {
        private IColorTimingUiService uiService;
        private IColorTimingSceneFlow sceneFlow;
        private Coroutine pendingBossTransition;

        public void Initialize(IColorTimingUiService service, IColorTimingSceneFlow flow)
        {
            uiService = service ?? throw new ArgumentNullException(nameof(service));
            sceneFlow = flow ?? throw new ArgumentNullException(nameof(flow));
        }

        private IEnumerator Start()
        {
            yield return null;
            var behaviours = gameObject.scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true)).ToArray();
            var heroes = behaviours.OfType<HeroController>().ToArray();
            var boss1Controllers = behaviours.OfType<Boss1_Controller>().ToArray();
            var boss2Controllers = behaviours.OfType<Boss2_Controller>().ToArray();
            if (heroes.Length != 1 || (boss1Controllers.Length == 1) == (boss2Controllers.Length == 1))
            {
                Debug.LogError("Battle scene requires one Hero and exactly one supported Boss controller.", this);
                yield break;
            }

            var hero = heroes[0];
            var boss1 = boss1Controllers.SingleOrDefault();
            var boss2 = boss2Controllers.SingleOrDefault();

            uiService.ShowBattleHud(new BattleHudPresentation(hero, boss1, boss2));
            uiService.ShowBattleTutorial(hero);
        }

        public void Show(BattlePresentationResult result)
        {
            if (result == BattlePresentationResult.Boss1Defeated)
            {
                if (pendingBossTransition == null) pendingBossTransition = StartCoroutine(LoadBoss2AfterDelay());
                return;
            }

            uiService.ShowBattleResult(result);
        }

        private IEnumerator LoadBoss2AfterDelay()
        {
            yield return new WaitForSecondsRealtime(1f);
            pendingBossTransition = null;
            sceneFlow.TryLoad(ColorTimingSceneId.Boss2);
        }

        private void OnDestroy()
        {
            if (pendingBossTransition != null) StopCoroutine(pendingBossTransition);
        }
    }
}
