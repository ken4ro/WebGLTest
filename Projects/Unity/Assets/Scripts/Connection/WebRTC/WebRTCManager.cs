using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// シグナリングサーバーと WebRTC(SkyWay) 接続を担当
public class WebRTCManager : IConnection
{
    /// <summary>
    /// 自身のピアID
    /// </summary>
    public string Self
    {
        get => SkyWayService.PeerId;
    }

    /// <summary>
    /// 接続対象のピアID
    /// </summary>
    public string Target
    {
        get => SkyWayService.TargetPeerId;
        set => SkyWayService.TargetPeerId = value;
    }

    /// <summary>
    /// 接続中かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsConnected { get { return SkyWayService.IsConnected; } }

    /// <summary>
    /// 利用可能かどうか
    /// </summary>
    public bool IsAvailable { get { return _isAvailable && IsAllDataChannelAvailable; } }
    private bool _isAvailable = false;

    /// <summary>
    /// 全てのデータチャンネルが利用可能かどうか
    /// </summary>
    public bool IsAllDataChannelAvailable
    {
        get
        {
            foreach (var dataChannel in SkyWayService.DataChannelMap.Values)
            {
                if (!dataChannel.IsAvailable)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// いずれかのデータチャンネルが利用可能かどうか
    /// </summary>
    public bool IsAnyDataChannelAvailable
    {
        get
        {
            foreach (var dataChannel in SkyWayService.DataChannelMap.Values)
            {
                if (dataChannel.IsAvailable)
                {
                    return true;
                }
            }
            return false;
        }
    }

    // 送受信委譲クラス生成
    private WebRTCSender _sender = null;
    private WebRTCReceiver _receiver = null;

    // SkyWay Gateway イベント排他処理用
    SemaphoreSlim _semaphoreForSkyWayDataChannelEvent = new SemaphoreSlim(1);

    /// <summary>
    /// 初期化
    /// </summary>
    /// <returns></returns>
    public async UniTask Initialize()
    {
        SkyWayService.OnPeerEventConnect += SkyWayDataChannelConnected;

        _sender = new WebRTCSender();
        _receiver = new WebRTCReceiver();

        await SkyWayService.CreatePeer();
    }

    /// <summary>
    /// 終了
    /// </summary>
    public async UniTask Dispose()
    {
        if (IsConnected)
        {
            await CloseDataChannel(DataType.SystemCall);
            await CloseDataChannel(DataType.Camera);
            await CloseDataChannel(DataType.Capture);
            await CloseDataChannel(DataType.Face);
            await CloseDataChannel(DataType.Audio);

            await SkyWayService.DeletePeer();
        }

        SkyWayService.OnPeerEventConnect -= SkyWayDataChannelConnected;

        _sender.Dispose();
        _receiver.Dispose();
    }

    public void Enable()
    {
        _isAvailable = true;
    }

    public void Disable()
    {
        _isAvailable = false;
    }

    /// <summary>
    /// データチャンネル送信用コールバックを追加
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="type"></param>
    public void AddSender(DataType type, ref Action<byte[]> callback) => _sender.Add(type, ref callback);

    /// <summary>
    /// データチャンネル受信用コールバックを追加
    /// </summary>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void AddReceiver(DataType type, Action<byte[]> callback) => _receiver.Add(type, callback);

    /// <summary>
    /// データチャンネル送信用コールバックを削除
    /// </summary>
    /// <param name="callback"></param>
    public void RemoveSender(ref Action<byte[]> callback) => _sender.Remove(ref callback);

    /// <summary>
    /// データチャンネル受信用コールバックを削除
    /// </summary>
    /// <param name="callback"></param>
    public void RemoveReceiver(DataType type) => _receiver.Remove(type);

    /// <summary>
    /// 接続開始
    /// </summary>
    /// <param name="peerId"></param>
    /// <returns></returns>
    public async UniTask Connect()
    {
        // データチャンネル初期化
        await OpenDataChannel(DataType.SystemCall);
        await OpenDataChannel(DataType.Camera);
        await OpenDataChannel(DataType.Capture);
        await OpenDataChannel(DataType.Face);
        await OpenDataChannel(DataType.Audio);
        // データチャンネル接続開始
        await ConnectDataChannel(DataType.SystemCall);
        await ConnectDataChannel(DataType.Camera);
        await ConnectDataChannel(DataType.Capture);
        await ConnectDataChannel(DataType.Face);
        await ConnectDataChannel(DataType.Audio);
    }

    /// <summary>
    /// データチャンネルでデータを送信する
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async UniTask Send(DataType type, byte[] data)
    {
        if (type != DataType.SystemCall && !IsAvailable) return;

        await _sender.SendData(type, data);
    }

    /// <summary>
    /// データチャンネルでデータを分割送信する
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async UniTask SendSplit(DataType type, byte[] data) => await _sender.SendSplitData(type, data);

    /// <summary>
    /// データ送信停止
    /// </summary>
    /// <param name="type"></param>
    public void Pause(DataType type) => _sender.Stop(type);

    /// <summary>
    /// データ送信再開
    /// </summary>
    /// <param name="type"></param>
    public void Restart(DataType type) => _sender.Restart(type);

    // データチャンネル初期化
    private async UniTask OpenDataChannel(DataType type)
    {
        var dataChannel = SkyWayService.DataChannelMap[(int)type];
        await dataChannel.Open();
        switch (type)
        {
            case DataType.SystemCall:
                dataChannel.OnReceived += SystemCallDataReceived;
                dataChannel.OnClosed += SystemCallDataChannelClosed;
                break;
            case DataType.Camera:
                dataChannel.OnReceived += CameraDataReceived;
                dataChannel.OnClosed += CameraDataChannelClosed;
                break;
            case DataType.Capture:
                dataChannel.OnReceived += CaptureDataReceived;
                dataChannel.OnClosed += CaptureDataChannelClosed;
                break;
            case DataType.Face:
                dataChannel.OnReceived += FaceDataReceived;
                dataChannel.OnClosed += FaceDataChannelClosed;
                break;
            case DataType.Audio:
                dataChannel.OnReceived += AudioDataReceived;
                dataChannel.OnClosed += AudioDataChannelClosed;
                break;
            default:
                break;
        }
    }

    // メディアストリーム初期化
    private async UniTask OpenMediaStream(DataType type)
    {
        var mediaStream = SkyWayService.MediaStreamMap[(int)type];
        switch (type)
        {
            case DataType.Camera:
                await mediaStream.Open(true, false, true, false);
                mediaStream.OnReceived += CameraMediaReceived;
                mediaStream.OnClosed += CameraMediaStreamClosed;
                break;
            case DataType.Capture:
                await mediaStream.Open(true, false, true, false);
                mediaStream.OnReceived += CaptureMediaReceived;
                mediaStream.OnClosed += CaptureMediaStreamClosed;
                break;
            case DataType.Audio:
                await mediaStream.Open(false, true, false, true);
                mediaStream.OnReceived += AudioMediaReceived;
                mediaStream.OnClosed += AudioMediaStreamClosed;
                break;
        }
    }

    // データチャンネルを閉じる
    private async UniTask CloseDataChannel(DataType type)
    {
        var dataChannel = SkyWayService.DataChannelMap[(int)type];

        switch (type)
        {
            case DataType.SystemCall:
                dataChannel.OnReceived -= SystemCallDataReceived;
                dataChannel.OnClosed -= SystemCallDataChannelClosed;
                break;
            case DataType.Camera:
                dataChannel.OnReceived -= CameraDataReceived;
                dataChannel.OnClosed -= CameraDataChannelClosed;
                break;
            case DataType.Capture:
                dataChannel.OnReceived -= CaptureDataReceived;
                dataChannel.OnClosed -= CaptureDataChannelClosed;
                break;
            case DataType.Face:
                dataChannel.OnReceived -= FaceDataReceived;
                dataChannel.OnClosed -= FaceDataChannelClosed;
                break;
            case DataType.Audio:
                dataChannel.OnReceived -= AudioDataReceived;
                dataChannel.OnClosed -= AudioDataChannelClosed;
                break;
            default:
                break;
        }
        await dataChannel.CloseDataConnection();
        await dataChannel.Close();
    }

    // メディアストリームを閉じる
    private async UniTask CloseMediaStream(DataType type)
    {
        var mediaStream = SkyWayService.MediaStreamMap[(int)type];
        switch (type)
        {
            case DataType.Camera:
                mediaStream.OnReceived -= CameraMediaReceived;
                mediaStream.OnClosed -= CameraMediaStreamClosed;
                break;
            case DataType.Capture:
                mediaStream.OnReceived -= CaptureMediaReceived;
                mediaStream.OnClosed -= CaptureMediaStreamClosed;
                break;
            case DataType.Audio:
                mediaStream.OnReceived -= AudioMediaReceived;
                mediaStream.OnClosed -= AudioMediaStreamClosed;
                break;
        }
        await mediaStream.CloseMediaStream();
        await mediaStream.Close();
    }

    // データチャンネル接続
    private async UniTask ConnectDataChannel(DataType type) => await SkyWayService.DataChannelMap[(int)type].Connect();

    // メディアストリーム接続
    private async UniTask CallMediaStream(DataType type) => await SkyWayService.MediaStreamMap[(int)type].Call();

#region イベントコールバック

    // データチャンネル接続がリクエストされた
    private async void SkyWayDataChannelConnected(string dataConnectionId)
    {
        // ロック
        await _semaphoreForSkyWayDataChannelEvent.WaitAsync();

        // データチャンネル初期化
        var index = SkyWayService.GetEmptyDataChannelIndex();
        var dataChannel = SkyWayService.DataChannelMap[index];
        dataChannel.ConnectionId = dataConnectionId;
        var dataType = (DataType)index;
        await OpenDataChannel(dataType);
        // データ転送設定
        await dataChannel.ChangeDataConnectionSetting();

        Debug.Log($"SkyWayDataChannelConnected: {dataType}");

        // ロック解除
        _semaphoreForSkyWayDataChannelEvent.Release();
    }

    // システムコールデータ送受信用データチャンネルがクローズした
    private void SystemCallDataChannelClosed() => CloseDataChannel(DataType.SystemCall).Forget();

    // カメラ映像データ送受信用データチャンネルがクローズした
    private void CameraDataChannelClosed() => CloseDataChannel(DataType.Camera).Forget();

    // キャプチャ映像データ送受信用データチャンネルがクローズした
    private void CaptureDataChannelClosed() => CloseDataChannel(DataType.Capture).Forget();

    // 顔情報データ送受信用データチャンネルがクローズした
    private void FaceDataChannelClosed() => CloseDataChannel(DataType.Face).Forget();

    // 音声データ送受信用データチャンネルがクローズした
    private void AudioDataChannelClosed() => CloseDataChannel(DataType.Audio).Forget();

    // カメラ映像送受信用メディアストリームがクローズした
    private void CameraMediaStreamClosed() => CloseMediaStream(DataType.Camera).Forget();

    // キャプチャ映像送受信用メディアストリームがクローズした
    private void CaptureMediaStreamClosed() => CloseMediaStream(DataType.Capture).Forget();

    // 音声送受信用メディアストリームがクローズした
    private void AudioMediaStreamClosed() => CloseMediaStream(DataType.Audio).Forget();

    // データチャンネルでシステムコールデータを受け取る
    private void SystemCallDataReceived(byte[] data) => _receiver.Receive(DataType.SystemCall, data);

    // データチャンネルでカメラ映像データを受け取る
    private void CameraDataReceived(byte[] data) => _receiver.Receive(DataType.Camera, data);

    // データチャンネルでキャプチャ映像データを受け取る
    private void CaptureDataReceived(byte[] data) => _receiver.Receive(DataType.Capture, data);

    // データチャンネルで顔情報データを受け取る
    private void FaceDataReceived(byte[] data) => _receiver.Receive(DataType.Face, data);

    // データチャンネルで音声データを受け取る
    private void AudioDataReceived(byte[] data) => _receiver.Receive(DataType.Audio, data);

    // メディアストリームでカメラ映像を受け取る
    private void CameraMediaReceived(byte[] data) => _receiver.Receive(DataType.Camera, data);

    // メディアストリームでキャプチャ映像を受け取る
    private void CaptureMediaReceived(byte[] data) => _receiver.Receive(DataType.Capture, data);

    // メディアストリームで音声を受け取る
    private void AudioMediaReceived(byte[] data) => _receiver.Receive(DataType.Audio, data);

#endregion イベントコールバック
}
