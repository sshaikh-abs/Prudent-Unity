using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.IO;

public static class BuildScript
{
    private static readonly string localBuildPath = "Builds/Local/webgl_1.1";
    private static readonly string brBuildPath = "Builds/BR/webgl_1.1";

    // Local WebGL build (for CI / GitHub)
    public static void BuildWebGLLocal()
    {
        Debug.Log("=== Starting Local WebGL Build ===");
        PrepareBuildFolder(localBuildPath);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainScene.unity" },
            locationPathName = localBuildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.CleanBuildCache
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        LogBuildResult(report, "Local WebGL");
    }

    // Brotli WebGL build (manual / local only)
    public static void BuildWebGLBR()
    {
        Debug.Log("=== Starting Brotli WebGL Build ===");
        ApplyBrotliSettings();
        PrepareBuildFolder(brBuildPath);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainScene.unity" },
            locationPathName = brBuildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.CleanBuildCache
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        LogBuildResult(report, "Brotli WebGL");
    }

    private static void ApplyBrotliSettings()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.stripEngineCode = true;

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.connectProfiler = false;
        EditorUserBuildSettings.allowDebugging = false;

        Debug.Log("Applied Brotli settings for production build");
    }

    private static void PrepareBuildFolder(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);

        Directory.CreateDirectory(path);
        Debug.Log("Prepared build folder: " + path);
    }

    private static void LogBuildResult(BuildReport report, string buildType)
    {
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"=== {buildType} Build Succeeded! Size: {report.summary.totalSize / 1024 / 1024} MB ===");
        else
            Debug.LogError($"=== {buildType} Build Failed ===");
    }
}














//using UnityEditor;
//using UnityEngine;
//using System.Diagnostics;
//using System.IO;

//public class BuildScript
//{
//    private static readonly string buildVersion = "webgl_1.1";

//    [MenuItem("Build/Build WebGL - Local (Fast)")]
//    public static void BuildWebGLLocal()
//    {
//        ApplyWebGLSettingsLocal();

//        string buildPath = @$"C:\Unity Projects\abs-unity\Builds\Local\{buildVersion}";
//        Build(buildPath);

//        UnityEngine.Debug.Log("Local WebGL Build Completed");
//    }

//    [MenuItem("Build/Build WebGL - BR (Optimized)")]
//    public static void BuildWebGLBR()
//    {
//        ApplyWebGLSettingsBR();

//        string buildPath = @$"C:\Unity Projects\abs-unity\Builds\BR\{buildVersion}";
//        Build(buildPath);

//        UnityEngine.Debug.Log("BR WebGL Build Completed");

//        // Optional: Push to Git
//        Process.Start("bash", "Scripts/PushWebGLToGit.sh");
//    }

//    private static void Build(string buildPath)
//    {
//        if (Directory.Exists(buildPath))
//        {
//            Directory.Delete(buildPath, true);
//            UnityEngine.Debug.Log($"Old build folder deleted: {buildPath}");
//        }

//        string parentDir = Path.GetDirectoryName(buildPath);
//        if (!Directory.Exists(parentDir))
//            Directory.CreateDirectory(parentDir);

//        BuildPlayerOptions options = new BuildPlayerOptions
//        {
//            scenes = new[] { "Assets/Scenes/MainScene.unity" },// Change if needed
//            locationPathName = buildPath,
//            target = BuildTarget.WebGL,
//            options = BuildOptions.None
//        };

//        BuildPipeline.BuildPlayer(options);
//    }

//    private static void ApplyWebGLSettingsLocal()
//    {
//        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
//        SetIL2CPPCodeGeneration("FasterRuntime");
//        UnityEngine.Debug.Log("Applied Local Build Settings");
//    }

//    private static void ApplyWebGLSettingsBR()
//    {
//        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
//        SetIL2CPPCodeGeneration("OptimizeSizeWithLTO");
//        UnityEngine.Debug.Log("Applied BR Build Settings");
//    }

//    private static void SetIL2CPPCodeGeneration(string optionName)
//    {
//        var type = typeof(PlayerSettings).Assembly.GetType("UnityEditor.Il2CppCodeGenerationOptions");
//        if (type != null)
//        {
//            var value = System.Enum.Parse(type, optionName);
//            typeof(PlayerSettings).GetMethod("SetIl2CppCodeGeneration")?.Invoke(null, new[] { value });
//        }
//        else
//        {
//            UnityEngine.Debug.LogWarning("Il2CppCodeGenerationOptions not found.");
//        }
//    }
//}



//using UnityEditor;
//using UnityEngine;
//using System.Diagnostics;
//using System.IO;
//using System;

//public class BuildScript
//{
//    private static readonly string buildVersion = "webgl_1.1";

//    [MenuItem("Build/Build WebGL - Local (Fast)")]
//    public static void BuildWebGLLocal()
//    {
//        ApplyWebGLSettingsLocal();

//        string buildPath = @"C:\Unityprojects\abs-unity\Builds\Local\" + buildVersion;
//        Build(buildPath);

//        UnityEngine.Debug.Log("Local WebGL Build Completed");
//    }

//    [MenuItem("Build/Build WebGL - BR (Deployement)")]
//    public static void BuildWebGLBR()
//    {
//        ApplyWebGLSettingsBR();

//        string buildPath = @"C:\Unityprojects\abs-unity\Builds\BR\" + buildVersion;
//        Build(buildPath);

//        UnityEngine.Debug.Log("BR WebGL Build Completed");

//        // Optional Git push
//        Process.Start("bash", "Scripts/PushWebGLToGit.sh");
//    }

//    private static void Build(string buildPath)
//    {
//        if (Directory.Exists(buildPath))
//        {
//            Directory.Delete(buildPath, true);
//            UnityEngine.Debug.Log("Old build folder deleted: " + buildPath);
//        }

//        string parentDir = Path.GetDirectoryName(buildPath);
//        if (!Directory.Exists(parentDir))
//            Directory.CreateDirectory(parentDir);

//        BuildPlayerOptions options = new BuildPlayerOptions
//        {
//            scenes = new[] { "Assets/Scenes/MainScene.unity" }, // Update your actual scene path if needed
//            locationPathName = buildPath,
//            target = BuildTarget.WebGL,
//            options = BuildOptions.None
//        };

//        BuildPipeline.BuildPlayer(options);
//    }

//    private static void ApplyWebGLSettingsLocal()
//    {
//        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
//        SetIl2CppCodeGeneration("FasterRuntime"); // Shorter Build Time
//        UnityEngine.Debug.Log("Applied Local Build Settings");
//    }

//    private static void ApplyWebGLSettingsBR()
//    {
//        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
//        SetIl2CppCodeGeneration("OptimizeSizeWithLTO"); // Disk Size with LTO
//        UnityEngine.Debug.Log("Applied BR Build Settings");
//    }

//    private static void SetIl2CppCodeGeneration(string enumName)
//    {
//        var enumType = typeof(PlayerSettings).Assembly.GetType("UnityEditor.Il2CppCodeGenerationOption");
//        var method = typeof(PlayerSettings).GetMethod("SetIl2CppCodeGeneration", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

//        if (enumType != null && method != null)
//        {
//            var enumValue = Enum.Parse(enumType, enumName);
//            method.Invoke(null, new object[] { BuildTargetGroup.WebGL, enumValue });
//            UnityEngine.Debug.Log($"IL2CPP Code Generation set to: {enumName}");
//        }
//        else
//        {
//            UnityEngine.Debug.LogWarning("Unable to set IL2CPP Code Generation. Unity version may not support reflection access.");
//        }
//    }
//}
