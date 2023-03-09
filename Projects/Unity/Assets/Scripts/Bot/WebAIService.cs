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
    public string NoMatchText => "よく分かりませんでした。もう一度お試しください。";

    // ユーザートークン
    public string UserToken { get; private set; } = "";

    // フローID
    private static readonly string FlowID = "d017cfa7-9570-4247-9218-dedc763d4977-00478dc6-bb12-44e7-85c2-ff22d40f3907";

    // ログインID
    private static readonly string LoginID = "71cf0d26-ddab-4744-82b5-f9e55b694e49-cd814c74-d4dd-49b6-b301-5236b33cc855-61ef3cd3-ace1-4d67-938f-e40a032351bf-c8fa9cdd-cbf7-4e5d-96ed-923c22c5914d";

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
            login_type = "anonymous",
            //password = LoginPassword
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
        var ret = new BotRequestResult();
        var responseStatus = BotResponseStatus.Success;

        // ボットリクエスト
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
            if (requestFirstNodeResponseJsonObject.response.Text.Jp == null)
            {
                responseStatus = BotResponseStatus.NoMatch;
                responseString = NoMatchText;
            }
            else
            {
                responseString = JsonUtility.ToJson(requestFirstNodeResponseJsonObject.response);
            }
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
            if (requestNextNodeResponseJsonObject.response.Text.Jp == null)
            {
                responseStatus = BotResponseStatus.NoMatch;
                responseString = NoMatchText;
            }
            else
            {
                responseString = JsonUtility.ToJson(requestNextNodeResponseJsonObject.response);
            }
        }

        ret.Status = responseStatus;
        ret.result = responseString;
        return ret;
    }
}