using System;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

public class ActionManager : SingletonBase<ActionManager>
{
    /// <summary>
    /// タイマータスク
    /// </summary>
    public IDisposable TimerTask { get; set; } = null;

    /// <summary>
    /// 指定されたアクションを実行する
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public void Execute(string action)
    {
        switch (action)
        {
            case "return":
                // n秒後に再スタート
                TimerTask = Observable.Timer(TimeSpan.FromSeconds(SettingHub.Instance.Signage.Cache.ReturnWaitTime)).Subscribe(_ =>
                {
                    GlobalState.Instance.CurrentState.Value = GlobalState.State.Starting;
                });
                // 音声認識は行わない
                return;
            case "delay":
                // n秒後に空リクエスト送信
                TimerTask = Observable.Timer(TimeSpan.FromSeconds(SettingHub.Instance.Signage.Cache.DelayTime)).Subscribe(_ =>
                {
                    BotManager.Instance.RequestEmpty().Forget();
                });
                // 音声認識は行わない
                return;
            case "translate":
                //Debug.Log("Translation Mode");
                // 翻訳モードに切り替える
                GlobalState.Instance.CurrentBotRequestMode.Value = GlobalState.BotRequestMode.Translation;
                break;
            case "nomatch":
                Debug.Log("NoMatch");
                break;
            case "telexistence":
                // 遠隔対話モード
                if (GlobalState.Instance.ApplicationGlobalSettings.EnableRecordingAgreement == "true")
                {
                    // 録音確認画面を表示
                    GlobalState.Instance.CurrentState.Value = GlobalState.State.PreOperating;
                }
                else
                {
                    // 録音確認画面をスキップ
                    GlobalState.Instance.CurrentState.Value = GlobalState.State.Operating;
                }
                // 音声認識は行わない
                return;
            default:
                //Debug.Log("Dictionary Mode");
                // 辞書モードに切り替える
                GlobalState.Instance.CurrentBotRequestMode.Value = GlobalState.BotRequestMode.Dictionary;
                break;
        }

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
    }
}
