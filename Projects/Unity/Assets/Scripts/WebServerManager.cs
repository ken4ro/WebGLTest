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
    private static readonly string RequestVersionUrl = GlobalState.Instance.ApplicationGlobalSettings.FileServer + "/api/v1/component/user/versions";

    private static readonly string RequestFileUrl = GlobalState.Instance.ApplicationGlobalSettings.FileServer + "/api/v1/component/user/file/";

    private string Token => Convert.ToBase64String(Encoding.UTF8.GetBytes(GlobalState.Instance.ApplicationGlobalSettings.CompanyCode + ":" + GlobalState.Instance.ApplicationGlobalSettings.SignalingUserName + ":" + GlobalState.Instance.ApplicationGlobalSettings.SignalingUserPassword));

    /// <summary>
    /// 正常に動いているか
    /// </summary>
    /// <returns></returns>
    public bool HealthCheck()
    {
        var headers = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("ContentType", "application/json"),
            new KeyValuePair<string, string>("Authorization", "Basic " + Token),
        };
        try
        {
            Debug.Log(GlobalState.Instance.ApplicationGlobalSettings.CompanyCode);
            Debug.Log(GlobalState.Instance.ApplicationGlobalSettings.SignalingUserName);
            Debug.Log(GlobalState.Instance.ApplicationGlobalSettings.SignalingUserPassword);

            // リクエスト
            var timeOut = new TimeSpan(0, 0, 10);
            var res = HttpRequest.RequestJsonSync(HttpRequestType.GET, RequestVersionUrl, headers, null, timeOut: timeOut);
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // 成功
                Debug.Log("FileServer Access Succeeded.");
                return true;
            }
            else if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // 権限不足
                Debug.Log("FileServer Access Unauthorized.");
                // アプリを終了させる
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
                UnityEngine.Application.Quit();
#endif
                return false;
            }
            else
            {
                // 失敗
                Debug.LogError("FileServer Access Failed.");
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// バージョン情報取得(非同期)
    /// </summary>
    /// <returns></returns>
    public async UniTask<ResponseAssetBundleVersions> RequestFileVersionsAsync()
    {
        var headers = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("ContentType", "application/json"),
            new KeyValuePair<string, string>("Authorization", "Basic " + Token),
        };
        try
        {
            // リクエスト
            var res = await HttpRequest.RequestJsonAsync(HttpRequestType.GET, RequestVersionUrl, headers, null);
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // レスポンス取得
                return JsonUtility.FromJson<ResponseAssetBundleVersions>(res.Json);
            }
            else
            {
                Debug.LogError($"Request file version error: {res.StatusCode.ToString()}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Request file version exception: {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// バージョン情報取得(同期)
    /// </summary>
    /// <returns></returns>
    public ResponseAssetBundleVersions RequestFileVersionsSync()
    {
        var headers = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("ContentType", "application/json"),
            new KeyValuePair<string, string>("Authorization", "Basic " + Token),
        };
        try
        {
            // リクエスト
            var res = HttpRequest.RequestJsonSync(HttpRequestType.GET, RequestVersionUrl, headers, null);
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // レスポンス取得
                return JsonUtility.FromJson<ResponseAssetBundleVersions>(res.Json);
            }
            else
            {
                Debug.LogError($"Request file version error: {res.StatusCode.ToString()}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Request file version exception: {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// ファイルダウンロード(非同期)
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public async UniTask<byte[]> RequestFileAsync(string fileId)
    {
        var headers = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("ContentType", "application/json"),
            new KeyValuePair<string, string>("Authorization", "Basic " + Token),
        };
        try
        {
            // リクエスト
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var requestFileUrl = RequestFileUrl + fileId;
            var res = await HttpRequest.RequestBytesAsync(HttpRequestType.GET, requestFileUrl, headers, null);
            sw.Stop();
            Debug.Log($"AssetBundle download time: {sw.ElapsedMilliseconds} fileId: {fileId}");
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return res.Bytes;
            }
            else
            {
                Debug.LogError($"Download file error: {res.StatusCode}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Download file exception: {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// ファイルダウンロード(同期)
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public byte[] RequestFileSync(string fileId)
    {
        var headers = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("ContentType", "application/json"),
            new KeyValuePair<string, string>("Authorization", "Basic " + Token),
        };
        try
        {
            // リクエスト
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var requestFileUrl = RequestFileUrl + fileId;
            var res = HttpRequest.RequestBytesSync(HttpRequestType.GET, requestFileUrl, headers, null);
            sw.Stop();
            Debug.Log($"AssetBundle download time: {sw.ElapsedMilliseconds} fileId: {fileId}");
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return res.Bytes;
            }
            else
            {
                Debug.LogError($"Download file error: {res.StatusCode}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Download file exception: {e.Message}");
        }

        return null;
    }

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
                Debug.Log($"Request download handler text: {ret}");
                return ret;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Request exception: {e.Message}");
        }

        return null;
    }

    [Serializable]
    public class ResponseAssetBundleVersions : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public ResponseAssetBundleVersion[] List;

        [SerializeField]
        private ResponseAssetBundleVersion[] list;

        public void OnBeforeSerialize()
        {
            list = List;
        }

        public void OnAfterDeserialize()
        {
            List = list;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType()) return false;

            ResponseAssetBundleVersions versions = (ResponseAssetBundleVersions)obj;
            var version = versions.list;

            if (version.Length != this.list.Length) return false;

            for (var i = 0; i < version.Length; i++)
            {
                if (version[i].Identifier != this.list[i].Identifier) return false;
                if (version[i].FileId != this.list[i].FileId) return false;
                if (version[i].Type != this.list[i].Type) return false;
                if (version[i].Version != this.list[i].Version) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [Serializable]
    public class ResponseAssetBundleVersion : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string Identifier;
        [NonSerialized]
        public string FileId;
        [NonSerialized]
        public string Type;
        [NonSerialized]
        public float Version;

        [SerializeField]
        private string identifier;
        [SerializeField]
        private string file_id;
        [SerializeField]
        private string type;
        [SerializeField]
        private float version;

        public void OnBeforeSerialize()
        {
            identifier = Identifier;
            file_id = FileId;
            type = Type;
            version = Version;
        }

        public void OnAfterDeserialize()
        {
            Identifier = identifier;
            FileId = file_id;
            Type = type;
            Version = version;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType()) return false;

            var version = (ResponseAssetBundleVersion)obj;

            if (version.Identifier != this.Identifier) return false;
            if (version.FileId != this.FileId) return false;
            if (version.Type != this.Type) return false;
            if (version.Version != this.Version) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
