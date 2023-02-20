using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

public static class WebRequest
{
    public enum Method
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    public static async UniTask<string> Request(string url, Method method, KeyValuePair<string,string>[] headers, string json)
    {
        var req = new UnityWebRequest(url, method.ToString());
        // ヘッダ
        foreach (var header in headers)
        {
            req.SetRequestHeader(header.Key, header.Value);
        }
        // ボディ
        byte[] body = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        // リクエスト
        await req.SendWebRequest();
        // レスポンスを返す
        return DownloadHandlerBuffer.GetContent(req);
    }
}
