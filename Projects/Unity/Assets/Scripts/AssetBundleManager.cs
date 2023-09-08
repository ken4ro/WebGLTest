using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Video;

using static GlobalState;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

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
    /// GoogleSettings用アセットバンドル
    /// </summary>
    public AssetBundle GoogleSettingsAssetBundle { get; set; } = null;

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
        if (AvatarAssetBundle != null)
        {
            //Debug.Log($"LoadAvatarAssetBundle completed.");
        }
        else
        {
            Debug.LogError($"LoadAvatarAssetBundle failed.");
        }
    }

    public async UniTask LoadAvatarAssetBundleFromStreamingAssets()
    {
        if (AvatarAssetBundle == null)
        {
            var assetBundlePath = "";
#if !UNITY_EDITOR && UNITY_WEBGL
            switch (GlobalState.Instance.CurrentCharacterModel.Value)
            {
                case CharacterModel.Una2D:
                    assetBundlePath = Path.Combine(Application.streamingAssetsPath, "avatar_2d_webgl.bundle");
                    break;
                case CharacterModel.Una3D:
                    assetBundlePath = Path.Combine(Application.streamingAssetsPath, "avatar_3d_webgl.bundle");
                    break;
                default:
                    assetBundlePath = Path.Combine(Application.streamingAssetsPath, "avatar_2d_webgl.bundle");
                    break;
            }
#else
            switch (GlobalState.Instance.CurrentCharacterModel.Value)
            {
                case CharacterModel.Una2D:
                    assetBundlePath = Path.Combine(Application.streamingAssetsPath, "avatar_2d_windows.bundle");
                    break;
                case CharacterModel.Una3D:
                    assetBundlePath = Path.Combine(Application.streamingAssetsPath, "avatar_3d_windows.bundle");
                    break;
                default:
                    assetBundlePath = Path.Combine(Application.streamingAssetsPath, "avatar_2d_windows.bundle");
                    break;
            }
#endif
            AvatarAssetBundle = await LoadAssetBundleWithWebRequestAsync(assetBundlePath);
        }
        if (AvatarAssetBundle != null)
        {
            //Debug.Log($"LoadAvatarAssetBundleFromStreamingAssets completed.");
        }
        else
        {
            Debug.LogError($"LoadAvatarAssetBundleFromStreamingAssets failed.");
        }
    }

    public async UniTask LoadGoogleSettingsAssetBundleFromStreamingAssets()
    {
        if (GoogleSettingsAssetBundle == null)
        {
            var assetBundlePath = Path.Combine(Application.streamingAssetsPath, "google_settings.bundle");
#if !UNITY_EDITOR && UNITY_WEBGL
            GoogleSettingsAssetBundle = await LoadAssetBundleWithWebRequestAsync(assetBundlePath);
#else
            //GoogleSettingsAssetBundle = await LoadAssetBundleAsync(avatarAssetBundlePath);
            GoogleSettingsAssetBundle = await LoadAssetBundleWithWebRequestAsync(assetBundlePath);
#endif
        }
        if (GoogleSettingsAssetBundle != null)
        {
            Debug.Log($"LoadGoogleSettingsAssetBundleFromStreamingAssets completed.");
        }
        else
        {
            Debug.Log($"LoadGoogleSettingsAssetBundleFromStreamingAssets failed.");
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
        if (ResourcePackAssetBundle != null)
        {
            Debug.Log($"LoadResourcePackAssetBundle completed.");
        }
        else
        {
            Debug.Log($"LoadResourcePackAssetBundle failed.");
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
        AssetBundle assetBundle = null;
        try
        {
            assetBundle = AssetBundle.LoadFromFile(filePath);
            if (assetBundle != null)
            {
                Debug.Log($"LoadAssetBundle completed.");
            }
            else
            {
                Debug.Log($"LoadAssetBundle failed.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadAssetBundle exception: {e.Message}");
        }
        return assetBundle;
    }

    public async UniTask<AssetBundle> LoadAssetBundleAsync(string filePath)
    {
        return await AssetBundle.LoadFromFileAsync(filePath);
    }

    public async UniTask<AssetBundle> LoadAssetBundleWithWebRequestAsync(string filePath)
    {
        var request = UnityWebRequest.Get(filePath);
        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"LoadAssetBundleWithWebRequestAsync failed. error = {request.error}");
        }

        return AssetBundle.LoadFromMemory(request.downloadHandler.data);
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
