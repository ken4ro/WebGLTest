using System;
using UniRx;
using Live2D.Cubism.Framework;
using static Background;

public class Operating : IState
{
    private const uint TimeoutTime = 30;
    private bool _isCalling = false;
    private IDisposable _timeoutObservable = null;

    private bool _isSignalingServerLogin = false;

    public async void OnEnter()
    {
        // シグナリング中はトップに戻れない
        GlobalState.Instance.CanReturnToTop = false;

        // UIリセット
        await UIManager.Instance.Reset();

        // キャラクターアニメーション無効化
        CharacterManager.Instance.DisableAnimation();
        // アイドルモーション時のキャラクタートランスフォームをセット
        CharacterManager.Instance.SetTransformsForIdle();

        // オペレーター呼び出し画面表示開始
        UIManager.Instance.OnCallingCancelled += CancelCalling;
        UIManager.Instance.BeginCalling();
        _isCalling = true;

        // ネットワーク接続初期化
        await ConnectionManager.Instance.Initialize();
        GameController.Instance.Client.AddHandler();

        // シグナリングサーバーへログイン(ローカル接続でも使用する)
        SignalingManager.Instance.OnConnected += LoginSignalingServer;
        SignalingManager.Instance.OnLoginSucceeded += LoginSucceeded;
        SignalingManager.Instance.OnLoginFailed += LoginFailed;
        if (SignalingManager.Instance.IsConnected)
        {
            SignalingManager.Instance.Login();
        }
        else
        {
            SignalingManager.Instance.Connect();
        }

        // タイムアウト処理
        _timeoutObservable = Observable.Timer(TimeSpan.FromSeconds(TimeoutTime)).Subscribe(_ =>
        {
            CancelCalling();
        });
    }

    public void OnUpdate()
    {
        if (_isCalling && ConnectionManager.Instance.IsAvailable)
        {
            // タイムアウト処理キャンセル
            _timeoutObservable?.Dispose();

            // ネットワーク接続完了メッセージをオペレーターに送信
            var msg = "connection ok";
            SignalingManager.Instance.RelayToOperator(msg);
            System.Threading.Thread.Sleep(20);

            // オペレーター呼び出し画面表示終了
            UIManager.Instance.EndCalling();
            _isCalling = false;

            // 接続画面移行
            Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(x =>
            {
                // 背景画像遷移
                _ = UIManager.Instance.TransitionBackground(BackgroundType.Telexistence, TransitionType.ScaleY, 1000);
            });

            // 接続完了後の処理
            Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(_ =>
            {
                // 顔情報更新開始
                FaceInfoManager.Instance.Enable();
                // 接続先音声再生開始
                AudioManager.Instance.PlayTargetVoice();
            });

            // マイク録音開始
            MicManager.Instance.Initialize();
            MicManager.Instance.StartRecording();

            // WebCamera 映像表示
            // 重いので先に実行しておく
            UIManager.Instance.EnableWebCameraPanel();
            UIManager.Instance.WebCameraPlay();

            // シグナリングサーバーログアウト
            // ログアウトコマンドは無くなった模様なのでセッションを切る
            //SignalingManager.Instance.Logout();
            SignalingManager.Instance.DisConnect();
            _isSignalingServerLogin = false;
        }
    }

    public void OnExit()
    {
        // タイムアウト処理キャンセル
        _timeoutObservable?.Dispose();
        // オペレーター呼び出し画面表示終了
        UIManager.Instance.OnCallingCancelled -= CancelCalling;
        UIManager.Instance.DisableCallingPanel();
        // 背景リセット
        _ = UIManager.Instance.TransitionBackground(BackgroundType.Default, TransitionType.ScaleY, 1000);
        // 受領資料非表示
        UIManager.Instance.DisableReceivedDocumentPanel();
        // 接続先音声再生終了
        AudioManager.Instance.StopTargetVoice();
        // WebCamera 終了
        UIManager.Instance.DisableWebCameraPanel();
        UIManager.Instance.WebCameraStop();
        // 顔情報更新終了
        FaceInfoManager.Instance.Disable();
        // マイク録音終了
        MicManager.Instance.StopRecording();
        MicManager.Instance.Quit();
        // シグナリングサーバーログアウト
        if (_isSignalingServerLogin)
        {
            //SignalingManager.Instance.Logout();
            SignalingManager.Instance.DisConnect();
            _isSignalingServerLogin = false;
        }
        SignalingManager.Instance.OnConnected -= LoginSignalingServer;
        SignalingManager.Instance.OnLoginSucceeded -= LoginSucceeded;
        SignalingManager.Instance.OnLoginFailed -= LoginFailed;
        // ネットワーク接続終了
        GameController.Instance.Client.RemoveHandler();
        _ = ConnectionManager.Instance.Dispose();
        // キャラクターアニメーションを有効にする
        CharacterManager.Instance.EnableAnimation();
        // 目パチ処理をリセット
        var eyeBlinkController = CharacterManager.Instance.gameObject.GetComponent<CubismEyeBlinkController>();
        eyeBlinkController?.Refresh();

        _isCalling = false;
    }

    private void LoginSignalingServer()
    {
        SignalingManager.Instance.Login();
    }

    private void LoginSucceeded()
    {
        _isSignalingServerLogin = true;
    }

    private void LoginFailed(string msg)
    {
        UIManager.Instance.DisplayError(msg);
        GlobalState.Instance.CurrentState.Value = GlobalState.State.Waiting;
    }

    private void CancelCalling()
    {
        GlobalState.Instance.CurrentState.Value = GlobalState.State.Starting;
    }
}
