using System;
using UnityEngine;

public class RuntimeInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeSceneLoad()
    {
        // コマンドライン引数解析
        //var args = Environment.GetCommandLineArgs();
        //for (var i = 0; i < args.Length; i++)
        //{
        //    //Debug.Log($"args[{i.ToString()}]: {args[i]}");
        //    if (args[i].Contains("debug"))
        //    {
        //        // デバッグモードON
        //        DebugWindow.Instance.DebugMode = true;
        //    }
        //}

        // アプリケーション設定ファイル読み込み
        // 本番環境では json ファイルは使用せず(配置もしない)、encファイルを読み込む
        //GlobalState.Instance.ApplicationGlobalSettings = FileHelper.LoadConfigFileSync<ApplicationSettings>(ApplicationSettings.ApplicationSettingFilePath);
    }
}
