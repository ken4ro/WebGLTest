using System;

using UnityEngine;

public class RuntimeInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeSceneLoad()
    {
        // コマンドライン引数解析
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
        {
            //Debug.Log($"args[{i.ToString()}]: {args[i]}");
            if (args[i].Contains("debug"))
            {
                // デバッグモードON
                DebugWindow.Instance.DebugMode = true;
            }
        }

#if false
        // アプリケーション設定ファイル読み込み
        // 本番環境では json ファイルは使用せず(配置もしない)、encファイルを読み込む
        GlobalState.Instance.ApplicationGlobalSettings = FileHelper.LoadConfigFileSync<ApplicationSettings>(ApplicationSettings.ApplicationSettingFilePath);
#else
        // オンメモリで値を設定(WebGL暫定対応)
        GlobalState.Instance.ApplicationGlobalSettings = new ApplicationSettings()
        {
            StartOffsetSec = 2,
            CompanyCode = "nttcom_openhub",
            FileServer = "https://xrccg.com:3000",
            SignalingServer = "wss://tlx.xrccg.com:3001",
            SignalingUserName = "user01",
            SignalingUserPassword = "password",
            ConnectionType = "local",
            WebRtcApiKey = "2f691d96-67af-4841-bb87-b0167b88d751",
            BotRequestMethod = "button",
            FontSize = 48,
            AcceptableAnimation = "simple",
            AudioCodec = ApplicationSettings.AudioCodecType.G722,
            VideoCodec = ApplicationSettings.VideoCodecType.H264,
            VideoResolution = "360p"
        };
#endif

        // アプリケーション終了時コールバック
        Application.quitting += ApplicationQuit;
    }

    private static void ApplicationQuit()
    {
    }
}
