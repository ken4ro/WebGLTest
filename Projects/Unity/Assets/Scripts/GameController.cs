using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UniRx;
using static GlobalState;
using static SignageSettings;

/// <summary>
// 主にゲーム全体のステート管理を担う
/// </summary>
public class GameController : SingletonMonoBehaviour<GameController>
{
    [SerializeField]
    public Camera MainCamera = null;

    /// <summary>
    /// メインスレッドコンテキスト
    /// </summary>
    public SynchronizationContext MainContext { get; private set; } = null;

    /// <summary>
    /// クライアント(ユーザー or オペレーター)
    /// </summary>
    public IClient Client { get; private set; } = null;

    /// <summary>
    /// 現在の待機時間[s]
    /// </summary>
    public float CurrentIdleTimeSec { get; set; } = 0.0f;

    /// <summary>
    /// スクリーンセーバー種別
    /// </summary>
    public SignageSettings.ScreenSaverTypes CurrentScreenSaverType { get; set; } = SignageSettings.ScreenSaverTypes.None;

    // State管理
    private List<IState> _states = new List<IState>();

    protected override async void Awake()
    {
        base.Awake();

        // サイネージ設定ファイル読み込み
        SignageSettings.LoadSettings();
        CurrentScreenSaverType = SignageSettings.Settings.ScreenSaver;

        // GoogleService 設定ファイル読み込み
        GoogleService.ImportSettings();

        // メインスレッド同期用コンテキストを取得しておく
        MainContext = SynchronizationContext.Current;

        // クライアント初期化
        Client = new UserClient();
        Client.Initialize();

        // 全Stateセット
        _states.Add(new Waiting());
        _states.Add(new Starting());
        _states.Add(new Loading());
        _states.Add(new LoadingComplete());
        _states.Add(new LoadingError());
        _states.Add(new Speakable());
        _states.Add(new Speaking());
        _states.Add(new SpeakingComplete());
        _states.Add(new Disconnect());
        _states.Add(new PreOperating());
        _states.Add(new Operating());

        // イベント購読
        GlobalState.Instance.CurrentState.ObserveOnMainThread().Pairwise().Subscribe(x => OnStateChanged(x.Previous, x.Current)).AddTo(this.gameObject);
        BotManager.Instance.OnStartRequest += OnStartBotRequest;
        BotManager.Instance.OnCompleteRequest += OnCompleteBotRequest;
        BotManager.Instance.OnNoMatch += OnNoMatchBotRequest;
        UIManager.Instance.OnSelectLanguage += SelectLanguage;
        UIManager.Instance.OnSelectWord += SelectWord;
        UIManager.Instance.OnClickScreenSaver += ClickScreenSaver;

#if UNITY_EDITOR || !UNITY_WEBGL // CORS 対策が落ち着くまで無効化
        // 使用キャラクターセット
        GlobalState.Instance.CurrentCharacterModel.Value = CharacterModel.Una2D;

        // アバター読み込み
        //await AssetBundleManager.Instance.LoadAvatarAssetBundleFromStreamingAssets();
        AssetBundleManager.Instance.LoadAvatarAssetBundle();
#else
        // 使用キャラクターセット
        GlobalState.Instance.CurrentCharacterModel.Value = CharacterModel.Una2D;

        // アバター読み込み
        await AssetBundleManager.Instance.LoadAvatarAssetBundleFromStreamingAssets();
#endif

        // キャラクターオブジェクト作成
        LoadCharacterObject();

        // キャラクター表示
        CharacterManager.Instance.Enable();

        // ボット処理初期化
        BotManager.Instance.Initialize();

        // 指定時間待機
#if false
        var offsetSec = GlobalState.Instance.ApplicationGlobalSettings.StartOffsetSec;
        Observable.Timer(TimeSpan.FromSeconds(offsetSec)).Subscribe(_ =>
        {
            // ボット処理開始
            GlobalState.Instance.CurrentState.Value = State.Starting;
        });
#else
        //await UniTask.Delay(GlobalState.Instance.ApplicationGlobalSettings.StartOffsetSec * 1000);
        //StartBotProcess();
        //await UniTask.Delay(6000);
        //SetSpeakingText("お問い");
        //await UniTask.Delay(2000);
        //SetUserMessage("お問い合わせ");
#endif
    }

    void OnApplicationQuit()
    {
        // 各ゲームオブジェクトが破棄される前に行なければならない後始末
        // 同期的に実行する(原則 await 禁止)

        // 現在の状態の終了処理を呼んでおく
        _states[(int)GlobalState.Instance.CurrentState.Value].OnExit();
    }

    // Update is called once per frame
    void Update()
    {
        _states[(int)GlobalState.Instance.CurrentState.Value].OnUpdate();
    }

    public void StartBotProcess()
    {
        // ボット処理開始
        GlobalState.Instance.CurrentState.Value = State.Starting;
    }

    // ユーザーメッセージをセット
    public void SetUserMessage(string text)
    {
        // 音声認識の最終文字列としてセット
        StreamingSpeechToText.Instance.RecognitionCompleteText = text;

        GlobalState.Instance.CurrentState.Value = State.SpeakingComplete;
    }

    // 発話中文字列をセット
    public void SetSpeakingText(string text)
    {
        // 発話中文字列を表示
        UIManager.Instance.SetSpeakingText(text);

        GlobalState.Instance.CurrentState.Value = State.Speaking;
    }

    // 現在の処理状態が変更された際に一度だけ呼ばれる
    private void OnStateChanged(State previous, State current)
    {
        Debug.Log($"State {previous} To {current}");

        _states[(int)previous].OnExit();

        // ここに遷移時に行いたい処理を追加する

        _states[(int)current].OnEnter();
    }

    // ボット処理開始
    private void OnStartBotRequest()
    {
        // ボットリクエスト開始

        // ボット処理待ち状態へ移行
        GlobalState.Instance.CurrentState.Value = State.Loading;
    }

    private void OnCompleteBotRequest()
    {
        // ボットリクエスト完了

        // ボット処理完了状態へ移行
        GlobalState.Instance.CurrentState.Value = State.LoadingComplete;
    }

    private void OnNoMatchBotRequest()
    {
        // ボットリクエスト失敗

        // ボット処理失敗状態へ移行
        GlobalState.Instance.CurrentState.Value = State.LoadingError;
    }

    // UI 上で言語が変更された
    private async void SelectLanguage(Language language)
    {
        // 音声入力とボタン入力モードで処理タイミングの同期を取るため、頭で実行する
        CurrentLanguage.Value = language;

        // 音声認識キャンセル
        await StreamingSpeechToText.Instance.CancelRecognition();

        GlobalState.Instance.CurrentState.Value = State.Starting;

        // アラビア語特例処理
        if (language == Language.Arabic)
        {
            UIManager.Instance.RightToLeft();
        }
    }

    // UI 上で選択肢中の単語が選択された
    private void SelectWord(string text)
    {
        SetUserMessage(text);
    }

    // UI 上でスクリーンセーバーが解除された
    private void ClickScreenSaver()
    {
        // 先頭から開始
        GlobalState.Instance.CurrentState.Value = State.Starting;
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
        var frontCanvasIndex = GameObject.Find("FrontCanvas").transform.GetSiblingIndex();
        characterObject.transform.SetSiblingIndex(frontCanvasIndex);
    }
}