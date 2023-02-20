using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordingCheckPanel : MonoBehaviour
{
    [SerializeField] CanvasGroup panelCanvasGroup = null;

    [SerializeField] Image backgroundImage = null;

    [SerializeField] Image yesBtnImage = null;

    [SerializeField] Image noBtnImage = null;

    /// <summary>
    /// 「Yes」ボタンを押下した
    /// </summary>
    public Action OnClickYesBtn = null;

    /// <summary>
    /// 「No」ボタンを押下した
    /// </summary>
    public Action OnClickNoBtn = null;

    private static readonly string SpriteBasePath = "Images/RecordingCheck/";

    void OnEnable()
    {
        yesBtnImage.GetComponent<UIShiny>().effectFactor = 0.0f;
        noBtnImage.GetComponent<UIShiny>().effectFactor = 0.0f;

        // 選択言語に応じて表示する画像を変更
        var currentLanguageString = SignageSettings.CurrentLanguage.Value.ToString().ToLower();
        var backgroundSpriteAssetName = "bg_recordingcheck_" + currentLanguageString;
        backgroundImage.sprite = Resources.Load<Sprite>(SpriteBasePath + backgroundSpriteAssetName);
        var yesBtnSpriteAssetName = "icon_recordingcheck_yes_" + currentLanguageString;
        yesBtnImage.sprite = Resources.Load<Sprite>(SpriteBasePath + yesBtnSpriteAssetName);
        var noBtnSpriteAssetName = "icon_recordingcheck_no_" + currentLanguageString;
        noBtnImage.sprite = Resources.Load<Sprite>(SpriteBasePath + noBtnSpriteAssetName);

        FadeIn();
    }

    void OnDisable()
    {
        Resources.UnloadAsset(backgroundImage.sprite);
        Resources.UnloadAsset(yesBtnImage.sprite);
        Resources.UnloadAsset(noBtnImage.sprite);
    }

    public void Initialize()
    {
    }

    public async void ClickYes()
    {
        AudioManager.Instance.PlaySE(AudioManager.SEType.SelectSentence).Forget();

        // ボタン押下アニメーション
        var yesBtnUiShiny = yesBtnImage.GetComponent<UIShiny>();
        DOTween.To(() => yesBtnUiShiny.effectFactor, (x) => yesBtnUiShiny.effectFactor = x, 1.0f, 0.3f);

        await UniTask.Delay(300);

        FadeOut();

        await UniTask.Delay(1000);

        OnClickYesBtn?.Invoke();
    }

    public async void ClickNo()
    {
        AudioManager.Instance.PlaySE(AudioManager.SEType.SelectSentence).Forget();

        // ボタン押下アニメーション
        var noBtnUiShiny = noBtnImage.GetComponent<UIShiny>();
        DOTween.To(() => noBtnUiShiny.effectFactor, (x) => noBtnUiShiny.effectFactor = x, 1.0f, 0.3f);

        await UniTask.Delay(300);

        FadeOut();

        await UniTask.Delay(1000);

        OnClickNoBtn?.Invoke();
    }

    private void FadeIn()
    {
        panelCanvasGroup.DOFade(1.0f, 0.3f);
        panelCanvasGroup.blocksRaycasts = true;
    }

    private void FadeOut()
    {
        panelCanvasGroup.DOFade(0.0f, 0.5f);
        panelCanvasGroup.blocksRaycasts = false;
    }
}
