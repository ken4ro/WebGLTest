using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using UniRx;
using TMPro;

using static GlobalState;

public class ErrorDisplayPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _errorText = null;

    [SerializeField]
    private Button _retryBtn = null;

    public void Initialize()
    {
        // リトライボタン押下時
        _retryBtn.onClick.AsObservable().Subscribe(_ =>
        {
            // 非表示
            Disable();
            // Botモード先頭から始める
            GlobalState.Instance.CurrentState.Value = State.Starting;
        }).AddTo(gameObject);
    }

    public void Enable()
    {
        UIManager.Instance.EnableSpeakingMaskPanel();
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        UIManager.Instance.DisableSpeakingMaskPanel();
    }

    public void Display(string error)
    {
        if (!gameObject.activeSelf)
        {
            Enable();
        }
        _errorText.text = error;
    }

    public void Display(ErrorCode code)
    {
        if (!gameObject.activeSelf)
        {
            Enable();
        }
        _errorText.text = CreateErrorText(code);
    }

    private string CreateErrorText(ErrorCode code)
    {
        var error = new StringBuilder();

        switch (code)
        {
            case ErrorCode.Network:
                error.Append(NetworkErrorText);
                break;
            case ErrorCode.ReplBadRequest:
            case ErrorCode.ReplUnauthorized:
            case ErrorCode.ReplUnrecognizedResponse:
                error.Append(ReplErrorText);
                break;
        }

        error.Append(Environment.NewLine);
        error.Append(Environment.NewLine);

        error.Append("ER-");

        error.Append((int)code);

        return error.ToString();
    }
}
