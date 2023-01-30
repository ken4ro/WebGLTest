using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

    public static string Request(string url, Method method, KeyValuePair<string,string>[] headers, string json)
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
        req.SendWebRequest();
        // レスポンスを返す
        return DownloadHandlerBuffer.GetContent(req);
    }
}
