using System.Net;
using System.IO;
using UnityEngine;
//using Grpc.Auth;
//using Grpc.Core;
//using Google.Apis.Auth.OAuth2;
//using Google.Cloud.Dialogflow.V2;
using static BotManager;
using Cysharp.Threading.Tasks;

/*
public partial class DialogflowService : IBotService
{
    // TODO: ファイル暗号化＆ファイル名統一
    private static readonly string _serviceAccountFilePath = "comcierge-db2dc67655c9.json";

    private string _projectId = "";
    private string _sessionId = "00330033";
    private static Channel _channel = null;
    private static SessionsClient _client = null;

    public void Initialize()
    {
        // 設定ファイル読み込み
        LoadSettingsSync();

        // サービスアカウントファイル読み込み
        var serviceAccountFileAsset = AssetBundleManager.Instance.ResourcePackAssetBundle.LoadAsset<TextAsset>(Path.GetFileNameWithoutExtension(_serviceAccountFilePath));
        if (serviceAccountFileAsset == null)
        {
            Debug.LogError("Dialogflow service account file load error.");
            return;
        }

        // Dialogflow 認証処理
        var credentialFileData = serviceAccountFileAsset.bytes;
        GoogleCredential _credential;
        using (var stream = new MemoryStream(credentialFileData))
        {
            _credential = GoogleCredential.FromStream(stream);
        }
        _channel = new Channel(SessionsClient.DefaultEndpoint.Host, _credential.ToChannelCredentials());
        _client = SessionsClient.Create(_channel);
    }

    public async UniTask<BotRequestResult> Reset()
    {
        // 現状は特に何もしない

        var ret = new BotRequestResult() { result = "", Status = HttpStatusCode.OK };

        return ret;
    }

    public async UniTask<BotRequestResult> Request(bool isInit, string inputText)
    {
        var ret = new BotRequestResult();

        var session = new SessionName(_projectId, _sessionId);
        var queryInput = new QueryInput() { Text = new TextInput() { Text = inputText, LanguageCode = "ja-JP" } };
        var response = await _client.DetectIntentAsync(session, queryInput);

        if (response.QueryResult.Intent.DisplayName == "Default Fallback Intent")
        {
            ret.Status = HttpStatusCode.NotFound;
            ret.result = "NOMATCH";
        }
        else
        {
            ret.Status = HttpStatusCode.OK;
            ret.result = response.QueryResult.FulfillmentText;
        }
        Debug.Log($"Query result: {ret.result}");

        return ret;
    }

    private void LoadSettingsSync()
    {
        var settingFileAsset = AssetBundleManager.Instance.LoadTextAssetFromResourcePack("DialogflowSettings");
        if (settingFileAsset == null)
        {
            Debug.LogError("Dialogflow setting file load error.");
            return;
        }
        var json = settingFileAsset.text.Trim(new char[] { '\uFEFF' });
        var settingObj = JsonUtility.FromJson<DialogflowSettings>(json);
        _projectId = settingObj.ProjectId;
    }
}
*/

public partial class DialogflowService : IBotService
{
    public UniTask Initialize()
    {
        return UniTask.CompletedTask;
    }

    public async UniTask<BotRequestResult> Reset()
    {
        return null;
    }

    public async UniTask<BotRequestResult> Request(bool isInit, string inputText)
    {
        return null;
    }

    private void LoadSettingsSync()
    {
    }
}
