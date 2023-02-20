using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : IState
{
    public void OnEnter()
    {
        // ローディングアイコンを有効にする
        UIManager.Instance.EnableLoadingIcon();
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {
        // ローディングアイコンを無効にする
        UIManager.Instance.DisableLoadingIcon();
    }
}
