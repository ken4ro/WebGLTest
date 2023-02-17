using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BotManager;
using static SignageSettings;

public class LoadingComplete : IState
{
    public async void OnEnter()
    {
        // ボットレスポンス取得
        var voice = BotManager.Instance.GetVoice();
        var text = BotManager.Instance.GetText();
        var image = BotManager.Instance.GetImage();
        //var imageType = BotManager.Instance.GetImageAccessType();
        var imageType = ImageAccessTypes.Local;
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
        UniTask audioTask;
        /*
        if (!string.IsNullOrEmpty(voice))
        {
            // 他タスクより先に音声合成を行う
            var audioClip = await AudioManager.Instance.GetAudioClip(voice);

            audioTask = AudioManager.Instance.Play(audioClip);
        }
        */

        // UIタスク作成
        // 選択肢の表示タイミングは録音開始と合わせる
        UniTask uiTask;
        var fullSizeImageName = BotManager.GetSelectParameter(options, OptionTypes.fullScreen);
        uiTask = UIManager.Instance.SetCharacterMessage(text, true, imageType, image, fullSizeImageName);

        // タスク実行
        if (Enum.TryParse(motion, true, out AnimationType result))
        {
            var animationTask = CharacterManager.Instance.ChangeAnimation(result);
            if (result == AnimationType.Here)
            {
                // アテンド時はボイス再生終了時に戻す
                await animationTask;
                //await audioTask;
                await CharacterManager.Instance.ChangeAnimation(AnimationType.HereReturn);
                await uiTask;
            }
            else
            {
                // 全タスクを並列で
                //await UniTask.WhenAll(audioTask, uiTask, animationTask);
                await UniTask.WhenAll(uiTask, animationTask);
            }
        }
        // 動画は全タスク終了後に再生
        UIManager.Instance.SetCharacterMessageMovie(movieFilePath);

        // 画像オブジェクトがアクティブになるまで待機させる
        await UniTask.DelayFrame(1);

        // キャラクター処理終了後のアクション
        var action = BotManager.Instance.GetAction();
        ActionManager.Instance.Execute(action);
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
