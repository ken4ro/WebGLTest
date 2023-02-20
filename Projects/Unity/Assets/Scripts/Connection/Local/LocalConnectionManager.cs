using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LocalConnectionManager : IConnection
{
    /// <summary>
    /// 自身のIPアドレス
    /// </summary>
    public string Self
    {
        get
        {
            if (string.IsNullOrEmpty(_self))
            {
                _self = NetworkHelper.GetNetworkAddress();
                if (string.IsNullOrEmpty(_self))
                {
                    _self = NetworkHelper.GetEthernetAddress();
                    if (string.IsNullOrEmpty(_self))
                    {
                        _self = NetworkHelper.GetWiFiAddress();
                    }
                }
            }
            if (string.IsNullOrEmpty(_self))
            {
                Debug.LogError("Get IP Address failed!");
            }
            return _self;
        }
    }
    private string _self;

    /// <summary>
    /// 接続対象のIPアドレス
    /// </summary>
    public string Target
    {
        get { return _target; }
        set
        {
            _target = value;
            if (!_isInitializeSender)
            {
                InitializeSender();
            }
        }
    }
    private string _target = "";

    /// <summary>
    /// 初期化済みかどうか
    /// </summary>
    public bool IsInitialized { get => _isInitializeReceiver && _isInitializeSender; }

    /// <summary>
    /// 接続済みかどうか
    /// </summary>
    public bool IsConnected { get => IsInitialized; }

    /// <summary>
    /// 利用可能かどうか
    /// </summary>
    public bool IsAvailable { get { return _isAvailable; } }
    private bool _isAvailable = false;

    private bool _isInitializeReceiver = false;
    private bool _isInitializeSender = false;

    private MyUdpClient _systemCallSendClient = null;
    private MyUdpClient _cameraSendClient = null;
    private MyUdpClient _audioSendClient = null;
    private MyUdpClient _captureSendClient = null;
    private MyUdpClient _faceSendClient = null;

    private MyUdpClient _systemCallReceiveClient = null;
    private MyUdpClient _cameraReceiveClient = null;
    private MyUdpClient _audioReceiveClient = null;
    private MyUdpClient _captureReceiveClient = null;
    private MyUdpClient _faceReceiveClient = null;

    private int _systemCallSendPort = 40000;
    private int _cameraSendPort = 40001;
    private int _audioSendPort = 40002;
    private int _captureSendPort = 40003;
    private int _faceSendPort = 40004;

    private int _systemCallReceivePort = 40000;
    private int _cameraReceivePort = 40001;
    private int _audioReceivePort = 40002;
    private int _captureReceivePort = 40003;
    private int _faceReceivePort = 40004;

    private Dictionary<Action<byte[]>, Action<byte[]>> _senderMap = new Dictionary<Action<byte[]>, Action<byte[]>>();
    private Dictionary<DataType, bool> _stoppedSenderMap = new Dictionary<DataType, bool>();
    private Dictionary<DataType, Action<byte[]>> _receiverMap = new Dictionary<DataType, Action<byte[]>>();

    public async UniTask Initialize()
    {
        InitializeReceiver();
    }

    public async UniTask Dispose()
    {
        _isAvailable = false;

        _self = "";
        _target = "";

        foreach (var callback in _senderMap.Keys)
        {
            RemoveSenderEvent(callback);
        }
        _senderMap.Clear();

        _receiverMap.Clear();

        _systemCallReceiveClient.OnDataReceived -= SystemCallReceived;
        _cameraReceiveClient.OnDataReceived -= CameraDataReceived;
        _audioReceiveClient.OnDataReceived -= AudioDataReceived;
        _captureReceiveClient.OnDataReceived -= CaptureDataReceived;
        _faceReceiveClient.OnDataReceived -= FaceDataReceived;

        _systemCallSendClient?.Close();
        _cameraSendClient?.Close();
        _audioSendClient?.Close();
        _captureSendClient?.Close();
        _faceSendClient?.Close();

        _systemCallReceiveClient?.Close();
        _cameraReceiveClient?.Close();
        _audioReceiveClient?.Close();
        _captureReceiveClient?.Close();
        _faceReceiveClient?.Close();

        _isInitializeSender = false;
        _isInitializeReceiver = false;

        Debug.Log("LocalConnectionManager.Dispose completed.");
    }

    public void Enable()
    {
        _isAvailable = true;
    }

    public void Disable()
    {
        _isAvailable = false;
    }

    public void AddSender(global::DataType type, ref Action<byte[]> callback)
    {
        Action<byte[]> sender = (data) =>
        {
            if (IsAvailable)
            {
                Send(type, data).Forget();
            }
        };
        callback += sender;
        _senderMap[callback] = sender;
    }

    public void AddReceiver(DataType type, Action<byte[]> callback)
    {
        if (callback == null) return;

        _receiverMap[type] = callback;
    }

    public void RemoveSender(ref Action<byte[]> callback)
    {
        if (callback == null) return;

        if (_senderMap.TryGetValue(callback, out Action<byte[]> sender))
        {
            _senderMap.Remove(callback);
            callback -= sender;
        }
    }

    public void RemoveReceiver(DataType type)
    {
        _receiverMap.Remove(type);
    }

    public async UniTask Connect()
    {
        // ローカル接続時は不要
    }

    public async UniTask Send(DataType type, byte[] data)
    {
        if (!IsAvailable) return;

        if (!_isInitializeSender) return;

        if (_stoppedSenderMap.ContainsKey(type) && _stoppedSenderMap[type]) return;

        switch (type)
        {
            case DataType.SystemCall:
                SendSystemCall(data);
                break;
            case DataType.Camera:
                await _cameraSendClient.SendAsync(data, data.Length);
                break;
            case DataType.Audio:
                await _audioSendClient.SendAsync(data, data.Length);
                break;
            case DataType.Capture:
                await _captureSendClient.SendAsync(data, data.Length);
                break;
            case DataType.Face:
                await _faceSendClient.SendAsync(data, data.Length);
                break;
            default:
                Debug.LogError($"LocalConnectionManager.Send datatype error: {type.ToString()}");
                break;
        }
    }

    public async UniTask SendSplit(DataType type, byte[] data)
    {
        var splitSize = 10;
        var splitArraySize = data.Length / splitSize;
        foreach (var chunk in data.Chunks(splitArraySize))
        {
            await Send(type, chunk.ToArray());
            await UniTask.Delay(100);
        }
        await Send(type, Encoding.UTF8.GetBytes("split end"));
    }

    public void Pause(DataType type)
    {
        _stoppedSenderMap[type] = true;
    }

    public void Restart(DataType type)
    {
        _stoppedSenderMap[type] = false;
    }

    private void InitializeSender()
    {
        // 送信用UDPクライアント生成
        _systemCallSendClient = new MyUdpClient(Target, _systemCallSendPort);
        _cameraSendClient = new MyUdpClient(Target, _cameraSendPort);
        _audioSendClient = new MyUdpClient(Target, _audioSendPort);
        _captureSendClient = new MyUdpClient(Target, _captureSendPort);
        _faceSendClient = new MyUdpClient(Target, _faceSendPort);

        _isInitializeSender = true;
    }

    private void InitializeReceiver()
    {
        // 受信用UDPクライアント生成
        _systemCallReceiveClient = new MyUdpClient(_systemCallReceivePort);
        _cameraReceiveClient = new MyUdpClient(_cameraReceivePort);
        _audioReceiveClient = new MyUdpClient(_audioReceivePort);
        _captureReceiveClient = new MyUdpClient(_captureReceivePort);
        _faceReceiveClient = new MyUdpClient(_faceReceivePort);

        // データ受信時コールバック設定
        _systemCallReceiveClient.OnDataReceived += SystemCallReceived;
        _cameraReceiveClient.OnDataReceived += CameraDataReceived;
        _audioReceiveClient.OnDataReceived += AudioDataReceived;
        _captureReceiveClient.OnDataReceived += CaptureDataReceived;
        _faceReceiveClient.OnDataReceived += FaceDataReceived;

        // 受信開始
        _systemCallReceiveClient.ReceiveAsync().Forget();
        _cameraReceiveClient.ReceiveAsync().Forget();
        _audioReceiveClient.ReceiveAsync().Forget();
        _captureReceiveClient.ReceiveAsync().Forget();
        _faceReceiveClient.ReceiveAsync().Forget();

        _isInitializeReceiver = true;
    }

    private void RemoveSenderEvent(Action<byte[]> callback)
    {
        if (_senderMap.TryGetValue(callback, out Action<byte[]> sender))
        {
            callback -= sender;
        }
    }

    private void Received(DataType type, byte[] data)
    {
        if (_receiverMap.Count == 0) return;

        if (!_receiverMap.ContainsKey(type))
        {
            //Debug.Log($"LocalConnectionManager.Received key not found: {type.ToString()}");
            return;
        }

        _receiverMap[type]?.Invoke(data);
    }

    private void SendSystemCall(byte[] data)
    {
        for (var i = 0; i < 10; i++)
        {
            _systemCallSendClient.SendSync(data, data.Length);
            System.Threading.Thread.Sleep(1);
        }
    }

    private void SystemCallReceived(byte[] data)
    {
        Target = _systemCallReceiveClient.TargetIPAddress;

        Received(DataType.SystemCall, data);
    }

    private void CameraDataReceived(byte[] data)
    {
        Target = _cameraReceiveClient.TargetIPAddress;

        Received(DataType.Camera, data);
    }

    private void AudioDataReceived(byte[] data)
    {
        Target = _audioReceiveClient.TargetIPAddress;

        Received(DataType.Audio, data);
    }

    private void CaptureDataReceived(byte[] data)
    {
        Target = _captureReceiveClient.TargetIPAddress;

        Received(DataType.Capture, data);
    }

    private void FaceDataReceived(byte[] data)
    {
        Target = _faceReceiveClient.TargetIPAddress;

        Received(DataType.Face, data);
    }
}
