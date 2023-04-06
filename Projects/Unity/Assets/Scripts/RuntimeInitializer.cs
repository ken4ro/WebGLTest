using System;
using System.Text;
using UnityEngine;
using static ApiServerManager;

public class RuntimeInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
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
        // オンメモリで値を設定(WebGL暫定対応)
        Settings = new SignageSetting()
        {
            BaseTextSpeed = 30,
            ChatbotService = ChatbotServices.CAIWeb,
            DelayTime = 5,
            ImageAccessType = ImageAccessTypes.UnityAsset,
            InputLimitTime = 60,
            LanguageVoiceMap = new Dictionary<Language, TextToSpeechEngine>()
            {
                { Language.Japanese, TextToSpeechEngine.Google },
                { Language.English, TextToSpeechEngine.Google },
            },
            RestartWaitTime = 60,
            ReturnWaitTime = 6,
            ScreenSaver = ScreenSaverTypes.None
        };
#endif

        // アプリケーション終了時コールバック
        Application.quitting += ApplicationQuit;
    }

    private static void ApplicationQuit()
    {
    }
}
