using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using static GlobalState;
using static Background;
using static BotManager;
using static SignageSettings;
using static CharacterMessage;

public class UIManager : SingletonMonoBehaviour<UIManager>
{
    /// <summary>
    /// 録音確認画面で賛同した
    /// </summary>
    public Action OnAgreeRecording = null;

    /// <summary>
    /// 録音確認画面で拒否した
    /// </summary>
    public Action OnRejectRecording = null;

    /// <summary>
    /// オペレーター呼び出し画面でキャンセルボタンが押された
    /// </summary>
    public Action OnCallingCancelled = null;

    /// <summary>
    /// 言語が選択された
    /// </summary>
    public Action<Language> OnSelectLanguage = null;

    /// <summary>
    /// 選択肢中の単語が選択された
    /// </summary>
    public Action<string> OnSelectWord = null;

    /// <summary>
    /// スクリーンセーバーがクリックされた
    /// </summary>
    public Action OnClickScreenSaver = null;

    /// <summary>
    /// UI描画用メインキャンバス(キャラクターより奥)
    /// </summary>
    [SerializeField] GameObject mainCanvas = null;

    /// <summary>
    /// UI描画用サブキャンバス(キャラクターより手前)
    /// </summary>
    [SerializeField] GameObject subCanvas = null;

    /// <summary>
    /// 背景
    /// </summary>
    [SerializeField] Background background = null;

    /// <summary>
    /// キャラクターメッセージ
    /// </summary>UIManager
    [SerializeField] CharacterMessage characterMessage = null;

    /// <summary>
    /// ユーザーメッセージ
    /// </summary>
    [SerializeField] UserMessage userMessage = null;

    /// <summary>
    /// 発話中メッセージ
    /// </summary>
    [SerializeField] SpeakingMessage speakingMessage = null;

    /// <summary>
    /// マスク処理用パネル
    /// </summary>
    [SerializeField] GlassMaskPanel glassMaskPanel = null;

    /// <summary>
    /// キャラクター処理待ち中アイコン
    /// </summary>
    [SerializeField] LoadingIcon loadingIcon = null;

    /// <summary>
    /// 発話可能状態表示アイコン
    /// </summary>
    [SerializeField] SpeakableIcon speakableIcon = null;

    /// <summary>
    /// ボタン押下可能状態表示アイコン(シンプル)
    /// </summary>
    [SerializeField] PushableSimpleIcon pushableSimpleIcon = null;

    /// <summary>
    /// ボタン押下可能状態表示アイコン(ココアくん)
    /// </summary>
    [SerializeField] PushableCocoaIcon pushableCocoaIcon = null;

    /// <summary>
    /// フルスクリーンパネル
    /// </summary>
    [SerializeField] FullScreenPanel fullScreenPanel = null;

    /// <summary>
    /// Webカメラ映像表示用パネル
    /// </summary>
    [SerializeField] WebCameraPanel webCameraPanel = null;

    /// <summary>
    /// 受領資料表示用パネル
    /// </summary>
    [SerializeField] ReceivedDocumentPanel receivedDocumentPanel = null;

    /// <summary>
    /// オペレーター呼び出し画面
    /// </summary>
    [SerializeField] CallingPanel callingPanel = null;

    /// <summary>
    /// エラー内容表示用パネル
    /// </summary>
    [SerializeField] ErrorDisplayPanel errorDisplayPanel = null;

    /// <summary>
    /// 言語グループ
    /// </summary>
    [SerializeField] CanvasGroup languageGroup = null;

    /// <summary>
    /// 言語ウィンドウ
    /// </summary>
    [SerializeField] LangWindow languageWindow = null;

    /// <summary>
    /// 録音確認画面
    /// </summary>
    [SerializeField] RecordingCheckPanel recordingCheckPanel = null;

    // メインスレッド同期用コンテキスト(いずれ必要になりそう)
    private SynchronizationContext _mainContext = null;

    // 言語変更を監視
    private IDisposable _changeLanguageObserver = null;

    protected override void Awake()
    {
        base.Awake();

        // メインスレッド同期用コンテキストを取得しておく
        _mainContext = SynchronizationContext.Current;

        // 各委譲クラス初期化
        // TODO: インターフェース作成
        background.Initialize();
        userMessage.Initialize();
        speakingMessage.Initialize();
        speakableIcon.Initialize();
        pushableSimpleIcon.Initialize();
        pushableCocoaIcon.Initialize();
        webCameraPanel.Initialize();
        receivedDocumentPanel.Initialize();
        callingPanel.Initialize();
        errorDisplayPanel.Initialize();
        fullScreenPanel.Initialize();
        recordingCheckPanel.Initialize();

        callingPanel.OnCancelled += CancelCalling;
        languageWindow.OnSelectLanguage += SelectLanguage;
        characterMessage.OnClickSelectWord += SelectWord;
        characterMessage.OnImageClick += ClickThumbnailPanel;
        fullScreenPanel.OnClick += ClickFullScreenPanel;
        recordingCheckPanel.OnClickYesBtn += AgreeRecording;
        recordingCheckPanel.OnClickNoBtn += RejectRecording;
    }

    protected override void OnDestroy()
    {
        callingPanel.OnCancelled -= CancelCalling;
        languageWindow.OnSelectLanguage -= SelectLanguage;
        characterMessage.OnClickSelectWord -= SelectWord;
        characterMessage.OnImageClick -= ClickThumbnailPanel;
        fullScreenPanel.OnClick -= ClickFullScreenPanel;
        recordingCheckPanel.OnClickYesBtn -= AgreeRecording;
        recordingCheckPanel.OnClickNoBtn -= RejectRecording;

        _changeLanguageObserver.Dispose();

        base.OnDestroy();
    }

    /// <summary>
    /// 言語変更を監視
    /// </summary>
    public void SetLanguageObserver()
    {
        _changeLanguageObserver = CurrentLanguage.Subscribe(x => { ChangeLanguage(x); });
    }

    /// <summary>
    /// 表示リセット
    /// </summary>
    /// <returns></returns>
    public async UniTask Reset()
    {
        // 受付可能アイコン
        FadeOutAcceptableIcon();
        // キャラクターメッセージ
        ResetCharacterMessage();
        // 発話中メッセージ
        DisableSpeakingMaskPanel();
        SetSpeakingText("");
        DisableSpeakingMessage();
        // ユーザーメッセージ
        SetUserText("");
        DisableUserMessage();
        // WebCamera
        DisableWebCameraPanel();
        // 受領資料
        DisableReceivedDocumentPanel();
        // オペレーター呼び出し画面
        DisableCallingPanel();
        // 言語ウィンドウ
        DisableLanguagePanel();
        //フルスクリーンパネル
        DisableFullScreenPanel();
    }

    /// <summary>
    /// キャラクターメッセージのフォントサイズをセット
    /// </summary>
    /// <param name="size"></param>
    public void SetFontSize(int size) => characterMessage.SetFontSize(size);

    /// <summary>
    /// 背景画像遷移
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fadeInSec"></param>
    /// <param name="fadeOutSec"></param>
    public async UniTask TransitionBackground(BackgroundType bgType, TransitionType transType, float msec) => await background.Transition(bgType, transType, msec);

    /// <summary>
    /// 受付可能状態表示用アイコンをフェードイン
    /// </summary>
    public void FadeInAcceptableIcon()
    {
        // TODO: インターフェースで書き直し
        switch (GlobalState.Instance.CurrentAcceptableAnimationType)
        {
            case AcceptableAnimationType.Simple:
                FadeInPushableSimpleIcon();
                break;
            case AcceptableAnimationType.Cocoa:
                FadeInPushableCocoaIcon();
                break;
            case AcceptableAnimationType.Pac:
                FadeInSpeakableIcon();
                break;
        }
    }

    /// <summary>
    /// 受付可能状態表示用アイコンをフェードアウト
    /// </summary>
    public void FadeOutAcceptableIcon()
    {
        // TODO: インターフェース使って書き直し
        switch (GlobalState.Instance.CurrentAcceptableAnimationType)
        {
            case AcceptableAnimationType.Simple:
                FadeOutPushableSimpleIcon();
                break;
            case AcceptableAnimationType.Cocoa:
                FadeOutPushableCocoaIcon();
                break;
            case AcceptableAnimationType.Pac:
                FadeOutSpeakableIcon();
                break;
        }
    }

    /// <summary>
    /// 発話受付中アイコンフェードイン
    /// </summary>
    public void FadeInSpeakableIcon() => speakableIcon.FadeIn();

    /// <summary>
    /// 発話受付中アイコンフェードアウト
    /// </summary>
    public void FadeOutSpeakableIcon() => speakableIcon.FadeOut();

    /// <summary>
    /// ボタン押下受付中アイコン(シンプル)フェードイン
    /// </summary>
    public void FadeInPushableSimpleIcon() => pushableSimpleIcon.FadeIn();

    /// <summary>
    /// ボタン押下受付中アイコン(シンプル)フェードアウト
    /// </summary>
    public void FadeOutPushableSimpleIcon() => pushableSimpleIcon.FadeOut();

    /// <summary>
    /// ボタン押下受付中アイコン(ココアくん)フェードイン
    /// </summary>
    public void FadeInPushableCocoaIcon() => pushableCocoaIcon.FadeIn();

    /// <summary>
    /// ボタン押下受付中アイコン(ココアくん)フェードアウト
    /// </summary>
    public void FadeOutPushableCocoaIcon() => pushableCocoaIcon.FadeOut();

    /// <summary>
    /// Bot処理待ち中アイコンを表示する
    /// </summary>
    public void EnableLoadingIcon() => loadingIcon.Enable();

    /// <summary>
    /// Bot処理待ち中アイコンを非表示にする
    /// </summary>
    public void DisableLoadingIcon() => loadingIcon.Disable();

    /// <summary>
    /// 発話ウィンドウを表示する
    /// </summary>
    public void EnableUserMessage() => userMessage.Enable();

    /// <summary>
    /// 発話ウィンドウを非表示にする
    /// </summary>
    public void DisableUserMessage() => userMessage.Disable();

    /// <summary>
    /// 発話中ウィンドウを表示する
    /// </summary>
    public void EnableSpeakingMessage() => speakingMessage.Enable();

    /// <summary>
    /// 発話中ウィンドウを非表示にする
    /// </summary>
    public void DisableSpeakingMessage() => speakingMessage.Disable();

    /// <summary>
    /// 発話中にバック背景を暗くするウィンドウを表示する
    /// </summary>
    public void EnableSpeakingMaskPanel() => glassMaskPanel.Enable();

    /// <summary>
    /// 発話中にバック背景を暗くするウィンドウを非表示にする
    /// </summary>
    public void DisableSpeakingMaskPanel() => glassMaskPanel.Disable();

    #region fullscreen
    /// <summary>
    /// フルスクリーンパネル表示
    /// </summary>
    public void EnableFullScreenPanel() => fullScreenPanel.EnablePanel();

    /// <summary>
    /// フルスクリーンパネル非表示
    /// </summary>
    public void DisableFullScreenPanel() => fullScreenPanel.DisablePanel();

    /// <summary>
    /// フルスクリーン用のイメージ設定
    /// </summary>
    /// <param name="img"></param>
    public void SetFullScreenImage(Texture2D img) => fullScreenPanel.SetImage(img);

    #endregion

    /// <summary>
    /// Webカメラ映像表示用パネルを有効にする
    /// </summary>
    public void EnableWebCameraPanel() => webCameraPanel.Enable();

    /// <summary>
    /// Webカメラ映像表示用パネルを無効にする
    /// </summary>
    public void DisableWebCameraPanel() => webCameraPanel.Disable();

    /// <summary>
    /// 受領資料表示用パネルを有効にする
    /// </summary>
    public void EnableReceivedDocumentPanel() => receivedDocumentPanel.Enable();

    /// <summary>
    /// 受領資料表示用パネルを無効にする
    /// </summary>
    public void DisableReceivedDocumentPanel() => receivedDocumentPanel.Disable();

    /// <summary>
    /// オペレーター呼び出し画面を有効にする
    /// </summary>
    public void EnableCallingPanel() => callingPanel.Enable();

    /// <summary>
    /// オペレーター呼び出し画面を無効にする
    /// </summary>
    public void DisableCallingPanel() => callingPanel.Disable();

    /// <summary>
    /// 言語パネルを表示する
    /// </summary>
    public void EnableLanguagePanel()
    {
        languageGroup.gameObject.SetActive(true);
        languageGroup.gameObject.transform.DOLocalMoveX(0.0f, 0.8f).SetEase(Ease.OutBack);
        languageGroup.DOFade(1.0f, 0.8f);
        Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
        {
            languageGroup.blocksRaycasts = true;
        });
    }

    /// <summary>
    /// 言語パネルを非表示にする
    /// </summary>
    public void DisableLanguagePanel()
    {
        languageGroup.blocksRaycasts = false;
        languageGroup.gameObject.transform.DOLocalMoveX(-700.0f, 0.5f).SetEase(Ease.InExpo);
        languageGroup.DOFade(0.0f, 0.5f);
        Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
        {
            languageGroup.gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// エラーを専用画面で表示する
    /// </summary>
    /// <param name="error"></param>
    public void DisplayError(string error) => errorDisplayPanel.Display(error);

    /// <summary>
    /// エラーを専用画面で表示する
    /// </summary>
    /// <param name="code"></param>
    public void DisplayError(ErrorCode code) => errorDisplayPanel.Display(code);

    /// <summary>
    /// 発話中メッセージのテキストを取得
    /// </summary>
    /// <returns></returns>
    public string GetSpeakingText() => speakingMessage.GetSpeakingText();

    /// <summary>
    /// 発話メッセージのテキストを取得
    /// </summary>
    /// <returns></returns>
    public string GetUserText() => userMessage.GetUserText();

    /// <summary>
    /// キャラクターメッセージをセット
    /// </summary>
    /// <param name="message"></param>
    /// <param name="isAnim"></param>
    /// <param name="imageType"></param>
    /// <param name="imageFileName"></param>
    /// <param name="fullImageFileName"></param>
    /// <param name="selectTexts"></param>
    /// <param name="selectImageName"></param>
    /// <returns></returns>
    public async UniTask SetCharacterMessage(string message, bool isAnim, ImageAccessTypes imageType, string imageFileName, string fullImageFileName)
    {
        if (!String.IsNullOrEmpty(fullImageFileName))
        {
            var tex = await GraphicsHelper.LoadImage(imageType, fullImageFileName);
            fullScreenPanel.SetImage(tex);
        }
        characterMessage.EnablePanelEvent(String.IsNullOrEmpty(fullImageFileName) ? false : true);

        await characterMessage.SetCharacterMessage(message, isAnim, imageType, imageFileName);
    }

    /// <summary>
    /// キャラクターメッセージ動画をセット
    /// </summary>
    /// <param name="movieFilePath"></param>
    public void SetCharacterMessageMovie(string movieFilePath)
    {
        characterMessage.SetCharacterMessageMovie(movieFilePath);
    }

    /// <summary>
    /// キャラクター選択肢用メッセージをセット
    /// </summary>
    /// <param name="selectTexts"></param>
    //public void SetCharacterSelectMessage(List<string> selectTexts) => characterMessage.SetSelectMessage(selectTexts);
    public void SetCharacterSelectMessage(List<BotResponseSelect> selectObjects) => characterMessage.SetSelectObjects(selectObjects, languageWindow.GetActiveLanguage());

    /// <summary>
    /// キャラクターメッセージをリセット
    /// </summary>
    /// <returns></returns>
    public void ResetCharacterMessage() => characterMessage.ResetCharacterMessage();

    /// <summary>
    /// 選択肢メッセージをリセット
    /// </summary>
    public void ResetSelectMessage() => characterMessage.ResetSelectMessage();

    /// <summary>
    /// ユーザーメッセージのテキストをセット
    /// </summary>
    /// <param name="text"></param>
    /// <param name="isAnim"></param>
    /// <returns></returns>
    public void SetUserText(string text) => userMessage.SetUserText(text);

    /// <summary>
    /// 発話中メッセージのテキストをセット
    /// </summary>
    /// <param name="text"></param>
    public void SetSpeakingText(string text) => speakingMessage.SetSpeakingText(text);

    /// <summary>
    /// WebCameraキャプチャ開始
    /// </summary>
    public void WebCameraPlay() => webCameraPanel.Play();

    /// <summary>
    /// WebCameraキャプチャ停止
    /// </summary>
    public void WebCameraStop() => webCameraPanel.Stop();

    /// オペレーター呼び出し開始
    /// </summary>
    public void BeginCalling() => callingPanel.Call();

    /// <summary>
    /// オペレーター呼び出し成功
    /// </summary>
    public void EndCalling() => callingPanel.Connect();

    /// <summary>
    /// 資料データを受け取る
    /// </summary>
    /// <param name="data"></param>
    public void ReceiveDocument(byte[] data) => receivedDocumentPanel.ReceiveTextureData(data);

    /// <summary>
    /// 文字表示を左から右に変更する
    /// </summary>
    public void RightToLeft()
    {
        characterMessage.RightToLeft();
    }

    /// <summary>
    /// 文字表示を右から左に変更する
    /// </summary>
    public void LeftToRight()
    {
        characterMessage.LeftToRight();
    }

    /// <summary>
    /// 録音確認画面を有効にする
    /// </summary>
    public void EnableRecordingCheckPanel() => recordingCheckPanel.gameObject.SetActive(true);

    /// <summary>
    /// 録音確認画面を無効にする
    /// </summary>
    public void DisableRecordingCheckPanel() => recordingCheckPanel.gameObject.SetActive(false);

    /// <summary>
    /// 録音確認画面の Yes ボタンを押下する
    /// </summary>
    public void ClickRecordingPanelBtnYes() => recordingCheckPanel.ClickYes();

    /// <summary>
    /// 録音確認画面の No ボタンを押下する
    /// </summary>
    public void ClickRecordingPanelBtnNo() => recordingCheckPanel.ClickNo();

    #region イベント

    // 発信がキャンセルされた
    private void CancelCalling()
    {
        OnCallingCancelled?.Invoke();
    }

    // 言語がUI上で選択(変更)された
    private void SelectLanguage(Language language)
    {
        // イベント発行
        OnSelectLanguage?.Invoke(language);
    }

    // 言語が外部から変更された
    private void ChangeLanguage(Language language)
    {
        // 言語ウィンドウに反映
        languageWindow.SetLanguage(language);
    }

    // 選択肢が選択された
    private void SelectWord(string text)
    {
        // イベント発行
        OnSelectWord?.Invoke(text);
    }

    // スクリーンセーバーがクリックされた
    private void ClickScreenSaver()
    {
        // イベント発行
        //languageWindow.SetDefaultLang(); // 不要なはず
        OnClickScreenSaver?.Invoke();
    }

    //フルスクリーンパネルがクリックされた
    private void ClickFullScreenPanel()
    {
        GameController.Instance.CurrentIdleTimeSec = 0.0f;
    }

    //サムネイル画像がクリックされた
    private void ClickThumbnailPanel()
    {
        EnableFullScreenPanel();
        GameController.Instance.CurrentIdleTimeSec = 0.0f;

    }

    // 録音確認画面で賛同した
    private void AgreeRecording()
    {
        OnAgreeRecording?.Invoke();
    }

    // 録音確認画面で拒否した
    private void RejectRecording()
    {
        OnRejectRecording?.Invoke();
    }

    #endregion イベント
}
