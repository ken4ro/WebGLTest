using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using Coffee.UIExtensions;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
/// 背景画像表示用ゲームオブジェクトにアタッチする
/// </summary>
public class Background : MonoBehaviour
{
    public enum TransitionType
    {
        ScaleX,
        ScaleY,
    }

    public enum BackgroundType
    {
        Default,
        Telexistence,
    }

    private GameObject _backGround = null;
    private RectTransform _rectTransform = null;
    private Image _backGroundImage = null;
    private List<Sprite> _sprites = new List<Sprite>();

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
        _backGround = this.gameObject;
        _rectTransform = _backGround.GetComponent<RectTransform>();
        _backGroundImage = GetComponent<Image>();

#if false
        // デフォルトの背景画像(顧客によって変わる)用スプライト作成
        var backGroundTexture2D = AssetBundleManager.Instance.LoadTexture2DFromResourcePack("Background");
        if (backGroundTexture2D == null)
        {
            Debug.LogError("Background.Initialize error: Background texture2D is null.");
            return;
        }
        var defaultSprite = Sprite.Create(backGroundTexture2D, new Rect(0.0f, 0.0f, backGroundTexture2D.width, backGroundTexture2D.height), Vector2.zero, 1.0f);
        if (defaultSprite == null)
        {
            Debug.LogError("Background.Initialize error: Background sprite is null.");
            return;
        }
#else
        var defaultSprite = Resources.Load<Sprite>("Images/background");
#endif

        // テレイグジスタンスモード中の背景画像(固定)用スプライト作成
        var telexistenceSprite = Resources.Load<Sprite>("Images/bg_telexistence");
        if (telexistenceSprite == null)
        {
            Debug.LogError("Background.Initialize error: Telexistence sprite is null.");
            return;
        }

        // スプライトリスト作成
        _sprites.Add(defaultSprite);
        _sprites.Add(telexistenceSprite);

        // 背景画像ロード
        _backGroundImage.sprite = defaultSprite;
    }

    /// <summary>
    /// キャラクターメッセージの表示を有効にする
    /// </summary>
    public void Enable() => _backGround.SetActive(true);

    /// <summary>
    /// キャラクターメッセージの表示を無効にする
    /// </summary>
    public void Disable() => _backGround.SetActive(false);

    /// <summary>
    /// 背景遷移(フェード)
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fadeInSec"></param>
    /// <param name="fadeOutSec"></param>
    public async UniTask Transition(BackgroundType bgType, TransitionType transType, float msec)
    {
        var halfTime = msec / 2;
        var sprite = _sprites[(int)bgType];
        if (sprite == null) return;
        switch (transType)
        {
            case TransitionType.ScaleX:
                _rectTransform.DOScaleX(0.0f, halfTime);
                await UniTask.Delay((int)(halfTime + 100));
                _backGroundImage.sprite = sprite;
                _rectTransform.DOScaleX(1.0f, halfTime);
                break;
            case TransitionType.ScaleY:
                _rectTransform.DOScaleY(0.0f, halfTime / 1000);
                await UniTask.Delay((int)(halfTime + 100));
                _backGroundImage.sprite = sprite;
                _rectTransform.DOScaleY(1.0f, halfTime / 1000);
                break;
        }
    }
}
