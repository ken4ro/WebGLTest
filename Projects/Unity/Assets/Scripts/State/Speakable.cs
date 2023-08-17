using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speakable : IState
{
    public void OnEnter()
    {
        // トップに戻れる
        GlobalState.Instance.CanReturnToTop = true;

        StreamingSpeechToText.Instance.OnStreamingDataAvailable += StreamingSpeechToTextAvailable;
        StreamingSpeechToText.Instance.OnStreamingDataComplete += StreamingSpeechToTextComplete;

        // 受付可能アイコンフェードイン
        UIManager.Instance.FadeInAcceptableIcon();
        // 発話可能状態開始用の効果音再生
        var audioTask = AudioManager.Instance.PlaySE(AudioManager.SEType.VoiceIn);
        // 言語パネル表示
        UIManager.Instance.EnableLanguagePanel();

        // 選択肢表示
        var selectObjects = BotManager.Instance.GetSelectObjects();
        UIManager.Instance.SetCharacterSelectMessage(selectObjects);

        // 音声認識開始
        JSHelper.StartSpeechRecognition();
    }

    public void OnUpdate()
    {
        // タイムアウト処理
        GameController.Instance.CurrentIdleTimeSec += Time.deltaTime;

        //トップ画面でタイムアウト
        if (BotManager.Instance.CurrentHierarchy == BotManager.ScenarioHierarchy.Top &&
            GameController.Instance.CurrentIdleTimeSec >= GlobalState.Instance.UserSettings.Bot.RestartSec)
        {
            if (GameController.Instance.ScreenSaverEnable =="true")
            {
                // 再スタート
                GlobalState.Instance.CurrentState.Value = GlobalState.State.Starting;
            }
            else
            {
                // プロセス移行無し
            }
        }

        //トップ画面以外でタイムアウト
        else if (BotManager.Instance.CurrentHierarchy != BotManager.ScenarioHierarchy.Top &&
            GameController.Instance.CurrentIdleTimeSec >= GlobalState.Instance.UserSettings.UI.InputLimitSec)
        {

            // ストリーミング音声認識をキャンセル
            //await StreamingSpeechToText.Instance.CancelRecognition();

            // 再スタート
            GlobalState.Instance.CurrentState.Value = GlobalState.State.Starting;

        }
    }

    public void OnExit()
    {
        StreamingSpeechToText.Instance.OnStreamingDataAvailable -= StreamingSpeechToTextAvailable;
        StreamingSpeechToText.Instance.OnStreamingDataComplete -= StreamingSpeechToTextComplete;

        // アイドル時間リセット
        GameController.Instance.CurrentIdleTimeSec = 0.0f;
        // 言語パネル非表示
        UIManager.Instance.DisableLanguagePanel();

    }

    // ストリーミング音声認識でフレーズを検知した
    private void StreamingSpeechToTextAvailable(string text)
    {
        // 発話中処理状態へ移行
        GlobalState.Instance.CurrentState.Value = GlobalState.State.Speaking;
    }

    // ストリーミング音声認識が完了した
    // ボタン押下時等は直接完了フェーズへ移行する
    private void StreamingSpeechToTextComplete(string text)
    {
        // 発話完了処理状態へ移行
        GlobalState.Instance.CurrentState.Value = GlobalState.State.SpeakingComplete;
    }
}
