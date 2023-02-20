using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UniRx;
using DG.Tweening;

public class MoviePlayPanel : MonoBehaviour
{
    private VideoPlayer _videoPlayer = null;
    private Image _movieImage = null;

    /// <summary>
    /// クリック時イベント
    /// </summary>
    public Action OnClick = null;

    public void Initialize()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        _movieImage = GetComponent<Image>();

        // リソースパックから Video Clip を読み込んでセット
        if (GameController.Instance.CurrentScreenSaverType == SignageSettings.ScreenSaverTypes.Movie)
        {
            _videoPlayer.clip = AssetBundleManager.Instance.LoadVideoClipFromResourcePack("video_screensaver");
            if (_videoPlayer.clip == null)
            {
                Debug.Log("Video for screen saver not found.");
            }
        }
    }

    public void Click()
    {
        // フェードアウト
        FadeOut();

        // イベント通知
        OnClick?.Invoke();
    }

    public void Enable() => gameObject.SetActive(true);

    public void Disable() => gameObject.SetActive(false);

    public void FadeIn()
    {
        if (!gameObject.activeSelf)
        {
            Enable();
        }

        _movieImage.DOFade(1.0f, 0.3f);
    }

    public void FadeOut()
    {
        _movieImage.DOFade(0.0f, 0.2f);

        Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ =>
        {
            Disable();
        });
    }
}
