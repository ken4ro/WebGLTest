using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

public static partial class SkyWayService
{
    /// <summary>
    /// 自IPアドレス
    /// </summary>
    public static readonly string MyAddress = "127.0.0.1";

    /// <summary>
    /// SkyWay WebRTC Gateway REST API アクセス用 URL
    /// </summary>
    public static readonly string SkyWayGatewayUrl = "http://127.0.0.1:8000/";

    /// <summary>
    /// Peer ID
    /// </summary>
    public static string PeerId { get; private set; } = "";

    /// <summary>
    /// Peer Token
    /// </summary>
    public static string PeerToken { get; private set; } = "";

    /// <summary>
    /// 接続対象の Peer ID
    /// </summary>
    public static string TargetPeerId { get; set; } = "";

    /// <summary>
    /// SkyWay と接続中かどうか
    /// </summary>
    public static bool IsConnected { get; private set; } = false;

    /// <summary>
    /// データチャンネルマップ
    /// </summary>
    public static Dictionary<int, DataChannel> DataChannelMap = new Dictionary<int, DataChannel>()
    {
        [0] = new DataChannel(),
        [1] = new DataChannel(),
        [2] = new DataChannel(),
        [3] = new DataChannel(),
        [4] = new DataChannel(),
    };

    /// <summary>
    /// メディアストリームマップ
    /// </summary>
    public static Dictionary<int, MediaStream> MediaStreamMap = new Dictionary<int, MediaStream>()
    {
        [0] = new MediaStream(),
        [1] = new MediaStream(),
        [2] = new MediaStream(),
    };

    /// <summary>
    /// SkyWay 接続成功時コールバック
    /// </summary>
    public static Action OnConnected = null;

    /// <summary>
    /// データチャンネル着信時コールバック
    /// </summary>
    public static Action<string> OnPeerEventConnect = null;

    /// <summary>
    /// メディアストリーム着信時コールバック
    /// </summary>
    public static Action<string> OnPeerEventCall = null;

    /// <summary>
    /// Peer オブジェクトを生成し、SkyWayサーバと接続します
    /// </summary>
    /// <returns></returns>
    public static async UniTask<bool> CreatePeer()
    {
        // Gateway に作成要求
        var key = GlobalState.Instance.ApplicationGlobalSettings.WebRtcApiKey;
        if (string.IsNullOrEmpty(key))
        {
            // 作成失敗
            Debug.LogError($"CreatePeer failed: SkyWay API Key is invalid.");
            return false;
        }
        var requestObj = new CreatePeerRequest
        {
            Key = key,
            Domain = "localhost",
            Turn = true,
            PeerId = ""
        };
        var requestJson = JsonConvert.SerializeObject(requestObj);
        var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, SkyWayGatewayUrl + "peers", null, requestBody);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            // 接続成功
            IsConnected = true;
            // レスポンス取得
            var responseObj = JsonConvert.DeserializeObject<CreatePeerResponse>(response.Json);
            PeerId = responseObj.Params.PeerId;
            PeerToken = responseObj.Params.Token;
            // Peer イベント監視タスク開始
            // バックグラウンドスレッドで行う
            UniTask.Run(async () =>
            {
                while (true)
                {
                    if (!IsConnected)
                    {
                        // 切断されたら終了
                        //Debug.Log("Event polling task end. (Peer Disconnect)");
                        break;
                    }
                    // TODO: エラーハンドリング
                    await GetPeerEvent();
                }
            }).Forget();
            // 接続成功イベント発行
            OnConnected?.Invoke();
            return true;
        }
        else
        {
            // 接続失敗
            Debug.LogError($"CreatePeer failed: {response.StatusCode}");
            return false;
        }
    }

    /// <summary>
    /// Peer オブジェクトを開放し、関連する全ての WebRTC セッションとデータ受け渡しのための UDP ポートをクローズします
    /// </summary>
    /// <returns></returns>
    public static async UniTask DeletePeer()
    {
        if (!IsConnected)
        {
            //Debug.Log("Peer already deleted");
            return;
        }

        // Gateway に削除要求
        var requestHeaders = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("accept", "application/json"),
        };
        var response = await HttpRequest.RequestJsonAsync(HttpRequestType.DELETE, SkyWayGatewayUrl + $"peers/{PeerId}?token={PeerToken}", requestHeaders);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            //Debug.Log($"DeletePeer succeeded");
            IsConnected = false;
            PeerId = "";
            PeerToken = "";
            TargetPeerId = "";
        }
        else
        {
            Debug.LogError($"DeletePeer failed: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Gateway からの Peer イベントを受信する
    /// </summary>
    /// <returns></returns>
    public static async UniTask<bool> GetPeerEvent()
    {
        // Gateway に Long Poll でイベント監視
        var requestHeaders = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("accept", "application/json"),
        };
        var response = await HttpRequest.RequestJsonAsync(HttpRequestType.GET, SkyWayGatewayUrl + $"peers/{PeerId}/events?token={PeerToken}", requestHeaders, isLongPolling:true);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseObj = JsonConvert.DeserializeObject<GetPeerEventResponse>(response.Json);
            if (responseObj.Event == "OPEN")
            {
                // SkyWay 接続完了
                //Debug.Log("GetPeerEvent: OPEN");
                // レスポンス取得
                PeerId = responseObj.Params.PeerId;
                PeerToken = responseObj.Params.Token;
                Debug.Log($"GetPeerEvent: OPEN [{PeerId}]");
            }
            else if (responseObj.Event == "CONNECTION")
            {
                // データチャンネル着信
                //Debug.Log("GetPeerEvent: CONNECTION");
                // レスポンス取得
                var dataConnectionId = responseObj.DataParams.DataConnectionId;
                Debug.Log($"GetPeerEvent: CONNECTION [{dataConnectionId}]");
                // イベント通知
                GameController.Instance.MainContext.Post(_ =>
                {
                    OnPeerEventConnect?.Invoke(dataConnectionId);
                }, null);
            }
            else if (responseObj.Event == "CALL")
            {
                // メディアストリーム着信
                //Debug.Log($"GetPeerEvent: CALL");
                // レスポンス取得
                var mediaConnectionId = responseObj.CallParams.MediaConnectionId;
                Debug.Log($"GetPeerEvent: CALL [{mediaConnectionId}]");
                // イベント通知
                GameController.Instance.MainContext.Post(_ =>
                {
                    OnPeerEventCall?.Invoke(mediaConnectionId);
                }, null);
            }
            else if (responseObj.Event == "CLOSE")
            {
                // 切断された
                Debug.Log("GetPeerEvent: CLOSE");
            }
            else if (responseObj.Event == "ERROR")
            {
                Debug.Log("GetPeerEvent: ERROR");
                return false;
            }
            return true;
        }
        else
        {
            // タイムアウトする仕様なのでコメントアウトしておく
            // TODO: タイムアウト対応
            //Debug.LogError($"GetPeerEvent failed: {response.StatusCode}");
            return false;
        }
    }

    public static int GetEmptyDataChannelIndex()
    {
        for (var i = 0; i < DataChannelMap.Values.Count; i++)
        {
            if (string.IsNullOrEmpty(DataChannelMap[i].ConnectionId))
            {
                return i;
            }
        }
        Debug.LogError("Empty DataChannel not found");
        return -1;
    }

    public static int GetEmptyMediaStreamIndex()
    {
        for (var i = 0; i < MediaStreamMap.Values.Count; i++)
        {
            if (string.IsNullOrEmpty(MediaStreamMap[i].ConnectionId))
            {
                return i;
            }
        }
        Debug.LogError("Empty MediaStream not found");
        return -1;
    }

    /// <summary>
    /// データチャンネル管理クラス
    /// </summary>
    public class DataChannel
    {
        public Action OnClosed = null;

        public Action<byte[]> OnReceived = null;

        public string ConnectionId { get; set; } = "";

        public string DataId { get; private set; } = "";

        public MyUdpClient ReceiveClient { get; private set; } = null;

        public MyUdpClient SendClient { get; private set; } = null;

        public string SendAddress { get; private set; } = "";

        public int ReceivePort { get; set; } = 60000;

        public int SendPort { get; private set; } = 0;

        public bool IsConnected { get { return ConnectionId != ""; } }

        public bool IsReleased { get { return DataId == ""; } }

        public bool IsAvailable { get; private set; } = false;

        /// <summary>
        /// データの待受ポート開放を要求
        /// </summary>
        /// <returns></returns>
        public async UniTask Open()
        {
            // Gateway に確立要求
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
                new KeyValuePair<string, string>("Content-Type", "application/json"),
            };
            var requestBody = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, SkyWayGatewayUrl + "data", requestHeaders, requestBody);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // レスポンス取得
                var responseObj = JsonConvert.DeserializeObject<OpenPortForDataChannelResponse>(response.Json);
                DataId = responseObj.DataId;
                SendAddress = responseObj.Ipv4;
                SendPort = responseObj.Port;
                //Debug.Log($"Open port for DataChannel succeeded. Send Address: {SendAddress}, Port: {SendPort}");
                // 受信用UDPクライアント生成
                while (true)
                {
                    try
                    {
                        ReceiveClient = new MyUdpClient(ReceivePort);
                        //Debug.Log($"UdpClient(Receive) create succeeded. Port: {ReceivePort}");
                        break;
                    }
                    catch (Exception)
                    {
                        //Debug.Log($"UdpClient(Receive) create failed. Port: {ReceivePort}");
                        ReceivePort++;
                    }
                    await UniTask.Delay(100);
                }
                // データ受信時コールバックを設定
                ReceiveClient.OnDataReceived += (data) => OnReceived?.Invoke(data);
                // 非同期受信開始
                ReceiveClient.ReceiveAsync().Forget();
                // データ送信用UDPクライアント生成
                while (true)
                {
                    try
                    {
                        SendClient = new MyUdpClient(SendAddress, SendPort);
                        //Debug.Log($"UdpClient(Send) create succeeded. Port: {SendPort}");
                        break;
                    }
                    catch (Exception)
                    {
                        Debug.LogError($"UdpClient(Send) create failed. Port: {SendPort}");
                        SendPort++;
                    }
                    await UniTask.Delay(100);
                }
            }
            else
            {
                Debug.LogError($"Open port for Datachannel failed: {response.StatusCode}");
            }
        }

        /// <summary>
        /// データの待受ポート閉鎖要求
        /// </summary>
        /// <returns></returns>
        public async UniTask Close()
        {
            if (IsReleased)
            {
                Debug.LogError($"Port for DataChannel [{ConnectionId}] already closed");
                return;
            }

            // Gateway にクローズ要求
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
            };
            var response = await HttpRequest.RequestJsonAsync(HttpRequestType.DELETE, SkyWayGatewayUrl + $"data/{DataId}", requestHeaders);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Debug.Log($"Close port for DataChannel [{ConnectionId}] succeeded");
                // 初期化
                DataId = "";
            }
            else
            {
                Debug.Log($"Close port for DataChannel [{ConnectionId}] failed: {response.StatusCode}");
            }
        }

        /// <summary>
        /// 対象のピアにデータチャンネル接続を要求
        /// </summary>
        /// <param name="ordered"></param>
        /// <param name="priority"></param>
        /// <param name="metaData"></param>
        /// <returns></returns>
        public async UniTask Connect(bool ordered = true, string priority = "low", string metaData = "")
        {
            // Gateway に接続要求
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
                new KeyValuePair<string, string>("Content-Type", "application/json"),
            };
            var requestObj = new ConnectRequest();
            requestObj.PeerId = PeerId;
            requestObj.Token = PeerToken;
            requestObj.TargetId = TargetPeerId;
            requestObj.Options = new ConnectRequestOptions();
            requestObj.Options.Metadata = metaData;
            requestObj.Options.Serialization = "";
            requestObj.Options.DcInit = new ConnectRequestOptionsDcInit();
            requestObj.Options.DcInit.Ordered = ordered;
            //requestObj.Options.DcInit.MaxPacketLifeTime = 0;
            requestObj.Options.DcInit.MaxRetransmits = ordered ? 2 : 0;
            //requestObj.Options.DcInit.Protocol = "";
            //requestObj.Options.DcInit.Negotiated = true;
            //requestObj.Options.DcInit.Id = 0;
            requestObj.Options.DcInit.Priority = priority;
            requestObj.Params = new ChangeDataChannelSettingRequestFeedParams();
            requestObj.Params.DataId = DataId;
            requestObj.RedirectParams = new ChangeDataChannelSettingRequestRedirectParams();
            requestObj.RedirectParams.Ipv4 = MyAddress;
            requestObj.RedirectParams.Port = ReceivePort;
            var requestJson = JsonConvert.SerializeObject(requestObj);
            var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, SkyWayGatewayUrl + "data/connections", requestHeaders, requestBody);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseObj = JsonConvert.DeserializeObject<ConnectResponse>(response.Json);
                ConnectionId = responseObj.Params.DataConnectionId;
                //Debug.Log($"Connect [{ConnectionId}] succeeded");
                // データチャンネルイベント監視タスク開始
                // バックグラウンドスレッドで行う
                UniTask.Run(async () =>
                {
                    while (true)
                    {
                        if (!IsConnected)
                        {
                            // 切断されたら終了
                            //Debug.Log("DataChannel event polling task end. (Disconnect)");
                            break;
                        }
                        // TODO: エラーハンドリング
                        await GetDataChannelEvent();
                    }
                }).Forget();
            }
            else
            {
                Debug.LogError($"Connect [{ConnectionId}] failed: {response.StatusCode}");
            }
        }

        /// <summary>
        /// データチャンネル接続をクローズ
        /// </summary>
        /// <returns></returns>
        public async UniTask CloseDataConnection()
        {
            if (string.IsNullOrEmpty(ConnectionId))
            {
                Debug.Log($"DataConnection already closed");
                return;
            }

            // Gateway にクローズ要求
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
            };
            var response = await HttpRequest.RequestJsonAsync(HttpRequestType.DELETE, SkyWayGatewayUrl + $"data/connections/{ConnectionId}", requestHeaders);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Debug.Log($"CloseDataConnection [{ConnectionId}] succeeded");
                // UDPクライアントクローズ
                SendClient.Close();
                ReceiveClient.Close();
                // 初期化
                ConnectionId = "";
                IsAvailable = false;
            }
            else
            {
                Debug.LogError($"CloseDataConnection [{ConnectionId}] failed: {response.StatusCode}");
            }
        }

        /// <summary>
        /// 受信データ設定の変更を要求
        /// </summary>
        /// <returns></returns>
        public async UniTask ChangeDataConnectionSetting()
        {
            // Gateway に変更要求
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
                new KeyValuePair<string, string>("Content-Type", "application/json"),
            };
            var feedParams = new ChangeDataChannelSettingRequestFeedParams
            {
                DataId = DataId
            };
            var redirectParams = new ChangeDataChannelSettingRequestRedirectParams
            {
                Ipv4 = MyAddress,
                //Ipv6 = "",
                Port = ReceivePort
            };
            var requestObj = new ChangeDataChannelSettingRequest
            {
                FeedParams = feedParams,
                RedirectParams = redirectParams
            };
            var requestJson = JsonConvert.SerializeObject(requestObj);
            var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await HttpRequest.RequestJsonAsync(HttpRequestType.PUT, SkyWayGatewayUrl + $"data/connections/{ConnectionId}", requestHeaders, requestBody);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Debug.Log($"ChangeDataConnectionSetting [{ConnectionId}] succeeded");
                // データチャンネルイベント監視タスク開始
                // バックグラウンドスレッドで行う
                UniTask.Run(async () =>
                {
                    while (true)
                    {
                        if (!IsConnected)
                        {
                            // 切断されたら終了
                            //Debug.Log("DataChannel event polling task end. (Disconnect)");
                            break;
                        }
                        // TODO: エラーハンドリング
                        await GetDataChannelEvent();
                    }
                }).Forget();
            }
            else
            {
                Debug.LogError($"ChangeDataConnectionSetting [{ConnectionId}] failed: {response.StatusCode}");
            }
        }

        /// <summary>
        /// Gateway からのデータチャンネルイベントを受信する
        /// </summary>
        /// <returns></returns>
        public async UniTask<bool> GetDataChannelEvent()
        {
            // Gateway に Long Poll でイベント監視
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
            };
            var response = await HttpRequest.RequestJsonAsync(HttpRequestType.GET, SkyWayGatewayUrl + $"data/connections/{ConnectionId}/events", requestHeaders, isLongPolling:true);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseObj = JsonConvert.DeserializeObject<GetDataChannelEventResponse>(response.Json);
                if (responseObj.Event == "OPEN")
                {
                    // データチャンネルが接続完了した
                    Debug.Log("GetDataChannelEvent: OPEN");
                    // データチャンネル利用準備完了
                    IsAvailable = true;
                }
                else if (responseObj.Event == "CLOSE")
                {
                    // データチャンネルが切断された
                    Debug.Log("GetDataChannelEvent: CLOSE");
                    // データチャンネル利用不可
                    IsAvailable = false;
                    // イベント通知
                    GameController.Instance.MainContext.Post(_ =>
                    {
                        OnClosed?.Invoke();
                    }, null);
                }
                else if (responseObj.Event == "ERROR")
                {
                    // エラー
                    Debug.LogError("GetDataChannelEvent: ERROR");
                    return false;
                }
                return true;
            }
            else
            {
                // タイムアウトする仕様なのでコメントアウトしておく
                // TODO: タイムアウト対応
                //Debug.LogError($"GetPeerEvent failed: {response.StatusCode}");
                return false;
            }
        }

        /// <summary>
        /// 指定されたデータを非同期送信
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async UniTask SendAsync(byte[] data)
        {
            if (IsAvailable)
            {
                await SendClient.SendAsync(data, data.Length);
            }
        }

        /// <summary>
        /// 指定されたデータを同期送信
        /// </summary>
        /// <param name="data"></param>
        public void SendSync(byte[] data)
        {
            if (IsAvailable)
            {
                SendClient.SendSync(data, data.Length);
            }
        }

        /// <summary>
        /// 指定された文字列をデータチャンネルで送信
        /// </summary>
        /// <param name="text">送信文字列</param>
        /// <returns></returns>
        public async UniTask SendData(string text) => await SendAsync(Encoding.UTF8.GetBytes(text));
    }

    public class MediaStream
    {
        public enum VideoCodec
        {
            H264,
        }

        public enum AudioCodec
        {
            PCMU,
            OPUS,
        }

        public Action OnClosed = null;

        public Action<byte[]> OnReceived = null;

        public string ConnectionId { get; set; } = "";

        public string VideoMediaId { get; private set; } = "";

        public string AudioMediaId { get; private set; } = "";

        public bool IsVideo { get; private set; } = false;

        public bool IsAudio { get; private set; } = false;

        public bool IsVideoReceiveEnabled { get; private set; } = false;

        public bool IsAudioReceiveEnabled { get; private set; } = false;

        public MyRtpClient VideoReceiveClient { get; private set; } = null;

        public MyRtpClient AudioReceiveClient { get; private set; } = null;

        public MyRtpClient VideoSendClient { get; private set; } = null;

        public MyRtpClient AudioSendClient { get; private set; } = null;

        public int VideoReceivePort { get; private set; } = 60010;

        public int AudioReceivePort { get; private set; } = 60020;

        public string VideoSendAddress { get; private set; } = "";

        public string AudioSendAddress { get; private set; } = "";

        public int VideoSendPort { get; private set; } = 0;

        public int AudioSendPort { get; private set; } = 0;

        public bool IsConnected { get { return ConnectionId != ""; } }

        public bool IsVideoReleased { get { return VideoMediaId == ""; } }

        public bool IsAudioReleased { get { return AudioMediaId == ""; } }

        public bool IsAvailable { get; private set; } = false;

        /// <summary>
        /// メディアの待受ポート開放を要求
        /// </summary>
        /// <returns></returns>
        public async UniTask Open(bool isVideo, bool isAudio, bool isVideoReceiveEnabled, bool isAudioReceiveEnabled)
        {
            // メディア送受信設定
            IsVideo = isVideo;
            IsAudio = isAudio;
            IsVideoReceiveEnabled = isVideoReceiveEnabled;
            IsAudioReceiveEnabled = isAudioReceiveEnabled;
            // Gateway に確立要求
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
                new KeyValuePair<string, string>("Content-Type", "application/json"),
            };
            if (isVideo)
            {
                var requestJson = $"{{ \"is_video\": true }}";
                var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, SkyWayGatewayUrl + "media", requestHeaders, requestBody);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // レスポンス取得
                    var responseObj = JsonConvert.DeserializeObject<OpenPortForMediaStreamResponse>(response.Json);
                    VideoMediaId = responseObj.MediaId;
                    //VideoSendAddress = responseObj.Ipv4;
                    VideoSendAddress = "127.0.0.1";
                    VideoSendPort = responseObj.Port;
                    Debug.Log($"Open port for MediaStream(Video) succeeded. Address: {VideoSendAddress}, Port: {VideoSendPort}");
                }
                else
                {
                    Debug.LogError($"Open port for MediaStream(Video) failed: {response.StatusCode}");
                }
            }
            else if (isAudio)
            {
                var requestJson = $"{{ \"is_video\": false }}";
                var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, SkyWayGatewayUrl + "media", requestHeaders, requestBody);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // レスポンス取得
                    var responseObj = JsonConvert.DeserializeObject<OpenPortForMediaStreamResponse>(response.Json);
                    AudioMediaId = responseObj.MediaId;
                    //AudioSendAddress = responseObj.Ipv4;
                    AudioSendAddress = "127.0.0.1";
                    AudioSendPort = responseObj.Port;
                    Debug.Log($"Open port for MediaStream(Audio) succeeded. Address: {AudioSendAddress}, Port: {AudioSendPort}");
                    // データ送信用UDPクライアント生成
                    while (true)
                    {
                        try
                        {
                            AudioSendClient = new MyRtpClient(AudioSendAddress, AudioSendPort);
                            //Debug.Log($"UdpClient(Send) create succeeded. Port: {SendPort}");
                            break;
                        }
                        catch (Exception)
                        {
                            Debug.LogError($"RtpClient(Send) create failed. Port: {AudioSendPort}");
                            AudioSendPort++;
                        }
                        await UniTask.Delay(100);
                    }
                }
                else
                {
                    Debug.LogError($"Open port for MediaStream(Audio) failed: {response.StatusCode}");
                }
            }
        }

        /// <summary>
        /// メディアの待受ポート閉鎖要求
        /// </summary>
        /// <returns></returns>
        public async UniTask Close()
        {
            if (IsVideoReleased && IsAudioReleased)
            {
                Debug.LogError($"Port for MediaStream [{ConnectionId}] already closed");
                return;
            }

            // Gateway にクローズ要求
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
            };
            if (IsVideo)
            {
                var response = await HttpRequest.RequestJsonAsync(HttpRequestType.DELETE, SkyWayGatewayUrl + $"media/{VideoMediaId}", requestHeaders);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Debug.Log($"Close port for MediaStream(Video) [{ConnectionId}] succeeded");
                    // 初期化
                    VideoMediaId = "";
                    IsVideo = false;
                }
                else
                {
                    Debug.Log($"Close port for MediaStream(Video) [{ConnectionId}] failed: {response.StatusCode}");
                }
            }
            if (IsAudio)
            {
                var response = await HttpRequest.RequestJsonAsync(HttpRequestType.DELETE, SkyWayGatewayUrl + $"media/{AudioMediaId}", requestHeaders);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Debug.Log($"Close port for MediaStream(Audio) [{ConnectionId}] succeeded");
                    // 初期化
                    AudioMediaId = "";
                    IsAudio = false;
                }
                else
                {
                    Debug.Log($"Close port for MediaStream(Audio) [{ConnectionId}] failed: {response.StatusCode}");
                }
            }
        }

        /// <summary>
        /// 対象のピアにデータチャンネル接続を要求
        /// </summary>
        /// <param name="ordered"></param>
        /// <param name="priority"></param>
        /// <param name="metaData"></param>
        /// <returns></returns>
        public async UniTask Call()
        {
            // Gateway に接続要求
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
                new KeyValuePair<string, string>("Content-Type", "application/json"),
            };
            var requestObj = new CallRequest();
            requestObj.PeerId = PeerId;
            requestObj.Token = PeerToken;
            requestObj.TargetId = TargetPeerId;
            // 送信メディア設定
            requestObj.Constraints = new Constraints();
            requestObj.Constraints.Video = IsVideo;
            requestObj.Constraints.VideoReceiveEnabled = IsVideoReceiveEnabled;
            requestObj.Constraints.Audio = IsAudio;
            requestObj.Constraints.AudioReceiveEnabled = IsAudioReceiveEnabled;
            if (IsVideo)
            {
                requestObj.Constraints.VideoParams = CreateConstraintsMediaParams(VideoCodec.H264);
            }
            if (IsAudio)
            {
                requestObj.Constraints.AudioParams = CreateConstraintsMediaParams(AudioCodec.PCMU);
            }
            // 受信メディア設定
            requestObj.RedirectParams = new AnswerRequestRedirectParams();
            if (IsVideoReceiveEnabled)
            {
                requestObj.RedirectParams.Video = new AnswerRequestRedirectParamsMedia();
                requestObj.RedirectParams.Video.Ipv4 = MyAddress;
                requestObj.RedirectParams.Video.Port = VideoReceivePort;
            }
            if (IsAudioReceiveEnabled)
            {
                requestObj.RedirectParams.Audio = new AnswerRequestRedirectParamsMedia();
                requestObj.RedirectParams.Audio.Ipv4 = MyAddress;
                requestObj.RedirectParams.Audio.Port = AudioReceivePort;
            }
            var requestJson = JsonConvert.SerializeObject(requestObj);
            var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, SkyWayGatewayUrl + "media/connections", requestHeaders, requestBody);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseObj = JsonConvert.DeserializeObject<CallResponse>(response.Json);
                ConnectionId = responseObj.Params.MediaConnectionId;
                // メディアストリームイベント監視タスク開始
                UniTask.Run(async () =>
                {
                    while (true)
                    {
                        if (!IsConnected)
                        {
                            // 切断されたら終了
                            //Debug.Log("DataChannel event polling task end. (Disconnect)");
                            break;
                        }
                        // TODO: エラーハンドリング
                        await GetMediaStreamEvent();
                    }
                }).Forget();
                // ビデオ受信用RTPクライアント生成
                if (IsVideoReceiveEnabled)
                {
                    while (true)
                    {
                        try
                        {
                            VideoReceiveClient = new MyRtpClient(VideoReceivePort);
                            //Debug.Log($"UdpClient(Receive) create succeeded. Port: {ReceivePort}");
                            break;
                        }
                        catch (Exception)
                        {
                            //Debug.Log($"UdpClient(Receive) create failed. Port: {ReceivePort}");
                            VideoReceivePort++;
                        }
                        await UniTask.Delay(100);
                    }
                    // データ受信時コールバックを設定
                    VideoReceiveClient.OnDataReceived += (data) => OnReceived?.Invoke(data);
                    // 非同期受信開始
                    VideoReceiveClient.ReceiveAsync().Forget();
                }
                // オーディオ受信用RTPクライアント生成
                if (IsAudioReceiveEnabled)
                {
                    while (true)
                    {
                        try
                        {
                            AudioReceiveClient = new MyRtpClient(AudioReceivePort);
                            //Debug.Log($"UdpClient(Receive) create succeeded. Port: {ReceivePort}");
                            break;
                        }
                        catch (Exception)
                        {
                            //Debug.Log($"UdpClient(Receive) create failed. Port: {ReceivePort}");
                            AudioReceivePort++;
                        }
                        await UniTask.Delay(100);
                    }
                    // データ受信時コールバックを設定
                    AudioReceiveClient.OnDataReceived += (data) => OnReceived?.Invoke(data);
                    // 非同期受信開始
                    AudioReceiveClient.ReceiveAsync().Forget();
                }
            }
            else
            {
                Debug.LogError($"Call [{ConnectionId}] failed: {response.StatusCode}");
            }
        }

        public async UniTask Answer()
        {
            // Gateway に接続要求
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
                new KeyValuePair<string, string>("Content-Type", "application/json"),
            };
            var requestObj = new AnswerRequest();
            // 送信メディア設定
            requestObj.Constraints = new Constraints();
            requestObj.Constraints.Video = IsVideo;
            requestObj.Constraints.VideoReceiveEnabled = IsVideoReceiveEnabled;
            requestObj.Constraints.Audio = IsAudio;
            requestObj.Constraints.AudioReceiveEnabled = IsAudioReceiveEnabled;
            if (IsVideo)
            {
                requestObj.Constraints.VideoParams = CreateConstraintsMediaParams(VideoCodec.H264);
            }
            if (IsAudio)
            {
                requestObj.Constraints.AudioParams = CreateConstraintsMediaParams(AudioCodec.PCMU);
            }
            // 受信メディア設定
            requestObj.RedirectParams = new AnswerRequestRedirectParams();
            if (IsVideoReceiveEnabled)
            {
                requestObj.RedirectParams.Video = new AnswerRequestRedirectParamsMedia();
                requestObj.RedirectParams.Video.Ipv4 = MyAddress;
                requestObj.RedirectParams.Video.Port = VideoReceivePort;
            }
            if (IsAudioReceiveEnabled)
            {
                requestObj.RedirectParams.Audio = new AnswerRequestRedirectParamsMedia();
                requestObj.RedirectParams.Audio.Ipv4 = MyAddress;
                requestObj.RedirectParams.Audio.Port = AudioReceivePort;
            }
            var requestJson = JsonConvert.SerializeObject(requestObj);
            var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await HttpRequest.RequestJsonAsync(HttpRequestType.POST, SkyWayGatewayUrl + $"media/connections/{ConnectionId}/answer", requestHeaders, requestBody);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // レスポンス取得
                var responseObj = JsonConvert.DeserializeObject<AnswerResponse>(response.Json);
                // 不要？
                //VideoMediaId = responseObj.Params.VideoId;
                //VideoReceivePort = responseObj.Params.VideoPort;
                //AudioMediaId = responseObj.Params.AudioId;
                //AudioReceivePort = responseObj.Params.AudioPort;
                // メディアストリームイベント監視タスク開始
                // バックグラウンドスレッドで行う
                UniTask.Run(async () =>
                {
                    while (true)
                    {
                        if (!IsConnected)
                        {
                            // 切断されたら終了
                            //Debug.Log("DataChannel event polling task end. (Disconnect)");
                            break;
                        }
                        // TODO: エラーハンドリング
                        await GetMediaStreamEvent();
                    }
                }).Forget();
                // ビデオ受信用RTPクライアント生成
                if (IsVideoReceiveEnabled)
                {
                    while (true)
                    {
                        try
                        {
                            VideoReceiveClient = new MyRtpClient(VideoReceivePort);
                            //Debug.Log($"UdpClient(Receive) create succeeded. Port: {ReceivePort}");
                            break;
                        }
                        catch (Exception)
                        {
                            //Debug.Log($"UdpClient(Receive) create failed. Port: {ReceivePort}");
                            VideoReceivePort++;
                        }
                        await UniTask.Delay(100);
                    }
                    // データ受信時コールバックを設定
                    VideoReceiveClient.OnDataReceived += (data) => OnReceived?.Invoke(data);
                    // 非同期受信開始
                    VideoReceiveClient.ReceiveAsync().Forget();
                }
                // オーディオ受信用RTPクライアント生成
                if (IsAudioReceiveEnabled)
                {
                    while (true)
                    {
                        try
                        {
                            AudioReceiveClient = new MyRtpClient(AudioReceivePort);
                            //Debug.Log($"UdpClient(Receive) create succeeded. Port: {ReceivePort}");
                            break;
                        }
                        catch (Exception)
                        {
                            //Debug.Log($"UdpClient(Receive) create failed. Port: {ReceivePort}");
                            AudioReceivePort++;
                        }
                        await UniTask.Delay(100);
                    }
                    // データ受信時コールバックを設定
                    AudioReceiveClient.OnDataReceived += (data) => OnReceived?.Invoke(data);
                    // 非同期受信開始
                    AudioReceiveClient.ReceiveAsync().Forget();
                }
            }
            else
            {
                Debug.LogError($"Call [{ConnectionId}] failed: {response.StatusCode}");
            }
        }

        /// <summary>
        /// メディアストリーム接続をクローズ
        /// </summary>
        /// <returns></returns>
        public async UniTask CloseMediaStream()
        {
            if (string.IsNullOrEmpty(ConnectionId))
            {
                Debug.Log($"MediaStream already closed");
                return;
            }

            // Gateway にクローズ要求
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
            };
            var response = await HttpRequest.RequestJsonAsync(HttpRequestType.DELETE, SkyWayGatewayUrl + $"media/connections/{ConnectionId}", requestHeaders);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Debug.Log($"CloseMediaStream [{ConnectionId}] succeeded");
                // RTPクライアントクローズ
                VideoReceiveClient?.Close();
                AudioReceiveClient?.Close();
                // 初期化
                ConnectionId = "";
                IsAvailable = false;
            }
            else
            {
                Debug.LogError($"CloseMediaStream [{ConnectionId}] failed: {response.StatusCode}");
            }
        }

        /// <summary>
        /// Gateway からのメディアストリームイベントを受信する
        /// </summary>
        /// <returns></returns>
        public async UniTask<bool> GetMediaStreamEvent()
        {
            // Gateway に Long Poll でイベント監視
            var requestHeaders = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("accept", "application/json"),
            };
            var response = await HttpRequest.RequestJsonAsync(HttpRequestType.GET, SkyWayGatewayUrl + $"media/connections/{ConnectionId}/events", requestHeaders, isLongPolling: true);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseObj = JsonConvert.DeserializeObject<GetMediaStreamEventResponse>(response.Json);
                if (responseObj.Event == "STREAM")
                {
                    // メディアストリームが接続完了した
                    Debug.Log("GetMediaStreamEvent: STREAM");
                    // ストリーム内容取得
                    // ドキュメント通りに値が返ってこない模様
                    //var mediaId = responseObj.StreamOptions.StreamParams.MediaId;
                    //var address = responseObj.StreamOptions.StreamParams.Ipv4;
                    //var port = responseObj.StreamOptions.StreamParams.Port;
                    //Debug.Log($"STREAM MediaId: {mediaId}, Address: {address}, Port: {port}");
                    // データチャンネル利用準備完了
                    IsAvailable = true;
                }
                else if (responseObj.Event == "CLOSE")
                {
                    // メディアストリームが切断された
                    Debug.Log("GetMediaStreamEvent: CLOSE");
                    // メディアストリーム利用不可
                    IsAvailable = false;
                    // イベント通知
                    GameController.Instance.MainContext.Post(_ =>
                    {
                        OnClosed?.Invoke();
                    }, null);
                }
                else if (responseObj.Event == "ERROR")
                {
                    // エラー
                    Debug.Log("GetMediaStreamEvent: ERROR");
                    return false;
                }
                return true;
            }
            else
            {
                // タイムアウトする仕様なのでコメントアウトしておく
                // TODO: タイムアウト対応
                //Debug.LogError($"GetPeerEvent failed: {response.StatusCode}");
                return false;
            }
        }

        /// <summary>
        /// 映像を送信
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async UniTask SendVideo(byte[] data)
        {
            if (IsAvailable)
            {
                await VideoSendClient.SendAsync(data, data.Length);
            }
        }

        /// <summary>
        /// 音声を送信
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async UniTask SendAudio(byte[] data)
        {
            if (IsAvailable)
            {
                await AudioSendClient.SendAsync(data, data.Length);
            }
        }

        private ConstraintsMediaParams CreateConstraintsMediaParams(VideoCodec codec)
        {
            var ret = new ConstraintsMediaParams();
            switch (codec)
            {
                case VideoCodec.H264:
                    ret.BandWidth = 0;
                    ret.Codec = "H264";
                    ret.MediaId = VideoMediaId;
                    ret.PayloadType = 100;
                    ret.SamplingRate = 90000;
                    break;
            }
            return ret;
        }

        private ConstraintsMediaParams CreateConstraintsMediaParams(AudioCodec codec)
        {
            var ret = new ConstraintsMediaParams();
            switch (codec)
            {
                case AudioCodec.PCMU:
                    ret.BandWidth = 0;
                    ret.Codec = "PCMU";
                    ret.MediaId = AudioMediaId;
                    ret.PayloadType = 0;
                    ret.SamplingRate = 8000;
                    break;
                case AudioCodec.OPUS:
                    ret.BandWidth = 0;
                    ret.Codec = "OPUS";
                    ret.MediaId = AudioMediaId;
                    ret.PayloadType = 111;
                    ret.SamplingRate = 48000;
                    break;
            }
            return ret;
        }
    }
}
