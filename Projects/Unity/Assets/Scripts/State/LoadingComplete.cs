using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BotManager;

public class LoadingComplete : IState
{
    public async void OnEnter()
    {
        // この時点では戻れない
        GlobalState.Instance.CanReturnToTop = false;

        StreamingSpeechToText.Instance.OnStartRecording += StartStreamingSpeechToText;

        //スクリプトだけ先んじて処理
        var scripts = BotManager.Instance.GetScripts();
        BotManager.Instance.ExecutionScript(scripts);

        // ボットレスポンス取得
        var voice = BotManager.Instance.GetVoice();
        var text = BotManager.Instance.GetText();
        var image = BotManager.Instance.GetImage();
        var imageType = BotManager.Instance.GetImageAccessType();
        var motion = BotManager.Instance.GetMotion();
        var scene = BotManager.Instance.GetScene();
        var options = BotManager.Instance.GetOptions();
        var movieFilePath = BotManager.Instance.GetMovie();

        // 現在のシナリオ階層をセット
        if (Enum.TryParse(scene, true, out BotManager.ScenarioHierarchy hierarchy))
        {
            BotManager.Instance.CurrentHierarchy = hierarchy;
        }
        else
        {
            BotManager.Instance.CurrentHierarchy = BotManager.ScenarioHierarchy.None;
        }

        // ボイス再生タスク作成
        UniTask audioTask = UniTask.CompletedTask;
        if (!string.IsNullOrEmpty(voice))
        {
            // 他タスクより先に音声合成を行う
            var audioClip = await AudioManager.Instance.GetAudioClip(voice);

            audioTask = AudioManager.Instance.Play(audioClip);
        }

        // UIタスク作成
        // 選択肢の表示タイミングは録音開始と合わせる
        UniTask uiTask = UniTask.CompletedTask;
        if (!string.IsNullOrEmpty(text))
        {
            var fullSizeImageName = BotManager.GetSelectParameter(options, OptionTypes.fullScreen);
            uiTask = UIManager.Instance.SetCharacterMessage(text, true, imageType, image, fullSizeImageName);
        }

        // タスク実行
        if (Enum.TryParse(motion, true, out AnimationType result))
        {
            var animationTask = CharacterManager.Instance.ChangeAnimation(result);
            if (result == AnimationType.Here)
            {
                // アテンド時はボイス再生終了時に戻す
                await animationTask;
                await audioTask;
                await CharacterManager.Instance.ChangeAnimation(AnimationType.HereReturn);
                await uiTask;
            }
            else
            {
                // 全タスクを並列で
                await UniTask.WhenAll(audioTask, uiTask, animationTask);
            }
        }
        // 動画は全タスク終了後に再生
        UIManager.Instance.SetCharacterMessageMovie(movieFilePath);

        // 画像オブジェクトがアクティブになるまで待機させる
        await UniTask.DelayFrame(1);

        // キャラクター処理終了後のアクション
        var action = BotManager.Instance.GetAction();
        ActionManager.Instance.Execute(action);

        // この時点では戻れる
        GlobalState.Instance.CanReturnToTop = true;
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
