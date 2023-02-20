using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

public class PushableCocoaIcon : MonoBehaviour
{
    /// <summary>
    /// 表示を有効にする
    /// </summary>
    public void Enable() => gameObject.SetActive(true);

    /// <summary>
    /// 表示を無効にする
    /// </summary>
    public void Disable() => gameObject.SetActive(false);

    private SpriteRenderer _spriteRenderer = null;

    public void Initialize()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    public void FadeIn()
    {
        if (!gameObject.activeSelf) Enable();

        _spriteRenderer.DOFade(1.0f, 0.5f);
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    public void FadeOut()
    {
        _spriteRenderer.DOFade(0.0f, 0.5f);

        Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
        {
            Disable();
        });
    }
}
