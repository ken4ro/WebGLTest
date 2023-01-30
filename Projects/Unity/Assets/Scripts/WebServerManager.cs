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
    private static readonly string RequestVersionUrl = "https://tlx.friendplus.jp:3000" + "/api/v1/component/user/versions";

    private static readonly string RequestFileUrl = "https://tlx.friendplus.jp:3000" + "/api/v1/component/user/file/";

    private string Token => Convert.ToBase64String(Encoding.UTF8.GetBytes("dev_generic_demo" + ":" + "user1" + ":" + "password1"));

    /// <summary>
    /// 正常に動いているか
    /// </summary>
    /// <returns></returns>
    public async UniTask<bool> HealthCheck()
    {
        var req = UnityWebRequest.Get(RequestVersionUrl);
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Basic " + Token);
        try
        {
            await req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"HealthCheck error = {req.error}");
                return false;
            }
            else
            {
                // レスポンス取得
                Debug.Log($"HealthCheck success.");
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"HealthCheck exception = {e.Message}");
            return false;
        }
    }

    public async UniTask<ResponseAssetBundleVersions> RequestVersionsSync(string url)
    {
        var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Basic " + Token);
        try
        {
            await req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"RequestJsonSync error = {req.error}");
            }
            else
            {
                return JsonUtility.FromJson<ResponseAssetBundleVersions>(req.downloadHandler.text);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RequestJsonSync exception = {e.Message}");
        }

        return null;
    }

    public async UniTask<byte[]> RequestBytesSync(string url)
    {
        var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Basic " + Token);
        try
        {
            await req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"RequestJsonSync error = {req.error}");
            }
            else
            {
                return req.downloadHandler.data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RequestJsonSync exception = {e.Message}");
        }

        return null;
    }

    public async UniTask<string> RequestUserToken(string url, string json)
    {
        using UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.SetRequestHeader("Content-Type", "application/json");
        //www.SetRequestHeader("accept", "text/plain");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        www.downloadHandler = new DownloadHandlerBuffer();
        try
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.result.ToString());
            }
            else
            {
                var ret = www.downloadHandler.text;
                Debug.Log(ret);
                return ret;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        return null;
    }

    public async UniTask UpdateUserToken(string url)
    {
        using UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.SetRequestHeader("Authorization", "Bearer NDAyYzhlYzctMGY2Ny00ZjNmLWFkYjEtN2QxMjg3MTU0MThkLTcyOGQxMTE0LWM1MDYtNDEyYy04OGQ1LTE5NjBlNzk0NjY4NS1jNTZjZGUwYy1jZWU2LTQzNzgtYTE2Mi0zOWU5YTkwZTUxYzk=");
        //www.SetRequestHeader("accept", "text/plain");
        www.downloadHandler = new DownloadHandlerBuffer();
        try
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.result.ToString());
            }
            else
            {
                var ret = www.downloadHandler.text;
                Debug.Log(ret);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
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
