using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenPanel : MonoBehaviour
{
    [SerializeField] Image ImagePanel = null;

    bool DiscardEvent = true;

    public Action OnClick = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// パネル初期化
    /// </summary>
    public void Initialize()
    {
        ImagePanel.DOComplete();
        DiscardEvent = true;
        ImagePanel.enabled = false;
        ImagePanel.color = new Color(1, 1, 1, 0);

        //イベントを有効に
        EnableEvent(false);

    }

    /// <summary>
    /// パネル有効化
    /// </summary>
    public void EnablePanel()
    {
        //進行中の処理を破棄
        ImagePanel.DOComplete();
        DiscardEvent = true;

        //イベントを有効に
        ImagePanel.enabled = true;
        EnableEvent(true);
        FadeIn();
    }

    /// <summary>
    /// パネル無効化
    /// </summary>
    public void DisablePanel()
    {
        //進行中の処理を破棄
        ImagePanel.DOComplete();
        DiscardEvent = true;

        //イベントを有効に
        EnableEvent(false);
        FadeOut();
    }

    /// <summary>
    /// イメージのセット
    /// </summary>
    /// <param name="img"></param>
    public void SetImage(Texture2D img) => ImagePanel.sprite = Sprite.Create(img, new Rect(0.0f, 0.0f, img.width, img.height), Vector2.zero, 1.0f);

    void FadeIn()
    {
        ImagePanel.DOColor(new Color(1, 1, 1, 1), 0.5f).OnComplete(() => DiscardEvent = false);
    }

    /// <summary>
    /// パネルフェードアウト
    /// </summary>
    void FadeOut()
    {
        DiscardEvent = true;
        ImagePanel.DOComplete();
        ImagePanel.DOColor(new Color(1, 1, 1, 0), 0.3f).OnComplete(() =>
        {
            EnableEvent(false);
            ImagePanel.enabled = false;
        });
    }

    /// <summary>
    /// イベントの有効化
    /// </summary>
    void EnableEvent(bool swt)
    {

        var im = this.GetComponent<Image>();
        im.raycastTarget = swt;
    }

    public void PushPanel()
    {
        if (DiscardEvent)
            return;

        FadeOut();

        // イベント通知
        OnClick?.Invoke();

    }
}
