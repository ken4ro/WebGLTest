using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class BuildCommand
{
    public static void PerformBuild()
    {
        Console.WriteLine("Auto Build Start!");

        // PlayerSettings 設定
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;

        // ビルドターゲット取得
        var buildTargetName = GetArgument("customBuildTarget");
        Console.WriteLine($"Auto Build Target: {buildTargetName}");
        if (Enum.TryParse(buildTargetName, out BuildTarget buildTarget))
        {
            // 出力先取得
            var buildPath = GetArgument("customBuildPath");
            Console.WriteLine($"Auto Build Path: {buildPath}");
            //EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            // ビルド対象のシーンを決定
            var buildScene = GetArgument("customBuildScene");
            string[] buildScenes = { buildScene };
            Console.WriteLine($"Audo Build Scenes: {buildScenes.ToString()}");
            // BuildPipeline.BuildPlayer を実行すると次回起動時も対象プラットフォームの指定が切り替わったままになってしまうので、実行前の対象プラットフォームを保持しておく
            var prevBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            Console.WriteLine($"Prev Build Target: {prevBuildTarget.ToString()}");
            // ビルド
            var buildResult = BuildPipeline.BuildPlayer(buildScenes, buildPath, buildTarget, BuildOptions.None);
            // 対象プラットフォームを戻す
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, prevBuildTarget);
            if (buildResult.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Console.WriteLine("Build Succeeded!");
            }
            else if (buildResult.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
            {
                Console.WriteLine("Build Failed!");
                // エラーを投げて終了
                EditorApplication.Exit(1);
            }
        }
        else
        {
            Console.WriteLine("Auto Build Parse Error");
            // エラーを投げて終了
            EditorApplication.Exit(1);
        }

        Console.WriteLine("Auto Build End!");
    }

    private static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(name))
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
