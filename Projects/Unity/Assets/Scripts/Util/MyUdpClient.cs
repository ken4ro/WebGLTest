using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class MyUdpClient
{
    /// <summary>
    /// データ受信時コールバック
    /// </summary>
    public Action<byte[]> OnDataReceived = null;

    /// <summary>
    /// 接続先IPアドレス
    /// </summary>
    public string TargetIPAddress { get; set; } = "";

    private UdpClient _udpClient = null;

    /// <summary>
    /// ポートを指定して初期化(受信)
    /// </summary>
    /// <param name="port"></param>
    public MyUdpClient(int port)
    {
        _udpClient = new UdpClient(port);
    }

    /// <summary>
    /// ホスト名とポートを指定して初期化(送信)
    /// </summary>
    /// <param name="hostname"></param>
    /// <param name="port"></param>
    public MyUdpClient(string hostname, int port)
    {
        TargetIPAddress = hostname;
        _udpClient = new UdpClient(hostname, port);
    }

    /// <summary>
    /// クローズ
    /// </summary>
    public void Close()
    {
        _udpClient.Close();
    }

    /// <summary>
    /// 非同期で受信
    /// </summary>
    /// <returns></returns>
    public async Task ReceiveAsync()
    {
        while (true)
        {
            byte[] data = null;
            try
            {
                var result = await _udpClient.ReceiveAsync();
                TargetIPAddress = result.RemoteEndPoint.Address.ToString();
                data = result.Buffer;
            }
            catch (Exception)
            {
                break;
            }
            OnDataReceived?.Invoke(data);
        }
    }

    /// <summary>
    /// 同期で受信
    /// </summary>
    public void ReceiveSync()
    {
        while (true)
        {
            IPEndPoint remoteEP = null;
            byte[] data = null;
            try
            {
                data = _udpClient.Receive(ref remoteEP);
            }
            catch (Exception e)
            {
                Debug.Log($"UDP Receive Exception: {e.Message}");
                break;
            }
            OnDataReceived?.Invoke(data);
        }
    }

    /// <summary>
    /// 非同期で送信
    /// </summary>
    /// <param name="data"></param>
    /// <param name="dataLength"></param>
    /// <returns></returns>
    public async Task SendAsync(byte[] data, int dataLength)
    {
        await _udpClient.SendAsync(data, dataLength);
    }

    /// <summary>
    /// 同期送信
    /// </summary>
    /// <param name="data"></param>
    /// <param name="dataLength"></param>
    public void SendSync(byte[] data, int dataLength)
    {
        _udpClient.Send(data, dataLength);
    }
}
