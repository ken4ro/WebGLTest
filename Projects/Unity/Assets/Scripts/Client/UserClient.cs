using System;
using UnityEngine;
using UniRx;

/// <summary>
/// ユーザークライアント独自の処理
/// </summary>
public class UserClient : IClient
{
    private bool _isChangeFacial = false;
    private bool _isChangeAnimation = false;

    public void Initialize()
    {
        SignalingManager.Instance.OnReceivedMessage += ReceivedSignalingMessage;
    }

    public void Dispose()
    {
        SignalingManager.Instance.OnReceivedMessage -= ReceivedSignalingMessage;
    }

    public string GetSignalingLoginKey() => "loginUser";

    public string GetSignalingLoginValue() => ConnectionManager.Instance.Self;

    public string GetSignalingLogoutKey() => "logoutUser";

    public string GetSignalingLogoutValue() => null;

    public string GetSignalingCancelKey() => "cancelUser";

    public void AddHandler()
    {
        // 送信
        ConnectionManager.Instance.AddSender(DataType.Audio, ref MicManager.Instance.OnMicDataAvailable);
        // 受信
        ConnectionManager.Instance.AddReceiver(DataType.SystemCall, ReceivedSystemCall);
        //ConnectionManager.Instance.AddReceiver(DataType.Audio, AudioManager.Instance.AudioDataReceived);
        ConnectionManager.Instance.AddReceiver(DataType.Face, FaceInfoManager.Instance.FaceInfoReceived);
        ConnectionManager.Instance.AddReceiver(DataType.Capture, UIManager.Instance.ReceiveDocument);
    }

    public void RemoveHandler()
    {
        // 送信
        ConnectionManager.Instance.RemoveSender(ref MicManager.Instance.OnMicDataAvailable);
        // 受信
        ConnectionManager.Instance.RemoveReceiver(DataType.SystemCall);
        //ConnectionManager.Instance.RemoveReceiver(DataType.Audio);
        ConnectionManager.Instance.RemoveReceiver(DataType.Face);
        ConnectionManager.Instance.RemoveReceiver(DataType.Capture);
    }

    public async void ReceivedSystemCall(byte[] bytes)
    {
        var msg = System.Text.Encoding.UTF8.GetString(bytes);
        Debug.Log($"ReceivedSystemCall: {msg}");

        switch (msg)
        {
            case "connection end":
                ConnectionManager.Instance.Disable();
                GlobalState.Instance.CurrentState.Value = GlobalState.State.Starting;
                break;
            case "animation smile":
                if (!_isChangeFacial)
                {
                    _isChangeFacial = true;
                    Observable.Timer(TimeSpan.FromMilliseconds(3000)).Subscribe(_ =>
                    {
                        _isChangeFacial = false;
                    });
                    await CharacterManager.Instance.ChangeFacial(FacialType.Smile, 3000);
                }
                break;
            case "animation angry":
                if (!_isChangeFacial)
                {
                    _isChangeFacial = true;
                    Observable.Timer(TimeSpan.FromMilliseconds(3000)).Subscribe(_ =>
                    {
                        _isChangeFacial = false;
                    });
                    await CharacterManager.Instance.ChangeFacial(FacialType.Angry, 3000);
                }
                break;
            case "animation cry":
                if (!_isChangeFacial)
                {
                    _isChangeFacial = true;
                    Observable.Timer(TimeSpan.FromMilliseconds(3000)).Subscribe(_ =>
                    {
                        _isChangeFacial = false;
                    });
                    await CharacterManager.Instance.ChangeFacial(FacialType.Sad, 3000);
                }
                break;
            case "animation ojigi":
                if (!_isChangeAnimation)
                {
                    _isChangeAnimation = true;
                    FaceInfoManager.Instance.Disable();
                    CharacterManager.Instance.EnableAnimation();
                    Observable.Timer(TimeSpan.FromMilliseconds(3000)).Subscribe(_ =>
                    {
                        _isChangeAnimation = false;
                    });
                    await CharacterManager.Instance.ChangeAnimation(AnimationType.Ojigi);
                    FaceInfoManager.Instance.Enable();
                    CharacterManager.Instance.DisableAnimation();
                    CharacterManager.Instance.SetTransformsForIdle();
                }
                break;
            case "enable camera":
                // カメラ有効
                UIManager.Instance.EnableWebCameraPanel();
                UIManager.Instance.WebCameraPlay();
                // キャプチャ停止
                GameController.Instance.MainCamera.GetComponent<SendCameraTexture>().DisableCapture();
                break;
            case "enable capture":
                // キャプチャ開始
                GameController.Instance.MainCamera.GetComponent<SendCameraTexture>().EnableCapture();
                // カメラ無効
                UIManager.Instance.DisableWebCameraPanel();
                UIManager.Instance.WebCameraStop();
                break;
            default:
                Debug.Log($"received invalid system call: msg = {msg}");
                break;
        }
    }

    public void ReceivedSignalingMessage(string msg)
    {
        if (string.IsNullOrEmpty(msg)) return;

        Debug.Log($"ReceivedSignalingMessage: {msg}");

        switch (msg)
        {
            case "connection start":
                ConnectionManager.Instance.Enable();
                break;
            case "connection cancel":
                GlobalState.Instance.CurrentState.Value = GlobalState.State.Starting;
                break;
            default:
                Debug.LogError($"Invalid signaling message received: {msg}");
                break;
        }
    }
}
