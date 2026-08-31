// 文件职责：从 GF DataTable 构建并校验 ColorTiming 唯一业务配置源。
// 所属模块：ColorTiming / Configuration。

using System;
using System.Collections.Generic;
using System.Linq;
using ColorTiming.Bosses.Boss1;
using ColorTiming.Bosses.Boss2;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using GameFramework;
using GameFramework.DataTable;

namespace ColorTiming.Configuration
{
    public sealed class GfColorTimingConfiguration : IColorTimingConfiguration
    {
        private readonly Dictionary<ColorTimingSceneId, ColorTimingBattleTable> battles;
        private readonly Dictionary<int, ColorTimingPlayerTable> players;
        private readonly Dictionary<int, ColorTimingBossTable> bosses;
        private readonly Dictionary<int, ColorTimingBossAttackTable[]> bossAttacks;
        private readonly Dictionary<int, WeaponSpawnConfiguration> spawnRules;
        private readonly Dictionary<WeaponIdentity, ColorTimingWeaponTable> weapons;
        private readonly Dictionary<int, ColorTimingSkillTable> skills;
        private readonly Dictionary<string, ColorTimingSkillTable> skillsByEntity;
        private readonly Dictionary<string, ColorTimingSoundCueTable> soundCues;

        public GfColorTimingConfiguration()
        {
            var battleRows = RequiredRows<ColorTimingBattleTable>();
            var playerRows = RequiredRows<ColorTimingPlayerTable>();
            var bossRows = RequiredRows<ColorTimingBossTable>();
            var attackRows = RequiredRows<ColorTimingBossAttackTable>();
            var spawnRows = RequiredRows<ColorTimingWeaponSpawnRuleTable>();
            var weaponRows = RequiredRows<ColorTimingWeaponTable>();
            var skillRows = RequiredRows<ColorTimingSkillTable>();
            var cueRows = RequiredRows<ColorTimingSoundCueTable>();
            var presentationRows = RequiredRows<ColorTimingPresentationTable>();

            battles = Unique(battleRows, row => ParseScene(row.SceneId), "battle scene");
            players = Unique(playerRows, row => row.Id, "player id");
            bosses = Unique(bossRows, row => row.Id, "boss id");
            bossAttacks = attackRows.GroupBy(row => row.BossId).ToDictionary(group => group.Key, group => group.ToArray());
            weapons = Unique(weaponRows, row => ParseWeapon(row.Color, row.Type), "weapon identity");
            skills = Unique(skillRows, row => row.Id, "skill id");
            skillsByEntity = Unique(skillRows.Where(row => !string.IsNullOrWhiteSpace(row.EntityName)),
                row => row.EntityName, "skill entity name");
            soundCues = Unique(cueRows, row => RequiredText(row.CueId, "sound cue id"), "sound cue id");
            Presentation = presentationRows.SingleOrDefault(row => row.Id == 1)
                ?? throw new GameFrameworkException("ColorTimingPresentationTable requires row 1.");
            spawnRules = BuildSpawnRules(spawnRows);
            ValidateReferences();
        }

        public ColorTimingPresentationTable Presentation { get; }

        public ColorTimingBattleTable GetBattle(ColorTimingSceneId sceneId) => Required(battles, sceneId, "battle scene");
        public ColorTimingPlayerTable GetPlayer(int id) => Required(players, id, "player");
        public ColorTimingBossTable GetBoss(int id) => Required(bosses, id, "boss");
        public ColorTimingSkillTable GetSkill(int id) => Required(skills, id, "skill");
        public bool TryGetSkillByEntity(string entityName, out ColorTimingSkillTable skill) =>
            skillsByEntity.TryGetValue(entityName, out skill);
        public ColorTimingSoundCueTable GetSoundCue(string cueId) => Required(soundCues, cueId, "sound cue");
        public IReadOnlyList<ColorTimingSoundCueTable> GetSoundCues(string cuePrefix)
        {
            if (string.IsNullOrWhiteSpace(cuePrefix))
                throw new ArgumentException("Sound cue prefix is required.", nameof(cuePrefix));
            return soundCues.Values
                .Where(row => row.CueId.StartsWith(cuePrefix, StringComparison.Ordinal))
                .OrderBy(row => row.Id)
                .ToArray();
        }
        public WeaponSpawnConfiguration GetWeaponSpawnRule(int ruleId) => Required(spawnRules, ruleId, "weapon spawn rule");
        public ColorTimingWeaponTable GetWeapon(WeaponIdentity identity) => Required(weapons, identity, "weapon");

        public BattleRulesConfiguration CreateBattleRules(ColorTimingSceneId sceneId)
        {
            var battle = GetBattle(sceneId);
            var player = GetPlayer(battle.PlayerId);
            var boss = GetBoss(battle.BossId);
            return new BattleRulesConfiguration(
                ParseBattleKind(battle.BattleKind),
                new PlayerCombatRules(player.MaximumHealth, player.DamagePerHit, player.DashHeal, player.HitInvulnerability),
                new WeaknessComposition(boss.RedWeaknesses, boss.GreenWeaknesses, boss.PurpleWeaknesses,
                    boss.OrangeWeaknesses, boss.UpcomingLimit),
                boss.TailActivationRemaining);
        }

        public Boss1AttackRules CreateBoss1AttackRules(int bossId)
        {
            var rows = Required(bossAttacks, bossId, "boss attack rows");
            return new Boss1AttackRules(rows.Select(row => new WeightedBoss1Attack(
                ParseEnum<Boss1DistanceZone>(row.DistanceZone, "Boss1 distance zone"),
                ParseEnum<Boss1Attack>(row.AttackId, "Boss1 attack"),
                row.Weight,
                row.DisallowRepeat,
                row.FallbackAttackId == 0
                    ? ParseEnum<Boss1Attack>(row.AttackId, "Boss1 fallback attack")
                    : ParseEnum<Boss1Attack>(row.FallbackAttackId, "Boss1 fallback attack"))));
        }

        public Boss2ActionRules CreateBoss2ActionRules(int bossId)
        {
            var row = GetBoss(bossId);
            return new Boss2ActionRules(row.HeadFarDistance, row.HeadMeleeDistance, row.HeadBurrowWeight,
                row.TailFarDistance, row.TailMeleeWeight);
        }

        private void ValidateReferences()
        {
            foreach (var battle in battles.Values)
            {
                var kind = ParseBattleKind(battle.BattleKind);
                var boss = GetBoss(battle.BossId);
                if (ParseBattleKind(boss.BattleKind) != kind)
                    throw new GameFrameworkException($"Battle {battle.Id} and Boss {boss.Id} use different BattleKind values.");
                GetPlayer(battle.PlayerId);
                GetWeaponSpawnRule(battle.WeaponSpawnRuleId);
                if (kind == BattleKind.Boss1) CreateBoss1AttackRules(boss.Id);
                else CreateBoss2ActionRules(boss.Id);
                if (!string.IsNullOrWhiteSpace(battle.BgmCueId) && !soundCues.ContainsKey(battle.BgmCueId))
                    throw new GameFrameworkException($"Battle {battle.Id} references missing BGM cue '{battle.BgmCueId}'.");
            }

            foreach (var weapon in weapons.Values)
            {
                if (string.IsNullOrWhiteSpace(weapon.ControllerAsset))
                    throw new GameFrameworkException($"Weapon row {weapon.Id} has no controller asset.");
                if (weapon.SkillId != 0) GetSkill(weapon.SkillId);
            }
            foreach (var skill in skills.Values)
                if (!string.IsNullOrWhiteSpace(skill.SoundCueId) && !soundCues.ContainsKey(skill.SoundCueId))
                    throw new GameFrameworkException($"Skill {skill.Id} references missing sound cue '{skill.SoundCueId}'.");
        }

        private static Dictionary<int, WeaponSpawnConfiguration> BuildSpawnRules(ColorTimingWeaponSpawnRuleTable[] rows)
        {
            var result = new Dictionary<int, WeaponSpawnConfiguration>();
            foreach (var group in rows.GroupBy(row => row.RuleId))
            {
                var first = group.First();
                foreach (var row in group)
                {
                    if (!Approximately(row.SpawnInterval, first.SpawnInterval)
                        || row.ActiveLimit != first.ActiveLimit
                        || row.GuaranteeThreshold != first.GuaranteeThreshold
                        || !Approximately(row.MinimumAnchorDistance, first.MinimumAnchorDistance)
                        || row.TutorialDamageLimit != first.TutorialDamageLimit)
                        throw new GameFrameworkException($"Weapon spawn rule {group.Key} has inconsistent repeated fields.");
                }

                var allowed = group.Select(row => ParseWeapon(row.Color, row.Type)).Distinct().ToArray();
                result.Add(group.Key, new WeaponSpawnConfiguration(first.SpawnInterval, first.ActiveLimit,
                    first.GuaranteeThreshold, first.MinimumAnchorDistance, first.TutorialDamageLimit, allowed));
            }
            return result;
        }

        private static T[] RequiredRows<T>() where T : IDataRow
        {
            if (GFBuiltin.DataTable == null || !GFBuiltin.DataTable.HasDataTable<T>())
                throw new GameFrameworkException($"Required ColorTiming data table '{typeof(T).Name}' is not loaded.");
            var rows = GFBuiltin.DataTable.GetDataTable<T>().GetAllDataRows();
            if (rows == null || rows.Length == 0)
                throw new GameFrameworkException($"Required ColorTiming data table '{typeof(T).Name}' is empty.");
            return rows;
        }

        private static Dictionary<TKey, TRow> Unique<TRow, TKey>(IEnumerable<TRow> rows, Func<TRow, TKey> key,
            string label)
        {
            var result = new Dictionary<TKey, TRow>();
            foreach (var row in rows)
            {
                var value = key(row);
                if (!result.TryAdd(value, row))
                    throw new GameFrameworkException($"Duplicate ColorTiming {label}: '{value}'.");
            }
            return result;
        }

        private static TValue Required<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> source, TKey key, string label)
        {
            if (source.TryGetValue(key, out var value)) return value;
            throw new GameFrameworkException($"Missing ColorTiming {label}: '{key}'.");
        }

        private static string RequiredText(string value, string label)
        {
            if (!string.IsNullOrWhiteSpace(value)) return value;
            throw new GameFrameworkException($"ColorTiming {label} cannot be empty.");
        }

        private static WeaponIdentity ParseWeapon(int color, int type) =>
            new WeaponIdentity(ParseEnum<WeaponColor>(color, "weapon color"), ParseEnum<WeaponType>(type, "weapon type"));

        private static ColorTimingSceneId ParseScene(int value) => ParseEnum<ColorTimingSceneId>(value, "scene id");
        private static BattleKind ParseBattleKind(int value) => ParseEnum<BattleKind>(value, "battle kind");

        private static T ParseEnum<T>(int value, string label) where T : struct, Enum
        {
            if (Enum.IsDefined(typeof(T), value)) return (T)Enum.ToObject(typeof(T), value);
            throw new GameFrameworkException($"Invalid ColorTiming {label} value '{value}'.");
        }

        private static bool Approximately(float left, float right) => Math.Abs(left - right) <= 0.0001f;
    }
}
