using System.Runtime.InteropServices;
using UnityEngine;

public static class JSHelper
{
#if UNITY_EDITOR
    public static void StartSpeechRecognition()
    {
        Debug.Log($"StartSpeechRecognition called by Editor");
    }

    public static void StopSpeechRecognition()
    {
        Debug.Log($"StopSpeechRecognition called by Editor");
    }

    public static void WebRTCConnect()
    {
        Debug.Log($"Connect called by Editor");
    }

    public static void WebRTCDisconnect()
    {
        Debug.Log($"Disconnect called by Editor");
    }

#else
    [DllImport("__Internal")]
    public static extern void StartSpeechRecognition();

    [DllImport("__Internal")]
    public static extern void StopSpeechRecognition();

    [DllImport("__Internal")]
    public static extern void WebRTCConnect();

    [DllImport("__Internal")]
    public static extern void WebRTCDisconnect();
#endif
}
