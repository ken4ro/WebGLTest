using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static WebRTCManager;

/// <summary>
/// WebRTC 送信管理
/// </summary>
public class WebRTCSender : IDisposable
{
    private static readonly int SplitSize = 10;

    private Dictionary<Action<byte[]>, Action<byte[]>> _dataSenderMap = new Dictionary<Action<byte[]>, Action<byte[]>>();
    private Dictionary<Action<byte[]>, Action<byte[]>> _mediaSenderMap = new Dictionary<Action<byte[]>, Action<byte[]>>();
    private Dictionary<DataType, bool> _stoppedDataSenderMap = new Dictionary<DataType, bool>();
    private Dictionary<DataType, bool> _stoppedMediaSenderMap = new Dictionary<DataType, bool>();

    public void Add(DataType type, ref Action<byte[]> callback)
    {
        Action<byte[]> sender = (data) =>
        {
            SendData(type, data).Forget();
        };
        callback += sender;
        _dataSenderMap[callback] = sender;
    }

    public void Remove(ref Action<byte[]> callback)
    {
        if (callback == null) return;

        if (_dataSenderMap.TryGetValue(callback, out Action<byte[]> dataSender))
        {
            _dataSenderMap.Remove(callback);
            callback -= dataSender;
        }
    }

    public async UniTask SendData(DataType type, byte[] data)
    {
        if (_stoppedDataSenderMap.ContainsKey(type) && _stoppedDataSenderMap[type]) return;

        if (type == DataType.SystemCall)
        {
            for (var i = 0; i < 10; i++)
            {
                //SkyWayService.DataChannelMap[(int)type].SendSync(data);
                System.Threading.Thread.Sleep(1);
            }
        }
        else
        {
            //await SkyWayService.DataChannelMap[(int)type].SendAsync(data);
        }
    }

    public async UniTask SendSplitData(DataType type, byte[] data)
    {
        var splitArraySize = data.Length / SplitSize;
        foreach(var chunk in data.Chunks(splitArraySize))
        {
            await SendData(type, chunk.ToArray());
            await UniTask.Delay(100);
        }
        await SendData(type, Encoding.UTF8.GetBytes("split end"));
    }

    public void Stop(DataType type)
    {
        _stoppedDataSenderMap[type] = true;
    }

    public void Restart(DataType type)
    {
        _stoppedDataSenderMap[type] = false;
    }

    public void Dispose()
    {
        foreach (var callback in _dataSenderMap.Keys)
        {
            RemoveDataSenderEvent(callback);
        }
        _dataSenderMap.Clear();
    }

    private void RemoveDataSenderEvent(Action<byte[]> callback)
    {
        if (_dataSenderMap.TryGetValue(callback, out Action<byte[]> sender))
        {
            callback -= sender;
        }
    }

    private void RemoveMediaSenderEvent(Action<byte[]> callback)
    {
        if (_mediaSenderMap.TryGetValue(callback, out Action<byte[]> sender))
        {
            callback -= sender;
        }
    }
}
