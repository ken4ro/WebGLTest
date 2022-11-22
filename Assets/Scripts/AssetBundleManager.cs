using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Video;

using static GlobalState;

public class AssetBundleManager : SingletonBase<AssetBundleManager>
{
    /// <summary>
    /// アバター用アセットバンドルファイルパス
    /// </summary>
    public string AvatarAssetBundleFilePath { get; set; } = FileHelper.GetLocalAssetBundleDataPath() + "avatar.bundle";

    /// <summary>
    /// リソースパック用アセットバンドルファイルパス
    /// </summary>
    public string ResourcePackBundleFilePath { get; set; } = FileHelper.GetLocalAssetBundleDataPath() + "resource_pack.bundle";

    /// <summary>
    /// アバター用アセットバンドル
    /// </summary>
    public AssetBundle AvatarAssetBundle { get; set; } = null;

    /// <summary>
    /// リソースパック用アセットバンドル
    /// </summary>
    public AssetBundle ResourcePackAssetBundle { get; set; } = null;

    /// <summary>
    /// アバター用アセットバンドルをロード
    /// </summary>
    public void LoadAvatarAssetBundle()
    {
        if (AvatarAssetBundle == null)
        {
            AvatarAssetBundle = LoadAssetBundle(AvatarAssetBundleFilePath);
        }
    }

    /// <summary>
    /// リソースパック用アセットバンドルをロード
    /// </summary>
    /// <returns></returns>
    public void LoadResourcePackAssetBundle()
    {
        if (ResourcePackAssetBundle == null)
        {
            ResourcePackAssetBundle = LoadAssetBundle(ResourcePackBundleFilePath);
        }
    }

    /// <summary>
    /// リソースパック用アセットバンドルからテクスチャ取得
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public Texture2D LoadTexture2DFromResourcePack(string assetName)
    {
        LoadResourcePackAssetBundle();

        return ResourcePackAssetBundle.LoadAsset<Texture2D>(assetName);
    }

    /// <summary>
    /// リソースパック用アセットバンドルからテキストアセット取得
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public TextAsset LoadTextAssetFromResourcePack(string assetName)
    {
        LoadResourcePackAssetBundle();

        return ResourcePackAssetBundle.LoadAsset<TextAsset>(assetName);
    }

    /// <summary>
    /// リソースパック用アセットバンドルからビデオクリップアセット取得
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public VideoClip LoadVideoClipFromResourcePack(string assetName)
    {
        LoadResourcePackAssetBundle();

        return ResourcePackAssetBundle.LoadAsset<VideoClip>(assetName);
    }

    /// <summary>
    /// リソースパック用アセットバンドルからオーディオクリップアセット取得
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public AudioClip LoadAudioClipFromResourcePack(string assetName)
    {
        LoadResourcePackAssetBundle();
        return ResourcePackAssetBundle.LoadAsset<AudioClip>(assetName);
    }

    /// <summary>
    /// アバター用アセットバンドルを出力
    /// </summary>
    /// <param name="fileId"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public void WriteAvatarAssetBundle(byte[] bytes)
    {
        WriteAssetBundle(AvatarAssetBundleFilePath, bytes);
    }

    /// <summary>
    /// リソースパック用アセットバンドルを出力
    /// </summary>
    /// <param name="bytes"></param>
    public void WriteResourcePackAssetBundle(byte[] bytes)
    {
        WriteAssetBundle(ResourcePackBundleFilePath, bytes);
    }

    /// <summary>
    /// アセットバンドル読み込み
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public AssetBundle LoadAssetBundle(string filePath)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var assetBundle = AssetBundle.LoadFromFile(filePath);
        sw.Stop();
        Debug.Log($"LoadAssetBundle: time = {sw.ElapsedMilliseconds} path = {filePath}");
        return assetBundle;
    }

    /// <summary>
    /// アセットバンドル書き込み
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="bytes"></param>
    public void WriteAssetBundle(string filePath, byte[] bytes)
    {
        File.WriteAllBytes(filePath, bytes);
        if (File.Exists(filePath))
        {
            Debug.Log($"WriteAssetBundle completed: path = {filePath}");
        }
        else
        {
            Debug.Log($"WriteAssetBundle failed.");
        }
    }
}
