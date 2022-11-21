using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBlinkEX : MonoBehaviour {

    enum EyeState { open, wait, single, blink };

    private EyeState eyeStatus = EyeState.open;     //現在のステータス
    private float statusTime = 0;
    private float delayTime = 0;
    

    public float minInterval = 3.0f;               //瞬き開始待ちまでの最小インターバル
    public float maxInterval = 7.0f;               //瞬き開始待ちまでの最大インターバル

    public float singleBlinkInterval = 0.04f;          //single瞬き時インターバル
    public float singleCloseInterval = 0.1f;          //single開閉インターバル
    public float doubleBlinkInterval = 0.02f;          //double瞬き時インターバル
    public float doubleCloseInterval = 0.04f;          //double瞬き時インターバル



    public bool isContinueClosing = false;    //trueの間は目を閉じ続ける

    private float closeStartTime = 0;   //isContinueClosing == trueの時に目を閉じた時間
    private float closeEndTime = 0; //isContinueClosing == trueの時に目を閉じるのをやめた時間

    public bool isActive = true;                //オート目パチ有効
    private SkinnedMeshRenderer _ref_SMR_EYE_DEF; //EYE_DEFへの参照

    private int _leftEyeKey = -1;
    private int _rightEyeKey = -1;

    enum Status {
        Close,
        HalfClose,
        Open    //目パチの状態
    }




    // Use this for initialization
    void Start() {
        _ref_SMR_EYE_DEF = CharacterManager.Instance.FacialSkinnedMesh;
        _leftEyeKey = CharacterManager.Instance.BlendShapleLeftEyeKey;
        _rightEyeKey = CharacterManager.Instance.BlendShapeRightEyeKey;
    }

    public void ResetState()
    {
        eyeStatus = EyeState.open;
        statusTime = delayTime = closeStartTime = closeEndTime = 0;
        isActive = true;
    }

    public IEnumerator StopBlink()
    {
        Debug.Log("★StopBlink starts:" + gameObject.name + " isActive:" + isActive);
        if (!isActive)
            yield break;
        isContinueClosing = false;
        yield return new WaitUntil(() => eyeStatus == EyeState.wait || eyeStatus == EyeState.open);
        //ref_SMR_EYE_DEF.SetBlendShapeWeight(7, 0);  //念のために
        _ref_SMR_EYE_DEF.SetBlendShapeWeight(_leftEyeKey, 0);  //念のために
        _ref_SMR_EYE_DEF.SetBlendShapeWeight(_rightEyeKey, 0);  //念のために
        isActive = false;
        Debug.Log("★StopBlink ends:" + gameObject.name);
    }

    // Update is called once per frame
    void Update() {

        if (!isActive) return;

        statusTime += Time.deltaTime;

        switch (eyeStatus) {

            case EyeState.open:
                stateOpen();
                break;

            case EyeState.wait:
                stateWait();
                break;

            case EyeState.single:
                stateSingle();
                break;

            case EyeState.blink:
                stateBlink();
                break;

        }
    }

    public void Pause()
    {
        isActive = false;
    }

    public void Play()
    {
        isActive = true;
    }

    /// <summary>
    /// 次のステータス設定待ち
    /// </summary>
    void stateOpen() {

        eyeStatus = EyeState.wait;
        statusTime = 0;
        delayTime = Random.Range(minInterval, maxInterval);
    }

    /// <summary>
    /// 瞬きまでの待ち
    /// </summary>
    void stateWait() {

        if (delayTime > statusTime)
            return;

        //瞬きのモード選択
        statusTime = 0;
        //eyeStatus = EyeState.blink;
        eyeStatus = isContinueClosing ? EyeState.single : (EyeState)Random.Range((int)EyeState.single, (int)EyeState.blink+1);
        //Debug.Log(eyeStatus);

    }

    /// <summary>
    /// レシオ取得
    /// </summary>
    float GetRatio(float startTime, float closeInterval, float blinkInterval) {

        float ratio = 0;

        //クローズへ
        if (startTime <= closeInterval)
        {
            ratio = startTime * 100 / closeInterval;
            //Debug.Log("クローズへ startTime:" + startTime + " closeInterval:" + closeInterval + " ratio:" + ratio + " gameObject.name:" + gameObject.name);
        }
        //クローズ
        else if (closeInterval + blinkInterval > startTime || isContinueClosing)
        {
            //Debug.Log("クローズ isContinueClosing:" + isContinueClosing + " startTime:" + startTime + " closeInterval + blinkInterval:" + (closeInterval + blinkInterval) + " gameObject.name:" + gameObject.name);
            ratio = 100;
            if (/*isContinueClosing && */closeStartTime == 0)
            {
                closeStartTime = startTime;
                //Debug.Log("closeStartTime=" + closeStartTime + " gameObject.name:" + gameObject.name);
            }
        }
        //オープンへ
        else
        {
            if (/*isContinueClosing && */closeEndTime == 0)
            {
                closeEndTime = startTime;
                //Debug.Log("closeEndTime=" + closeEndTime + " gameObject.name:" + gameObject.name);
            }
            ratio = 100 - ((startTime - closeInterval - (isContinueClosing ? closeEndTime - closeStartTime : blinkInterval)) * 100 / closeInterval);
            //Debug.Log("オープンへ startTime:" + startTime + " closeInterval:" + closeInterval + " getBlinkInterval(blinkInterval):" + getBlinkInterval(blinkInterval) + " ratio:" + ratio + " gameObject.name:" + gameObject.name);
        }

        // 0 ～ 100で丸める
        ratio = ratio < 0 ? 0 : ratio;
        ratio = ratio > 100 ? 100 : ratio;

        return ratio;
    }

    /// <summary>
    /// 深い瞬き
    /// </summary>
    void stateSingle() {


        //現在のレシオで瞬き設定
        float ratio = GetRatio(statusTime, singleCloseInterval, singleBlinkInterval);
        //ref_SMR_EYE_DEF.SetBlendShapeWeight(7, ratio);
        _ref_SMR_EYE_DEF.SetBlendShapeWeight(_leftEyeKey, ratio);
        _ref_SMR_EYE_DEF.SetBlendShapeWeight(_rightEyeKey, ratio);

        float totalTime = singleCloseInterval * 2 + singleBlinkInterval;
        if (statusTime < totalTime || isContinueClosing)
            return;


        eyeStatus = EyeState.open;
        statusTime = 0;

    }

    /// <summary>
    /// 連続瞬き
    /// </summary>
    void stateBlink() {

        float startTime = statusTime > doubleCloseInterval + doubleBlinkInterval ? statusTime - (doubleCloseInterval + doubleBlinkInterval) : statusTime;
        


        //現在のレシオで瞬き設定
        float ratio = GetRatio(startTime, doubleCloseInterval, doubleBlinkInterval);
        //ref_SMR_EYE_DEF.SetBlendShapeWeight(7, ratio);
        _ref_SMR_EYE_DEF.SetBlendShapeWeight(_leftEyeKey, ratio);
        _ref_SMR_EYE_DEF.SetBlendShapeWeight(_rightEyeKey, ratio);



        float totalTime = (singleCloseInterval * 2 + singleBlinkInterval)*2;
        if (statusTime < totalTime)
            return;


        eyeStatus = EyeState.open;
        statusTime = 0;


    }
    
}
