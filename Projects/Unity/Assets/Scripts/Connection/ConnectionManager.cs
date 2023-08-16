using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ConnectionManager : SingletonBase<ConnectionManager>
{
    public string Self { get => _connection.Self; }

    public string Target { get => _connection.Target; set => _connection.Target = value; }

    public bool IsConnected { get => _connection == null ? false : _connection.IsConnected; }

    public bool IsAvailable { get => _connection == null ? false : _connection.IsAvailable; }

    public async UniTask Initialize()
    {
        switch (GlobalState.Instance.UserSettings.Rtc.ServiceType)
        {
            case "sgateway":
                _connection = new WebRTCManager();
                break;
            case "local":
                Debug.LogError("WebGL is not supported local connection");
                break;
            default:
                _connection = new WebRTCManager();
                break;
        }
        await _connection.Initialize();
    }

    public async UniTask Dispose() => await _connection.Dispose();

    public void Enable() => _connection.Enable();

    public void Disable() => _connection.Disable();

    public void AddSender(DataType type, ref Action<byte[]> callback) => _connection.AddSender(type, ref callback);

    public void AddReceiver(DataType type, Action<byte[]> callback) => _connection.AddReceiver(type, callback);

    public void RemoveSender(ref Action<byte[]> callback) => _connection.RemoveSender(ref callback);

    public void RemoveReceiver(DataType type) => _connection.RemoveReceiver(type);

    public async UniTask Connect() => await _connection.Connect();

    public async UniTask Send(DataType type, byte[] data) => await _connection.Send(type, data);

    public async UniTask SendSplit(DataType type, byte[] data) => await _connection.SendSplit(type, data);

    public void Pause(DataType type) => _connection.Pause(type);

    public void Restart(DataType type) => _connection.Restart(type);

    private IConnection _connection;
}
