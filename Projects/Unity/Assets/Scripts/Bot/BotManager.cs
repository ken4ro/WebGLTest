using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using UnityEngine;
using static Global;
using Cysharp.Threading.Tasks;
using static ApiServerManager;
using System.Text;

public partial class BotManager : SingletonBase<BotManager>
{
    // フローID
    private static readonly string FlowID = "fba931ed-0a78-43a8-830f-22a17e4352ad-25b95b05-9622-4554-9965-ca4dbcd4bddb";

    // ログインID
    private static readonly string LoginID = "user01user01user01";

    // ログインパスワ－ド
    private static readonly string LoginPassword = "passpasspass";

    /// <summary>
    /// Bot処理開始時コールバック
    /// </summary>
    public Action OnStartRequest = null;

    /// <summary>
    /// Bot処理完了時コールバック
    /// </summary>
    public Action OnCompleteRequest = null;

    /// <summary>
    /// Bot処理失敗時コールバック
    /// </summary>
    public Action OnNoMatch = null;

    /// <summary>
    /// Bot初期化完了フラグ
    /// </summary>
    public bool IsInitialized { get; set; } = false;

    /// <summary>
    /// Botレスポンス
    /// </summary>
    public BotResponse Response { get; private set; } = null;

    /// <summary>
    /// 現在のシナリオ階層(仮)
    /// </summary>
    public ScenarioHierarchy CurrentHierarchy { get; set; } = ScenarioHierarchy.None;
    public enum ScenarioHierarchy
    {
        None,
        Top,
        Second,
        Third,
        Four,
        Five,
    }

    public enum BotResponseStatus
    {
        Success,
        BadRequest,
        ParseError,
        NoMatch,
    }

    public class BotRequestResult
    {
        public HttpStatusCode Status { get; set; }

        public string result { get; set; }
    }

    // ユーザートークン
    public string UserToken { get; private set; } = "";

    // 選択肢
    private static readonly int MaxSelectCount = 4;
    private List<string> _selectImages = new List<string>(MaxSelectCount);

    /// <summary>
    /// 初期化
    /// </summary>
    public async UniTask Initialize()
    {
        IsInitialized = false;

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

        IsInitialized = true;
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// リクエスト
    /// </summary>
    /// <param name="init"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public async UniTask Request(bool isInit, string inputText = null)
    {
        if (!IsInitialized)
        {
            Debug.Log("BotManager.Request: BotManager not initialized");

            return;
        }

        // イベント通知
        OnStartRequest?.Invoke();

        // ボットリクエスト
        BotResponseStatus responseStatus;
        string ret;
        if (isInit)
        {
            // フローの初期ノードをリクエスト
            var requestFirstNodeJsonObject = new RequestFirstNodeJson()
            {
                flow_id = FlowID
            };
            var json = JsonUtility.ToJson(requestFirstNodeJsonObject);
            ret = await ApiServerManager.Instance.RequestFirstNode(UserToken, json);
            var requestFirstNodeResponseJsonObject = JsonUtility.FromJson<RequestFirstNodeResponseJson>(ret);
            Response = requestFirstNodeResponseJsonObject.response;
            if (requestFirstNodeResponseJsonObject.response.Text.Jp == null)
            {
                Debug.LogError($"RequestFirstNode response parse error.");
                responseStatus = BotResponseStatus.ParseError;
            }
            else
            {
                Debug.Log($"RequestFirstNode response text: {requestFirstNodeResponseJsonObject.response.Text.Jp}");
                responseStatus = BotResponseStatus.Success;
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
            ret = await ApiServerManager.Instance.RequestNextNode(UserToken, requestFlowJson);
            var requestNextNodeResponseJsonObject = JsonUtility.FromJson<RequestNextNodeResponseJson>(ret);
            Response = requestNextNodeResponseJsonObject.response;
            if (requestNextNodeResponseJsonObject.response.Text.Jp == null)
            {
                Debug.LogError($"RequestNextNode response parse error.");
                responseStatus = BotResponseStatus.ParseError;
            }
            else
            {
                Debug.Log($"RequestNextNode response text: {requestNextNodeResponseJsonObject.response.Text.Jp}");
                responseStatus = BotResponseStatus.Success;
            }
        }

        if (responseStatus == BotResponseStatus.NoMatch)
        {
            // イベント通知
            OnNoMatch?.Invoke();

            return;
        }

        // イベント通知
        OnCompleteRequest?.Invoke();
    }

    /// <summary>
    /// 空リクエスト送信
    /// </summary>
    /// <returns></returns>
    public async UniTask RequestEmpty()
    {
        await Request(false, "");
    }

    /// <summary>
    /// リクエスト結果からシーン種別を取得
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public string GetScene() => Response.Scene;

    /// <summary>
    /// リクエスト結果から音声文字列を取得
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public string GetVoice() => Response.Voice.Jp;

    /// <summary>
    /// リクエスト結果から表示文字列を取得
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public string GetText() => Response.Text.Jp;

    /// <summary>
    /// リクエスト結果からキャラクターアニメーションを取得
    /// </summary>
    /// <returns></returns>
    public string GetMotion() => Response.Motion;

    /// <summary>
    /// リクエスト結果からアクション番号を取得
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public string GetAction() => Response.Action;

    /// <summary>
    /// リクエスト結果から画像種別を取得
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public string GetImage() => Response.Image;

    /// <summary>
    /// リクエスト結果からブラウザ情報を取得
    /// </summary>
    /// <returns></returns>
    public BotResponseBrowser GetBrowser()
    {
        if (Response.Browser.Size == null) return null;

        return Response.Browser;
    }

    /// <summary>
    /// リクエスト結果から選択肢画像を取得
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public List<string> GetSelectImages()
    {
        if (Response.Selects == null || Response.Selects.Length == 0) return null;

        _selectImages.Clear();
        for (var i = 0; i < Response.Selects.Length; i++)
        {
            _selectImages.Add(Response.Selects[i].Image);
        }
        return _selectImages;
    }

    /// <summary>
    /// リクエスト結果から選択肢のリストを取得
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public List<BotResponseSelect> GetSelectObjects() => new List<BotResponseSelect>(Response.Selects);

    /// <summary>
    /// リクエスト結果からコントローラーデバイスに送信用のテキストリストを取得
    /// </summary>
    /// <returns></returns>
    public string[] GetSendTexts() => Response.Send;

    /// <summary>
    /// リクエスト結果から表示動画ファイル名を取得
    /// </summary>
    public string GetMovie() => Response.Movie;
}

