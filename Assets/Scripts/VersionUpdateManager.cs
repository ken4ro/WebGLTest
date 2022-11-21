using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using static WebServerManager;
using Cysharp.Threading.Tasks;

public class VersionUpdateManager : SingletonBase<VersionUpdateManager>
{
    private static readonly string RequestVersionUrl = "https://tlx.friendplus.jp:3000" + "/api/v1/component/user/versions";
    private static readonly string RequestFileUrl = "https://tlx.friendplus.jp:3000" + "/api/v1/component/user/file/";

    /// <summary>
    /// クライアント更新の必要があるかどうか
    /// </summary>
    /// <returns></returns>
    public async UniTask<bool> IsClientUpdate()
    {
        // 現在のクライアントバージョン取得
        var localVersions = LoadLocalVersions();
        if (localVersions == null) return true;
        var localClientVersion = GetClientVersion(localVersions);
        if (localClientVersion == null) return true;

        // 最新のクライアントバージョン取得
        var latestVersions = await LoadServerVersionsSync();
        var latestClientVersion = GetClientVersion(latestVersions);

        // 比較
        if (localClientVersion.Version < latestClientVersion.Version ||
            localClientVersion.Identifier != latestClientVersion.Identifier) return true;

        return false;
    }

    /// <summary>
    /// クライアントバージョンアップ
    /// </summary>
    public void ClientUpdate()
    {
        // アップデータ起動
        var arg = GlobalState.Instance.ApplicationGlobalSettings.CompanyCode + " " + GlobalState.Instance.ApplicationGlobalSettings.SignalingUserName + " " + GlobalState.Instance.ApplicationGlobalSettings.SignalingUserPassword;
        System.Diagnostics.Process.Start("ClientUpdater.exe", arg);

        // シャットダウン
        Application.Quit();
    }

    /// <summary>
    /// アセット更新(同期)
    /// </summary>
    public async UniTask AssetUpdateSync()
    {
        var serverVersions = await LoadServerVersionsSync();

        // アバター更新チェック
        var avatarVersion = GetAvatarVersion(serverVersions);

        if (IsAvatarUpdate(avatarVersion))
        {
            // 更新
            Debug.Log("Avatar need update.");

            await UpdateAvatarSync(avatarVersion);
        }
        else
        {
            // 更新不要
            Debug.Log("Avatar is up to date.");
        }

        // リソースパック更新チェック
        var resourcePackVersion = GetResourcePackVersion(serverVersions);

        if (IsResourcePackUpdate(resourcePackVersion))
        {
            // 更新
            Debug.Log("Resource pack need update.");

            await UpdateResourcePackSync(resourcePackVersion);
        }
        else
        {
            // 更新不要
            Debug.Log("Resource pack is up to date.");
        }

        // バージョンファイル出力
        WriteVersions(serverVersions);
    }

    /// <summary>
    /// アバタ－識別子を取得
    /// </summary>
    /// <returns></returns>
    public string GetAvatarIdentifier()
    {
        var versions = LoadLocalVersions();
        if (versions == null) return null;
        var avatarVersion = GetAvatarVersion(versions);
        if (avatarVersion == null) return null;

        return avatarVersion.Identifier;
    }

    // サーバーのバージョン情報をロード(同期)
    private async UniTask<ResponseAssetBundleVersions> LoadServerVersionsSync() => await WebServerManager.Instance.RequestVersionsSync(RequestVersionUrl);

    // ローカルのバージョン情報をロード
    private ResponseAssetBundleVersions LoadLocalVersions()
    {
        var filePath = FileHelper.GetLocalDataFolderPath() + "Versions.json";
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            try
            {
                return JsonUtility.FromJson<ResponseAssetBundleVersions>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Version json file parse failed: {e.Message}");
            }
        }
        else
        {
            Debug.Log("Version json file not found.");
        }
        return null;
    }

    // アバターの更新の必要があるかどうか
    private bool IsAvatarUpdate(ResponseAssetBundleVersion avatarVersion)
    {
        var localVersions = LoadLocalVersions();
        if (localVersions == null) return true;
        var localAvatarVersion = GetAvatarVersion(localVersions);
        if (localAvatarVersion == null) return true;

        return !Equals(localAvatarVersion, avatarVersion);
    }

    // リソースパックの更新の必要があるかどうか
    private bool IsResourcePackUpdate(ResponseAssetBundleVersion resourcePackVersion)
    {
        var localVersions = LoadLocalVersions();
        if (localVersions == null) return true;
        var localResourcePackVersion = GetResourcePackVersion(localVersions);
        if (localResourcePackVersion == null) return true;

        return !Equals(localResourcePackVersion, resourcePackVersion);
    }


    /// <summary>
    /// 指定タイプのバージョン取得
    /// </summary>
    /// <param name="versions"></param>
    /// <param name="typeName">取得するタイプ名</param>
    /// <returns></returns>
    private ResponseAssetBundleVersion GetVersionBlock(ResponseAssetBundleVersions versions, string typeName)
    {
        foreach (var version in versions.List)
        {
            if (version.Type == typeName)
            {
                return version;
            }
        }
        Debug.Log(typeName + " version not found.");
        return null;
    }


    // クライアントバージョン取得
    private ResponseAssetBundleVersion GetClientVersion(ResponseAssetBundleVersions versions) => GetVersionBlock(versions, "userClient");
    /*
    private ResponseAssetBundleVersion GetClientVersion(ResponseAssetBundleVersions versions)
    {

        return GetVersionBlock(versions, "userClient");
        foreach (var version in versions.List)
        {
            if (version.Type == "userClient")
            {
                return version;
            }
        }
        Debug.Log("Client version not found.");
        return null;
    }
    */

    // アバターバージョン取得
    private ResponseAssetBundleVersion GetAvatarVersion(ResponseAssetBundleVersions versions) => GetVersionBlock(versions, "avatar");
    /*
    private ResponseAssetBundleVersion GetAvatarVersion(ResponseAssetBundleVersion[] versions)
    {
        foreach (var version in versions)
        {
            if (version.Type == "avatar")
            {
                return version;
            }
        }
        Debug.Log("Avatar version not found.");
        return null;
    }
    */

    // リソースパックバージョン取得
    private ResponseAssetBundleVersion GetResourcePackVersion(ResponseAssetBundleVersions versions) => GetVersionBlock(versions, "assetPack");
    /*
    private ResponseAssetBundleVersion GetResourcePackVersion(ResponseAssetBundleVersion[] versions)
    {
        foreach (var version in versions)
        {
            if (version.Type == "assetPack")
            {
                return version;
            }
        }
        Debug.Log("Resource pack version not found.");
        return null;
    }
    */

    // アバター更新(同期)
    private async UniTask UpdateAvatarSync(ResponseAssetBundleVersion avatarVersion)
    {
        // ファイルダウンロード
        var bytes = await WebServerManager.Instance.RequestBytesSync(RequestFileUrl + avatarVersion.FileId);

        // アセットバンドルファイル出力
        AssetBundleManager.Instance.WriteAvatarAssetBundle(bytes);
    }

    // リソースパック更新(同期)
    private async UniTask UpdateResourcePackSync(ResponseAssetBundleVersion resoucePackVersion)
    {
        // ファイルダウンロード
        var bytes = await WebServerManager.Instance.RequestBytesSync(RequestFileUrl + resoucePackVersion.FileId);

        // アセットバンドルファイル出力
        AssetBundleManager.Instance.WriteResourcePackAssetBundle(bytes);
    }

    // バージョンファイル出力
    private void WriteVersions(ResponseAssetBundleVersions versions)
    {
        var json = JsonUtility.ToJson(versions);
        var filePath = FileHelper.GetLocalDataFolderPath() + "Versions.json";
        File.WriteAllText(filePath, json, Encoding.UTF8);
    }
}
