using System;
using UnityEngine;


public class SignalingManager : SingletonMonoBehaviour<SignalingManager>
{
    /// <summary>
    /// シグナリングサーバーと接続された
    /// </summary>
    public Action OnConnected = null;

    /// <summary>
    /// シグナリングサーバーへのログインに成功した
    /// </summary>
    public Action OnLoginSucceeded = null;

    /// <summary>
    /// シグナリングサーバーへのログインに失敗した
    /// </summary>
    public Action<string> OnLoginFailed = null;

    /// <summary>
    /// シグナリングサーバーからのログアウトに成功した
    /// </summary>
    public Action OnLogoutSucceeded = null;

    /// <summary>
    /// シグナリングサーバーからのログアウトに失敗した
    /// </summary>
    public Action OnLogoutFailed = null;

    /// <summary>
    /// シグナリングサーバーにユーザーがログインした
    /// </summary>
    public Action OnUserLogin = null;

    /// <summary>
    /// シグナリングサーバーから待機中のユーザー数を受け取った
    /// </summary>
    public Action<int> OnReceivedUserCount = null;

    /// <summary>
    /// シグナリングサーバーからユーザーの Peer ID を受け取った
    /// </summary>
    public Action<string> OnReceivedUserPeerId = null;

    /// <summary>
    /// ユーザーがシグナリングをキャンセルした
    /// </summary>
    public Action OnUserCancel = null;

    /// <summary>
    /// シグナリングサーバー経由でメッセージを受信
    /// </summary>
    public Action<string> OnReceivedMessage = null;

    /// <summary>
    /// シグナリングサーバーに接続済みかどうか
    /// </summary>
    public bool IsConnected { get; private set; } = false;

    /// <summary>
    /// ログインタイプ
    /// </summary>
    public string LoginType { get; set; } = null;

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string UserName { get; set; } = null;

    /// <summary>
    /// ユーザーパスワード
    /// </summary>
    public string UserPassword { get; set; } = null;

    // 接続予定のクライアント(ユーザーorオペレーター)ID
    private string _targetId = null;

    void Start()
    {
        // 設定ファイルからユーザー情報取得
        //LoginType = GlobalState.Instance.ApplicationGlobalSettings.SignalingLoginType;
        //UserName = GlobalState.Instance.ApplicationGlobalSettings.SignalingUserName;
        //UserPassword = GlobalState.Instance.ApplicationGlobalSettings.SignalingUserPassword;
    }

    /// <summary>
    /// Socket I.O 接続
    /// </summary>
    public void Connect()
    {
        Debug.Log("SignalingManager Connect");

        // JS関数コール
        JSHelper.SignalingConnect();
    }

    /// <summary>
    /// Socket I.O 切断
    /// </summary>
    public void DisConnect()
    {
        Debug.Log("SignalingManager DisConnect");

        // JS関数コール
        JSHelper.SignalingDisconnect();
    }

    /// <summary>
    /// シグナリングサーバーにログイン
    /// ここでいうログインはシグナリングサーバーにクライアント情報を登録すること
    /// </summary>
    public void Login()
    {
        Debug.Log("SignalingManager Login");

        // JS関数コール
        JSHelper.SignalingLogin();
    }

    /// <summary>
    /// シグナリングサーバーからログアウト
    /// </summary>
    public void Logout()
    {
        Debug.Log("SignalingManager Logout.");

        // TODO: JS関数コール
        //socketIOComponent.Emit(GameController.Instance.Client.GetSignalingLogoutKey(), json);
    }

    /// <summary>
    /// シグナリングキャンセル
    /// </summary>
    public void Cancel()
    {
        Debug.Log("SignalingManager Cancel.");

        // TODO: JS関数コール
        //socketIOComponent.Emit(GameController.Instance.Client.GetSignalingCancelKey(), json);
    }

    /// <summary>
    /// シグナリングサーバー経由でオペレーター「に」メッセージ送信
    /// </summary>
    /// <param name="msg"></param>
    public void RelayToOperator(string msg)
    {
        Debug.Log($"SignalingManager RelayToOperator: msg = {msg}");

        if (string.IsNullOrEmpty(_targetId))
        {
            Debug.LogError("RelayToOperator failed. Target Id is null.");
        }

        // TODO: JS関数コール
        //json.Add(GlobalState.Instance.UserSettings.UserToken + "," + _targetId + "," + msg);
    }

    /// <summary>
    /// シグナリングサーバ接続確立時コールバック
    /// </summary>
    public void OnOpen()
    {
        Debug.Log("SignalingManager OnOpen.");

        IsConnected = true;

        OnConnected?.Invoke();
    }

    /// <summary>
    /// シグナリングサーバ通知受信時コールバック
    /// </summary>
    /// <param name="notification"></param>
    public void OnNotification(string json)
    {
        var notification = JsonUtility.FromJson<SocketIOEventNotification>(json);
        var type = notification.Type;
        Debug.Log($"SignalingManager OnNotification: type = {type}");
        switch (type)
        {
            case "socketOn":
                OnOpen();
                break;
            case "userLogin":
                OnUserLogin?.Invoke();
                break;
            case "cancelUser":
                OnUserCancel?.Invoke();
                break;
            case "relayToUser":
                var postMessageObj = JsonUtility.FromJson<SocketIOEventPostMessage>(notification.ToString());
                _targetId = postMessageObj.FromId;
                var msg = postMessageObj.Message;
                Debug.Log($"Received operator message. target id: {_targetId}, msg: {msg}");
                OnReceivedMessage?.Invoke(msg);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// シグナリングサーバレスポンス受信時コールバック
    /// </summary>
    public void OnResponse(string json)
    {
        var response = JsonUtility.FromJson<SocketIOEventNotification>(json);
        var method = response.Method;
        var result = response.Result;
        Debug.Log($"SignalingManager OnResponse: method = {method}, result = {result}");
        switch (method)
        {
            case "loginUser":
                if (result == "success") OnLoginSucceeded?.Invoke();
                else if (result == "failed")
                {
                    var error = ParseLoginFailedMessage(response.Message);
                    OnLoginFailed?.Invoke(error);
                }
                break;
            case "logoutUser":
                if (result == "success") OnLogoutSucceeded?.Invoke();
                else if (result == "failed") OnLogoutFailed?.Invoke();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// シグナリングサーバソケット切断時コールバック
    /// </summary>
    public void OnClose()
    {
        Debug.Log("SignalingManager OnClose.");

        IsConnected = false;
    }

    /// <summary>
    /// シグナリングサーバエラー受信時コールバック
    /// </summary>
    public void OnError()
    {
        Debug.Log("SignalingManager OnError.");
    }

    private string ParseLoginFailedMessage(string message)
    {
        var head = "シグナリングサーバーへのログインに失敗しました。";
        var body = "";
        switch (message)
        {
            case "user not found":
            case "operator not found":
                body = "企業コード・ユーザー名・パスワードをご確認ください。";
                break;
            case "already logined":
                body = "ログイン済みのユーザー名です。";
                break;
        }
        return head + Environment.NewLine + body;
    }

    // For Serialize/Deserialize
    [Serializable]
    public class SocketIOEventNotification : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string Type;
        [NonSerialized]
        public string Method;
        [NonSerialized]
        public string Result;
        [NonSerialized]
        public string Message;
        [NonSerialized]
        public string PeerId;
        [NonSerialized]
        public string UserId;

        [SerializeField]
        private string type;
        [SerializeField]
        private string method;
        [SerializeField]
        private string result;
        [SerializeField]
        private string message;
        [SerializeField]
        private string peer_id;
        [SerializeField]
        private string user_id;

        public void OnBeforeSerialize()
        {
            type = Type;
            method = Method;
            result = Result;
            message = Message;
            peer_id = PeerId;
            user_id = UserId;
        }

        public void OnAfterDeserialize()
        {
            Type = type;
            Method = method;
            Result = result;
            Message = message;
            PeerId = peer_id;
            UserId = user_id;
        }
    }

    [Serializable]
    public class SocketIOEventPostMessage : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string FromId;
        [NonSerialized]
        public string Message;

        [SerializeField]
        private string from_id;
        [SerializeField]
        private string message;

        public void OnBeforeSerialize()
        {
            message = Message;
            from_id = FromId;
        }

        public void OnAfterDeserialize()
        {
            Message = message;
            FromId = from_id;
        }
    }
}
