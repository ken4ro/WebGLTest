using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using static GlobalState;
using System.Text;
using System.IO;

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
    public TextMeshProUGUI Json1 = null;
    public TextMeshProUGUI Json2 = null;

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

        // ユーザートークン取得
        var userTokenUrl = "https://development.studio-sylphid.com:6500/cloud/api/v1/user/token/get";
        var jsonObject = new RequestUserTokenJson()
        {
            login_id = "user01user01user01",
            login_type = "basic",
            password = "passpasspass"
        };
        var json = JsonUtility.ToJson(jsonObject);
        var ret = await WebServerManager.Instance.RequestUserToken(userTokenUrl, json);

        var responseJsonObject = JsonUtility.FromJson<RequestUserTokenResponseJson>(ret);
        AccessToken.text = "Access token: " + Environment.NewLine + responseJsonObject.access_token;
        RefreshToken.text = "Refresh token: " + Environment.NewLine + responseJsonObject.refresh_token;
        ExpiresIn.text = "Expires in: " + responseJsonObject.expires_in;
        var base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseJsonObject.access_token));

        // フローの初期ノード呼び出し
        var callFirstNodeUrl = "https://development.studio-sylphid.com:6500/cloud/api/v1/flow/dialog/get";
        var requestCallFirstNodeJsonObject = new CallFirstNodeJson()
        {
            flow_id = "fba931ed-0a78-43a8-830f-22a17e4352ad-25b95b05-9622-4554-9965-ca4dbcd4bddb"
        };
        json = JsonUtility.ToJson(requestCallFirstNodeJsonObject);
        ret = await WebServerManager.Instance.CallFirstNode(callFirstNodeUrl, base64Token, json);
        var callFirstNodeResponseJsonObject = JsonUtility.FromJson<CallFirstNodeResponseJson>(ret);
        if (callFirstNodeResponseJsonObject.response.Text.Jp == null)
        {
            Debug.LogError($"CallFirstNode response parse error.");
        }
        else
        {
            Json1.text = callFirstNodeResponseJsonObject.response.Text.Jp;
        }

        // フローレスポンス取得
        var requestFlowUrl = "https://development.studio-sylphid.com:6500/cloud/api/v1/flow/dialog/put";
        var requestFlowJsonObject = new RequestFlowJson()
        {
            flow_id = "fba931ed-0a78-43a8-830f-22a17e4352ad-25b95b05-9622-4554-9965-ca4dbcd4bddb",
            utterance = "問い合わせ"
            //utterance = "もにょもにょ"
        };
        var requestFlowJson = JsonUtility.ToJson(requestFlowJsonObject);
        ret = await WebServerManager.Instance.RequestFlow(requestFlowUrl, base64Token, requestFlowJson);
        var requestFlowResponseJsonObject = JsonUtility.FromJson<RequestFlowResponseJson>(ret);
        if (requestFlowResponseJsonObject.response.Text.Jp == null)
        {
            Debug.LogError($"RequestFlow response parse error.");
        }
        else
        {
            Json2.text = requestFlowResponseJsonObject.response.Text.Jp;
        }

        /*
        // ユーザートークン更新
        var updateuserTokenUrl = "https://development.studio-sylphid.com:6500/cloud/api/v1/user/token/put";
        await WebServerManager.Instance.UpdateUserToken(updateuserTokenUrl);
        */
    }

    [Serializable]
    public class RequestUserTokenJson
    {
        public string login_id;
        public string login_type;
        public string password;
    }

    [Serializable]
    public class RequestUserTokenResponseJson
    {
        public string token_type;
        public string access_token;
        public string refresh_token;
        public int expires_in;
    }

    [Serializable]
    public class CallFirstNodeJson
    {
        public string flow_id;
    }

    [Serializable]
    public class CallFirstNodeResponseJson
    {
        public BotManager.BotResponse response;
    }

    [Serializable]
    public class RequestFlowJson
    {
        public string flow_id;
        public string utterance;
    }

    [Serializable]
    public class RequestFlowResponseJson
    {
        public BotManager.BotResponse response;
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