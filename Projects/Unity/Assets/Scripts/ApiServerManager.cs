using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public class ApiServerManager : SingletonBase<ApiServerManager>
{
    private static readonly string RequestUserTokenUrl = "https://development.studio-sylphid.com:6500/cloud/api/v1/user/token/get";

    private static readonly string UpdateUserTokenUrl = "https://development.studio-sylphid.com:6500/cloud/api/v1/user/token/put";

    private static readonly string RequestFirstNodeUrl = "https://development.studio-sylphid.com:6500/cloud/api/v1/flow/dialog/get";

    private static readonly string RequestNextNodeUrl = "https://development.studio-sylphid.com:6500/cloud/api/v1/flow/dialog/put";

    /// <summary>
    /// ユーザートークンをリクエスト
    /// </summary>
    /// <param name="url"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public async UniTask<string> RequestUserToken(string body)
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
                Debug.LogError($"RequestUserToken error result: {www.result}");
            }
            else
            {
                var ret = www.downloadHandler.text;
                Debug.Log($"RequestUserToken download handler text: {ret}");
                return ret;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RequestUserToken exception: {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// ユーザートークンを更新
    /// </summary>
    /// <param name="base64Token"></param>
    /// <returns></returns>
    public async UniTask UpdateUserToken(string base64Token)
    {
        using UnityWebRequest www = new UnityWebRequest(UpdateUserTokenUrl, "POST");
        www.SetRequestHeader("Authorization", "Bearer " + base64Token);
        www.downloadHandler = new DownloadHandlerBuffer();
        try
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"UpdateUserToken error result: {www.result}");
            }
            else
            {
                var ret = www.downloadHandler.text;
                Debug.Log($"UpdateUserToken download handler text: {ret}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"UpdateUserToken exception: {e.Message}");
        }
    }

    /// <summary>
    /// 初期ノードをリクエスト
    /// </summary>
    /// <param name="url"></param>
    /// <param name="base64Token"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public async UniTask<string> RequestFirstNode(string base64Token, string body)
    {
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
                Debug.LogError($"RequestFirstNode error result: {www.result}");
            }
            else
            {
                var ret = www.downloadHandler.text;
                return ret;
            }
        }
        catch (Exception e) 
        {
            Debug.LogError($"RequestFirstNode exception: {e.Message}");
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
    public async UniTask<string> RequestNextNode(string base64Token, string body)
    {
        using UnityWebRequest www = new UnityWebRequest(RequestNextNodeUrl, "POST");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + base64Token);
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        www.downloadHandler = new DownloadHandlerBuffer();
        try
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"RequestNextNode error result: {www.result}");
            }
            else
            {
                var ret = www.downloadHandler.text;
                Debug.Log($"RequestNextNode download handler text: {ret}");
                return ret;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RequestNextNode exception: {e.Message}");
        }

        return null;
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
}
