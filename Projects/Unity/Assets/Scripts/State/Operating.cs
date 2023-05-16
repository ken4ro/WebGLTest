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
        // 接続
        JSHelper.WebRTCConnect();

        /*
        Observable.Timer(TimeSpan.FromSeconds(TimeoutTimeSec)).Subscribe(_ =>
        {
            // 10秒で切断
            JSHelper.WebRTCDisconnect();

            GlobalState.Instance.CurrentState.Value = GlobalState.State.Starting;
        });
        */
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
