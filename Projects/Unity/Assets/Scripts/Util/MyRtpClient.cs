using Cysharp.Threading.Tasks;
using System;
using System.Net.Sockets;
using UnityEngine;

public class MyRtpClient
{
    /// <summary>
    /// データ受信時コールバック
    /// </summary>
    public Action<byte[]> OnDataReceived = null;

    private static readonly int HeaderSize = 12;

    private RtpHeader _rtpHeader = new RtpHeader();
    private UdpClient _client = null;
    private int _port = 0;

    /// <summary>
    /// ポートを指定して初期化(受信)
    /// </summary>
    /// <param name="port"></param>
    public MyRtpClient(int port)
    {
        _port = port;
        _client = new UdpClient(port);
    }

    /// <summary>
    /// ホスト名とポートを指定して初期化(送信)
    /// </summary>
    /// <param name="hostname"></param>
    /// <param name="port"></param>
    public MyRtpClient(string hostname, int port)
    {
        _port = port;
        _client = new UdpClient(hostname, port);
    }

    /// <summary>
    /// クローズ
    /// </summary>
    public void Close()
    {
        _client.Close();
    }

    /// <summary>
    /// 非同期で受信
    /// </summary>
    /// <returns></returns>
    public async UniTask ReceiveAsync()
    {
        while (true)
        {
            byte[] data = null;
            try
            {
                var result = await _client.ReceiveAsync();
                data = GetRTPPayloadValue(result.Buffer);
                Debug.Log($"RTP Received: {data.Length}");
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
    public async UniTask SendAsync(byte[] data, int dataLength)
    {
        var header = _rtpHeader.GetHeader();
        var packet = new byte[HeaderSize + dataLength];
        Buffer.BlockCopy(header, 0, packet, 0, HeaderSize);
        Buffer.BlockCopy(data, 0, packet, HeaderSize, dataLength);
        Debug.Log($"SendAsync packet size: {packet.Length}");
        await _client.SendAsync(packet, dataLength);
    }

    private byte[] GetRTPPayloadValue(byte[] packet)
    {
        var payload = new byte[packet.Length - HeaderSize];
        Buffer.BlockCopy(packet, HeaderSize, payload, 0, payload.Length);

        return payload;
    }
}
