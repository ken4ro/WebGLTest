using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Operating : IState
{
    private const uint TimeoutTimeSec = 10;

    public void OnEnter()
    {
        // 未実装

        Observable.Timer(TimeSpan.FromSeconds(TimeoutTimeSec)).Subscribe(_ =>
        {
            GlobalState.Instance.CurrentState.Value = GlobalState.State.Starting;
        });
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
