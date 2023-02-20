using System;
using System.Text;
using SocketIO;
using UnityEngine;

public class SignalingManager : SingletonMonoBehaviour<SignalingManager>
{
    // Socket.IO
    [SerializeField] SocketIOComponent socketIOComponent = null;

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
    /// 企業コード
    /// </summary>
    public string CompanyCode { get; set; } = null;

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

    protected override void Awake()
    {
        base.Awake();

        // SocketIOComponent.Awake 内で WebSocket の初期化処理を行っているので、このタイミングかつ Script Order の優先順位を上げていないと間に合わない
        var url = GlobalState.Instance.ApplicationGlobalSettings.SignalingServer + "/socket.io/?EIO=4&transport=websocket";
        socketIOComponent.url = url;
    }

    void Start()
    {
        // 設定ファイルからユーザー情報取得
        CompanyCode = GlobalState.Instance.ApplicationGlobalSettings.CompanyCode;
        UserName = GlobalState.Instance.ApplicationGlobalSettings.SignalingUserName;
        UserPassword = GlobalState.Instance.ApplicationGlobalSettings.SignalingUserPassword;

        // Socket.IO コールバック登録
        //socketIOComponent.On("open", OnOpen);
        socketIOComponent.On("message", OnMessage);
        socketIOComponent.On("close", OnClose);
        socketIOComponent.On("error", OnError);
        socketIOComponent.On("notification", OnNotification);
        socketIOComponent.On("stackResult", OnStackResult);
        socketIOComponent.On("stackCountResult", OnStackCountResult);
        socketIOComponent.On("postMessage", OnPostMessage);
    }

    /// <summary>
    /// Socket I.O 接続
    /// </summary>
    public void Connect()
    {
        if (socketIOComponent.IsConnected)
        {
            Debug.Log("Signaling server already connected");
            return;
        }

        socketIOComponent.Connect();
    }

    /// <summary>
    /// Socket I.O 切断
    /// </summary>
    public void DisConnect()
    {
        if (!socketIOComponent.IsConnected)
        {
            Debug.Log("Signaling server already disconnected");
            return;
        }

        socketIOComponent.Close();
    }

    /// <summary>
    /// シグナリングサーバーにログイン
    /// ここでいうログインはシグナリングサーバーにクライアント情報を登録すること
    /// </summary>
    /// <param name="companyCode"></param>
    /// <param name="userName"></param>
    /// <param name="userPassword"></param>
    /// <param name="status"></param>
    public void Login(string status = "calling")
    {
        var json = new JSONObject();
        json.Add(CompanyCode + "," + UserName + "," + UserPassword + "," + GameController.Instance.Client.GetSignalingLoginValue() + "," + status + "," + SettingHub.Instance.System.Cache.PreSendPayload);
        socketIOComponent.Emit(GameController.Instance.Client.GetSignalingLoginKey(), json);
    }

    /// <summary>
    /// シグナリングサーバーからログアウト
    /// </summary>
    public void Logout()
    {
        var json = new JSONObject();
        // ログアウト時は値は空
        socketIOComponent.Emit(GameController.Instance.Client.GetSignalingLogoutKey(), json);
    }

    /// <summary>
    /// シグナリングキャンセル
    /// </summary>
    public void Cancel()
    {
        if (!socketIOComponent.IsConnected)
        {
            Debug.Log("Signaling server not connected.");
        }

        var json = new JSONObject();
        // 現状とりあえず空で送る
        //json.Add(UserName);
        socketIOComponent.Emit(GameController.Instance.Client.GetSignalingCancelKey(), json);
    }

    /// <summary>
    /// 待機ユーザーの Peer ID を問い合わせる
    /// 接続直前に実行される想定とのこと
    /// </summary>
    public void StackUser()
    {
        socketIOComponent.Emit("stackUser");
    }

    /// <summary>
    /// 待機ユーザーの人数を問い合わせる
    /// </summary>
    public void StackUserCount()
    {
        socketIOComponent.Emit("stackUserCount");
    }

    /// <summary>
    /// シグナリングサーバー経由でメッセージ送信
    /// </summary>
    /// <param name="msg"></param>
    public void PostMessage(string msg)
    {
        if (string.IsNullOrEmpty(_targetId))
        {
            Debug.LogError("PostMessage failed. Target Id is null.");
        }

        var json = new JSONObject();
        json.Add(_targetId + "," + msg);
        socketIOComponent.Emit("postMessage", json);
    }

    private void OnOpen(SocketIOEvent e)
    {
        Debug.Log($"[SocketIO] open received: {e.name}, {e.data}");

        IsConnected = true;

        OnConnected?.Invoke();
    }

    private void OnMessage(SocketIOEvent e)
    {
        Debug.Log($"[SocketIO] message received: {e.name}, {e.data}");
    }

    private void OnNotification(SocketIOEvent e)
    {
        Debug.Log($"[SocketIO] notification received: {e.name}, {e.data}");

        var notificationObj = JsonUtility.FromJson<SocketIOEventNotification>(e.data.ToString());
        var type = notificationObj.Type;
        switch (type)
        {
            case "loginSuccess":
                OnLoginSucceeded?.Invoke();
                break;
            case "loginFailed":
                var error = ParseLoginFailedMessage(notificationObj.Message);
                OnLoginFailed?.Invoke(error);
                break;
            case "logoutSuccess":
                OnLogoutSucceeded?.Invoke();
                break;
            case "logoutFailed":
                OnLogoutFailed?.Invoke();
                break;
            case "clientConnect":
                OnOpen(e);
                break;
            case "userLogin":
                OnUserLogin?.Invoke();
                break;
            case "cancelUser":
                OnUserCancel?.Invoke();
                break;
            default:
                break;
        }
    }

    private void OnStackResult(SocketIOEvent e)
    {
        Debug.Log($"[SocketIO] stackResult received: {e.name}, {e.data}");

        var stackResultObj = JsonUtility.FromJson<SocketIOEventStackResult>(e.data.ToString());
        _targetId = stackResultObj.UserId;
        var peerId = stackResultObj.PeerId;
        Debug.Log($"Target user id: {_targetId}, peer id: {peerId}");

        // ログイン中のユーザーがいない場合は空のIDが送られる
        if (!string.IsNullOrEmpty(peerId))
        {
            OnReceivedUserPeerId?.Invoke(peerId);
        }
    }

    private void OnStackCountResult(SocketIOEvent e)
    {
        Debug.Log($"[SocketIO] stackCountResult received: {e.name}, {e.data}");

        var stackCountResultObj = JsonUtility.FromJson<SocketIOEventStackCountResult>(e.data.ToString());
        var count = stackCountResultObj.Count;
        Debug.Log($"User count: {count.ToString()}");

        OnReceivedUserCount?.Invoke(count);
    }

    private void OnCancelUser(SocketIOEvent e)
    {
        Debug.Log($"[SocketIO] cancelUser received: {e.name}, {e.data}");

        var cancelUserObj = JsonUtility.FromJson<SocketIOEventCancelUser>(e.data.ToString());
        var userId = cancelUserObj.UserId;
        Debug.Log($"Cancel user: {userId}");
    }

    private void OnPostMessage(SocketIOEvent e)
    {
        Debug.Log($"[SocketIO] postMessage received: {e.name}, {e.data}");

        if (e.data.ToString().Contains("postFailed")) return;

        var postMessageObj = JsonUtility.FromJson<SocketIOEventPostMessage>(e.data.ToString());
        _targetId = postMessageObj.FromId;
        var msg = postMessageObj.Msg;
        Debug.Log($"Received post message. id: {_targetId}, msg: {msg}");

        OnReceivedMessage?.Invoke(msg);
    }

    private void OnClose(SocketIOEvent e)
    {
        Debug.Log($"[SocketIO] close received: {e.name}, {e.data}");

        IsConnected = false;
    }

    private void OnError(SocketIOEvent e)
    {
        Debug.Log($"[SocketIO] error received: {e.name}, {e.data}");
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
        public string Message;

        [SerializeField]
        private string type;
        [SerializeField]
        private string message;

        public void OnBeforeSerialize()
        {
            type = Type;
            message = Message;
        }

        public void OnAfterDeserialize()
        {
            Type = type;
            Message = message;
        }
    }

    [Serializable]
    public class SocketIOEventStackResult : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string UserId;
        [NonSerialized]
        public string PeerId;

        [SerializeField]
        private string user_id;
        [SerializeField]
        private string peer_id;

        public void OnBeforeSerialize()
        {
            user_id = UserId;
            peer_id = PeerId;
        }

        public void OnAfterDeserialize()
        {
            UserId = user_id;
            PeerId = peer_id;
       }
    }

    [Serializable]
    public class SocketIOEventStackCountResult : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public int Count;

        [SerializeField]
        private int count;

        public void OnBeforeSerialize()
        {
            count = Count;
        }

        public void OnAfterDeserialize()
        {
            Count = count;
        }
    }

    [Serializable]
    public class SocketIOEventCancelUser : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string UserId;

        [SerializeField]
        private string user_id;

        public void OnBeforeSerialize()
        {
            user_id = UserId;
        }

        public void OnAfterDeserialize()
        {
            UserId = user_id;
        }
    }

    [Serializable]
    public class SocketIOEventPostMessage : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string FromId;
        [NonSerialized]
        public string Msg;

        [SerializeField]
        private string from_id;
        [SerializeField]
        private string msg;

        public void OnBeforeSerialize()
        {
            msg = Msg;
            from_id = FromId;
        }

        public void OnAfterDeserialize()
        {
            Msg = msg;
            FromId = from_id;
        }
    }
}
