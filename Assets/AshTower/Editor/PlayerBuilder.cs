using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace AshTower.Editor
{
    public static class PlayerBuilder
    {
        const string BootScene = "Assets/Scenes/Boot.unity";
        const string Output = "Build/AshTower.exe";

        [MenuItem("Ash Tower/Build Windows 64")]
        public static void BuildWindowsMenu() => BuildWindows();

        public static void BuildWindows()
        {
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;

            string scene = EnsureBootScene();
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { scene },
                locationPathName = Output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CompressWithLz4
            };
            var report = BuildPipeline.BuildPlayer(opts);
            bool ok = report.summary.result == BuildResult.Succeeded;
            if (ok)
                Debug.Log("Ash Tower Windows build succeeded: " + Output);
            else
                Debug.LogError("Ash Tower Windows build failed: " + report.summary.result);
            if (Application.isBatchMode)
                EditorApplication.Exit(ok ? 0 : 1);
        }

        static string EnsureBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.09f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.AddComponent<AudioListener>();
            EditorSceneManager.SaveScene(scene, BootScene);
            return BootScene;
        }
    }
}
