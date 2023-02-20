using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

public class MyBuildPostprocessor
{
#if UNITY_EDITOR
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
#if false
        Debug.Log($"OnPostprocessBuild: {pathToBuiltProject}");

        // 空の設定ファイルを作成しておく
        var builtFolderPath = pathToBuiltProject.Substring(0, pathToBuiltProject.LastIndexOf("/") + 1);
        var applicationSettingsFilePath = builtFolderPath + "ApplicationSettings.json";
        var settingsJson = JsonUtility.ToJson(new ApplicationSettings());
        File.WriteAllText(applicationSettingsFilePath, settingsJson);

        // 実行時に必要なフォルダ＆ファイルをコピー
        var targetFolderName = "exe";
        FileUtil.CopyFileOrDirectory(FileHelper.GetCurrentDirectory() + targetFolderName, builtFolderPath + targetFolderName);
#endif
    }
#endif
    }
