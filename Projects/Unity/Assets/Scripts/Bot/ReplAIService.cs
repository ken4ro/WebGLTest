using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using static BotManager;
using Cysharp.Threading.Tasks;

public partial class ReplAIService : IBotService
{
    /// <summary>
    /// 全角→半角変換テーブル
    /// Repl側でなぜか全角に変換されて返してくることがある
    /// 全角未対応のフォント対策
    /// </summary>
    public static readonly Dictionary<char, char> CharacterByteTable = new Dictionary<char, char>() {
        {'１','1'},{'２','2'},{'３','3'},{'４','4'},{'５','5'},
        {'６','6'},{'７','7'},{'８','8'},{'９','9'},{'０','0'},
        {'Ａ','A'},{'Ｂ','B'},{'Ｃ','C'},{'Ｄ','D'},{'Ｅ','E'},
        {'Ｆ','F'},{'Ｇ','G'},{'Ｈ','H'},{'Ｉ','I'},{'Ｊ','J'},
        {'Ｋ','K'},{'Ｌ','L'},{'Ｍ','M'},{'Ｎ','N'},{'Ｏ','O'},
        {'Ｐ','P'},{'Ｑ','Q'},{'Ｒ','R'},{'Ｓ','S'},{'Ｔ','T'},
        {'Ｕ','U'},{'Ｖ','V'},{'Ｗ','W'},{'Ｘ','X'},{'Ｙ','Y'},
        {'Ｚ','Z'},
        {'ａ','a'},{'ｂ','b'},{'ｃ','c'},{'ｄ','d'},{'ｅ','e'},
        {'ｆ','f'},{'ｇ','g'},{'ｈ','h'},{'ｉ','i'},{'ｊ','j'},
        {'ｋ','k'},{'ｌ','l'},{'ｍ','m'},{'ｎ','n'},{'ｏ','o'},
        {'ｐ','p'},{'ｑ','q'},{'ｒ','r'},{'ｓ','s'},{'ｔ','t'},
        {'ｕ','u'},{'ｖ','v'},{'ｗ','w'},{'ｘ','x'},{'ｙ','y'},
        {'ｚ','z'},
        {'　',' '},
    };

    /// <summary>
    /// APIキー
    /// プロジェクト毎に固有の値が設定される
    /// デフォルトは「穴吹2019」プロジェクト
    /// </summary>
    public string ApiKey { get; set; } = "CPeHyw2gVc6oVu2oP3UTeoJNASKYZJNVzbSGQb4J";

    /// <summary>
    /// ボットID
    /// BOT毎に固有の値が設定される
    /// デフォルトは「マンション管理人」ボット
    /// </summary>
    public string BotId { get; set; } = "b4s3kldj559y0ag";

    /// <summary>
    /// シナリオID
    /// シナリオ毎に固有の値が設定される
    /// デフォルトは「v3」シナリオ
    /// </summary>
    public string ScenarioId { get; set; } = "s4sm2pztvgl80oc";

    // ユーザーID取得用URL
    private static readonly string GetUserIdUrl = "https://api.repl-ai.jp/v1/registration";

    // 対話用URL
    private static readonly string DialogueUrl = "https://api.repl-ai.jp/v1/dialogue";

    // Relp-AI ユーザーID
    private static string _replAIUserId = "";

    /// <summary>
    /// 初期化
    /// </summary>
    public UniTask Initialize()
    {
        LoadSettingsSync();

        return UniTask.CompletedTask;
    }

    public async UniTask<BotRequestResult> Reset()
    {
        var response = await GetUserIdAsync();

        _replAIUserId = response.result;

        return response;
    }

    public async UniTask<BotRequestResult> Request(bool isInit, string inputText)
    {
        var response = await Dialogue(_replAIUserId, isInit, inputText);

        return response;
    }

    // ユーザーID取得(非同期)
    private async UniTask<BotRequestResult> GetUserIdAsync()
    {
        var headers = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("Content-Type", "application/json"),
            new KeyValuePair<string, string>("x-api-key", ApiKey),
        };
        var reqObj = new GetUserIdRequest
        {
            BotId = BotId
        };
        var reqJson = JsonUtility.ToJson(reqObj);
        var reqBody = new StringContent(reqJson, Encoding.UTF8, "application/json");
        try
        {
            // リクエスト
            var res = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, GetUserIdUrl, headers, reqBody);
            // レスポンス取得
            var ret = new BotRequestResult();
            ret.Status = res.StatusCode;
            if (ret.Status == HttpStatusCode.OK)
            {
                var resObj = JsonUtility.FromJson<GetUserIdResponse>(res.Json);
                ret.result = resObj.AppUserId;
            }
            return ret;
        }
        catch (Exception e)
        {
            Debug.LogError($"GetUserId request failed: {e.Message}");
            return null;
        }
    }

    // ユーザーID取得
    private BotRequestResult GetUserIdSync()
    {
        var headers = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("Content-Type", "application/json"),
            new KeyValuePair<string, string>("x-api-key", ApiKey),
        };
        var reqObj = new GetUserIdRequest
        {
            BotId = BotId
        };
        var reqJson = JsonUtility.ToJson(reqObj);
        var reqBody = new StringContent(reqJson, Encoding.UTF8, "application/json");
        try
        {
            // リクエスト
            var res = HttpRequest.RequestJsonSync(HttpRequestType.POST, GetUserIdUrl, headers, reqBody);
            // レスポンス取得
            var ret = new BotRequestResult
            {
                Status = res.StatusCode
            };
            if (ret.Status == HttpStatusCode.OK)
            {
                var resObj = JsonUtility.FromJson<GetUserIdResponse>(res.Json);
                ret.result = resObj.AppUserId;
            }
            return ret;
        }
        catch (Exception e)
        {
            Debug.LogError($"GetUserId request failed: {e.Message}");
            return null;
        }
    }

    // 対話
    private async UniTask<BotRequestResult> Dialogue(string userId, bool isInit, string inputText)
    {
        // リクエスト
        var headers = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("Content-Type", "application/json"),
            new KeyValuePair<string, string>("x-api-key", ApiKey),
        };
        var reqObj = new DialogueRequest
        {
            AppUserId = userId,
            BotId = BotId,
            InitTalkingFlag = isInit,
            VoiceText = inputText,
            InitTopicId = ScenarioId
        };
        var reqJson = JsonUtility.ToJson(reqObj);
        var requestBody = new StringContent(reqJson, Encoding.UTF8, "application/json");
        try
        {
            // リクエスト
            var res = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, DialogueUrl, headers, requestBody);
            var ret = new BotRequestResult
            {
                Status = res.StatusCode
            };
            if (ret.Status == HttpStatusCode.OK)
            {
                // レスポンスを解析してシステム返答を取得
                var resObj = JsonUtility.FromJson<DialogueResponse>(res.Json);
                var expression = resObj.SystemText.Expression;
                // 全角→半角変換
                string convertExpression = new string(expression.Select(c => (CharacterByteTable.ContainsKey(c) ? CharacterByteTable[c] : c)).ToArray());
                ret.result = convertExpression;
            }
            return ret;
        }
        catch (Exception e)
        {
            Debug.LogError($"Dialogue request failed: {e.Message}");
            return null;
        }
    }

    // 設定ファイル読み込み(同期)
    private void LoadSettingsSync()
    {
        var settingFileAsset = AssetBundleManager.Instance.LoadTextAssetFromResourcePack("ReplAISettings");
        if (settingFileAsset == null)
        {
            Debug.LogError("Repl setting file load error.");
            return;
        }
        var json = settingFileAsset.text.Trim(new char[] { '\uFEFF' });
        var settingObj = JsonUtility.FromJson<ReplAISettings>(json);
        ApiKey = settingObj.ApiKey;
        BotId = settingObj.BotId;
        ScenarioId = settingObj.ScenarioId;
    }
}
