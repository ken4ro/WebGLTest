using System;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class ApiServerManager : SingletonBase<ApiServerManager>
{
    private static readonly string RequestUserTokenUrl = "https://dev.xrccg.com:4000/api/v1/user/token/get";

    private static readonly string UpdateUserTokenUrl = "https://dev.xrccg.com:4000/api/v1/user/token/put";

    private static readonly string RequestUserSettingUrl = "https://dev.xrccg.com:4000/api/v1/user/setting/get";

    private static readonly string RequestFirstNodeUrl = "https://dev.xrccg.com:4000/api/v1/flow/dialog/get";

    private static readonly string RequestNextNodeUrl = "https://dev.xrccg.com:4000/api/v1/flow/dialog/put";
    
    private static readonly string RequestNodeVoiceUrl = "https://dev.xrccg.com:4000/api/v1/flow/voice/get";

    /// <summary>
    /// ユーザートークンをリクエスト
    /// </summary>
    /// <param name="url"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public async UniTask<string> RequestUserTokenAsync(string body)
    {
        using UnityWebRequest www = new UnityWebRequest(RequestUserTokenUrl, "POST");
        www.SetRequestHeader("Content-Type", "application/json");
        //www.SetRequestHeader("accept", "text/plain");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        www.downloadHandler = new DownloadHandlerBuffer();
        try
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"RequestUserTokenAsync error result: {www.result}");
            }
            else
            {
                var ret = www.downloadHandler.text;
                Debug.Log($"RequestUserTokenAsync download handler text: {ret}");
                return ret;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RequestUserTokenAsync exception: {e.Message}, {www.error}");
            var headers = www.GetResponseHeaders();
            Debug.Log($"RequestUserTokenAsync response header count = {headers.Count}");
            for (int i = 0; i < headers.Count; i++)
            {
                Debug.Log("KEY: " + headers.Keys.ToList()[i] + "    -    VALUE: " + headers[headers.Keys.ToList()[i]]);
            }
        }

        return null;
    }

    /// <summary>
    /// ユーザートークンを更新
    /// </summary>
    /// <param name="base64Token"></param>
    /// <returns></returns>
    public async UniTask<string> UpdateUserTokenAsync(string base64Token)
    {
        using UnityWebRequest www = new UnityWebRequest(UpdateUserTokenUrl, "POST");
        www.SetRequestHeader("Authorization", "Bearer " + base64Token);
        www.downloadHandler = new DownloadHandlerBuffer();
        try
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"UpdateUserTokenAsync error result: {www.result}");
            }
            else
            {
                var ret = www.downloadHandler.text;
                Debug.Log($"UpdateUserTokenAsync download handler text: {ret}");
                return ret;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"UpdateUserTokenAsync exception: {e.Message}, {www.error}");
            var headers = www.GetResponseHeaders();
            Debug.Log($"UpdateUserTokenAsync response header count = {headers.Count}");
            for (int i = 0; i < headers.Count; i++)
            {
                Debug.Log("KEY: " + headers.Keys.ToList()[i] + "    -    VALUE: " + headers[headers.Keys.ToList()[i]]);
            }
        }

        return null;
    }

    /// <summary>
    /// ユーザー設定をリクエスト
    /// </summary>
    /// <param name="base64Token"></param>
    /// <returns></returns>
    public async UniTask<string> RequestUserSettingAsync(string base64Token)
    {
        using UnityWebRequest www = new UnityWebRequest(RequestUserSettingUrl, "POST");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + base64Token);
        www.downloadHandler = new DownloadHandlerBuffer();
        try
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"RequestUserSettingAsync error result: {www.result}");
            }
            else
            {
                var ret = www.downloadHandler.text;
                Debug.Log($"RequestUserSettingAsync download handler text: {ret}");
                return ret;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RequestUserSettingAsync exception: {e.Message}, {www.error}");
            var headers = www.GetResponseHeaders();
            Debug.Log($"RequestUserSettingAsync response header count = {headers.Count}");
            for (int i = 0; i < headers.Count; i++)
            {
                Debug.Log("KEY: " + headers.Keys.ToList()[i] + "    -    VALUE: " + headers[headers.Keys.ToList()[i]]);
            }
        }

        return null;
    }

    /// <summary>
    /// 初期ノードをリクエスト
    /// </summary>
    /// <param name="url"></param>
    /// <param name="base64Token"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public async UniTask<string> RequestFirstNodeAsync(string base64Token, string body)
    {
        Debug.Log($"RequestFirstNodeAsync body = {body}");
        using UnityWebRequest www = new UnityWebRequest(RequestFirstNodeUrl, "POST");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + base64Token);
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        www.downloadHandler = new DownloadHandlerBuffer();
        try
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"RequestFirstNodeAsync error result: {www.result}");
            }
            else
            {
                var ret = www.downloadHandler.text;
                return ret;
            }
        }
        catch (Exception e) 
        {
            Debug.LogError($"RequestFirstNodeAsync exception: {e.Message}, {www.error}");
            var headers = www.GetResponseHeaders();
            Debug.Log($"RequestFirstNodeAsync response header count = {headers.Count}");
            for (int i = 0; i < headers.Count; i++)
            {
                Debug.Log("KEY: " + headers.Keys.ToList()[i] + "    -    VALUE: " + headers[headers.Keys.ToList()[i]]);
            }
        }

        return null;
    }

    /// <summary>
    /// 次のノードをリクエスト
    /// </summary>
    /// <param name="url"></param>
    /// <param name="base64Token"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public async UniTask<string> RequestNextNodeAsync(string base64Token, string body)
    {
        Debug.Log($"RequestNextNodeAsync body = {body}");
        using UnityWebRequest www = new UnityWebRequest(RequestNextNodeUrl, "POST");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + base64Token);
        //www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        www.downloadHandler = new DownloadHandlerBuffer();
        try
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"RequestNextNodeAsync error result: {www.result}");
            }
            else
            {
                var ret = www.downloadHandler.text;
                Debug.Log($"RequestNextNodeAsync download handler text: {ret}");
                return ret;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RequestNextNodeAsync exception: {e.Message}, {www.error}");
            var headers = www.GetResponseHeaders();
            if (headers == null)
            {
                Debug.Log($"RequestNextNodeAsync response headers is null");
            }
            else
            {
                Debug.Log($"RequestNextNodeAsync response header count = {headers.Count}");
            }
            for (int i = 0; i < headers.Count; i++)
            {
                Debug.Log("KEY: " + headers.Keys.ToList()[i] + "    -    VALUE: " + headers[headers.Keys.ToList()[i]]);
            }
        }

        return null;
    }

    /// <summary>
    /// VoiceVoxボイス取得
    /// </summary>
    public async UniTask<byte[]> RequestNodeVoice(string base64Token, string body)
    {
        Debug.Log("RequestNodeVoice request");
        using (UnityWebRequest www = new UnityWebRequest(RequestNodeVoiceUrl, "POST"))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + base64Token);
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            www.downloadHandler = new DownloadHandlerBuffer();
            try
            {
                await www.SendWebRequest();
                if (www.isHttpError || www.isNetworkError)
                {
                    Debug.LogError($"RequestNodeVoice error result: {www.error}");
                }
                else
                {
                    var ret = www.downloadHandler.data;
                    Debug.Log($"RequestNodeVoice download handler data length: {ret.Length}");
                    return ret;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"RequestNodeVoice exception: {e.Message}");
            }

            return null;
        }
    }

    [Serializable]
    public class RequestUserTokenJson
    {
        public string login_id;
        public string login_type;
        public string password;
    }

    [Serializable]
    public class RequestUserTokenResponseJson
    {
        public string token_type;
        public string access_token;
        public string refresh_token;
        public int expires_in;
    }

    [Serializable]
    public class RequestFirstNodeJson
    {
        public string flow_id;
    }

    [Serializable]
    public class RequestFirstNodeResponseJson
    {
        public BotManager.BotResponse response;
    }

    [Serializable]
    public class RequestNextNodeJson
    {
        public string flow_id;
        public string utterance;
    }

    [Serializable]
    public class RequestNextNodeResponseJson
    {
        public BotManager.BotResponse response;
    }

    [Serializable]
    public class RequestUserSettingsResponseJson
    {
        public string google_key;
        public RequestUserSettingsResponseUI ui;
        public RequestUserSettingsResponseBot bot;
        public RequestUserSettingsResponseRtc rtc;
    }

    [Serializable]
    public class RequestUserSettingsResponseUI
    {
        public string request_type;
        public int font_size;
        public string wait_animation_type;
        public string recording_agreement_enable;
        public string screensaver_enable;
        public int text_speed;
        public int input_limit_sec;
        public string[] languages;
    }

    [Serializable]
    public class RequestUserSettingsResponseBot
    {
        public string service_type;
        public int start_delay_sec;
        public int restart_sec;
        public int return_sec;
        public int action_delay_sec;
        public string voice_type;
        public string ccg_flow_id;
    }

    [Serializable]
    public class RequestUserSettingsResponseRtc
    {
        public string service_type;
    }

    [Serializable]
    public class RequestNodeVoiceJson
    {
        public string source_type;
        public string language;
    }
}
