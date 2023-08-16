using System;
using Cysharp.Threading.Tasks;

public enum DataType
{
    SystemCall,
    Camera,
    Capture,
    Face,
    Audio,
}

public interface IConnection
{
    string Self { get; }

    string Target { get; set; }

    bool IsConnected { get; }

    bool IsAvailable { get; }

    UniTask Initialize();

    UniTask Dispose();

    void Enable();

    void Disable();

    void AddSender(DataType type, ref Action<byte[]> callback);

    void AddReceiver(DataType type, Action<byte[]> callback);

    void RemoveSender(ref Action<byte[]> callback);

    void RemoveReceiver(DataType type);

    UniTask Connect();

    UniTask Send(DataType type, byte[] data);

    UniTask SendSplit(DataType type, byte[] data);

    void Pause(DataType type);

    void Restart(DataType type);
}
