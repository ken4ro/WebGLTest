using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class SpeakableIcon : MonoBehaviour
{
    /// <summary>
    /// 発話可能状態表示用ゲームオブジェクト
    /// </summary>
    [SerializeField] GameObject speakableIcon = null;

    /// <summary>
    /// フェードイン時の色
    /// </summary>
    [SerializeField] Color onColor = Color.clear;

    /// <summary>
    /// フェードオフ時の色
    /// </summary>
    [SerializeField] Color offColor = Color.clear;

    private bool isEnabled = false;
    private GameObject _pacMan = null;
    private GameObject _ring = null;
    private SpriteRenderer _pacManSpriteRenderer = null;
    private SpriteRenderer _ringSpriteRenderer = null;

    // GC対策
    private WaitForSeconds _waitForRotateRing = new WaitForSeconds(0.01f);
    private WaitForSeconds _waitForChangeActiveAsync = new WaitForSeconds(0.26f);
    private WaitForSeconds _waitForChangeDisableAsync = new WaitForSeconds(0.2f);

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
        _pacMan = speakableIcon.transform.GetChild(0).gameObject;
        _ring = speakableIcon.transform.GetChild(1).gameObject;
        _pacManSpriteRenderer = _pacMan.GetComponent<SpriteRenderer>();
        _ringSpriteRenderer = _ring.GetComponent<SpriteRenderer>();
        _pacManSpriteRenderer.color = offColor;
        _ringSpriteRenderer.color = onColor;
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    public void FadeIn()
    {
        if (!isEnabled)
        {
            if (!speakableIcon.activeSelf) speakableIcon.SetActive(true);
            StartCoroutine("PopUpEffect");
            StartCoroutine("ChangeActiveAsync");
            isEnabled = true;
        }
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    public void FadeOut()
    {
        if (isEnabled)
        {
            StartCoroutine("PopDownEffect");
            StartCoroutine("ChangeDisableAsync");
            isEnabled = false;
        }
    }

    /// <summary>
    /// アニメーションキャンセル
    /// </summary>
    public void Cancel()
    {
        StopCoroutine("PopUpEffect");
        StopCoroutine("ChangeActiveAsync");
        StopCoroutine("PopDownEffect");
        StopCoroutine("ChangeDisableAsync");
        StopCoroutine("RotateRing");
    }

    IEnumerator RotateRing()
    {
        while (true)
        {
            yield return _waitForRotateRing;
            _ring.transform.Rotate(0.0f, 0.0f, -7.0f);
        }
    }

    IEnumerator PopUpEffect()
    {
        float frame = 0;

        var MiniSize = new Vector3(0.7f, 0.7f, 0.7f);
        var NormalSize = new Vector3(1, 1, 1);

        //縮小
        while (true)
        {
            frame += Time.deltaTime;
            var ratio = frame / 0.2;

            _pacMan.transform.localScale = Vector3.Lerp(NormalSize, MiniSize, (float)ratio);

            if (ratio >= 1)
                break;

            yield return null;
        }

        //拡大
        frame = 0;
        while (true)
        {
            frame += Time.deltaTime;
            var ratio = frame / 0.1;

            _pacMan.transform.localScale = Vector3.Lerp(MiniSize, NormalSize, (float)ratio);

            if (ratio >= 1)
                break;

            yield return null;
        }
    }

    IEnumerator PopDownEffect()
    {
        float frame = 0;

        var BigSize = new Vector3(1.3f, 1.3f, 1.3f);
        var NormalSize = new Vector3(1, 1, 1);
        var MiniSize = new Vector3(0, 0, 0);

        //拡大
        while (true)
        {
            frame += Time.deltaTime;
            var ratio = frame / 0.2;

            _pacMan.transform.localScale = Vector3.Lerp(NormalSize, BigSize, (float)ratio);

            if (ratio >= 1)
                break;

            yield return null;
        }

        //縮小
        frame = 0;
        while (true)
        {
            frame += Time.deltaTime;
            var ratio = frame / 0.3;

            _pacMan.transform.localScale = Vector3.Lerp(BigSize, MiniSize, (float)ratio);

            if (ratio >= 1)
                break;

            yield return null;
        }
    }

    IEnumerator ChangeActiveAsync()
    {
        yield return _waitForChangeActiveAsync;

        var ringOffScale = new Vector3(0.6f, 0.6f, 1);
        var ringOnScale = new Vector3(1, 1, 1);

        var alphaOffColor = onColor;
        alphaOffColor.a = 0;

        float frame = 0;

        StartCoroutine("RotateRing");

        while (true)
        {
            //0.3秒で切り替え
            frame += Time.deltaTime;
            var ratio = frame / 0.3;

            //パックマン
            _pacManSpriteRenderer.color = Color.Lerp(offColor, onColor, (float)ratio);

            //リング
            _ringSpriteRenderer.color = Color.Lerp(alphaOffColor, onColor, (float)ratio);
            _ring.transform.localScale = Vector3.Lerp(ringOffScale, ringOnScale, (float)ratio);

            if (ratio >= 1)
                break;

            yield return null;
        }
    }

    IEnumerator ChangeDisableAsync()
    {
        yield return _waitForChangeDisableAsync;

        var alphaOffColor = onColor;
        alphaOffColor.a = 0;

        float frame = 0;
        while (true)
        {
            frame += Time.deltaTime;
            var ratio = frame / 0.3;

            _ringSpriteRenderer.color = Color.Lerp(onColor, alphaOffColor, (float)ratio);

            if (ratio >= 1)
                break;

            yield return null;
        }

        StopCoroutine("RotateRing");
    }
}
