using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GlobalState;

public class JSManager : SingletonMonoBehaviour<JSManager>
{
    // ボット処理開始
    public void StartBotProcess()
    {
        GlobalState.Instance.CurrentState.Value = State.Starting;
    }

    // ボット処理停止
    public void StopBotProcess()
    {
        //SceneManager.LoadScene(0);
        GlobalState.Instance.CurrentState.Value = State.Waiting;
    }

    // ユーザーメッセージをセット
    public void SetUserMessage(string text)
    {
        // 音声認識の最終文字列としてセット
        StreamingSpeechToText.Instance.RecognitionCompleteText = text;

        GlobalState.Instance.CurrentState.Value = State.SpeakingComplete;
    }

    // 発話中文字列をセット
    public void SetSpeakingText(string text)
    {
        // 発話中文字列を表示
        UIManager.Instance.SetSpeakingText(text);

        GlobalState.Instance.CurrentState.Value = State.Speaking;
    }

    public class AudioVolumeJson
    {
        public float Volume;
    }

    public void SetVoiceVolume(float volume)
    {
        Debug.Log($"SetVoiceVolume: volume = {volume}");
        //var volume = JsonUtility.FromJson<AudioVolumeJson>(json).Volume;
        CharacterManager.Instance.SetMouseOpenYParameter(volume);
    }

    public void WebRTCDisconnect()
    {
        GlobalState.Instance.CurrentState.Value = GlobalState.State.Starting;
    }

    private bool _isCharacterAnimation = true;
    public void SetFaceInfo(string faceInfoJson)
    {
        if (_isCharacterAnimation)
        {
            _isCharacterAnimation = false;
            // キャラクターアニメーション無効化
            CharacterManager.Instance.DisableAnimation();
            // アイドルモーション時のキャラクタートランスフォームをセット
            CharacterManager.Instance.SetTransformsForIdle();
            // 顔認識有効
            FaceInfoManager.Instance.Enable();
        }

        FaceInfoManager.Instance.FaceInfoReceived(faceInfoJson);
    }

    #region for Signaling

    public void SignalingServerOnConnect()
    {

    }

    public void SignalingServerOnDisconnect()
    {

    }

    public void SignalingServerOnNotification(string json)
    {
        SignalingManager.Instance.OnNotification(json);
    }

    public void SignalingServerOnResponse(string json)
    {
        SignalingManager.Instance.OnResponse(json);
    }

    public void SignalingServerOnClose()
    {
        SignalingManager.Instance.OnClose();
    }

    public void SignalingServerOnError()
    {
        SignalingManager.Instance.OnError();
    }

    #endregion for Signaling
}
