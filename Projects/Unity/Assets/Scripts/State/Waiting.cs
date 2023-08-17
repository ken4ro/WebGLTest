using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waiting : IState
{
    public async void OnEnter()
    {
        // タイマー処理をリセット
        ActionManager.Instance.TimerTask?.Dispose();
        // UIリセット
        await UIManager.Instance.Reset();
        // キャラクターリセット
        CharacterManager.Instance.DisableAnimation();
        CharacterManager.Instance.SetTransformsForIdle();
        CharacterManager.Instance.EnableAnimation();
        CharacterManager.Instance.ResetAnimation();
        CharacterManager.Instance.ResetPosition();
        //CharacterManager.Instance.Disable();
        // サウンドリセット
        AudioManager.Instance.ResetCharacterVoice();
        // デフォルト言語に戻しておく
        SignageSettings.InitializeCurrentLanguage();
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
