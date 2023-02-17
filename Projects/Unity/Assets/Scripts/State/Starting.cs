using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Starting : IState
{
    public void OnEnter()
    {
        Debug.Log("State Starting OnEnter");
        _ = BotManager.Instance.Request(isInit: true);
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
