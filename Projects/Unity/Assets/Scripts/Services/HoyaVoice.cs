using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HoyaVoice
{

    //フォーマット
    public enum formatSelect
    {
        wav,
        ogg,
        mp3
    }


    const string VoiceRequestUrl = "https://api.voicetext.jp/v1/tts";

    private static readonly string SettingFilePath = "HoyaServiceSettings.json";
    private static string _apiKey = "";
    private static string _speaker = "";
    private static int _pitch = 100;
    private static int _speed = 100;
    private static int _volume = 100;

    [Serializable]
    private class HoyaSettings : ISerializationCallbackReceiver
    {
        [NonSerialized] public string ApiKey;
        [NonSerialized] public string Speaker;
        [NonSerialized] public int Pitch;
        [NonSerialized] public int Speed;
        [NonSerialized] public int Volume;

        [SerializeField] private string api_key;
        [SerializeField] private string speaker;
        [SerializeField] private int pitch;
        [SerializeField] private int speed;
        [SerializeField] private int volume;

        public void OnBeforeSerialize()
        {
            api_key = ApiKey;
            speaker = Speaker;
            pitch = Pitch;
            speed = Speed;
            volume = Volume;
        }

        public void OnAfterDeserialize()
        {
            ApiKey = api_key;
            Speaker = speaker;
            Pitch = pitch;
            Speed = speed;
            Volume = volume;
        }
    }


    /// <summary>
    /// 音声データ取得
    /// </summary>
    /// <param name="text"></param>
    /// <param name="speaker"></param>
    /// <param name="pitch"></param>
    /// <param name="speed"></param>
    /// <param name="volume"></param>
    /// <returns></returns>
    static public async UniTask<byte[]> GetVoice(string text)
    {
        return await RequestVoice(GetApiKey(), text);
    }

    /// <summary>
    /// 入力値を指定範囲内に丸める
    /// </summary>
    /// <param name="maxValue"></param>
    /// <param name="minValue"></param>
    /// <param name="inputValue"></param>
    /// <returns></returns>
    static private int FixLimitValue(int maxValue, int minValue, int inputValue)
    {
        inputValue = inputValue > maxValue ? maxValue : inputValue;
        return inputValue < minValue ? minValue : inputValue;
    }

    static private async UniTask<byte[]> RequestVoice(string api, string text)
    {
        using (var client = new HttpClient())
        {
            //ヘッダ
            var credentials = Encoding.ASCII.GetBytes(api+ ":");
            var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
            client.DefaultRequestHeaders.Authorization = header;

            //ボディ
            var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "text", text },
                { "speaker", _speaker },
                { "pitch", _pitch.ToString() },
                { "speed", _speed.ToString() },
                { "volume", _volume.ToString() }

            });

            using (var response = await client.PostAsync(VoiceRequestUrl, body).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                using (var dataStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    return GetByteArrayFromStream(dataStream);
                }
            }
        }
    }

    /// <summary>
    /// ストリームからbyte配列抽出
    /// </summary>
    /// <param name="sm"></param>
    /// <returns></returns>
    public static byte[] GetByteArrayFromStream(Stream sm)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            sm.CopyTo(ms);
            return ms.ToArray();
        }
    }


    /// <summary>
    /// APIキー取得
    /// </summary>
    /// <returns></returns>
    private static string GetApiKey()
    {
        if (string.IsNullOrEmpty(_apiKey))
            ImportParameter();

        return _apiKey;

    }


    /// <summary>
    /// 設定取り込み
    /// </summary>
    private static void ImportParameter()
    {
        var settingFileAsset = AssetBundleManager.Instance.LoadTextAssetFromResourcePack(SettingFilePath);
        if (settingFileAsset == null)
        {
            Debug.LogError("Hoya setting file load error.");
            return;
        }
        var json = settingFileAsset.text.Trim(new char[] { '\uFEFF' });
        var settingObj = JsonUtility.FromJson<HoyaSettings>(json);

        _apiKey = settingObj.ApiKey;
        _speaker = settingObj.Speaker;
        _pitch = FixLimitValue(200, 50, settingObj.Pitch);
        _speed = FixLimitValue(400, 50, settingObj.Speed);
        _volume = FixLimitValue(200, 50, settingObj.Volume);
    }

}
