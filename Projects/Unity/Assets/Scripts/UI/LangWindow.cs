using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using static SignageSettings;

public class LangWindow : MonoBehaviour
{
    [SerializeField]
    GameObject VerticalLine = null;
    [SerializeField]
    GameObject HorizontalLine = null;
    [SerializeField]
    Image CurtainImage = null;
    [SerializeField]
    GameObject SelectMessage = null;
    [SerializeField]
    Image ActiveLangImage = null;
    [SerializeField]
    GameObject NoSelect = null;
    [SerializeField]
    GameObject SelectLangs = null;
    [SerializeField]
    GameObject SelectBar = null;
    [SerializeField]
    GameObject SelectGuard = null;
    [SerializeField]
    List<Vector2> BarPositions = null;
    [SerializeField]
    AudioClip SpreadSE = null;
    [SerializeField]
    AudioClip NarrowSE = null;
    [SerializeField]
    AudioClip SelectSE = null;
    [SerializeField]
    AudioSource SESpeaker = null;
    [SerializeField]
    Vector2 VerticalLongSize;
    [SerializeField]
    Vector2 VerticalMaxSize;
    [SerializeField]
    Vector2 HorizontalLongSize;
    [SerializeField]
    Vector3 HorizontalLongPosition;
    [SerializeField]
    Vector2 HorizontalMaxSize;

    /// <summary>
    /// 言語選択時コールバック
    /// </summary>
    public Action<Language> OnSelectLanguage = null;

    /// <summary>
    /// 対応言語数
    /// </summary>
    public int LanguageCount { get; private set; } = 0;

    private static readonly int _langImageBasePosX = -1100;
    private static readonly int _langImageMargin = 280;

    private Vector3 _originHLinePosition;
    private Vector3 _originSelectMsgPosition;
    private Vector3 _originSelectBarPosition;
    private Vector2 _originVLineSize;
    private Vector2 _originHLineSize;

    private Color _originVLineColor;
    private Color _originHLineColor;
    private Color _originSelectMsgColor;
    private Color _originSelectBarColor;

    private RectTransform _verticalLineRectTransform = null;
    private RectTransform _horizontalLineRectTransform = null;
    private RectTransform _selectMessageRectTransform = null;
    private RectTransform _selectBarRectTransform = null;
    private Image _verticalLineImage = null;
    private Image _horizontalLineImage = null;
    private Image _selectMessageImage = null;
    private Image _selectBarImage = null;

    private Color _passiveColor = new Color(1, 1, 1, 0);
    private Color _activeColor = new Color(1, 1, 1, 1);
    private Color _curtainPassiveColor = new Color(0, 0, 0, 0);
    private Color _curtainActiveColor = new Color(0, 0, 0, 0.6f);

    private List<GameObject> _selectLangs = new List<GameObject>();

    // GC 対策
    private WaitForSeconds _waitForBlinkLine = new WaitForSeconds(1.0f);

    void Awake()
    {
        // 各コンポーネントをキャッシュしておく
        _verticalLineRectTransform = VerticalLine.GetComponent<RectTransform>();
        _horizontalLineRectTransform = HorizontalLine.GetComponent<RectTransform>();
        _selectMessageRectTransform = SelectMessage.GetComponent<RectTransform>();
        _selectBarRectTransform = SelectBar.GetComponent<RectTransform>();
        _verticalLineImage = VerticalLine.GetComponent<Image>();
        _horizontalLineImage = HorizontalLine.GetComponent<Image>();
        _selectMessageImage = SelectMessage.GetComponent<Image>();
        _selectBarImage = SelectBar.GetComponent<Image>();

        // オブジェクトの初期状態を保持
        _originHLinePosition = _horizontalLineRectTransform.localPosition;
        _originSelectMsgPosition = _selectMessageRectTransform.localPosition;
        _originSelectBarPosition = _selectBarRectTransform.localPosition;
        _originVLineSize = _verticalLineRectTransform.sizeDelta;
        _originHLineSize = _horizontalLineRectTransform.sizeDelta;
        _originVLineColor = _verticalLineImage.color;
        _originHLineColor = _horizontalLineImage.color;
        _originSelectMsgColor = _selectMessageImage.color;
        _originSelectBarColor = _selectBarImage.color;

        // ガード解除
        SelectGuard.SetActive(false);

        // デフォルト言語をセット
        ActiveLangImage.sprite = GetLanguageSprite(SignageSettings.CurrentLanguage.Value);

        // 対応言語数取得
        LanguageCount = SignageSettings.Settings.LanguageVoiceMap.Keys.Count;

        // 対応言語数に応じてサイズ変更
        VerticalMaxSize.x = 280 + 295 * LanguageCount;

        // 選択言語オブジェクト
        var langCount = 1;
        foreach (var lang in SignageSettings.Settings.LanguageVoiceMap.Keys)
        {
            // プレハブからゲームオブジェクト生成
            var selectLangPrefab = Resources.Load<GameObject>("Prefabs/SelectLang");
            var selectLang = Instantiate(selectLangPrefab, SelectLangs.transform);
            _selectLangs.Add(selectLang);

            // 標示位置調整
            selectLang.transform.localPosition = new Vector3(_langImageBasePosX + langCount * _langImageMargin, selectLang.transform.localPosition.y, selectLang.transform.localPosition.z);

            // 画像と色をセット
            var selectLangImage = selectLang.GetComponent<Image>();
            selectLangImage.sprite = GetLanguageSprite(lang);
            selectLangImage.color = _passiveColor;

            // イベントトリガー設定
            var selectLangEventTrigger = selectLang.GetComponent<EventTrigger>();
            selectLangEventTrigger.triggers[0].eventID = EventTriggerType.PointerClick;
            selectLangEventTrigger.triggers[0].callback.AddListener((eventData) => { SelectButton(lang.ToString()); });

            // 非アクティブにしておく
            selectLang.SetActive(false);

            langCount++;
        }

        // 選択バー
        _selectBarImage.color = _passiveColor;
    }

    void OnEnable()
    {
        ResetComponent();
    }

    void OnDisable()
    {
        // コルーチン全停止
        StopAllCoroutines();
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void ResetComponent()
    {
        // 初期位置、サイズ、色に戻す
        _horizontalLineRectTransform.localPosition = _originHLinePosition;
        _selectMessageRectTransform.localPosition = _originSelectMsgPosition;
        _selectBarRectTransform.localPosition = _originSelectBarPosition;
        _verticalLineRectTransform.sizeDelta = _originVLineSize;
        _horizontalLineRectTransform.sizeDelta = _originHLineSize;
        _verticalLineImage.color = _originVLineColor;
        _horizontalLineImage.color = _originHLineColor;
        _selectMessageImage.color = _originSelectMsgColor;
        _selectBarImage.color = _originSelectBarColor;

        NoSelect.GetComponent<Image>().color = _passiveColor;
        NoSelect.SetActive(false);

        // 選択言語は非表示に
        foreach (var obj in _selectLangs)
        {
            obj.GetComponent<Image>().color = _passiveColor;
            obj.SetActive(false);
        }
        _selectBarImage.color = _passiveColor;

        // カーテン非表示
        CurtainImage.color = _curtainPassiveColor;
        CurtainImage.gameObject.SetActive(false);

        // 選択中の言語表示
        ActiveLang();

        // 点滅スタート
        StartCoroutine("BlinkLine");

        // ガード解除
        SelectGuard.SetActive(false);
    }

    /// <summary>
    /// 外部から言語をセットする
    /// </summary>
    /// <param name="language"></param>
    public void SetLanguage(Language lang)
    {
        ActiveLangImage.sprite = GetLanguageSprite(lang);
    }

    /// <summary>
    /// 現在有効になっている言語タイプの取得
    /// </summary>
    /// <returns></returns>
    public Language GetActiveLanguage()
    {
        return CurrentLanguage.Value;
    }

    /// <summary>
    /// 選択ウィンドウ表示
    /// </summary>
    public void OpenSelect()
    {
        // ガード
        SelectGuard.SetActive(true);

        SESpeaker.PlayOneShot(SpreadSE);

        StartCoroutine("AsyncOpenMainWindow");
    }

    /// <summary>
    /// 選択ウィンドウを閉じる
    /// </summary>
    public void CloseSelect()
    {
        StartCoroutine("AsyncCloseMainWindow");
    }

    public void SelectButton(string param)
    {
        // ガード
        SelectGuard.SetActive(true);

        if (Enum.TryParse(param, true, out Language lang))
        {
            // 言語ボタンが選択された
            OnSelectLanguage?.Invoke(lang);
            StartCoroutine("AsyncSelectAnimation");
        }
        else
        {
            // 言語選択メニュークローズ
            SESpeaker.PlayOneShot(NarrowSE);
            StartCoroutine("AsyncCloseMainWindow");
        }
    }

    /// <summary>
    /// セレクトアニメーション
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncSelectAnimation()
    {
        SESpeaker.PlayOneShot(SelectSE);

        //点滅？

        var barPos = BarPositions[GetLanguageIndex(CurrentLanguage.Value)];
        _selectBarRectTransform.DOLocalMoveX(barPos.x, 0.2f);
        yield return new WaitForSeconds(0.5f);
        _selectBarRectTransform.DOComplete();
        _selectBarRectTransform.localPosition = barPos;

        //SESpeaker.PlayOneShot(NarrowSE); // アニメーションの変更に合わせていったん削除
        StartCoroutine("AsyncCloseMainWindow");
    }

    public void SetDefaultLang()
    {
        var selectedLangColor = new Color(1, 1, 1, 0.9f);

        ActiveLangImage.sprite = GetLanguageSprite(SignageSettings.CurrentLanguage.Value);
        ActiveLangImage.DOColor(selectedLangColor, 0);
    }

    /// <summary>
    /// 選択中言語の表示
    /// </summary>
    /// <returns></returns>
    void ActiveLang()
    {
        var selectedLangColor = new Color(1, 1, 1, 0.9f);
        ActiveLangImage.DOColor(selectedLangColor, 0.3f);
    }

    /// <summary>
    /// 非同期でメインウィンドウ非表示
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncCloseMainWindow()
    {
        //選択言語消去
        StartCoroutine("FadeoutLang");

        //タイトル消去
        StartCoroutine("AsyncCloseTitle");

        //verライン横縮小
        _verticalLineRectTransform.DOSizeDelta(VerticalLongSize, 0.2f);
        yield return new WaitForSeconds(0.25f);
        _verticalLineRectTransform.DOComplete();
        _verticalLineRectTransform.sizeDelta = VerticalLongSize;

        //verライン縦縮小
        _verticalLineImage.DOColor(new Color(1, 1, 1, 1), 0.2f);
        _verticalLineRectTransform.DOSizeDelta(_originVLineSize, 0.2f);
        yield return new WaitForSeconds(0.25f);
        _verticalLineRectTransform.DOComplete();
        _verticalLineRectTransform.sizeDelta = _originVLineSize;

        // 選択中の言語を表示
        ActiveLang();

        // 点滅開始
        StartCoroutine("BlinkLine");

        // カーテンフェードアウト
        CurtainImage.DOColor(_curtainPassiveColor, 0.2f);
        yield return new WaitForSeconds(0.25f);
        CurtainImage.DOComplete();
        CurtainImage.color = _curtainPassiveColor;
        CurtainImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 非同期でメインウィンドウ表示
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncOpenMainWindow()
    {
        // カーテンフェードイン
        CurtainImage.gameObject.SetActive(true);
        CurtainImage.DOColor(_curtainActiveColor, 0.2f);

        // 点滅開始
        StopCoroutine("BlinkLine");

        ActiveLangImage.DOColor(_passiveColor, 0.3f);

        //初期状態に戻す
        _verticalLineImage.DOComplete();
        _verticalLineImage.color = _activeColor;
        _horizontalLineImage.DOComplete();
        _horizontalLineImage.color = _activeColor;

        StartCoroutine("AsyncOpenTitle");

        //verライン縦拡大
        _verticalLineRectTransform.DOSizeDelta(VerticalLongSize, 0.2f);

        yield return new WaitForSeconds(0.25f);
        _verticalLineRectTransform.DOComplete();
        _verticalLineRectTransform.sizeDelta = VerticalLongSize;

        //verライン横拡大
        _verticalLineImage.DOColor(new Color(1, 1, 1, 0.2f), 0.4f);
        _verticalLineRectTransform.DOSizeDelta(VerticalMaxSize, 0.4f);

        yield return new WaitForSeconds(0.5f);
        _verticalLineRectTransform.DOComplete();
        _verticalLineRectTransform.sizeDelta = VerticalMaxSize;
    }

    /// <summary>
    /// 非同期で選択タイトルの非表示
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncCloseTitle()
    {
        //タイトル消滅
        _selectMessageImage.DOColor(_passiveColor, 0.2f);

        //horライン横縮小
        _horizontalLineRectTransform.DOSizeDelta(HorizontalLongSize, 0.3f);
        _horizontalLineRectTransform.DOLocalMove(HorizontalLongPosition, 0.3f);
        yield return new WaitForSeconds(0.35f);
        _horizontalLineRectTransform.DOComplete();
        _horizontalLineRectTransform.sizeDelta = HorizontalLongSize;
        _horizontalLineRectTransform.localPosition = HorizontalLongPosition;

        //horライン移動
        _horizontalLineRectTransform.DOSizeDelta(_originHLineSize, 0.2f);
        _horizontalLineRectTransform.DOLocalMove(_originHLinePosition, 0.2f);
        yield return new WaitForSeconds(0.25f);
        _horizontalLineRectTransform.DOComplete();
        _horizontalLineRectTransform.sizeDelta = _originHLineSize;
        _horizontalLineRectTransform.localPosition = _originHLinePosition;
    }

    /// <summary>
    /// 非同期で選択タイトルの表示
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncOpenTitle()
    {
        _selectMessageRectTransform.localPosition = _originSelectMsgPosition;

        //horライン縦移動
        _horizontalLineRectTransform.DOLocalMove(HorizontalLongPosition, 0.2f);
        _horizontalLineRectTransform.DOSizeDelta(_originHLineSize, 0.2f);
        yield return new WaitForSeconds(0.25f);
        _horizontalLineRectTransform.DOComplete();
        _horizontalLineRectTransform.localPosition = HorizontalLongPosition;
        _horizontalLineRectTransform.sizeDelta = _originHLineSize;

        //horライン横拡大
        _horizontalLineRectTransform.DOSizeDelta(HorizontalMaxSize, 0.3f);

        //タイトルイン
        _selectMessageImage.DOColor(_activeColor, 0.4f);
        _selectMessageRectTransform.DOLocalMoveX(_selectMessageRectTransform.localPosition.x + 75, 2.5f);

        //言語フェードイン
        yield return new WaitForSeconds(0.5f);
        StartCoroutine("FadeinLang");

        _horizontalLineRectTransform.DOComplete();
        _horizontalLineRectTransform.sizeDelta = HorizontalMaxSize;
    }

    /// <summary>
    /// 選択言語の表示
    /// </summary>
    IEnumerator FadeinLang()
    {
        var displayLangColor = new Color(1, 1, 1, 0.7f);

        NoSelect.SetActive(true);
        NoSelect.GetComponent<Image>().DOColor(displayLangColor, 0.3f);
        yield return new WaitForSeconds(0.1f);

        foreach (var obj in _selectLangs)
        {
            obj.SetActive(true);
            obj.GetComponent<Image>().DOColor(displayLangColor, 0.3f);
            yield return new WaitForSeconds(0.1f);
        }

        //選択バー表示
        Vector3 barPos = BarPositions[GetLanguageIndex(CurrentLanguage.Value)];
        _selectBarRectTransform.localPosition = barPos;

        _selectBarImage.DOColor(_activeColor, 0.2f);
        yield return new WaitForSeconds(0.2f);
        _selectBarImage.DOComplete();
        _selectBarImage.color = _activeColor;

        // ガード解除
        SelectGuard.SetActive(false);
    }

    /// <summary>
    /// 選択言語非表示
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeoutLang()
    {
        _selectBarImage.DOColor(_passiveColor, 0.5f);

        for (int i = _selectLangs.Count - 1; i >= 0; i--)
        {
            _selectLangs[i].GetComponent<Image>().DOColor(_passiveColor, 0.3f);
            yield return new WaitForSeconds(0.1f);
        }

        NoSelect.GetComponent<Image>().DOColor(_passiveColor, 0.3f);
        yield return new WaitForSeconds(0.1f);

        yield return new WaitForSeconds(0.3f);

        NoSelect.SetActive(false);

        foreach (var obj in _selectLangs)
        {
            obj.SetActive(false);
        }

        yield return new WaitForSeconds(0.3f);
        _selectBarImage.DOComplete();

        // ガード解除
        SelectGuard.SetActive(false);
    }

    /// <summary>
    /// ラインの明滅アニメ
    /// </summary>
    /// <returns></returns>
    IEnumerator BlinkLine()
    {
        while (true)
        {
            //表示
            _horizontalLineImage.DOColor(_activeColor, 0.7f);
            _verticalLineImage.DOColor(_activeColor, 0.7f);

            //表示停止
            yield return _waitForBlinkLine;

            //非表示
            _horizontalLineImage.DOColor(_passiveColor, 0.7f);
            _verticalLineImage.DOColor(_passiveColor, 0.7f);

            //非表示停止
            yield return _waitForBlinkLine;
        }
    }

    private Sprite GetLanguageSprite(Language lang)
    {
        Sprite ret = null;

        // TODO: 画像ファイル名を json に含める
        switch (lang)
        {
            case Language.Japanese:
                ret = Resources.Load<Sprite>("Images/Icon/icon_lang_ja");
                break;
            case Language.English:
                ret = Resources.Load<Sprite>("Images/Icon/icon_lang_en");
                break;
            case Language.Russian:
                ret = Resources.Load<Sprite>("Images/Icon/icon_lang_ru");
                break;
            case Language.Arabic:
                ret = Resources.Load<Sprite>("Images/Icon/icon_lang_ar");
                break;
            case Language.Chinese:
                ret = Resources.Load<Sprite>("Images/Icon/icon_lang_cn");
                break;
            case Language.Vietnamese:
                ret = Resources.Load<Sprite>("Images/Icon/icon_lang_vi");
                break;
            default:
                Debug.LogError("Not implemented language.");
                break;
        }

        return ret;
    }

    private int GetLanguageIndex(Language lang)
    {
        int ret = 0;

        var langCount = 0;
        foreach (var i in SignageSettings.Settings.LanguageVoiceMap.Keys)
        {
            if (lang == i)
            {
                ret = langCount;
                break;
            }
            langCount++;
        }

        return ret;
    }
}
