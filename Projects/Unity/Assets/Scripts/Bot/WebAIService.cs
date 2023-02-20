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
using static ApiServerManager;

public partial class WebAIService : IBotService
{
    /// <summary>
    /// リクエスト失敗時テキスト
    /// </summary>
    public string NoMatchText => "よく分かりませんでした";

    // ユーザートークン
    public string UserToken { get; private set; } = "";

    public enum BotResponseStatus
    {
        Success,
        BadRequest,
        ParseError,
        NoMatch,
    }

    // フローID
    private static readonly string FlowID = "fba931ed-0a78-43a8-830f-22a17e4352ad-25b95b05-9622-4554-9965-ca4dbcd4bddb";

    // ログインID
    private static readonly string LoginID = "user01user01user01";

    // ログインパスワ－ド
    private static readonly string LoginPassword = "passpasspass";

    /// <summary>
    /// 初期化
    /// </summary>
    public async UniTask Initialize()
    {
        // ユーザートークン取得
        var jsonObject = new RequestUserTokenJson()
        {
            login_id = LoginID,
            login_type = "basic",
            password = LoginPassword
        };
        var json = JsonUtility.ToJson(jsonObject);
        var ret = await ApiServerManager.Instance.RequestUserToken(json);
        var responseJsonObject = JsonUtility.FromJson<RequestUserTokenResponseJson>(ret);
        UserToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseJsonObject.access_token));
    }

    public async UniTask<BotRequestResult> Reset()
    {
        return null;
    }

    public async UniTask<BotRequestResult> Request(bool isInit, string inputText)
    {
        Debug.Log("Request");

        var ret = new BotRequestResult();

        // ボットリクエスト
        BotResponseStatus responseStatus;
        string responseString;
        if (isInit)
        {
            // フローの初期ノードをリクエスト
            var requestFirstNodeJsonObject = new RequestFirstNodeJson()
            {
                flow_id = FlowID
            };
            var json = JsonUtility.ToJson(requestFirstNodeJsonObject);
            responseString = await ApiServerManager.Instance.RequestFirstNode(UserToken, json);
            var requestFirstNodeResponseJsonObject = JsonUtility.FromJson<RequestFirstNodeResponseJson>(responseString);
            if (requestFirstNodeResponseJsonObject.response.ToString() == ".")
            {
                responseStatus = BotResponseStatus.NoMatch;
            }
            responseString = JsonUtility.ToJson(requestFirstNodeResponseJsonObject.response);
        }
        else
        {
            // 次のノードをリクエスト
            var requestFlowJsonObject = new RequestNextNodeJson()
            {
                flow_id = FlowID,
                utterance = inputText
                //utterance = "もにょもにょ"
            };
            var requestFlowJson = JsonUtility.ToJson(requestFlowJsonObject);
            responseString = await ApiServerManager.Instance.RequestNextNode(UserToken, requestFlowJson);
            var requestNextNodeResponseJsonObject = JsonUtility.FromJson<RequestNextNodeResponseJson>(responseString);
            if (requestNextNodeResponseJsonObject.response.ToString() == ".")
            {
                responseStatus = BotResponseStatus.NoMatch;
            }
            responseString = JsonUtility.ToJson(requestNextNodeResponseJsonObject.response);
        }

        ret.Status = HttpStatusCode.OK;
        ret.result = responseString;
        return ret;
    }
}