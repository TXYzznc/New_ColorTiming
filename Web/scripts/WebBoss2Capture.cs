#if UNITY_EDITOR
// Temporary Editor-only recording helper. Copy into Assets/Game/Scripts/ColorTiming to run (UNITY_EDITOR only);
// remove that copy after capture. Gameplay input uses the project's IGameInput boundary.
using System;
using System.Collections;
using System.IO;
using ColorTiming.Input;
using ColorTiming.Infrastructure.GF.Settings;
using ColorTiming.Presentation.UI.Forms;
using UnityEditor;
using UnityEditor.Media;
using UnityEngine;

namespace ColorTiming.Editor.Migration
{
    public sealed class WebBoss2Capture : MonoBehaviour, IGameplayPointerWorld
    {
        const int Fps = 30;
        const int Frames = 900;
        readonly FakeGameInput input = new FakeGameInput();
        readonly WaitForEndOfFrame endFrame = new WaitForEndOfFrame();
        PlayerActorView hero;
        Boss2ActorView boss;
        WeaponPickupView[] pickups = Array.Empty<WeaponPickupView>();
        MediaEncoder encoder;
        RenderTexture screen;
        RenderTexture scaled;
        Texture2D pixels;
        string output;
        bool oldTips;
        bool settingsBound;
        float oldCapture;
        bool oldBackground;
        bool capturing;
        float elapsed;
        float nextScan;
        int frame;
        Vector2 target;
        Vector2 arenaCenter;
        GfColorTimingSettings settings;

        [MenuItem("Game Framework/GameTools/ColorTiming/Web Capture/Record Boss2")]
        public static void Begin()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
            if (FindObjectOfType<WebBoss2Capture>() != null) return;
            var go = new GameObject("Temporary website capture");
            DontDestroyOnLoad(go);
            go.AddComponent<WebBoss2Capture>();
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }

        IEnumerator Start()
        {
            output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../Web/work/boss2-capture"));
            Directory.CreateDirectory(output);
            oldCapture = Time.captureDeltaTime;
            oldBackground = UnityEngine.Application.runInBackground;
            UnityEngine.Application.runInBackground = true;
            var deadline = Time.realtimeSinceStartup + 45;
            MainMenuForm menu = null;
            while (menu == null && Time.realtimeSinceStartup < deadline)
            { menu = FindObjectOfType<MainMenuForm>(); yield return null; }
            if (menu == null) { Status("Failed: main menu unavailable"); Destroy(gameObject); yield break; }
            settings = new GfColorTimingSettings();
            oldTips = settings.KeyTipsDisabled;
            settingsBound = true;
            settings.KeyTipsDisabled = true;
            menu.GoTest2();
            deadline = Time.realtimeSinceStartup + 40;
            while ((hero == null || boss == null) && Time.realtimeSinceStartup < deadline)
            { hero = FindObjectOfType<PlayerActorView>(); boss = FindObjectOfType<Boss2ActorView>(); yield return null; }
            if (hero == null || boss == null) { Status("Failed: Boss2 unavailable"); Destroy(gameObject); yield break; }
            yield return new WaitForSeconds(1.5f);
            arenaCenter = hero.transform.position;
            hero.BindGameInput(input);
            hero.BindGameplayPointer(this);
            var emitter = hero.GetComponent<PlayerSkillEmitter>();
            emitter.BindGameInput(input);
            emitter.BindGameplayPointer(this);
            screen = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            scaled = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32);
            pixels = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            encoder = new MediaEncoder(Path.Combine(output, "boss2-raw.mp4"), new VideoTrackAttributes
            { width = 1280, height = 720, frameRate = new MediaRational(Fps), includeAlpha = false });
            Time.captureDeltaTime = 1f / Fps;
            capturing = true;
            Status("Recording");
            while (frame < Frames && hero != null && boss != null)
            {
                yield return endFrame;
                ScreenCapture.CaptureScreenshotIntoRenderTexture(screen);
                Graphics.Blit(screen, scaled);
                var previous = RenderTexture.active;
                RenderTexture.active = scaled;
                pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0, false);
                pixels.Apply(false);
                RenderTexture.active = previous;
                encoder.AddFrame(pixels);
                if (frame % 150 == 0)
                {
                    File.WriteAllBytes(Path.Combine(output, "frame-" + frame.ToString("D4") + ".png"), pixels.EncodeToPNG());
                    Status("Recording frame " + frame);
                }
                frame++;
            }
            capturing = false;
            encoder.Dispose(); encoder = null;
            Status("Complete: " + frame + " frames; automated input; silent footage");
            EditorApplication.isPaused = true;
            Destroy(gameObject);
        }

        void Update()
        {
            if (!capturing || hero == null || boss == null) return;
            elapsed += Time.deltaTime;
            Vector2 heroPosition = hero.transform.position;
            Vector2 bossPosition = boss.transform.position;
            target = bossPosition;
            if (elapsed >= nextScan)
            { pickups = FindObjectsOfType<WeaponPickupView>(); nextScan = elapsed + 1f; }
            var destination = arenaCenter + new Vector2(Mathf.Cos(elapsed * .45f) * 5f, Mathf.Sin(elapsed * .45f) * 3f);
            if (hero.nowweapon.IsNormal)
            {
                float nearest = 25f;
                foreach (var pickup in pickups)
                {
                    if (pickup == null || !pickup.HasWeapon || !pickup.isActiveAndEnabled) continue;
                    Vector2 position = pickup.transform.position;
                    if ((position - arenaCenter).sqrMagnitude > 45f) continue;
                    float distance = (position - heroPosition).sqrMagnitude;
                    if (distance < nearest) { nearest = distance; destination = position; }
                }
            }
            var delta = destination - heroPosition;
            float bossDistance = Vector2.Distance(heroPosition, bossPosition);
            bool attacking = bossDistance < 9;
            var move = delta.magnitude > .5f ? delta.normalized : Vector2.zero;
            bool attack = attacking && frame % 36 == 0;
            bool dash = bossDistance < 4f && frame % 75 == 0;
            input.SetFrame(new GameInputFrame(move, dash, attack, attacking && frame % 36 < 10, false, false,
                Vector2.zero, false, false));
        }
        public Vector2 Resolve(Vector2 screenPosition) => target;
        void Status(string text) => File.WriteAllText(Path.Combine(output, "status.txt"), text);
        void OnDestroy()
        {
            encoder?.Dispose();
            Time.captureDeltaTime = oldCapture;
            UnityEngine.Application.runInBackground = oldBackground;
            if (settingsBound) settings.KeyTipsDisabled = oldTips;
            if (screen != null) { screen.Release(); Destroy(screen); }
            if (scaled != null) { scaled.Release(); Destroy(scaled); }
            if (pixels != null) Destroy(pixels);
        }
    }
}


#endif
