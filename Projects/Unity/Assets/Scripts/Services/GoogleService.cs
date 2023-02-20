﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

public static partial class GoogleService
{
    /// <summary>
    /// 対象言語
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

    /// <summary>
    /// 設定クラス
    /// </summary>
    public static GoogleServiceSettings Settings { get; private set; } = null;

    /// <summary>
    /// 認証ファイルデータ
    /// </summary>
    public static byte[] CredentialFileData { get; private set; } = null;

    // SpeechToText
    private static readonly string SpeechToTextRequestUrl = "https://speech.googleapis.com/v1/speech:recognize?key=";
    private static readonly int SpeechToTextSampleRateHertz = 16000;

    // TextToSpeech
    private static readonly string TextToSpeechRequestUrl = "https://texttospeech.googleapis.com/v1/text:synthesize?key=";
    private static readonly int TextToSpeechSampleRateHertz = 48000;
    private static readonly string TextToSpeechGender = "FEMALE";
    private static readonly string TextToSpeechAudioEncoding = "LINEAR16";
    //private static readonly string TextToSpeechAudioEncoding = "MP3";
    //private static readonly string TextToSpeechAudioEncoding = "OGG_OPUS";

    // Translation
    private static readonly string TranslationRequestUrl = "https://translation.googleapis.com/language/translate/v2?key=";

    /// <summary>
    /// 設定ファイル読み込み
    /// </summary>
    /// <returns></returns>
    public static void ImportSettings()
    {
#if false
        // 設定ファイル読み込み
        var settingFileAsset = AssetBundleManager.Instance.LoadTextAssetFromResourcePack("GoogleServiceSettings");
        if (settingFileAsset == null)
        {
            Debug.LogError("GoogleService setting file load error.");
            return;
        }
        var settingJson = settingFileAsset.text.Trim(new char[] { '\uFEFF' });
        Settings = JsonUtility.FromJson<GoogleServiceSettings>(settingJson);

        // サービスアカウントファイル読み込み
        var serviceAccountFileAsset = AssetBundleManager.Instance.ResourcePackAssetBundle.LoadAsset<TextAsset>(Path.GetFileNameWithoutExtension(Settings.ServiceAccountFilePath));
        if (serviceAccountFileAsset == null)
        {
            Debug.LogError("GoogleService service account file load error.");
            return;
        }
        CredentialFileData = serviceAccountFileAsset.bytes;
        //var serviceAccountJson = serviceAccountFileAsset.text.Trim(new char[] { '\uFEFF' });
        //File.WriteAllText(Settings.ServiceAccountFilePath, serviceAccountJson);
        //CredentialFilePath = Settings.ServiceAccountFilePath;
#else
        // オンメモリで値を設定(WebGL暫定対応)
        Settings = new GoogleServiceSettings()
        {
            ApiKey = "AIzaSyBh-rg5shUlThS07CqxUHRTqbRmE7CK-lQ",
            ServiceAccountFilePath = "google_service_account"
        };
        var serviceAccountFileAsset = Resources.Load<TextAsset>(Settings.ServiceAccountFilePath);
        if (serviceAccountFileAsset == null)
        {
            Debug.LogError("GoogleService service account file load error.");
            return;
        }
        CredentialFileData = serviceAccountFileAsset.bytes;
#endif
    }

    /// <summary>
    /// 音声認識(音声ファイル指定版)
    /// </summary>
    /// <param name="targetFilePath"></param>
    /// <returns></returns>
    public static async UniTask<string> SpeechToText(string targetFilePath, Language language)
    {
        // raw(waveヘッダ無し)データ読み込み
        return await SpeechToText(File.ReadAllBytes(targetFilePath), language);
    }

    /// <summary>
    /// 音声認識(AudioClip指定版)
    /// </summary>
    /// <param name="audioClip"></param>
    /// <returns></returns>
    public static async UniTask<string> SpeechToText(AudioClip audioClip, Language language)
    {
        return await SpeechToText(AudioConverter.ConvertToLinear16(audioClip), language);
    }

    /// <summary>
    /// バイト配列指定版
    /// </summary>
    /// <param name="audioData"></param>
    /// <returns></returns>
    public static async UniTask<string> SpeechToText(byte[] audioData, Language language)
    {
        var ret = new StringBuilder();

        // リクエストボディ作成
        var base64File = Convert.ToBase64String(audioData);
        var requestObj = new GoogleSpeechToTextRequest
        {
            Config = new GoogleSpeechToTextRequestConfig
            {
                Encoding = "LINEAR16",
                SampleRateHertz = SpeechToTextSampleRateHertz,
                LanguageCode = GetLanguageCodeForSpeechToText(language),
                EnableWordTimeOffsets = false
            },
            Audio = new GoogleSpeechToTextRequestAudio
            {
                Content = base64File
            }
        };
        var requestJson = JsonConvert.SerializeObject(requestObj);
        var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
        // リクエスト開始
        var response = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, SpeechToTextRequestUrl + Settings.ApiKey, null, requestBody);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            // 成功
            var responseJson = response.Json;
            //Debug.Log("GoogleService.SpeechToText request succeeded. response json: ");
            Debug.Log(responseJson);
            var responseJsonObj = JsonConvert.DeserializeObject<GoogleSpeechToTextResponse>(responseJson);
            if (responseJsonObj != null && responseJsonObj.Results != null && responseJsonObj.Results.Length > 0)
            {
                //var transcript = responseJsonObj.Results[0].Alternatives[0].Transcript;
                //ret.AppendLine(transcript);
                foreach (var result in responseJsonObj.Results)
                {
                    foreach (var alternative in result.Alternatives)
                    {
                        ret.AppendLine(alternative.Transcript);
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"GoogleService.SpeechToText request failed: {response.StatusCode}");
        }

        return ret.ToString();
    }

    /// <summary>
    /// 音声合成
    /// </summary>
    /// <param name="text"></param>
    /// <param name="outFilePath"></param>
    /// <returns></returns>
    public static async UniTask<byte[]> TextToSpeech(string text, Language language, string outFilePath = "")
    {
        // リクエストボディ作成
        var requestObj = new GoogleTextToSpeechRequest();
        requestObj.Input = new GoogleTextToSpeechRequestInput();
        requestObj.Input.Text = text;
        requestObj.Voice = new GoogleTextToSpeechRequestVoice();
        requestObj.Voice.LanguageCode = GetLanguageCodeForTextToSpeech(language);
        requestObj.Voice.Name = GetVoiceName(language);
        requestObj.Voice.SsmlGender = TextToSpeechGender;
        requestObj.AudioConfig = new GoogleTextToSpeechRequestAudioConfig();
        requestObj.AudioConfig.AudioEncoding = TextToSpeechAudioEncoding;
        requestObj.AudioConfig.SampleRateHertz = TextToSpeechSampleRateHertz;
        var requestJson = JsonConvert.SerializeObject(requestObj);
        var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
        // リクエスト開始
#if false
        var response = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, TextToSpeechRequestUrl + Settings.ApiKey, null, requestBody);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            // 成功
            var responseJson = response.Json;
            //Debug.Log("GoogleService.TextToSpeech request succeeded. response json: ");
            //Debug.Log(responseJson);
            var responseObj = JsonConvert.DeserializeObject<GoogleTextToSpeechResponse>(responseJson);
            var audioData = Convert.FromBase64String(responseObj.AudioContent);
            if (!string.IsNullOrEmpty(outFilePath))
            {
                // ファイル出力
                File.WriteAllBytes(outFilePath, audioData);
            }
            return audioData;
        }
        else
        {
            Debug.LogError($"GoogleService.TextToSpeech request failed: {response.StatusCode}");
        }
        return null;
#else
        var response = await WebServerManager.Instance.Request(TextToSpeechRequestUrl + Settings.ApiKey, "POST", requestJson);
        var responseObj = JsonConvert.DeserializeObject<GoogleTextToSpeechResponse>(response);
        var audioData = Convert.FromBase64String(responseObj.AudioContent);
        return audioData;
#endif
    }

    /// <summary>
    /// 翻訳
    /// </summary>
    /// <param name="text"></param>
    /// <param name="srcLanguage"></param>
    /// <param name="targetLanguage"></param>
    /// <returns></returns>
    public static async UniTask<string> Translation(string text, Language srcLanguage, Language targetLanguage)
    {
        var ret = new StringBuilder();

        // リクエストボディ作成
        var srcLanguageCode = GetLanguageCodeForTranslation(srcLanguage);
        var targetLanguageCode = GetLanguageCodeForTranslation(targetLanguage);
        var requestObj = new GoogleTranslationRequest
        {
            Q = text,
            Target = targetLanguageCode,
            Format = "text",
            Source = srcLanguageCode
        };
        //requestObj.Key = "";
        var requestJson = JsonConvert.SerializeObject(requestObj);
        var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
        // リクエスト開始
        var response = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, TranslationRequestUrl + Settings.ApiKey, null, requestBody);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            // 成功
            var responseJson = response.Json;
            //Debug.Log("GoogleService.Translation request succeeded. response json: ");
            //Debug.Log(responseJson);
            var responseObj = JsonConvert.DeserializeObject<GoogleTranslationResponse>(responseJson);
            if (responseObj != null && responseObj.Data != null && responseObj.Data.Translations != null)
            {
                foreach (var translation in responseObj.Data.Translations)
                {
                    ret.Append(translation.TranslatedText);
                }
            }
            else
            {
                Debug.LogError("GoogleService.Translation invalid response.");
            }
            return ret.ToString();
        }
        else
        {
            //  失敗
            Debug.LogError($"GoogleService.Translation request failed: {response.StatusCode}");
        }
        return null;
    }

    // 音声認識用言語コードを取得
    private static string GetLanguageCodeForSpeechToText(Language language)
    {
        var languageCode = "";
        switch (language)
        {
            case Language.Japanese:
                languageCode = "ja-JP";
                break;
            case Language.English:
                // 英語(アメリカ)
                languageCode = "en-US";
                break;
            case Language.Chinese:
                // 中国語(中国本土)
                languageCode = "zh";
                break;
            case Language.Russian:
                // ロシア語
                languageCode = "ru-RU";
                break;
            case Language.Arabic:
                // アラビア語
                languageCode = "ar-EG";
                break;
            case Language.Vietnamese:
                // ベトナム語
                languageCode = "vi-VN";
                break;
        }
        return languageCode;
    }

    // 音声合成用言語コードを取得
    // 中国語、アラビア語は未対応
    private static string GetLanguageCodeForTextToSpeech(Language language)
    {
        var languageCode = "";
        switch (language)
        {
            case Language.Japanese:
                languageCode = "ja-JP";
                break;
            case Language.English:
                // 英語(アメリカ)
                languageCode = "en-US";
                break;
            case Language.Chinese:
                // 中国語(中国本土)
                languageCode = "cmn-CN";
                break;
            case Language.Russian:
                // ロシア語
                languageCode = "ru-RU";
                break;
            case Language.Arabic:
                // アラビア語
                languageCode = "ar-XA";
                break;
            case Language.Vietnamese:
                languageCode = "vi-VN";
                break;
        }
        return languageCode;
    }

    // 言語コードを取得(翻訳API用)
    private static string GetLanguageCodeForTranslation(Language language)
    {
        var languageCode = "";
        switch (language)
        {
            case Language.Japanese:
                languageCode = "ja";
                break;
            case Language.English:
                languageCode = "en";
                break;
            case Language.Chinese:
                // 中国語(中国本土)
                languageCode = "zh-CN";
                break;
            case Language.Russian:
                // ロシア語
                languageCode = "ru";
                break;
            case Language.Arabic:
                // アラビア語
                languageCode = "ar";
                break;
            case Language.Vietnamese:
                // ベトナム語
                languageCode = "vi";
                break;
        }
        return languageCode;
    }

    // 音声合成用の音声名を取得
    private static string GetVoiceName(Language language)
    {
        var voiceName = "";
        switch (language)
        {
            case Language.Japanese:
                voiceName = "ja-JP-Standard-A";
                break;
            case Language.English:
                voiceName = "en-US-Standard-E";
                break;
            case Language.Chinese:
                voiceName = "cmn-CN-Standard-A";
                break;
            case Language.Russian:
                voiceName = "ru-RU-Standard-C";
                break;
            case Language.Arabic:
                voiceName = "ar-XA-Standard-A";
                break;
            case Language.Vietnamese:
                voiceName = "vi-VN-Standard-A";
                break;
            default:
                break;
        }
        return voiceName;
    }
}
