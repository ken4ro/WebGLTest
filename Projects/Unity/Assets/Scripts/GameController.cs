using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using static Global;
using UniRx;

/// <summary>
// 主にゲーム全体のステート管理を担う
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

        // メインスレッド同期用コンテキストを取得しておく
        MainContext = SynchronizationContext.Current;

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
        Global.Instance.CurrentState.ObserveOnMainThread().Pairwise().Subscribe(x => OnStateChanged(x.Previous, x.Current)).AddTo(this.gameObject);
        BotManager.Instance.OnStartRequest += OnStartBotRequest;
        BotManager.Instance.OnCompleteRequest += OnCompleteBotRequest;
        BotManager.Instance.OnNoMatch += OnNoMatchBotRequest;

#if UNITY_EDITOR || !UNITY_WEBGL // CORS 対策が落ち着くまで無効化

        // 使用キャラクターセット
        Global.Instance.CurrentCharacterModel = CharacterModel.Una2D;

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

        // ボット処理初期化
        await BotManager.Instance.Initialize();

        // 指定時間待機
#if false
        var offsetSec = Global.Instance.ApplicationGlobalSettings.StartOffsetSec;
        Observable.Timer(TimeSpan.FromSeconds(offsetSec)).Subscribe(_ =>
        {
            // ボット処理開始
            Global.Instance.CurrentState.Value = State.Starting;
        });
#else
        // 1秒後に実行（仮）
        await UniTask.Delay(millisecondsDelay: 1000);
        // ボット処理開始
        Global.Instance.CurrentState.Value = State.Starting;
#endif
    }

    void OnApplicationQuit()
    {
        // 各ゲームオブジェクトが破棄される前に行なければならない後始末
        // 同期的に実行する(原則 await 禁止)

        // 現在の状態の終了処理を呼んでおく
        _states[(int)Global.Instance.CurrentState.Value].OnExit();
    }

    // Update is called once per frame
    void Update()
    {
        _states[(int)Global.Instance.CurrentState.Value].OnUpdate();
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
        Global.Instance.CurrentState.Value = State.Loading;
    }

    private void OnCompleteBotRequest()
    {
        // ボットリクエスト完了

        // ボット処理完了状態へ移行
        Global.Instance.CurrentState.Value = State.LoadingComplete;
    }

    private void OnNoMatchBotRequest()
    {
        // ボットリクエスト失敗

        // ボット処理失敗状態へ移行
        Global.Instance.CurrentState.Value = State.LoadingError;
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