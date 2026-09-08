using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

namespace UnityCameraRecording.Tests
{
    public sealed class CameraRecordingTests
    {
        [UnitySetUp]
        public IEnumerator EnterRecordingMode()
        {
            yield return new EnterPlayMode();
        }

        [Test]
        public void RequiresPlayModeAndSupportsToolboxDiscovery()
        {
            Assert.IsNotNull(CameraRecordingSession.Validate(null, 1920, 1080, 30, "Recordings"));
            Assert.IsTrue(typeof(IToolHubPanel).IsAssignableFrom(typeof(CameraRecorderPanel)));
            Assert.IsTrue(System.Attribute.IsDefined(typeof(CameraRecorderPanel), typeof(ToolHubItemAttribute)));
            CameraRecordingSession.Stop();
            CameraRecordingSession.Stop();
        }

        [UnityTest]
        public IEnumerator EncodesAudioAndVideoAndRestoresStateOnStopAndPlayExit()
        {
            var go = new GameObject("CameraRecorder Test Camera");
            Assert.IsNotNull(go);
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.red;
            camera.cullingMask = 0;
            camera.transform.position = new Vector3(0, 0, -10);
            var data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = false;
            var audio = go.AddComponent<AudioSource>();
            var clip = AudioClip.Create("Recording test tone", 48000, 1, 48000, false);
            var samples = new float[48000];
            for (int i = 0; i < samples.Length; i++) samples[i] = 0.15f * Mathf.Sin(i * 2 * Mathf.PI * 440 / 48000);
            clip.SetData(samples, 0);
            audio.clip = clip;
            audio.loop = true;
            audio.spatialBlend = 0;
            if (Object.FindObjectOfType<AudioListener>() == null) go.AddComponent<AudioListener>();
            audio.Play();
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../outputs/camera-recorder-tests"));
            Assert.IsNotNull(CameraRecordingSession.Validate(camera, 321, 240, 30, folder));
            Assert.IsNotNull(CameraRecordingSession.Validate(camera, 320, 240, 0, folder));
            float delta = Time.captureDeltaTime;
            bool background = Application.runInBackground;
            int targetRate = Application.targetFrameRate;
            float aspect = camera.aspect;
            var originalTarget = camera.targetTexture;
            try
            {
                CameraRecordingSession.Start(camera, 320, 240, 30, folder, false, true);
                Assert.IsTrue(CameraRecordingSession.IsRecording);
                Assert.Throws<System.InvalidOperationException>(() => CameraRecordingSession.Start(camera, 320, 240, 30, folder, false, true));
                // A visible color transition verifies that frames continue updating.
                for (int i = 0; i < 30; i++) yield return null;
                camera.backgroundColor = Color.blue;
                for (int i = 0; i < 30; i++) yield return null;
                string file = CameraRecordingSession.LastFile;
                CameraRecordingSession.Stop();
                CameraRecordingSession.Stop();
                Assert.IsFalse(CameraRecordingSession.IsRecording);
                Assert.IsNull(CameraRecordingSession.Preview);
                Assert.AreSame(originalTarget, camera.targetTexture);
                Assert.AreEqual(aspect, camera.aspect);
                Assert.AreEqual(delta, Time.captureDeltaTime);
                Assert.AreEqual(background, Application.runInBackground);
                Assert.AreEqual(targetRate, Application.targetFrameRate);
                Assert.IsTrue(File.Exists(file));
                Assert.Greater(new FileInfo(file).Length, 1024);
                File.WriteAllText(Path.Combine(folder, "manual-stop-path.txt"), file);

                CameraRecordingSession.Start(camera, 3840, 2160, 30, folder, false, true);
                for (int i = 0; i < 4; i++) yield return null;
                string fourKFile = CameraRecordingSession.LastFile;
                CameraRecordingSession.Stop();
                Assert.IsTrue(File.Exists(fourKFile));
                Assert.Greater(new FileInfo(fourKFile).Length, 1024);

                CameraRecordingSession.Start(camera, 320, 240, 30, folder, false, true);
                for (int i = 0; i < 4; i++) yield return null;
                camera.enabled = false;
                double timeout = UnityEditor.EditorApplication.timeSinceStartup + 5;
                while (CameraRecordingSession.IsRecording && UnityEditor.EditorApplication.timeSinceStartup < timeout)
                    yield return null;
                Assert.IsFalse(CameraRecordingSession.IsRecording, "Disabling source camera must stop capture.");
                Assert.IsNull(CameraRecordingSession.Preview);
                camera.enabled = true;

                CameraRecordingSession.Start(camera, 1920, 1080, 60, folder, false, true);
                for (int i = 0; i < 12; i++) yield return null;
                SessionState.SetString("CameraRecorder.TestExitPath", CameraRecordingSession.LastFile);
            }
            finally
            {
                // Keep the second recording alive for the Play Mode exit cleanup assertion.
                audio.Stop();
                Object.Destroy(clip);
            }
            yield return new ExitPlayMode();
            Assert.IsFalse(CameraRecordingSession.IsRecording);
            Assert.IsNull(CameraRecordingSession.Preview);
            string exitFile = SessionState.GetString("CameraRecorder.TestExitPath", "");
            Assert.IsTrue(File.Exists(exitFile));
            Assert.Greater(new FileInfo(exitFile).Length, 1024);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            CameraRecordingSession.Stop();
            if (Application.isPlaying) yield return new ExitPlayMode();
        }
    }
}
