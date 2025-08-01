using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.IO;

public static class BuildScript
{
    // Versioned folder for WebGL Brotli build
    private static readonly string buildVersion = "webgl_1.1";
    private static readonly string buildPath = "Builds/BR/" + buildVersion;

    [MenuItem("Build/Build WebGL - BR (Production)")]
    public static void BuildWebGLBR()
    {
        Debug.Log("=== Starting WebGL Brotli Build ===");

        ApplyWebGLSettingsBR();
        PrepareBuildFolder(buildPath);

        string[] scenes = { "Assets/Scenes/MainScene.unity" }; // Update scene path if needed

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"=== WebGL Brotli Build Succeeded! Size: {report.summary.totalSize / 1024 / 1024} MB ===");
        else
            Debug.LogError("=== WebGL Brotli Build Failed ===");
    }

    private static void ApplyWebGLSettingsBR()
    {
        // Brotli compression
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;

        // Force .br files instead of .unityweb
        PlayerSettings.WebGL.decompressionFallback = false;

        // Optimize for smallest wasm using LTO
        EditorUserBuildSettings.SetPlatformSettings("WebGL", "CodeOptimization", "size");
        PlayerSettings.stripEngineCode = true;

        Debug.Log("Applied Brotli + LTO settings (.br output)");
    }

    private static void PrepareBuildFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            Debug.Log("Old build folder deleted: " + path);
        }

        string parentDir = Path.GetDirectoryName(path);
        if (!Directory.Exists(parentDir))
            Directory.CreateDirectory(parentDir);
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
