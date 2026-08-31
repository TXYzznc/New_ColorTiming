// 文件职责：声明业务配置仓库及显式注入合同。
// 所属模块：ColorTiming / Configuration。

using System.Collections.Generic;
using ColorTiming.Bosses.Boss1;
using ColorTiming.Bosses.Boss2;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using ColorTiming.Player;

namespace ColorTiming.Configuration
{
    public interface IColorTimingConfiguration
    {
        ColorTimingBattleTable GetBattle(ColorTimingSceneId sceneId);
        ColorTimingPlayerTable GetPlayer(int id);
        ColorTimingBossTable GetBoss(int id);
        ColorTimingPresentationTable Presentation { get; }
        BattleRulesConfiguration CreateBattleRules(ColorTimingSceneId sceneId);
        Boss1AttackRules CreateBoss1AttackRules(int bossId);
        Boss2ActionRules CreateBoss2ActionRules(int bossId);
        WeaponSpawnConfiguration GetWeaponSpawnRule(int ruleId);
        ColorTimingWeaponTable GetWeapon(WeaponIdentity identity);
        ColorTimingSkillTable GetSkill(int id);
        bool TryGetSkillByEntity(string entityName, out ColorTimingSkillTable skill);
        ColorTimingSoundCueTable GetSoundCue(string cueId);
        IReadOnlyList<ColorTimingSoundCueTable> GetSoundCues(string cuePrefix);
    }

    public interface IColorTimingConfigurationConsumer
    {
        void BindConfiguration(IColorTimingConfiguration configuration, ColorTimingSceneId sceneId);
    }

    public interface IColorTimingPresentationConfigurationConsumer
    {
        void BindPresentationConfiguration(ColorTimingPresentationTable configuration);
    }

    public interface IColorTimingSkillConfigurationConsumer
    {
        void BindSkillConfiguration(ColorTimingSkillTable configuration);
    }

    public sealed class WeaponSpawnConfiguration
    {
        public WeaponSpawnConfiguration(float interval, int activeLimit, int guaranteeThreshold,
            float minimumAnchorDistance, int tutorialDamageLimit, IReadOnlyList<WeaponIdentity> allowedWeapons)
        {
            SpawnInterval = interval;
            ActiveLimit = activeLimit;
            GuaranteeThreshold = guaranteeThreshold;
            MinimumAnchorDistance = minimumAnchorDistance;
            TutorialDamageLimit = tutorialDamageLimit;
            AllowedWeapons = allowedWeapons;
        }

        public float SpawnInterval { get; }
        public int ActiveLimit { get; }
        public int GuaranteeThreshold { get; }
        public float MinimumAnchorDistance { get; }
        public int TutorialDamageLimit { get; }
        public IReadOnlyList<WeaponIdentity> AllowedWeapons { get; }
        public WeaponSpawnPolicy CreatePolicy() => new WeaponSpawnPolicy(AllowedWeapons, ActiveLimit, GuaranteeThreshold);
    }

    public readonly struct PlayerCameraConfiguration
    {
        public PlayerCameraConfiguration(float minimumSize, float maximumSize, float distanceRange, float startDistance)
        {
            MinimumSize = minimumSize;
            MaximumSize = maximumSize;
            DistanceRange = distanceRange;
            StartDistance = startDistance;
        }

        public float MinimumSize { get; }
        public float MaximumSize { get; }
        public float DistanceRange { get; }
        public float StartDistance { get; }
    }
}
