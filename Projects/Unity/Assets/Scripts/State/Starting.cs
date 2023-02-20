using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Starting : IState
{
    public async void OnEnter()
    {
        Debug.Log("State Starting OnEnter");

        // UIリセット
        await UIManager.Instance.Reset();

        Debug.Log("Reset Completed");

        // キャラクター表示
        CharacterManager.Instance.Enable();

        Debug.Log("Enable Completed");

        // チャットボット処理開始
        _ = BotManager.Instance.Request(isInit: true);

        Debug.Log("Request Completed");
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
