using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using static AudioManager;

/// <summary>
/// サイネージ設定管理クラス
/// </summary>
/// [Serializable]
public class SignageSettings
{
    /// <summary>
    /// 対応言語
    /// </summary>
    public enum Language
    {
        Japanese,
        English,
        Chinese,
        Russian,
        Arabic,
        Vietnamese
    }

    public enum ChatbotServices
    {
        Dialogflow,
        CAIWeb,
        CAIFile,
        CAIAsset
    }

    public enum ScreenSaverTypes
    {
        None,
        Loop,
        Movie,
    }

    public enum ImageAccessTypes
    {
        Local,
        UnityAsset,
        Web
    }

    // 言語と音声合成エンジン
    [Serializable]
    public class LanguageVoice
    {
        public string language;
        public string voice;
    }

    [Serializable]
    public class SignageSetting : ISerializationCallbackReceiver
    {
        [SerializeField] private string chatbot_service;
        [SerializeField] private LanguageVoice[] languages;
        [SerializeField] private string screensaver;        //スクリーンセーバータイプ
        [SerializeField] private int text_speed;            //テキスト表示ディレイ
        [SerializeField] private string image_access;       //イメージファイル格納場所
        [SerializeField] private int input_limit_time;      //入力待ち制限時間
        [SerializeField] private int restart_wait_time;     //TOPノード時のプログラム再起動待ち時間
        [SerializeField] private int return_wait_time;      //action return待ち時間
        [SerializeField] private int delay_time;            //action delay時間

        [NonSerialized] public ChatbotServices ChatbotService;
        [NonSerialized] public ScreenSaverTypes ScreenSaver;
        [NonSerialized] public Dictionary<Language, TextToSpeechEngine> LanguageVoiceMap = new Dictionary<Language, TextToSpeechEngine>();
        [NonSerialized] public int BaseTextSpeed;
        [NonSerialized] public ImageAccessTypes ImageAccessType;
        [NonSerialized] public int InputLimitTime;
        [NonSerialized] public int RestartWaitTime;
        [NonSerialized] public int ReturnWaitTime;
        [NonSerialized] public int DelayTime;

        public void OnAfterDeserialize()
        {
            ImageAccessType = String.IsNullOrEmpty(image_access)? ImageAccessTypes.UnityAsset : (ImageAccessTypes)Enum.Parse(typeof(ImageAccessTypes), image_access);
            BaseTextSpeed = text_speed < 0 ? 0 : text_speed;
            InputLimitTime = input_limit_time <= 0 ? 60 : input_limit_time;
            RestartWaitTime = restart_wait_time <= 0 ? 60 : restart_wait_time;
            ReturnWaitTime = return_wait_time <= 0 ? 6 : return_wait_time;
            DelayTime = delay_time <= 0 ? 5 : delay_time;

            // チャットボットシステム
            try
            {
                ChatbotService = (ChatbotServices)Enum.Parse(typeof(ChatbotServices), chatbot_service, true);
            }
            catch
            {
                ChatbotService = ChatbotServices.CAIWeb;
            }

            //スクリーンセーバー
            try
            {
                ScreenSaver = (ScreenSaverTypes)Enum.Parse(typeof(ScreenSaverTypes), screensaver, true);
            }
            catch
            {
                ScreenSaver = ScreenSaverTypes.None;
            }

            //音声サービス
            LanguageVoiceMap.Clear();
            foreach (var obj in languages)
            {
                try
                {
                    var key = (Language)Enum.Parse(typeof(Language), obj.language);
                    var val = (TextToSpeechEngine)Enum.Parse(typeof(TextToSpeechEngine), obj.voice);
                    LanguageVoiceMap.Add(key, val);
                }
                catch { }
            }
        }

        public void OnBeforeSerialize()
        {
            //きっと書き出すことは無い
        }

        public SignageSetting Clone()
        {
            return (SignageSetting)MemberwiseClone();
        }

    }

    public static SignageSetting Settings;

    /// <summary>
    /// 選択中の言語
    /// </summary>
    public static ReactiveProperty<Language> CurrentLanguage { get; set; }

    /// <summary>
    /// 選択中の言語を初期化
    /// </summary>
    public static void InitializeCurrentLanguage()
    {
        // デフォルト言語は SinageSettings で最初に記述されたものにする
        if (CurrentLanguage == null)
        {
            CurrentLanguage = new ReactiveProperty<Language>(Settings.LanguageVoiceMap.Keys.First());
        }
        else
        {
            CurrentLanguage.Value = Settings.LanguageVoiceMap.Keys.First();
        }
    }

    /// <summary>
    /// 設定読み込み
    /// </summary>
    public static void LoadSettings()
    {
        // オンメモリで値を設定(WebGL暫定対応)
        Settings = new SignageSetting()
        {
            LanguageVoiceMap = new Dictionary<Language, TextToSpeechEngine>()
            {
                { Language.Japanese, TextToSpeechEngine.Google },
                { Language.English, TextToSpeechEngine.Google },
            },
        };

        InitializeCurrentLanguage();
    }

    /// <summary>
    /// 指定音声サービスの利用チェック
    /// </summary>
    /// <param name="type">音声サービスタイプ</param>
    /// <returns></returns>
    public static bool IsActiveVoiceService(TextToSpeechEngine type)
    {
        foreach(var o in Settings.LanguageVoiceMap)
        
            if (o.Value == type)
                return true;
        
        return false;
    }
}
