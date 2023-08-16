using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

// シグナリングサーバーと WebRTC(SkyWay) 接続を担当
public class WebRTCManager : IConnection
{
    /// <summary>
    /// 自身のピアID
    /// </summary>
    public string Self
    {
        get => /*SkyWayService.PeerId*/"";
    }

    /// <summary>
    /// 接続対象のピアID
    /// </summary>
    public string Target
    {
        get => /*SkyWayService.TargetPeerId*/"";
        set => /*SkyWayService.TargetPeerId = value*/value = value;
    }

    /// <summary>
    /// 接続中かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsConnected { get { return /*SkyWayService.IsConnected*/false; } }

    /// <summary>
    /// 利用可能かどうか
    /// </summary>
    public bool IsAvailable { get { return _isAvailable; } }
    private bool _isAvailable = false;

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
        _sender = new WebRTCSender();
        _receiver = new WebRTCReceiver();

        //await SkyWayService.CreatePeer();
    }

    /// <summary>
    /// 終了
    /// </summary>
    public async UniTask Dispose()
    {
        if (IsConnected)
        {
            //await SkyWayService.DeletePeer();
        }

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
}
