using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class MyTcpServer : IDisposable
{
    /// <summary>
    /// データ受信時コールバック
    /// </summary>
    public Action<byte[]> OnDataReceived = null;

    /// <summary>
    /// 接続先のIPアドレス
    /// </summary>
    public string TargetIPAddress { get; set; } = "";

    private TcpClient _tcpClient = null;
    private TcpListener _tcpListener = null;

    /// <summary>
    /// 初期化(サーバー)
    /// </summary>
    /// <param name="port"></param>
    public MyTcpServer(int port)
    {
        _tcpListener = new TcpListener(IPAddress.Loopback, port);
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    public void Dispose()
    {
        Stop();
        Close();
    }

    /// <summary>
    /// 接続待機開始
    /// </summary>
    public void Listen()
    {
        _tcpListener.Start();
    }

    /// <summary>
    /// 接続待機停止
    /// </summary>
    public void Stop()
    {
        _tcpListener.Stop();
    }

    /// <summary>
    /// 接続確立
    /// </summary>
    /// <returns></returns>
    public async UniTask Accept()
    {
        _tcpClient = await _tcpListener.AcceptTcpClientAsync();
        var remoteEndPoint = (IPEndPoint)_tcpClient.Client.RemoteEndPoint;
        TargetIPAddress = remoteEndPoint.Address.ToString();
        Debug.Log($"Client ( {TargetIPAddress}:{remoteEndPoint.Port.ToString()} ) connected.");
    }

    /// <summary>
    /// 接続クローズ
    /// </summary>
    public void Close()
    {
        _tcpClient.Close();
    }

    /// <summary>
    /// 非同期受信
    /// </summary>
    /// <returns></returns>
    public async UniTask ReceiveAsync()
    {
        if (_tcpClient == null) return;

        var stream = _tcpClient.GetStream();
        var buffer = new byte[_tcpClient.ReceiveBufferSize];
        while (_tcpClient.Connected)
        {
            var readSize = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (readSize > 0)
            {
                var data = new byte[readSize];
                Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
                OnDataReceived?.Invoke(data);
            }
        }
    }

    /// <summary>
    /// 非同期送信
    /// </summary>
    /// <param name="data"></param>
    /// <param name="dataLength"></param>
    /// <returns></returns>
    public async UniTask SendAsync(byte[] data)
    {
        if (_tcpClient == null) return;

        if (data.Length > _tcpClient.SendBufferSize)
        {
            Debug.LogError($"SendAsync canceled. Buffer size [{data.Length}] over limit.");
        }

        var stream = _tcpClient.GetStream();
        await stream.WriteAsync(data, 0, data.Length);
    }
}
