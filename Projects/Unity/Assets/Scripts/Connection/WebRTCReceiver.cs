using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WebRTCManager;

/// <summary>
/// WebRTC 受信管理
/// </summary>
public class WebRTCReceiver : IDisposable
{
    private Dictionary<DataType, Action<byte[]>> _dataReceiverMap = new Dictionary<DataType, Action<byte[]>>();
    private Dictionary<DataType, Action<byte[]>> _mediaReceiverMap = new Dictionary<DataType, Action<byte[]>>();

    public void Add(DataType type, Action<byte[]> callback)
    {
        if (callback == null) return;

        _dataReceiverMap[type] = callback;
    }

    public void Remove(DataType type)
    {
        _dataReceiverMap.Remove(type);
    }

    public void Receive(DataType type, byte[] data)
    {
        if (_dataReceiverMap.Count == 0) return;

        if (!_dataReceiverMap.ContainsKey(type))
        {
            Debug.Log($"WebRTC data receiver key not found: {type.ToString()}");
            return;
        }

        _dataReceiverMap[type]?.Invoke(data);
    }

    public void Dispose()
    {
        _dataReceiverMap.Clear();
    }
}
