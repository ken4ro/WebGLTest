using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;


public class AzureVoice
{
    // enumの定義
    public enum VoiceSelect
    {
        Japanese,
        English,
        Chinese,
        Russian,
        Arabic,
    }

    const string CacheName = "\\azureVoice.txt";
    const string TokenRequestUrl = "https://eastasia.api.cognitive.microsoft.com/sts/v1.0/issueToken";
    const string VoiceRequestUrl = "https://eastasia.tts.speech.microsoft.com/cognitiveservices/v1";
    const int tokenExpireTime = 420;

    private static readonly string SettingFilePath = "MicrosoftServiceSettings.json";
    private static string _subscriptionKey = "";

    [Serializable]
    private class AzureSettings : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string SubscriptionKey;

        [SerializeField]
        private string subscription_key;

        public void OnBeforeSerialize()
        {
            subscription_key = SubscriptionKey;
        }

        public void OnAfterDeserialize()
        {
            SubscriptionKey = subscription_key;
        }
    }

    /// <summary>
    /// キャッシュパス取得
    /// </summary>
    /// <returns></returns>
    static private string GetCachePath()
    {

        
        string path = "";
        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            path += "/../../" + CacheName;
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            path += Path.GetFullPath(".") + @CacheName;
        }

        return path;

    }


    /// <summary>
    /// トークン取得
    /// </summary>
    /// <param name="subscriptionKey">サブスクリプションキー</param>
    /// <returns></returns>
    static private async UniTask<string> GetToken(string subscriptionKey)
    {
        string token = "";

        //ファイルを読み込む
        try
        {
            string buffer = "";
            System.IO.StreamReader file = new System.IO.StreamReader(GetCachePath());
            buffer = file.ReadToEnd();
            file.Close();

            //トークン期限確認
            var param = buffer.Split(',');
            if(param.Length == 2)
            {
                if(GetUnixTime() - int.Parse(param[0]) < tokenExpireTime)
                {
                    return param[1];
                }
            }

        }
        catch (Exception ex)
        {
            Debug.Log("Error loading token file " + ex );
        
        }

        //トークンを取得して保存
        token = await FetchTokenAsync(subscriptionKey);
        var unixTime = GetUnixTime();
        File.WriteAllText(GetCachePath(), unixTime.ToString() + "," + token);

        return token;
    }


    /// <summary>
    /// 現在時間をUNIXタイムで取得
    /// </summary>
    /// <returns></returns>
    static private long GetUnixTime()
    {
        DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        DateTime targetTime = DateTime.Now;
        targetTime = targetTime.ToUniversalTime();
        TimeSpan elapsedTime = targetTime - UNIX_EPOCH;
        return (long)elapsedTime.TotalSeconds;
    }


    /// <summary>
    /// トークン取得リクエスト
    /// </summary>
    /// <param name="subscriptionKey"></param>
    /// <returns></returns>
    static private async UniTask<string> FetchTokenAsync(string subscriptionKey)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            UriBuilder uriBuilder = new UriBuilder(TokenRequestUrl);

            var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null);
            return await result.Content.ReadAsStringAsync();
        }
    }


    
    static private async UniTask<byte[]> RequestVoice(string accessToken, string text, VoiceSelect voice)
    {
        using (var client = new HttpClient())
        {
            using (var request = new HttpRequestMessage())
            {
            
                string body = String.Format("<speak version='1.0' xmlns='https://www.w3.org/2001/10/synthesis' xml:lang='{0}'>\r\n                    "
                    + "<voice name = 'Microsoft Server Speech Text to Speech Voice ({1})' > {2}</voice></speak>", GetLanguageTag(voice),GetVoiceTag(voice),text);
                    

                // Set the HTTP method
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(VoiceRequestUrl);
                request.Content = new StringContent(body, Encoding.UTF8, "application/ssml+xml");
                request.Headers.Add("Authorization", "Bearer " + accessToken);
                request.Headers.Add("Connection", "Keep-Alive");
                request.Headers.Add("User-Agent", "YOUR_RESOURCE_NAME");
                request.Headers.Add("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm");
                //request.Headers.Add("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm");

                using (var response = await client.SendAsync(request).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    using (var dataStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {

                        return GetByteArrayFromStream(dataStream);
                        /*
                        using (var fileStream = new FileStream(@"sample.wav", FileMode.Create, FileAccess.Write, FileShare.Write))
                        {
                            await dataStream.CopyToAsync(fileStream).ConfigureAwait(false);
                            fileStream.Close();
                        }
                        */

                    }
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
    /// 言語に応じたボイスタグを取得
    /// </summary>
    /// <param name="voice"></param>
    /// <returns></returns>
    static private string GetVoiceTag(VoiceSelect voice)
    {
        switch (voice)
        {
            case VoiceSelect.Japanese:
                //return "ja-JP, HarukaRUS";
                return "ja-JP, Ayumi, Apollo";
                //return "ja-JP, Ichiro, Apollo";
            case VoiceSelect.English:
                return "en-US, ZiraRUS";
            case VoiceSelect.Chinese:
                return "zh-CN, HuihuiRUS";
            case VoiceSelect.Russian:
                return "ru-RU, Irina, Apollo";
            case VoiceSelect.Arabic:
                return "ar-EG, Hoda";
            default:
                return "ja-JP, Ayumi, Apollo";
        }
    }

    static private string GetLanguageTag(VoiceSelect voice)
    {
        switch (voice)
        {
            case VoiceSelect.Japanese:
                return "ja-JP";
            case VoiceSelect.English:
                return "en-US";
            case VoiceSelect.Chinese:
                return "zh-CN";
            case VoiceSelect.Russian:
                return "ru-RU";
            case VoiceSelect.Arabic:
                return "ar-EG";
            default:
                return "ja-JP";
        }
    }

    /// <summary>
    /// 音声データ取得
    /// </summary>
    /// <returns></returns>
    static public async UniTask<byte[]> GetVoice(string text, VoiceSelect voice)
    {
        if (string.IsNullOrEmpty(_subscriptionKey))
        {
            _subscriptionKey = GetSubscriptionKey();
        }
        var token =  await GetToken(_subscriptionKey);
        return await RequestVoice(token, text, voice);
    }

    private static string GetSubscriptionKey()
    {
        var settingFileAsset = AssetBundleManager.Instance.LoadTextAssetFromResourcePack(SettingFilePath);
        if (settingFileAsset == null)
        {
            Debug.LogError("Azure setting file load error.");
            return null;
        }
        var json = settingFileAsset.text.Trim(new char[] { '\uFEFF' });
        var settingObj = JsonUtility.FromJson<AzureSettings>(json);

        return settingObj.SubscriptionKey;
    }
}
