using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using static GlobalState;
using static SignageSettings;
using static ApiServerManager;

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
    /// 現在の待機時間[s]
    /// </summary>
    public float CurrentIdleTimeSec { get; set; } = 0.0f;

    /// <summary>
    /// スクリーンセーバーが有効か
    /// </summary>
    public string ScreenSaverEnable { get; set; }

    // 初期化フラグ
    private bool _isInitialized = false;

    // State管理
    private List<IState> _states = new List<IState>();

    protected override async void Awake()
    {
        base.Awake();

        // ユーザー設定(Web版は現状 anonymous 固定)
        GlobalState.Instance.UserSettings = new UserSettings()
        {
            LoginId = "724e242c-03fd-40b3-bcf1-a6071b613f86-b3608b4b-a347-467f-a2e0-5aa3cb3b9c78-3a8482bc-14a1-47ab-b192-1a4963b3858f-44966299-8d87-463d-a168-487cadf1ffd3",
            LoginType = "anonymous",
        };

        // ユーザートークン取得
        var userTokenJsonObject = new RequestUserTokenJson()
        {
            login_id = GlobalState.Instance.UserSettings.LoginId,
            login_type = GlobalState.Instance.UserSettings.LoginType,
            password = GlobalState.Instance.UserSettings.Password
        };
        var userTokenJson = JsonUtility.ToJson(userTokenJsonObject);
        var responseUserTokenJson = await ApiServerManager.Instance.RequestUserTokenAsync(userTokenJson);
        var responseUserTokenJsonObject = JsonUtility.FromJson<RequestUserTokenResponseJson>(responseUserTokenJson);
        GlobalState.Instance.UserSettings.UserToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseUserTokenJsonObject.access_token));
        GlobalState.Instance.UserSettings.RefreshToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseUserTokenJsonObject.refresh_token));
        GlobalState.Instance.UserSettings.ExpiresIn = responseUserTokenJsonObject.expires_in;

        // ユーザー設定取得
        var userSettingsJson = await ApiServerManager.Instance.RequestUserSettingAsync(GlobalState.Instance.UserSettings.UserToken);
        var userSettingsObject = JsonUtility.FromJson<RequestUserSettingsResponseJson>(userSettingsJson);
        GlobalState.Instance.UserSettings.GoogleKey = userSettingsObject.google_key;
        GlobalState.Instance.UserSettings.UI = new UserSettingsUI();
        GlobalState.Instance.UserSettings.UI.RequestType = userSettingsObject.ui.request_type;
        GlobalState.Instance.UserSettings.UI.FontSize = userSettingsObject.ui.font_size;
        GlobalState.Instance.UserSettings.UI.WaitAnimationType = userSettingsObject.ui.wait_animation_type;
        GlobalState.Instance.UserSettings.UI.RecordingAgreementEnable = userSettingsObject.ui.recording_agreement_enable;
        GlobalState.Instance.UserSettings.UI.ScreensaverEnable = userSettingsObject.ui.screensaver_enable;
        GlobalState.Instance.UserSettings.UI.TextSpeed = userSettingsObject.ui.text_speed;
        GlobalState.Instance.UserSettings.UI.InputLimitSec = userSettingsObject.ui.input_limit_sec;
        GlobalState.Instance.UserSettings.UI.Languages = userSettingsObject.ui.languages;
        GlobalState.Instance.UserSettings.Bot = new UserSettingsBot();
        GlobalState.Instance.UserSettings.Bot.ActionDelaySec = userSettingsObject.bot.action_delay_sec;
        GlobalState.Instance.UserSettings.Bot.CcgFlowId = userSettingsObject.bot.ccg_flow_id;
        GlobalState.Instance.UserSettings.Bot.RestartSec = userSettingsObject.bot.restart_sec;
        GlobalState.Instance.UserSettings.Bot.ReturnSec = userSettingsObject.bot.return_sec;
        GlobalState.Instance.UserSettings.Bot.ServiceType = userSettingsObject.bot.service_type;
        GlobalState.Instance.UserSettings.Bot.StartDelaySec = userSettingsObject.bot.start_delay_sec;
        GlobalState.Instance.UserSettings.Bot.VoiceType = userSettingsObject.bot.voice_type;
        GlobalState.Instance.UserSettings.Rtc = new UserSettingsRtc();
        GlobalState.Instance.UserSettings.Rtc.ServiceType = userSettingsObject.rtc.service_type;

        // フォントサイズセット
        UIManager.Instance.SetFontSize(GlobalState.Instance.UserSettings.UI.FontSize);

        // その他設定
        SignageSettings.LoadSettings();
        GoogleService.ImportSettings();

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

        // 言語変更監視
        UIManager.Instance.SetLanguageObserver();

        // メインスレッド同期用コンテキストを取得しておく
        MainContext = SynchronizationContext.Current;

        // スクリーンセーバー設定
        ScreenSaverEnable = GlobalState.Instance.UserSettings.UI.ScreensaverEnable;

        // イベント購読
        GlobalState.Instance.CurrentState.ObserveOnMainThread().Pairwise().Subscribe(x => OnStateChanged(x.Previous, x.Current)).AddTo(this.gameObject);
        BotManager.Instance.OnStartRequest += OnStartBotRequest;
        BotManager.Instance.OnCompleteRequest += OnCompleteBotRequest;
        BotManager.Instance.OnNoMatch += OnNoMatchBotRequest;
        UIManager.Instance.OnSelectLanguage += SelectLanguage;
        UIManager.Instance.OnSelectWord += SelectWord;
        UIManager.Instance.OnClickScreenSaver += ClickScreenSaver;

        // 使用キャラクターセット
        GlobalState.Instance.CurrentCharacterModel.Value = CharacterModel.Una2D;

        // アバター読み込み
        await AssetBundleManager.Instance.LoadAvatarAssetBundleFromStreamingAssets();

        // キャラクターオブジェクト作成
        LoadCharacterObject();

        // キャラクター表示
        CharacterManager.Instance.Enable();

        // ボット処理初期化
        _ = BotManager.Instance.Initialize();

        // 指定時間待機
#if false
        var offsetSec = GlobalState.Instance.ApplicationGlobalSettings.StartOffsetSec;
        Observable.Timer(TimeSpan.FromSeconds(offsetSec)).Subscribe(_ =>
        {
            // ボット処理開始
            GlobalState.Instance.CurrentState.Value = State.Starting;
        });
#else
        //await UniTask.Delay(GlobalState.Instance.UserSettings.Bot.StartDelaySec + 2 * 1000);
        //StartBotProcess();
        //await UniTask.Delay(6000);
        //SetSpeakingText("お問い");
        //await UniTask.Delay(2000);
        //SetUserMessage("お問い合わせ");
#endif

        _isInitialized = true;
    }

    void OnApplicationQuit()
    {
        if (_isInitialized)
        {
            // 現在の状態の終了処理を呼んでおく
            _states[(int)GlobalState.Instance.CurrentState.Value].OnExit();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isInitialized)
        {
            _states[(int)GlobalState.Instance.CurrentState.Value].OnUpdate();
        }
    }

    public float[] ConvertToFloat(byte[] buf)
    {
        // IEEE Float
        float[] ret = new float[buf.Length / 4];

        for (int i = 0; i < buf.Length - buf.Length % 4; i += 4)
        {
            float v = BitConverter.ToSingle(buf, i);
            ret[i / 4] = v;
        }
        return ret;
    }

    #region for JavaScript

    public class AudioVolumeJson
    {
        public float Volume;
    }

    public void SetVoiceVolume(float volume)
    {
        Debug.Log($"SetVoiceVolume: volume = {volume}");
        //var volume = JsonUtility.FromJson<AudioVolumeJson>(json).Volume;
        CharacterManager.Instance.SetMouseOpenYParameter(volume);
    }

    public class FaceInfoJson
    {
        public float Yaw;
        public float Pitch;
        public float Roll;
        public double BodyYaw;
        public double BodyPitch;
        public double BodyRoll;
    }

    private bool _isCharacterAnimation = true;
    public void SetFaceInfo(string faceInfoJson)
    {
        if (_isCharacterAnimation)
        {
            _isCharacterAnimation = false;
            // キャラクターアニメーション無効化
            CharacterManager.Instance.DisableAnimation();
            // アイドルモーション時のキャラクタートランスフォームをセット
            CharacterManager.Instance.SetTransformsForIdle();
            // 顔認識有効
            FaceInfoManager.Instance.Enable();
        }

        FaceInfoManager.Instance.FaceInfoReceived(faceInfoJson);
    }

    #endregion for JavaScript

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
    private void SelectLanguage(Language language)
    {
        // 音声入力とボタン入力モードで処理タイミングの同期を取るため、頭で実行する
        CurrentLanguage.Value = language;

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