using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class MyTcpClient : IDisposable
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

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    public MyTcpClient(string address, int port)
    {
        try
        {
            _tcpClient = new TcpClient(TargetIPAddress, port);
            TargetIPAddress = address;
            Debug.Log($"Connected to server: {address}:{port.ToString()}");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    public void Dispose()
    {
        Close();
    }

    /// <summary>
    /// クローズ
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
