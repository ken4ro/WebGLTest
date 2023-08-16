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

    /// <summary>
    /// 初期化
    /// </summary>
    public async UniTask Initialize()
    {
        /*
        // ユーザートークン取得
        var jsonObject = new RequestUserTokenJson()
        {
            login_id = "kenken4ro",
            login_type = "basic",
            password = "00330033"
        };
        var json = JsonUtility.ToJson(jsonObject);
        var ret = await ApiServerManager.Instance.RequestUserToken(json);
        var responseJsonObject = JsonUtility.FromJson<RequestUserTokenResponseJson>(ret);
        UserToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseJsonObject.access_token));
        //var refreshToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseJsonObject.refresh_token));
        //var newToken = await ApiServerManager.Instance.UpdateUserToken(refreshToken);
        //Debug.Log($"new token = {newToken}");
        //UserToken = Convert.ToBase64String(Encoding.UTF8.GetBytes("ff614d48-edf6-41f6-a801-ef9d432ee009-71b02eee-8b3e-4129-b487-415cbd9e2cff-d8613f58-2321-4554-993e-9b093745d7cb"));
        // ユーザー設定取得
        var settings = await ApiServerManager.Instance.RequestUserSetting(UserToken);
        Debug.Log($"UserSettings = {settings}");
        */
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
        var userTokenBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(GlobalState.Instance.UserSettings.UserToken));
        if (isInit)
        {
            // フローの初期ノードをリクエスト
            var requestFirstNodeJsonObject = new RequestFirstNodeJson()
            {
                flow_id = GlobalState.Instance.UserSettings.Bot.CcgFlowId
            };
            var requestFirstNodeJson = JsonUtility.ToJson(requestFirstNodeJsonObject);
            responseString = await ApiServerManager.Instance.RequestFirstNodeAsync(userTokenBase64, requestFirstNodeJson);
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
                flow_id = GlobalState.Instance.UserSettings.Bot.CcgFlowId,
                utterance = inputText
                //utterance = "もにょもにょ"
            };
            var requestFlowJson = JsonUtility.ToJson(requestFlowJsonObject);
            responseString = await ApiServerManager.Instance.RequestNextNodeAsync(userTokenBase64, requestFlowJson);
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