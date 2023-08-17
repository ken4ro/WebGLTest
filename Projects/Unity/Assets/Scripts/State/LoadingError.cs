using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingError : IState
{
    public async void OnEnter()
    {
        // この時点では戻れない
        GlobalState.Instance.CanReturnToTop = false;

        StreamingSpeechToText.Instance.OnStartRecording += StartStreamingSpeechToText;

        // 他タスクより先に音声合成を行う
        var audioClip = await AudioManager.Instance.GetAudioClip(BotManager.Instance.NoMatchText, true);
        // オーディオ再生
        var audioTask = AudioManager.Instance.Play(audioClip);
        // キャラクターアニメーション
        var animationTask = CharacterManager.Instance.ChangeAnimation(AnimationType.What);
        // 全て並列で実行
        await UniTask.WhenAll(audioTask, animationTask);

        GlobalState.Instance.CurrentState.Value = GlobalState.State.Speakable;

        /*
        if (GlobalState.Instance.CurrentBotRequestMethod == GlobalState.BotRequestMethod.Button)
        {
            // 発話可能状態へダイレクトに移行
            GlobalState.Instance.CurrentState.Value = GlobalState.State.Speakable;
        }
        else
        {
            // ストリーミング音声認識開始要求
            var runTask = StreamingSpeechToText.Instance.RunOneShotTask();
        }
        */

    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
        StreamingSpeechToText.Instance.OnStartRecording -= StartStreamingSpeechToText;
    }

    // ストリーミング音声認識が開始した
    private void StartStreamingSpeechToText()
    {
        // 発話可能状態へ移行
        GlobalState.Instance.CurrentState.Value = GlobalState.State.Speakable;
    }
}
