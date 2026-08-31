using System;
using System.Collections.Generic;
using System.Reflection;
using Cinemachine;
using ColorTiming.Application.Battle;
using ColorTiming.Bootstrap;
using ColorTiming.Combat;
using ColorTiming.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace ColorTiming.Tests.EditMode
{
    public sealed class BattleSceneAnchorsTests
    {
        private readonly List<UnityEngine.Object> _owned = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            for (var i = _owned.Count - 1; i >= 0; i--)
            {
                if (_owned[i] != null) UnityEngine.Object.DestroyImmediate(_owned[i]);
            }
            _owned.Clear();
        }

        [Test]
        public void Validate_AcceptsRuntimePlayerSetupAndOneMatchingBossRole()
        {
            var anchor = CreateAnchor(BattleKind.Boss1, 1);
            Assert.DoesNotThrow(() => anchor.Validate(BattleKind.Boss1));
        }

        [TestCase(0)]
        [TestCase(2)]
        public void Validate_RejectsMissingOrDuplicateBossRoles(int bossCount)
        {
            var anchor = CreateAnchor(BattleKind.Boss1, bossCount);
            Assert.Throws<InvalidOperationException>(() => anchor.Validate(BattleKind.Boss1));
        }

        [Test]
        public void Validate_RejectsMismatchedBattleKind()
        {
            var anchor = CreateAnchor(BattleKind.Boss2, 1);
            Assert.Throws<InvalidOperationException>(() => anchor.Validate(BattleKind.Boss1));
        }

        [Test]
        public void PlayerManager_SpawnsCloneConfiguresSceneReferencesAndDisposesIt()
        {
            var anchor = CreateAnchor(BattleKind.Boss1, 1);
            var setup = anchor.Player;
            var manager = new BattlePlayerManager();

            var player = manager.Spawn(anchor, new PlayerCameraConfiguration(8f, 12f, 5f, 5f));

            Assert.That(player.name, Is.EqualTo("Player(Clone)"));
            Assert.That(setup.VirtualCamera.Follow, Is.SameAs(player.transform));
            Assert.That(setup.DeathSequence.hero, Is.SameAs(player.transform));
            Assert.That(manager.RuntimeBindings, Is.Not.Empty);

            manager.Dispose();
            Assert.That(setup.VirtualCamera.Follow, Is.Null);
            Assert.That(setup.DeathSequence.hero, Is.Null);
            Assert.That(manager.Player, Is.Null);
        }

        private BattleSceneAnchors CreateAnchor(BattleKind bossKind, int bossCount)
        {
            var root = Own(new GameObject("BattleSceneAnchorsTests"));
            var anchor = root.AddComponent<BattleSceneAnchors>();
            var camera = root.AddComponent<Camera>();
            var weaponSpawner = root.AddComponent<WeaponSpawnerView>();
            var virtualCamera = root.AddComponent<CinemachineVirtualCamera>();
            var deathSequence = root.AddComponent<PlayerDeathSequenceView>();

            var prefabRoot = Own(new GameObject("Player"));
            var playerPrefab = prefabRoot.AddComponent<PlayerActorView>();
            prefabRoot.AddComponent<PlayerCameraLifecycleView>();
            prefabRoot.AddComponent<PlayerSoundView>();

            var setup = new BattleSceneAnchors.PlayerSetup();
            SetField(setup, "_prefab", playerPrefab);
            SetField(setup, "_spawnPosition", new Vector3(2f, 3f, 0f));
            SetField(setup, "_weaponSpawner", weaponSpawner);
            SetField(setup, "_virtualCamera", virtualCamera);
            SetField(setup, "_cameraTarget", root.transform);
            SetField(setup, "_deathSequence", deathSequence);

            var bindings = new MonoBehaviour[bossCount];
            for (var i = 0; i < bossCount; i++)
            {
                bindings[i] = bossKind == BattleKind.Boss1
                    ? (MonoBehaviour)root.AddComponent<Boss1ActorView>()
                    : root.AddComponent<Boss2ActorView>();
            }

            SetField(anchor, "player", setup);
            SetField(anchor, "gameplayCamera", camera);
            SetField(anchor, "explicitBindings", bindings);
            return anchor;
        }

        private T Own<T>(T value) where T : UnityEngine.Object
        {
            _owned.Add(value);
            return value;
        }

        private static void SetField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            var field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                        ?? throw new MissingFieldException(typeof(TTarget).Name, fieldName);
            field.SetValue(target, value);
        }
    }
}
