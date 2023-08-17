using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Starting : IState
{
    public async void OnEnter()
    {
        // トップに戻れない
        GlobalState.Instance.CanReturnToTop = false;

        // UIリセット
        await UIManager.Instance.Reset();

        // キャラクター表示
        CharacterManager.Instance.Enable();

        // チャットボット処理開始
        _ = BotManager.Instance.Request(isInit: true);
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
