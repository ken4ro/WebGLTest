using System.IO;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 音声キャッシュクラス
/// </summary>
public class TextToSpeechAudioCache
{
    /// <summary>
    /// ハッシュ関数(SHA256)のキー
    /// </summary>
    private const string SECRET_KEY = "aj8jtg3a9tnh3int23atn3u";

    /// <summary>
    /// デフォルト(Application.dataPath)からの相対パス
    /// </summary>
    private const string CACHE_DIR = "/Data/AudioCache";

    /// <summary>
    /// 保存先ディレクトリpath
    /// </summary>
    private string dirPath = "";

    /// <summary>
    /// 保存先path
    /// </summary>
    private string filePath = "";

    public TextToSpeechAudioCache(string key)
    {
        dirPath = InitDir();
        filePath = dirPath + Sha256(key, SECRET_KEY) + ".wav";
    }

    /// <summary>
    /// キャッシュ済みかどうか
    /// </summary>
    /// <returns></returns>
    public bool IsCached()
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// キャッシュ済みファイルの読み込み処理を行う
    /// </summary>
    /// <returns>Task<AudioClip></returns>
    public async UniTask<AudioClip> GetCacheFile()
    {
        //using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath, AudioType.WAV))
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.WAV))
        {
            await www.SendWebRequest();
            if (www.isNetworkError)
            {
                Debug.Log(www.error);
                return null;
            }
            else
            {
                return DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }

    public void SaveFile(byte[] bytes)
    {
        using (var fs = File.Create(filePath))
        {
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    /// <summary>
    /// キャッシュクリア処理
    /// </summary>
    /// <param name="dir">ディレクトリパス</param>
    public static void AllClear()
    {
        string dirPath = InitDir();
        DirectoryInfo target = new DirectoryInfo(dirPath);
        foreach (FileInfo file in target.GetFiles(@"*.wav"))
        {
            file.Delete();
        }
    }

    /// <summary>
    /// ハッシュ関数
    /// </summary>
    /// <param name="planeStr">変換元文字列</param>
    /// <param name="key">シークレットキー</param>
    /// <returns>ハッシュ文字列</returns>
    public static string Sha256(string planeStr, string key)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] planeBytes = ue.GetBytes(planeStr);
        byte[] keyBytes = ue.GetBytes(key);

        System.Security.Cryptography.HMACSHA256 sha256 = new System.Security.Cryptography.HMACSHA256(keyBytes);
        byte[] hashBytes = sha256.ComputeHash(planeBytes);
        string hashStr = "";
        foreach (byte b in hashBytes)
        {
            hashStr += string.Format("{0,0:x2}", b);
        }
        return hashStr;
    }

    public string GetFilePath() => filePath;

    private static string InitDir()
    {
        var dir = Application.streamingAssetsPath + CACHE_DIR;
        if (dir.LastIndexOf("/") != dir.Length - 1)
        {
            dir += "/";
        }
#if false
        var dir = Application.dataPath + CACHE_DIR;
        if (!File.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if (dir.LastIndexOf("/") != dir.Length - 1)
        {
            dir += "/";
        }
#endif
        return dir;
    }
}