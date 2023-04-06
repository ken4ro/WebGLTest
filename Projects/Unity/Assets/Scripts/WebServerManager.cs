using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public class WebServerManager : SingletonBase<WebServerManager>
{
    public async UniTask<string> Request(string url, string method, string body)
    {
        using UnityWebRequest www = new UnityWebRequest(url, method);
        www.SetRequestHeader("Content-Type", "application/json");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        www.downloadHandler = new DownloadHandlerBuffer();
        try
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Request error result: {www.result}");
            }
            else
            {
                var ret = www.downloadHandler.text;
                //Debug.Log($"Request download handler text: {ret}");
                return ret;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Request exception: {e.Message}");
        }

        return null;
    }

}
