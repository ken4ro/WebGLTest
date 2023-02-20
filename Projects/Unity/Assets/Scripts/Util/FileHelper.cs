using System.IO;

using UnityEngine;

public static class FileHelper
{


    /// <summary>
    /// 設定ファイル読み込み
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T LoadConfigFileSync<T>(string fileName)
    {

        var filePath = GetCurrentDirectory() + fileName;
        var encPath = filePath + ".enc";

        var enc = false;
        if (File.Exists(encPath))
            enc = true;
        
        var www = new WWW(enc ? encPath : filePath);
        while (!www.isDone) { }
        var buff = enc ? EncryptHelper.Decrypt(www.text) : www.text;
        var json = JsonUtility.FromJson<T>(buff);


        //未暗号データは暗号化しておく
        if (!enc)
            File.WriteAllText(encPath, EncryptHelper.Encrypt( buff));


        return json;
 
    }

    /// <summary>
    /// StreamingAssets フォルダパスを返す(Editor/Windowsのみ)
    /// </summary>
    /// <returns></returns>
    public static string GetStreamingAssetsFolderPath()
    {
        return Application.streamingAssetsPath + "/";
    }

    /// <summary>
    /// アプリケーションの実行フォルダを返す(Editor/Windowsのみ)
    /// </summary>
    /// <returns></returns>
    public static string GetCurrentDirectory()
    {
        var dataFolderPath = Application.dataPath;
        var lastIndex = dataFolderPath.LastIndexOf("/");
        return dataFolderPath.Substring(0, lastIndex + 1);
    }

    /// <summary>
    /// ローカルデータが保存されているフォルダパスを返す
    /// </summary>
    /// <returns></returns>
    public static string GetLocalDataFolderPath()
    {
        var localDataFolderPath = Application.dataPath + "/Data/";
        if (!File.Exists(localDataFolderPath))
        {
            Directory.CreateDirectory(localDataFolderPath);
        }
        return localDataFolderPath;
    }

    /// <summary>
    /// ローカルに保存されたアセットバンドルが含まれるフォルダパスを返す
    /// </summary>
    /// <returns></returns>
    public static string GetLocalAssetBundleDataPath()
    {
        var assetBundlePath = GetLocalDataFolderPath() + "AssetBundle/";
        if (!File.Exists(assetBundlePath))
        {
            Directory.CreateDirectory(assetBundlePath);
        }
        return assetBundlePath;
    }
}
