using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;


public class InternetCheck : MonoBehaviour
{
    private static readonly int CheckIntervalSec = 10;

    private Image _image = null;
    private bool _isValid = true;
    private Color _validColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    private Color _inValidColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();

        var fadeTween = _image.DOFade(0.0f, 1.0f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);

        Observable.Interval(TimeSpan.FromSeconds(CheckIntervalSec)).Subscribe(async _ =>
        {
            var isValid = await NetworkHelper.Ping();
            if (_isValid && !isValid)
            {
                if (GlobalState.Instance.CurrentState.Value != GlobalState.State.LoadingComplete)
                {
                    UIManager.Instance.DisplayError(GlobalState.ErrorCode.Network);
                }
            }
            _isValid = isValid;
        }).AddTo(this.gameObject);
    }
}
