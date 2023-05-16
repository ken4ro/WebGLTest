using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UniRx;
using TMPro;
using DG.Tweening;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using static BotManager;
using static SignageSettings;

public class CharacterMessage : MonoBehaviour
{
    /// <summary>
    /// 選択肢ボタン押下時コールバック
    /// </summary>
    public Action<string> OnClickSelectWord = null;

    /// <summary>
    /// テキストの文字送りディレイタイム
    /// </summary>
    public int TextAnimationDelayTimeMs { get; set; } = 60;

    /// <summary>
    /// ウィンドウ画像
    /// </summary>
    [SerializeField] Image messageWindow = null;

    /// <summary>
    /// テキスト
    /// </summary>
    [SerializeField] TextMeshProUGUI messageText = null;

    /// <summary>
    /// 画像
    /// </summary>
    [SerializeField] Image messageImage = null;

    /// <summary>
    /// 動画
    /// </summary>
    [SerializeField] VideoPlayer messageMovie = null;

    /// <summary>
    /// 画像クリック
    /// </summary>
    public Action OnImageClick = null;

    private static readonly uint MaxEnableSelectButtonNum = 3;
    private static readonly Color _enableColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private static readonly Color _disableColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);

    private RectTransform _rectTransform = null;
    private Vector3 _defaultPos = Vector3.zero;
    private float _selectWindowBottomPos = 0.0f;

    private GameObject _select = null;
    private ReactiveProperty<int> _pageIndex = new ReactiveProperty<int>(-1);
    private Dictionary<int, List<BotResponseSelect>> _selectObjectsInPage = new Dictionary<int, List<BotResponseSelect>>();
    private Button _leftButton = null;
    private Button _rightButton = null;
    private Language _viewLanguage = Language.Japanese;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _defaultPos = _rectTransform.localPosition;
        _pageIndex.Subscribe(x => PageChanged(x)).AddTo(gameObject);
    }

    /// <summary>
    /// キャラクターメッセージの表示を有効にする
    /// </summary>
    public void Enable()
    {
        messageWindow.color = _enableColor;
    }

    /// <summary>
    /// キャラクターメッセージの表示を無効にする
    /// </summary>
    public void Disable()
    {
        messageWindow.color = _disableColor;
    }

    /// <summary>
    /// フォントサイズセット
    /// </summary>
    /// <param name="size"></param>
    public void SetFontSize(int size)
    {
        messageText.fontSize = size;
    }

    /// <summary>
    /// キャラクターメッセージをセット
    /// </summary>
    /// <param name="message"></param>
    /// <param name="isAnim"></param>
    /// <param name="imageType"></param>
    /// <param name="imageFileName"></param>
    /// <param name="selectTexts"></param>
    /// <param name="selectImageFileName"></param>
    public async UniTask SetCharacterMessage(string message, bool isAnim, ImageAccessTypes imageType, string imageFileName)
    {
        // 文字表示速度リセット
        TextAnimationDelayTimeMs = GlobalState.Instance.UserSettings.UI.TextSpeed;
        // 表示リセット
        ResetCharacterMessage();
        // 表示を有効化
        Enable();
        // テキストセット
        await SetText(message, isAnim);
        // 画像セット
        await SetImage(imageType, imageFileName);
    }

    /// <summary>
    /// キャラクターメッセージ動画をセット
    /// </summary>
    /// <param name="movieFilePath"></param>
    public void SetCharacterMessageMovie(string movieFilePath)
    {
        SetMovie(movieFilePath);
    }

    /// <summary>
    /// 選択肢構造のセット
    /// </summary>
    /// <param name="selectObjects"></param>
    /// <param name="lang"></param>
    public void SetSelectObjects(List<BotResponseSelect> selectObjects, Language lang)
    {
        if (selectObjects == null || selectObjects.Count <= 0)
            return;

        CreateSelectObject();
        InitializeSelectButton(selectObjects,lang);
        AlignSelectObject();
    }

    /// <summary>
    /// キャラクターメッセージをリセット
    /// </summary>
    public void ResetCharacterMessage()
    {
        // コンテンツレイアウト設定リセット
        GetComponent<VerticalLayoutGroup>().childControlHeight = true;
        GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        // テキストリセット
        ResetText();
        // 画像リセット
        ResetImage();
        // 動画リセット
        ResetMovie();
        // 表示位置リセット
        ResetPosition();
        // 選択肢リセット
        ResetSelect();
        // 表示を無効にする
        Disable();
    }

    /// <summary>
    /// 選択肢メッセージをリセット
    /// </summary>
    public void ResetSelectMessage()
    {
        ResetSelect();
    }

    /// <summary>
    /// 左から右へ表示する(デフォルト)
    /// </summary>
    public void LeftToRight() => messageText.isRightToLeftText = false;

    /// <summary>
    /// 右から左へ表示する(主にアラビア語用)
    /// </summary>
    public void RightToLeft() => messageText.isRightToLeftText = true;

    private async UniTask SetText(string text, bool isAnim = false)
    {
        if (isAnim)
        {
            await StartTextGradationAnimation(messageText, text);
            //await StartTextAnimation(messageText, text);
        }
        else
        {
            messageText.text = text;
        }
    }

    // グラデーション表示対応テキストアニメーション
    private async UniTask StartTextGradationAnimation(TextMeshProUGUI target, string text)
    {
        if (target == null) return;

        if (string.IsNullOrEmpty(text) || TextAnimationDelayTimeMs <= 0)
        {
            target.text = text;
            return;
        }

        var step = 5;
        var wait = TextAnimationDelayTimeMs / 5;

        var stepResolution = TextAnimationDelayTimeMs / step;
        DateTime startDt = DateTime.Now;
        while (true)
        {
            //経過時間から表示文字数を算出
            TimeSpan ts = DateTime.Now - startDt;
            var putCount = (int)ts.TotalMilliseconds / TextAnimationDelayTimeMs - step;

            if (putCount >= text.Length)
            {
                target.text = text;
                break;
            }

            string sb = "";
            if (putCount > 0)
                sb = "<alpha=#FF>" + text.Substring(0, putCount);

            //アルファ文字追記
            var rem = ((int)ts.TotalMilliseconds % TextAnimationDelayTimeMs) + stepResolution * (step - 1);
            rem = TextAnimationDelayTimeMs < rem ? TextAnimationDelayTimeMs : rem;

            for (int i = 0; i < step; i++)
            {

                if (rem > 0 && putCount + i >= 0 && putCount + i < text.Length)
                    sb += String.Format("<alpha=#{0}>{1}", (rem * 255 / TextAnimationDelayTimeMs).ToString("X2"), text.Substring(putCount + i, 1));

                rem -= stepResolution;
            }

            target.text = sb.ToString();

            await UniTask.Delay(wait);
        }
    }

    private async UniTask SetImage(ImageAccessTypes type, string filePath)
    {
        var texture2D = await GraphicsHelper.LoadImage(type, filePath);
        if (texture2D == null)
            return;

        messageImage.sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), Vector2.zero, 1.0f);
        messageImage.type = Image.Type.Filled;
        messageImage.preserveAspect = true;
        messageImage.gameObject.SetActive(true);

        // フェードインさせてみる
        messageImage.color = _disableColor;
        messageImage.DOFade(1.0f, 0.5f);
    }

    private void SetMovie(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        messageMovie.gameObject.SetActive(true);
        messageMovie.url = FileHelper.GetCurrentDirectory() + filePath;
        messageMovie.prepareCompleted += OnPrepareCompleted;
        messageMovie.Prepare();
    }

    private void OnPrepareCompleted(VideoPlayer vp)
    {
        Debug.Log("Message movie has prepared.");

        var rawImage = messageMovie.gameObject.GetComponent<RawImage>();
        rawImage.texture = messageMovie.texture;

        // RawImage だと Content Size Filter で動的にサイズ変更されないので暫定対処
        var verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.childControlHeight = false;
        var contentSizeFilter = GetComponent<ContentSizeFitter>();
        contentSizeFilter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        var rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y + messageMovie.texture.height);
        messageMovie.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(messageMovie.texture.width, messageMovie.texture.height);

        messageMovie.prepareCompleted -= OnPrepareCompleted;
        messageMovie.Play();
    }

    private void ResetPosition()
    {
        _rectTransform.localPosition = _defaultPos;
    }

    private void ResetText()
    {
        messageText.text = "";
    }

    private void ResetImage()
    {
        messageImage.color = _disableColor;
        messageImage.gameObject.SetActive(false);
        messageImage.sprite = null;
    }

    private void ResetMovie()
    {
        messageMovie.gameObject.SetActive(false);
        if (messageMovie.isPlaying)
        {
            messageMovie.Stop();
            messageMovie.url = null;
        }
    }

    private void ResetSelect()
    {
        Destroy(_select);

        _pageIndex.Value = -1;
        _selectObjectsInPage.Clear();
        _leftButton = null;
        _rightButton = null;
    }

    private void CreateSelectObject()
    {
        var selectPrefab = Resources.Load<GameObject>("Prefabs/Select");

        _select = Instantiate(selectPrefab, transform.parent);
    }

    private void AlignSelectObject()
    {
        // 選択肢の表示数は固定(最大3つ)。残りはスクロール or ページ送り
        var selectObjectRectTransform = _select.GetComponent<RectTransform>();
        // キャラクターメッセージのボトム＋マージン位置に表示する
        //var margin = 20;
        var margin = 0;
        var posY = _rectTransform.localPosition.y - _rectTransform.sizeDelta.y / 2 - selectObjectRectTransform.sizeDelta.y / 2 - margin;
        selectObjectRectTransform.localPosition = new Vector3(491, posY);

        // 選択肢の数に応じて表示位置変更
        _selectWindowBottomPos = /*-640.0f*/ -690.0f + _selectObjectsInPage[_pageIndex.Value].Count * /*120.0f*/ 150.0f;

        if (posY < _selectWindowBottomPos)
        {
            // 表示領域が重複した場合はキャラクターメッセージを上に詰める
            var offsetY = _selectWindowBottomPos - posY;
            _rectTransform.DOLocalMoveY(_rectTransform.localPosition.y + offsetY, 0.2f);
            selectObjectRectTransform.DOLocalMoveY(selectObjectRectTransform.localPosition.y + offsetY, 0.2f);
        }

        // フェードさせてみる
        Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ =>
        {
            var canvasGroup = _select.GetComponent<CanvasGroup>();
            canvasGroup.DOFade(1.0f, 0.5f);
        });
    }

    /// <summary>
    /// ボタン情報の初期化（構造対応)
    /// </summary>
    /// <param name="selectObjects"></param>
    /// <param name="lang">使用中の言語</param>
    private void InitializeSelectButton(List<BotResponseSelect> selectObjects, Language lang)
    {
        var buttons = _select.GetComponentsInChildren<Button>(true);

        // 選択肢リスト作成
        for (var i = 0; i < selectObjects.Count; i++)
        {
            var page = i / 3;
            if (!_selectObjectsInPage.ContainsKey(page))
            {
                _selectObjectsInPage[page] = new List<BotResponseSelect>();
            }
            _selectObjectsInPage[page].Add(selectObjects[i]);
        }

        // ページ送り設定
        if (selectObjects.Count > MaxEnableSelectButtonNum)
        {
            // 次ページ
            var right = _select.transform.Find("Right").gameObject;
            _rightButton = right.GetComponent<Button>();
            _rightButton.onClick.AddListener(NextPage);

            // 前ページ(表示は無効にしておく)
            var left = _select.transform.Find("Left").gameObject;
            _leftButton = left.GetComponent<Button>();
            _leftButton.onClick.AddListener(PrevPage);
        }
        _pageIndex.Value = 0;
        _viewLanguage = lang;

        // ボタン設定
        SetSelectObjectButton();
    }

    /// <summary>
    /// 選択肢をセット
    /// </summary>
    private void SetSelectObjectButton()
    {
        var buttons = _select.GetComponentsInChildren<Button>(true);

        // ボタンテキストをセット
        for (var i = 0; i < _selectObjectsInPage[_pageIndex.Value].Count; i++)
        {
            buttons[i].gameObject.transform.parent.gameObject.SetActive(true);
            buttons[i].GetComponentInChildren<TextMeshProUGUI>(true).text = BotResponseSelect.GetTargetText(_selectObjectsInPage[_pageIndex.Value][i], _viewLanguage);
            var col = buttons[i].colors;
            col.normalColor = _selectObjectsInPage[_pageIndex.Value][i].Color;
            buttons[i].colors = col;
        }

        // ボタン押下時イベントをセット
        for (var i = 0; i < _selectObjectsInPage[_pageIndex.Value].Count; i++)
        {
            var button = buttons[i];
            var text = button.GetComponentInChildren<TextMeshProUGUI>(true).text;
            button.onClick.AddListener(() => SelectWord(button, text));
        }
    }

    private void ResetButton()
    {
        var buttons = _select.GetComponentsInChildren<Button>(true);

        for (var i = 0; i < _selectObjectsInPage[_pageIndex.Value].Count; i++)
        {
            buttons[i].GetComponentInChildren<TextMeshProUGUI>(true).text = "";
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].gameObject.transform.parent.gameObject.SetActive(false);
        }
    }

    private void NextPage()
    {
        // SE 再生
        AudioManager.Instance.PlaySE(AudioManager.SEType.NextPage).Forget();

        // ボタンリセット
        ResetButton();

        // ページ数インクリメント
        _pageIndex.Value++;

        // ボタン設定
        SetSelectObjectButton();
    }

    private void PrevPage()
    {
        // SE 再生
        AudioManager.Instance.PlaySE(AudioManager.SEType.NextPage).Forget();

        // ボタンリセット
        ResetButton();

        // ページ数デクリメント
        _pageIndex.Value--;

        // ボタン設定
        SetSelectObjectButton();
    }


    // ページ変化時コールバック (page: 0, 1, 2, ...)
    private void PageChanged(int page)
    {
        if (page == -1) return;

        var pageNum = _selectObjectsInPage.Keys.Count;
        if (pageNum == 1) return;

        // 次ページ
        if (page + 1 < pageNum)
        {
            _rightButton.gameObject.SetActive(true);
        }
        else
        {
            _rightButton.gameObject.SetActive(false);
        }

        // 前ページ
        if (page == 0)
        {
            _leftButton.gameObject.SetActive(false);
        }
        else
        {
            _leftButton.gameObject.SetActive(true);
        }
    }

    private async void SelectWord(Button button, string text)
    {
        // ボタン連打防止
        button.onClick.RemoveAllListeners();

        // SE 再生
        AudioManager.Instance.PlaySE(AudioManager.SEType.SelectSentence).Forget();

        // ボタン押下アニメーション
        var uiShiny = button.gameObject.GetComponent<UIShiny>();
        DOTween.To(() => uiShiny.effectFactor, (x) => uiShiny.effectFactor = x, 1.0f, 0.2f);

        // アニメーション終了待機
        await UniTask.Delay(200);

        // サウンド的に少し間を空けておく(?)
        await UniTask.Delay(100);

        // イベント発行
        OnClickSelectWord?.Invoke(text);
    }

    /// <summary>
    /// パネルイベント有効化
    /// </summary>
    /// <param name="swt"></param>
    public void EnablePanelEvent(bool swt)=> messageImage.raycastTarget = swt;

    public void PushPanel()
    {
        OnImageClick?.Invoke();
    }
}
