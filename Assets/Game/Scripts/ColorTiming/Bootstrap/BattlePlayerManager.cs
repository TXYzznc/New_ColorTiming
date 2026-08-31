// 文件职责：创建、配置并释放当前战斗唯一的运行时 Player。
// 所属模块：ColorTiming / Bootstrap。

using System;
using System.Collections.Generic;
using Cinemachine;
using ColorTiming.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorTiming.Bootstrap
{
    /// <summary>由 BattleRuntimeContext 持有的非全局 Player 生命周期对象。</summary>
    public sealed class BattlePlayerManager : IDisposable
    {
        private PlayerActorView _player;
        private MonoBehaviour[] _runtimeBindings = Array.Empty<MonoBehaviour>();
        private CinemachineVirtualCamera _virtualCamera;
        private PlayerDeathSequenceView _deathSequence;

        public PlayerActorView Player => _player;
        public IReadOnlyList<MonoBehaviour> RuntimeBindings => _runtimeBindings;

        /// <summary>创建 Player(Clone)，并在首帧前完成所有场景侧引用配置。</summary>
        public PlayerActorView Spawn(BattleSceneAnchors anchors, PlayerCameraConfiguration cameraConfiguration)
        {
            if (anchors == null) throw new ArgumentNullException(nameof(anchors));
            if (_player != null) throw new InvalidOperationException("Battle Player is already spawned.");

            var setup = anchors.Player;
            _player = UnityEngine.Object.Instantiate(
                setup.Prefab,
                setup.SpawnPosition,
                Quaternion.identity);
            try
            {
                SceneManager.MoveGameObjectToScene(_player.gameObject, anchors.gameObject.scene);

                _player.ConfigureSceneReferences(setup.WeaponSpawner, setup.DeathSequence.gameObject);

                var cameraView = _player.GetComponent<PlayerCameraLifecycleView>();
                if (cameraView == null)
                {
                    throw new MissingComponentException("Player Prefab requires PlayerCameraLifecycleView.");
                }
                cameraView.Configure(setup.VirtualCamera, setup.CameraTarget,
                    cameraConfiguration.MinimumSize, cameraConfiguration.MaximumSize,
                    cameraConfiguration.DistanceRange, cameraConfiguration.StartDistance);

                var soundView = _player.GetComponentInChildren<PlayerSoundView>(true);
                if (soundView == null)
                {
                    throw new MissingComponentException("Player Prefab requires PlayerSoundView.");
                }
                _virtualCamera = setup.VirtualCamera;
                _virtualCamera.Follow = _player.transform;
                _deathSequence = setup.DeathSequence;
                _deathSequence.ConfigureHero(_player.transform);
                _runtimeBindings = _player.GetComponentsInChildren<MonoBehaviour>(true);
            }
            catch
            {
                Dispose();
                throw;
            }

            Debug.Log(
                $"[ColorTiming.Player] action=Spawn result=Success instance={_player.name} scene={anchors.gameObject.scene.name} bindings={_runtimeBindings.Length}",
                _player);
            return _player;
        }

        public void Dispose()
        {
            if (_virtualCamera != null && _player != null && _virtualCamera.Follow == _player.transform)
            {
                _virtualCamera.Follow = null;
            }
            _deathSequence?.ConfigureHero(null);

            if (_player != null)
            {
                if (UnityEngine.Application.isPlaying)
                    UnityEngine.Object.Destroy(_player.gameObject);
                else
                    UnityEngine.Object.DestroyImmediate(_player.gameObject);
                _player = null;
            }

            _runtimeBindings = Array.Empty<MonoBehaviour>();
            _virtualCamera = null;
            _deathSequence = null;
        }
    }
}
