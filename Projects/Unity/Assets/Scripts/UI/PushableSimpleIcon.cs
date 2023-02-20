using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

public class PushableSimpleIcon : MonoBehaviour
{
    private RectTransform _rectTransform = null;
    private Image _fingerImage = null;

    /// <summary>
    /// 表示を有効にする
    /// </summary>
    public void Enable() => gameObject.SetActive(true);

    /// <summary>
    /// 表示を無効にする
    /// </summary>
    public void Disable() => gameObject.SetActive(false);

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
        _rectTransform = GetComponent<RectTransform>();
        _fingerImage = GetComponent<Image>();
    }

    public void ResetObject()
    {
        _rectTransform.DOComplete();
        _fingerImage.DOComplete();

        _rectTransform.localScale = new Vector3(1.0f, 1.0f);
        _fingerImage.rectTransform.localScale = new Vector3(1.0f, 1.0f);
        _fingerImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    public void FadeIn()
    {
        if (!gameObject.activeSelf) Enable();

        ResetObject();

        _fingerImage.DOFade(1.0f, 0.5f);

        Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
        {
            StartCoroutine("HandAnime");
        });
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    public void FadeOut()
    {
        StopCoroutine("HandAnime");

        _fingerImage.DOFade(0.0f, 0.3f);

        Observable.Timer(TimeSpan.FromSeconds(0.3f)).Subscribe(_ =>
        {
            Disable();
        });
    }

    IEnumerator HandAnime()
    {
        var activeColor = new Color(1, 1, 1, 1);
        var passiveColor = new Color(1, 1, 1, 0.4f);
        var maxScale = new Vector3(1.05f, 1.05f, 1.05f);
        var minScale = new Vector3(0.95f, 0.95f, 0.95f);
        var waitForScale = new WaitForSeconds(1.9f);
        while (true)
        {
            _rectTransform.DOScale(maxScale, 2);
            _fingerImage.DOColor(activeColor, 2);
            yield return waitForScale;
            _rectTransform.DOComplete();
            _fingerImage.DOComplete();
            _rectTransform.DOScale(minScale, 2);
            _fingerImage.DOColor(passiveColor, 2);
            yield return waitForScale;
            _rectTransform.DOComplete();
            _fingerImage.DOComplete();
        }
    }
}
