using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using UnityEngine;
using UniRx;
using static GlobalState;
using static SignageSettings;
using Cysharp.Threading.Tasks;

public partial class BotManager : SingletonBase<BotManager>
{
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
    /// リクエスト失敗時テキスト
    /// </summary>
    public string NoMatchText { get => _botLanguageProcessor.NoMatchText; }

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
        public BotResponseStatus Status { get; set; }

        public string result { get; set; }
    }

    // Botサービス別処理
    private IBotService _botServiceProcessor = null;

    // 言語別処理
    private ABotLanguageProcessor _botLanguageProcessor = null;
    private IDisposable _currentBotLanguageObserver = null;

    // 選択肢
    private static readonly int MaxSelectCount = 4;
    private List<string> _selectImages = new List<string>(MaxSelectCount);

    private List<BotResponseOption> _options = new List<BotResponseOption>();
    
    /// <summary>
    /// 初期化
    /// </summary>
    public async UniTask Initialize()
    {
        IsInitialized = false;

        // チャットボットサービス別処理用クラス生成
        // 動的に変わることは無いので監視対象外
        CreateChatbotServiceProcessor();

        // チャットボットサービス初期化
        await _botServiceProcessor.Initialize();

        // 言語月処理用クラス生成
        // 動的に変わるので監視対象
        _currentBotLanguageObserver = CurrentLanguage.Subscribe(x => CreateBotLanguageProcessor(x));

        IsInitialized = true;
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    public void Dispose()
    {
        _botServiceProcessor = null;
        _botLanguageProcessor = null;
        _currentBotLanguageObserver.Dispose();
    }

    /// <summary>
    /// チャットボットサービスを初期化して先頭から開始する
    /// </summary>
    /// <returns></returns>
    public async UniTask Reset()
    {
        IsInitialized = false;

        // Botサービスリセット
        var resetResponse = await _botServiceProcessor.Reset();

        IsInitialized = true;

        await Request(true, "init");
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
            Debug.LogError("BotManager not initialized");
        }

        // イベント通知
        OnStartRequest?.Invoke();

        //Debug.Log($"BotManager Request: {inputText}");

        // リクエスト
        var response = await _botServiceProcessor.Request(isInit, inputText);

        Debug.Log($"BotManager response result = {response.result}");

        switch (response.Status)
        {
            case BotResponseStatus.Success:
                break;
            case BotResponseStatus.BadRequest:
                break;
            case BotResponseStatus.ParseError:
                break;
            case BotResponseStatus.NoMatch:
                OnNoMatch?.Invoke();
                return;
            default:
                break;
        }

        // レスポンス取得
        BotResponse responseObj = null;
        try
        {
            var str = ReplaceVariable(response.result);
            responseObj = JsonUtility.FromJson<BotResponse>(str);
        }
        catch (Exception)
        {
            // JSON パースエラー
            UnrecognizedResponse();
        }
        Response = responseObj;

        // イベント通知
        OnCompleteRequest?.Invoke();
    }

    /// <summary>
    /// 空リクエスト送信
    /// </summary>
    /// <returns></returns>
    public async UniTask RequestEmpty() => await Request(false, "");

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
    public string GetVoice() => _botLanguageProcessor.GetVoice(Response);

    /// <summary>
    /// リクエスト結果から表示文字列を取得
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public string GetText() => _botLanguageProcessor.GetText(Response);

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
    /// リクエスト結果から選択肢のテキストリストを取得
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    //public List<string> GetSelectTexts() => _botLanguageProcessor.GetSelectTexts(Response);
    public List<BotResponseSelect> GetSelectObjects() => _botLanguageProcessor.GetSelectObjects(Response);

    /// <summary>
    /// リクエスト結果からコントローラーデバイスに送信用のテキストリストを取得
    /// </summary>
    /// <returns></returns>
    public string[] GetSendTexts() => Response.Send;

    /// <summary>
    /// リクエスト結果から表示動画ファイル名を取得
    /// </summary>
    public string GetMovie() => Response.Movie;

    private void CreateChatbotServiceProcessor()
    {
        _botServiceProcessor = new WebAIService();
    }

    private void CreateBotLanguageProcessor(Language language)
    {
        switch (language)
        {
            case Language.Japanese:
                _botLanguageProcessor = new BotLanguageProcessorJapanese();
                break;
            case Language.English:
                _botLanguageProcessor = new BotLanguageProcessorEnglish();
                break;
            case Language.Chinese:
                _botLanguageProcessor = new BotLanguageProcessorChinese();
                break;
            case Language.Russian:
                _botLanguageProcessor = new BotLanguageProcessorRussian();
                break;
            case Language.Arabic:
                _botLanguageProcessor = new BotLanguageProcessorArabic();
                break;
            case Language.Vietnamese:
                _botLanguageProcessor = new BotLanguageProcessorVietnamese();
                break;
            default:
                break;
        }
    }

    private void UnrecognizedResponse()
    {
        UIManager.Instance.DisplayError(ErrorCode.ReplUnrecognizedResponse);
        GlobalState.Instance.CurrentState.Value = State.Waiting;
    }

    private void RequestTimeout()
    {
        UIManager.Instance.DisplayError(ErrorCode.Network);
        GlobalState.Instance.CurrentState.Value = State.Waiting;
    }

    private void Unauthorized()
    {
        UIManager.Instance.DisplayError(ErrorCode.ReplUnauthorized);
        GlobalState.Instance.CurrentState.Value = State.Waiting;
    }

    private void BadRequest()
    {
        UIManager.Instance.DisplayError(ErrorCode.ReplBadRequest);
        GlobalState.Instance.CurrentState.Value = State.Waiting;
    }
}

