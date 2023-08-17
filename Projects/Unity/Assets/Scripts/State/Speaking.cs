using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaking : IState
{
    public void OnEnter()
    {
        // 発話中は戻るの禁止
        GlobalState.Instance.CanReturnToTop = false;

        StreamingSpeechToText.Instance.OnStreamingDataComplete += StreamingSpeechToTextComplete;

        // 発話中ウィンドウ表示
        UIManager.Instance.EnableSpeakingMaskPanel();
        UIManager.Instance.EnableSpeakingMessage();
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {
        StreamingSpeechToText.Instance.OnStreamingDataComplete -= StreamingSpeechToTextComplete;

        // 発話中ウィンドウ非表示
        UIManager.Instance.DisableSpeakingMessage();
        UIManager.Instance.DisableSpeakingMaskPanel();
    }

    // ストリーミング音声認識が完了した
    private void StreamingSpeechToTextComplete(string text)
    {
        // 発話完了処理状態へ移行
        GlobalState.Instance.CurrentState.Value = GlobalState.State.SpeakingComplete;
    }
}
