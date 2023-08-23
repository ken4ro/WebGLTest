using System.Runtime.InteropServices;
using UnityEngine;

public static class JSHelper
{
#if UNITY_EDITOR || !UNITY_WEBGL

    public static void EnableResetBtn()
    {
        Debug.Log($"EnableResetBtn called by Editor");
    }
    public static void DisableResetBtn()
    {
        Debug.Log($"DisableResetBtn called by Editor");
    }

    public static void GameControllerInitialized()
    {
        Debug.Log("GameControllerInitialized called by Editor");
    }

    #region for SpeechRecognition

    public static void StartSpeechRecognition()
    {
        Debug.Log($"StartSpeechRecognition called by Editor");
    }

    public static void StopSpeechRecognition()
    {
        Debug.Log($"StopSpeechRecognition called by Editor");
    }

    #endregion for SpeechRecognition

    #region for Signaling

    public static void SendUserToken(string userToken)
    {
        Debug.Log($"SendUserToken called by Editor: token = {userToken}");
    }

    public static void SignalingConnect()
    {
        Debug.Log($"SignalingConnect called by Editor");
    }

    public static void SignalingDisconnect()
    {
        Debug.Log($"SignalingDisconnect called by Editor");
    }

    public static void SignalingLogin()
    {
        Debug.Log($"SignalingLogin called by Editor");
    }

    public static void SignalingRelayToOperator(string message)
    {
        Debug.Log($"SignalingRelayToOperator called by Editor: message = {message}");
    }

    #endregion for Signaling

    #region for WebRTC

    public static void WebRTCConnect()
    {
        Debug.Log($"Connect called by Editor");
    }

    public static void WebRTCDisconnect()
    {
        Debug.Log($"Disconnect called by Editor");
    }

    #endregion for WebRTC

#else

    [DllImport("__Internal")]
    public static extern void EnableResetBtn();

    [DllImport("__Internal")]
    public static extern void DisableResetBtn();

    [DllImport("__Internal")]
    public static extern void GameControllerInitialized();

    #region for SpeechRecognition

    [DllImport("__Internal")]
    public static extern void StartSpeechRecognition();

    [DllImport("__Internal")]
    public static extern void StopSpeechRecognition();

    #endregion for SpeechRecognition

    #region for Signaling

    [DllImport("__Internal")]
    public static extern void SendUserToken(string userToken);

    [DllImport("__Internal")]
    public static extern void SignalingConnect();

    [DllImport("__Internal")]
    public static extern void SignalingDisconnect();

    [DllImport("__Internal")]
    public static extern void SignalingLogin();

    [DllImport("__Internal")]
    public static extern void SignalingRelayToOperator(string message);

    #endregion for Signaling

    #region for WebRTC

    [DllImport("__Internal")]
    public static extern void WebRTCConnect();

    [DllImport("__Internal")]
    public static extern void WebRTCDisconnect();

    #endregion for WebRTC

#endif
}
