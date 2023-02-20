using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class CallingPanel : MonoBehaviour
{
    public Action OnCancelled = null;

    [SerializeField]
    private Button _cancelButton = null;

    private static readonly string CallingIconPath = "Images/Icon/icon_calling";
    private static readonly string CallingEndIconPath = "Images/Icon/icon_calling_end";
    private static readonly float AnimationIntervalSec = 2.5f;
    private static readonly Vector3 CallingIconShakeVec = new Vector3(0.0f, 0.0f, 5.0f);
    private static readonly Color CallingTextFadeColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    private Image _callingIcon = null;
    private TextMeshProUGUI _callingText = null;
    private IDisposable _callingTask = null;

    public void Initialize()
    {
        _callingIcon = GetComponentsInChildren<Image>().Where(x => x.gameObject != this.gameObject).First();
        _callingText = GetComponentInChildren<TextMeshProUGUI>();
        _cancelButton.OnClickAsObservable().Subscribe(_ =>
        {
            Cancel();
        }).AddTo(gameObject);
    }

    public void Enable()
    {
        ResetIcon();
        ResetText();
        _cancelButton.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        _callingTask?.Dispose();
        _cancelButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Call()
    {
        if (!gameObject.activeSelf)
        {
            Enable();
        }

        // 待機アニメーション開始
        _callingTask = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(AnimationIntervalSec)).Subscribe(_ =>
        {
            // アイコンを揺らしながらテキストをフェード
            ShakeIcon(AnimationIntervalSec);
            FadeText(AnimationIntervalSec);
            AudioManager.Instance.StopSE();
            AudioManager.Instance.PlaySE(AudioManager.SEType.CallingStart, false).Forget();
        });
    }

    public void Connect()
    {
        // 待機アニメーション終了
        _callingTask.Dispose();

        // キャンセルアイコン非表示
        _cancelButton.gameObject.SetActive(false);

        // アイコン変更
        ChangeIcon();
        // テキスト変更
        ChangeText();
        // 待機SE停止
        AudioManager.Instance.StopSE();
        // 接続成功SE再生
        AudioManager.Instance.PlaySE(AudioManager.SEType.CallingEnd).Forget();

        // テキスト明滅
        Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0.3f)).Take(3).Subscribe(_ =>
        {
            FadeText(0.2f);
        }, ex =>
        {
            Debug.LogError(ex.Message);
        }, async () =>
        {
            // 少し待つ
            await UniTask.Delay(250);
            // 非表示にする
            Disable();
        });
    }

    private void Cancel()
    {
        Disable();

        OnCancelled?.Invoke();
    }

    private void ShakeIcon(float intervalSec)
    {
        _callingIcon.rectTransform.DOKill();
        _callingIcon.rectTransform.DOPunchRotation(CallingIconShakeVec, intervalSec).SetEase(Ease.Linear);
    }

    private void ResetIcon()
    {
        _callingIcon.sprite = Resources.Load<Sprite>(CallingIconPath);
    }

    private void ResetText()
    {
        _callingText.text = "Calling ...";
    }

    private void ChangeIcon()
    {
        _callingIcon.sprite = Resources.Load<Sprite>(CallingEndIconPath);
    }

    private void ChangeText()
    {
        _callingText.text = "Connected";
    }

    private void FadeText(float intervalSec)
    {
        _callingText.DOKill();
        _callingText.color = CallingTextFadeColor;
        _callingText.DOFade(0.0f, intervalSec).SetEase(Ease.Linear);
    }
}
