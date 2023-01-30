using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using static GlobalState;

/// <summary>
// 主にゲーム全体のステート管理やプロセス遷移を担う
/// </summary>
public class GameController : SingletonMonoBehaviour<GameController>
{
    [SerializeField]
    public Camera MainCamera = null;

    [SerializeField]
    public TextMeshProUGUI AccessToken = null;
    public TextMeshProUGUI RefreshToken = null;
    public TextMeshProUGUI ExpiresIn = null;

    /// <summary>
    /// メインスレッドコンテキスト
    /// </summary>
    public SynchronizationContext MainContext { get; private set; } = null;

    protected override async void Awake()
    {
        base.Awake();

        // メインスレッド同期用コンテキストを取得しておく
        MainContext = SynchronizationContext.Current;

#if UNITY_EDITOR || !UNITY_WEBGL // CORS 対策が落ち着くまで無効化
        if (await WebServerManager.Instance.HealthCheck())
        {
            // アセットバージョンアップ
            await VersionUpdateManager.Instance.AssetUpdateSync();
        }

        // 使用キャラクターセット
        var identifier = VersionUpdateManager.Instance.GetAvatarIdentifier();
        if (Enum.TryParse(identifier, true, out CharacterModel characterModel))
        {
            GlobalState.Instance.CurrentCharacterModel = characterModel;
        }
        else if (identifier == "una2d_webgl")
        {
            GlobalState.Instance.CurrentCharacterModel = CharacterModel.Una2D;
        }
        else
        {
            Debug.LogError($"Mismatch between identifier and character model: {identifier}");
        }

        // アバター読み込み
        AssetBundleManager.Instance.LoadAvatarAssetBundle();
#else
        // アバター読み込み
        await AssetBundleManager.Instance.LoadAvatarAssetBundleFromStreamingAssets();
#endif

        // キャラクターオブジェクト作成
        LoadCharacterObject();

        // キャラクター表示
        CharacterManager.Instance.Enable();

        // 1秒後に実行
        await UniTask.Delay(millisecondsDelay: 1000);

#if false
        // ユーザートークン取得
        var userTokenUrl = "https://development.studio-sylphid.com:6500/cloud/api/v1/user/token/get";
        var jsonObject = new RequestUserTokenJson()
        {
            login_id = "dummy@client1",
            login_type = "basic",
            password = "4nZaR6On"
        };
        var json = JsonUtility.ToJson(jsonObject);
        var ret = await WebServerManager.Instance.RequestUserToken(userTokenUrl, json);
        var responseJsonObject = JsonUtility.FromJson<ResponseUserTokenJson>(ret);
        AccessToken.text = "Access token: " + Environment.NewLine + responseJsonObject.access_token;
        RefreshToken.text = "Refresh token: " + Environment.NewLine + responseJsonObject.refresh_token;
        ExpiresIn.text = "Expires in: " + responseJsonObject.expires_in;

        // ユーザートークン更新
        //var updateuserTokenUrl = "https://development.studio-sylphid.com:6500/cloud/api/v1/user/token/put";
        //await WebServerManager.Instance.UpdateUserToken(updateuserTokenUrl);
#endif
    }

    public class RequestUserTokenJson
    {
        public string login_id;
        public string login_type;
        public string password;
    }

    public class ResponseUserTokenJson
    {
        public string token_type;
        public string access_token;
        public string refresh_token;
        public string expires_in;
    }

    void OnApplicationQuit()
    {
        // 各ゲームオブジェクトが破棄される前に行なければならない後始末
        // 同期的に実行する(原則 await 禁止)
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LoadCharacterObject()
    {
        var assetBundle = AssetBundleManager.Instance.AvatarAssetBundle;

        // キャラクターオブジェクト作成
        GameObject characterObject = assetBundle.LoadAsset<GameObject>("Una2D");
        if (characterObject != null)
        {
            Debug.Log($"LoadCharacterObject: LoadAsset completed.");
        }
        else
        {
            Debug.Log($"LoadCharacterObject: LoadAsset failed.");
        }
        //switch (GlobalState.Instance.CurrentCharacterModel)
        //{
        //    case CharacterModel.Una3D:
        //        characterObject = assetBundle.LoadAsset<GameObject>("Una");
        //        break;
        //    case CharacterModel.Una2D:
        //        characterObject = assetBundle.LoadAsset<GameObject>("Una2D");
        //        break;
        //    case CharacterModel.Una2D_Rugby:
        //        characterObject = assetBundle.LoadAsset<GameObject>("Una2D_Rugby");
        //        break;
        //}
        characterObject = Instantiate(characterObject);
        if (characterObject != null)
        {
            Debug.Log($"LoadCharacterObject: Instantiate completed.");
        }
        else
        {
            Debug.Log($"LoadCharacterObject: Instantiate failed.");
        }

        // Front Canvas より奥に描画するようにする
        //var frontCanvasIndex = GameObject.Find("FrontCanvas").transform.GetSiblingIndex();
        //characterObject.transform.SetSiblingIndex(frontCanvasIndex);
    }
}