using System.Runtime.InteropServices;
using UnityEngine;

public static class JSHelper
{
#if UNITY_EDITOR
    public static void StartRecognition()
    {
        Debug.Log($"StartRecognition called by Editor");
    }
#else
    [DllImport("__Internal")]
    public static extern void StartRecognition();
#endif
}
