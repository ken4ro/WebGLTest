using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

public enum HttpRequestType
{
    GET,
    POST,
    DELETE,
    PUT
}

public class HttpJsonResponse
{
    public string Json { get; set; } = "";
    public HttpStatusCode StatusCode { get; set; }
}

public class HttpBytesResponse
{
    public byte[] Bytes { get; set; } = null;
    public HttpStatusCode StatusCode { get; set; }
}

public class JsonHelper
{
    public static List<T> ListFromJson<T>(string json)
    {
        var newJson = "{ \"list\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.list;
    }

    [Serializable]
    class Wrapper<T>
    {
        public List<T> list = null;
    }
}

public class HttpRequest : MonoBehaviour
{
    // タイムアウト(秒)
    public static readonly double TimeoutSeconds = 30;

    // static にしておくが、DNS変更が反映されない or Cookie がキャッシュされてしまうといった問題が発生する場合は要検討
    public static HttpClient Client { get; set; } = new HttpClient() { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) };

    /// <summary>
    /// JSONリクエスト(非同期)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="content"></param>
    /// <param name="mediaType"></param>
    /// <param name="isLongPolling"></param>
    /// <returns></returns>
    public static async UniTask<HttpJsonResponse> RequestJsonAsync(HttpRequestType type, string url, KeyValuePair<string, string>[] headers, HttpContent content = default, MediaTypeHeaderValue mediaType = default, bool isLongPolling = false)
    {
        var ret = new HttpJsonResponse();

        HttpMethod method = null;
        switch (type)
        {
            case HttpRequestType.GET:
                method = new HttpMethod("GET");
                break;
            case HttpRequestType.POST:
                method = new HttpMethod("POST");
                break;
            case HttpRequestType.DELETE:
                method = new HttpMethod("DELETE");
                break;
            case HttpRequestType.PUT:
                method = new HttpMethod("PUT");
                break;
        }

        using (var request = new HttpRequestMessage(method, url))
        {
            try
            {
                // ヘッダ
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                // ボディ
                if (content != null)
                {
                    request.Content = content;
                    // メディアタイプ
                    if (mediaType != null)
                    {
                        request.Content.Headers.ContentType = mediaType;
                    }
                }
                // リクエスト開始
                HttpResponseMessage response = null;
                if (!isLongPolling)
                {
                    response = await Client.SendAsync(request);
                }
                else
                {
                    // Long Polling 用に Client を新規作成
                    using (var client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite) })
                    {
                        response = await client.SendAsync(request);
                    }
                }
                // 結果参照
                if (response.IsSuccessStatusCode)
                {
                    // 成功
                    var responseString = await response.Content.ReadAsStringAsync();
                    ret.Json = responseString;
                    ret.StatusCode = HttpStatusCode.OK;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // アクセストークン期限切れ
                    ret.StatusCode = HttpStatusCode.Unauthorized;
                }
                else
                {
                    // その他エラー
                    ret.StatusCode = response.StatusCode;
                }
            }
            catch (Exception)
            {
                //Debug.Log($"HttpRequestService HttpRequest exception: {e.Message}");
                ret.StatusCode = HttpStatusCode.RequestTimeout;
            }
        }

        return ret;
    }

    /// <summary>
    /// JSONリクエスト(同期)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="content"></param>
    /// <param name="mediaType"></param>
    /// <param name="isLongPolling"></param>
    /// <returns></returns>
    public static HttpJsonResponse RequestJsonSync(HttpRequestType type, string url, KeyValuePair<string, string>[] headers, HttpContent content = default, MediaTypeHeaderValue mediaType = default, bool isLongPolling = false, TimeSpan timeOut = default)
    {
        var ret = new HttpJsonResponse();

        HttpMethod method = null;
        switch (type)
        {
            case HttpRequestType.GET:
                method = new HttpMethod("GET");
                break;
            case HttpRequestType.POST:
                method = new HttpMethod("POST");
                break;
            case HttpRequestType.DELETE:
                method = new HttpMethod("DELETE");
                break;
            case HttpRequestType.PUT:
                method = new HttpMethod("PUT");
                break;
        }

        using (var request = new HttpRequestMessage(method, url))
        {
            try
            {
                // ヘッダ
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                // ボディ
                if (content != null)
                {
                    request.Content = content;
                    // メディアタイプ
                    if (mediaType != null)
                    {
                        request.Content.Headers.ContentType = mediaType;
                    }
                }
                // リクエスト開始
                HttpResponseMessage response = null;
                if (!isLongPolling)
                {
                    if (timeOut != default)
                    {
                        Client.Timeout = timeOut;
                    }
                    response = Client.SendAsync(request).Result;
                }
                else
                {
                    // Long Polling 用に Client を新規作成
                    using (var client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite) })
                    {
                        response = client.SendAsync(request).Result;
                    }
                }
                // 結果参照
                if (response.IsSuccessStatusCode)
                {
                    // 成功
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    ret.Json = responseString;
                    ret.StatusCode = HttpStatusCode.OK;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // アクセストークン期限切れ
                    ret.StatusCode = HttpStatusCode.Unauthorized;
                }
                else
                {
                    // その他エラー
                    ret.StatusCode = response.StatusCode;
                }
            }
            catch (Exception)
            {
                //Debug.Log($"HttpRequestService HttpRequest exception: {e.Message}");
                ret.StatusCode = HttpStatusCode.RequestTimeout;
            }
        }

        return ret;
    }

    /// <summary>
    /// バイナリリクエスト(非同期)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="content"></param>
    /// <param name="mediaType"></param>
    /// <param name="isLongPolling"></param>
    /// <returns></returns>
    public static async UniTask<HttpBytesResponse> RequestBytesAsync(HttpRequestType type, string url, KeyValuePair<string, string>[] headers, HttpContent content = default, MediaTypeHeaderValue mediaType = default, bool isLongPolling = false)
    {
        var ret = new HttpBytesResponse();

        HttpMethod method = null;
        switch (type)
        {
            case HttpRequestType.GET:
                method = new HttpMethod("GET");
                break;
            case HttpRequestType.POST:
                method = new HttpMethod("POST");
                break;
            case HttpRequestType.DELETE:
                method = new HttpMethod("DELETE");
                break;
            case HttpRequestType.PUT:
                method = new HttpMethod("PUT");
                break;
        }

        using (var request = new HttpRequestMessage(method, url))
        {
            try
            {
                // ヘッダ
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                // ボディ
                if (content != null)
                {
                    request.Content = content;
                    // メディアタイプ
                    if (mediaType != null)
                    {
                        request.Content.Headers.ContentType = mediaType;
                    }
                }
                // リクエスト開始
                HttpResponseMessage response = null;
                if (!isLongPolling)
                {
                    response = await Client.SendAsync(request);
                }
                else
                {
                    // Long Polling 用に Client を新規作成
                    using (var client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite) })
                    {
                        response = await client.SendAsync(request);
                    }
                }
                // 結果参照
                if (response.IsSuccessStatusCode)
                {
                    // 成功
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    ret.Bytes = bytes;
                    ret.StatusCode = HttpStatusCode.OK;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // アクセストークン期限切れ
                    ret.StatusCode = HttpStatusCode.Unauthorized;
                }
                else
                {
                    // その他エラー
                    ret.StatusCode = response.StatusCode;
                }
            }
            catch (Exception)
            {
                //Debug.Log($"HttpRequestService HttpRequest exception: {e.Message}");
                ret.StatusCode = HttpStatusCode.RequestTimeout;
            }
        }

        return ret;
    }

    /// <summary>
    /// バイナリリクエスト(同期)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="content"></param>
    /// <param name="mediaType"></param>
    /// <param name="isLongPolling"></param>
    /// <returns></returns>
    public static HttpBytesResponse RequestBytesSync(HttpRequestType type, string url, KeyValuePair<string, string>[] headers, HttpContent content = default, MediaTypeHeaderValue mediaType = default, bool isLongPolling = false)
    {
        var ret = new HttpBytesResponse();

        HttpMethod method = null;
        switch (type)
        {
            case HttpRequestType.GET:
                method = new HttpMethod("GET");
                break;
            case HttpRequestType.POST:
                method = new HttpMethod("POST");
                break;
            case HttpRequestType.DELETE:
                method = new HttpMethod("DELETE");
                break;
            case HttpRequestType.PUT:
                method = new HttpMethod("PUT");
                break;
        }

        using (var request = new HttpRequestMessage(method, url))
        {
            try
            {
                // ヘッダ
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                // ボディ
                if (content != null)
                {
                    request.Content = content;
                    // メディアタイプ
                    if (mediaType != null)
                    {
                        request.Content.Headers.ContentType = mediaType;
                    }
                }
                // リクエスト開始
                HttpResponseMessage response = null;
                if (!isLongPolling)
                {
                    response = Client.SendAsync(request).Result;
                }
                else
                {
                    // Long Polling 用に Client を新規作成
                    using (var client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite) })
                    {
                        response = client.SendAsync(request).Result;
                    }
                }
                // 結果参照
                if (response.IsSuccessStatusCode)
                {
                    // 成功
                    var bytes = response.Content.ReadAsByteArrayAsync().Result;
                    ret.Bytes = bytes;
                    ret.StatusCode = HttpStatusCode.OK;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // アクセストークン期限切れ
                    ret.StatusCode = HttpStatusCode.Unauthorized;
                }
                else
                {
                    // その他エラー
                    ret.StatusCode = response.StatusCode;
                }
            }
            catch (Exception)
            {
                //Debug.Log($"HttpRequestService HttpRequest exception: {e.Message}");
                ret.StatusCode = HttpStatusCode.RequestTimeout;
            }
        }

        return ret;
    }
}
