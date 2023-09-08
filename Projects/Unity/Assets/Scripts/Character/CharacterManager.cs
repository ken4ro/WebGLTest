using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Motion;
using Cysharp.Threading.Tasks;
using Live2D.Cubism.Framework.MouthMovement;

/// <summary>
/// キャラクター管理クラス(キャラクターオブジェクトにアタッチ)
/// </summary>
public class CharacterManager : SingletonMonoBehaviour<CharacterManager>
{
    [SerializeField]
    private RuntimeAnimatorController _motionAnimatorController = null;

    [SerializeField]
    private RuntimeAnimatorController _facialAnimatorController = null;

    /// <summary>
    /// フェイシャル
    /// </summary>
    [SerializeField]
    private SkinnedMeshRenderer _facialSkinnedMesh = null;
    public SkinnedMeshRenderer FacialSkinnedMesh { get => _facialSkinnedMesh; private set => _facialSkinnedMesh = value; }

    /// <summary>
    /// 首制御用ボーン
    /// </summary>
    [SerializeField]
    private Transform _neckTransform = null;
    public Transform NeckTransform { get => _neckTransform; private set => _neckTransform = value; }

    /// <summary>
    /// 身体制御用ボーン
    /// </summary>
    [SerializeField]
    private Transform _bodyTransform = null;
    public Transform BodyTransform { get => _bodyTransform; private set => _bodyTransform = value; }

    /// <summary>
    /// 自動目パチ用コンポーネント(3D用)
    /// </summary>
    [SerializeField]
    private AutoBlinkEX _autoBlinkEX = null;

    /// <summary>
    /// オーディオコールバック
    /// </summary>
    public Action<float[], int> OnAudioFilterReadCallback = null;

    /// <summary>
    /// 初期配置位置
    /// </summary>
    public Vector3 DefaultPosition { get => _characterModel.DefaultPosition; }

    /// <summary>
    /// 初期配置クォータニオン
    /// </summary>
    public Quaternion DefaultRotation { get => _characterModel.DefaultRotation; }

    /// <summary>
    /// 左目パチ用ブレンドシェイプキー
    /// </summary>
    public int BlendShapleLeftEyeKey { get => _characterModel.LeftEyeKey; }

    /// <summary>
    /// 右目パチ用ブレンドシェイプキー
    /// </summary>
    public int BlendShapeRightEyeKey { get => _characterModel.RightEyeKey; }

    /// <summary>
    /// 口パク用ブレンドシェイプキー
    /// </summary>
    public int BlendShapeMouthKey { get => _characterModel.MouthKey; }

    // Telexistance モード中の初期表示位置
    private static readonly Vector3 DefaultPositionForTelexistance = new Vector3(-0.04f, -0.05f, 1.63f);

    // 主にアニメーション処理を委譲
    private ICharacterModel _characterModel = null;

    private Vector3[] _characterPositions = null;
    private Quaternion[] _characterRotations = null;

    // 口パク(Live2D)
    private CubismMouthController _mouthController = null;
    private CubismAutoMouthInput _autoMouthInput = null;
    private CubismParameter _mouthOpenYParameter = null;
    private float _mouthOpenYValue = 0.0f;
    private float _power = 20.0f;
    private float _threshold = 0.1f;
    private float _currentVolume = 0.0f;
    private float _velocity = 0.0f;

    // 口パク(3D)
    private AutoLipSync _autoLipSync = null;


    protected override void Awake()
    {
        base.Awake();

        if (GlobalState.Instance.CurrentCharacterModel.Value == GlobalState.CharacterModel.Una2D)
        {
            _mouthController = this.gameObject.GetComponent<CubismMouthController>();
            _autoMouthInput = this.gameObject.GetComponent<CubismAutoMouthInput>();
            _mouthOpenYParameter = GameObject.Find("ParamMouthOpenY").GetComponent<CubismParameter>();
            _mouthController.enabled = false;
            _autoMouthInput.enabled = false;
        }
        else if (GlobalState.Instance.CurrentCharacterModel.Value == GlobalState.CharacterModel.Una3D)
        {
            _autoLipSync = this.gameObject.GetComponent<AutoLipSync>();
            _autoLipSync._skinnedMeshRenderer = FacialSkinnedMesh;
            _autoLipSync.enabled = false;
        }

        AudioManager.Instance.SetAudioSourceForCharacter();
    }

    void LateUpdate()
    {
        if (GlobalState.Instance.CurrentCharacterModel.Value == GlobalState.CharacterModel.Una2D)
        {
            //float targetVolume = _mouthOpenYValue * _power;
            float targetVolume = _mouthOpenYValue;
            targetVolume = targetVolume < _threshold ? 0 : targetVolume;
            _currentVolume = Mathf.SmoothDamp(_currentVolume, targetVolume, ref _velocity, 0.05f);
            _mouthOpenYParameter.Value = Mathf.Clamp01(_currentVolume);
        }
    }

    void OnEnable()
    {
        // キャラクターモデルアニメーション初期化
        var live2DComponent = GetComponent<CubismModel>();
        if (live2DComponent != null)
        {
            // Live2D キャラクター管理クラス生成
            var model = gameObject.FindCubismModel();
            if (model == null)
            {
                Debug.LogError("Cubism model not found");
                return;
            }
            var animator = gameObject.GetComponent<Animator>();
            var parameters = gameObject.transform.Find("Parameters");
            var eyeBlinkController = gameObject.GetComponent<CubismEyeBlinkController>();
            switch (GlobalState.Instance.CurrentCharacterModel.Value)
            {
                case GlobalState.CharacterModel.Usagi:
                    _characterModel = new UsagiChan(this.gameObject, model, animator, _motionAnimatorController, _facialAnimatorController, parameters, eyeBlinkController);
                    break;
                case GlobalState.CharacterModel.Una2D:
                    _characterModel = new UnaChan2DModel(this.gameObject, model, animator, _motionAnimatorController, _facialAnimatorController, parameters, eyeBlinkController);
                    break;
                case GlobalState.CharacterModel.Una2D_Rugby:
                    _characterModel = new UnaChan2DRugbyModel(this.gameObject, model, animator, _motionAnimatorController, _facialAnimatorController, parameters, eyeBlinkController);
                    break;
            }
        }
        else
        {
            // 3D キャラクターモデル管理クラス生成
            var animator = gameObject.GetComponent<Animator>();
            if (animator == null || FacialSkinnedMesh == null || NeckTransform == null || BodyTransform == null)
            {
                Debug.LogError("Check 3D character object!");
                return;
            }
            switch (GlobalState.Instance.CurrentCharacterModel.Value)
            {
                case GlobalState.CharacterModel.Maru:
                    // まるちゃん
                    _characterModel = new PolygonModel(this.gameObject, animator, _motionAnimatorController, _facialAnimatorController, FacialSkinnedMesh, NeckTransform, BodyTransform);
                    break;
                case GlobalState.CharacterModel.Una3D:
                    // うなちゃん
                    _characterModel = new UnaChanModel(this.gameObject, animator, _motionAnimatorController, _facialAnimatorController, FacialSkinnedMesh, NeckTransform, BodyTransform);
                    break;
                default:
                    Debug.LogError($"Character model {GlobalState.Instance.CurrentCharacterModel.Value.ToString()} is not implemented yet.");
                    break;
            }
        }
    }

    async void Start()
    {
        StopAutoLipSync();

        // アイドルモーション中の各ボーンの座標を保持しておく
        // TODO: キャラクターモデルにアイドルモーションに繋がるようなデフォルトポーズを設定してもらう
        await UniTask.Delay(100);
        GetTransforms();
    }

    void OnDisable()
    {
        _characterModel?.Dispose();
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        OnAudioFilterReadCallback?.Invoke(data, channels);
    }

    /// <summary>
    /// キャラクターの表示を有効にする
    /// </summary>
    public void Enable() => gameObject.SetActive(true);

    /// <summary>
    /// キャラクターの表示を無効にする
    /// </summary>
    public void Disable() => gameObject.SetActive(false);

    /// <summary>
    /// キャラクターにアイドルモーション用のトランスフォームをセットする
    /// </summary>
    public void SetTransformsForIdle()
    {
        var transforms = gameObject.GetComponentsInChildren<Transform>();
        for (var i = 0; i < transforms.Length; i++)
        {
            transforms[i].SetPositionAndRotation(_characterPositions[i], _characterRotations[i]);
        }
    }

    /// <summary>
    /// キャラクターの位置をリセットする
    /// </summary>
    public void ResetPosition() => _characterModel.ResetPosition();

    /// <summary>
    /// キャラクターの回転をリセットする
    /// </summary>
    public void ResetRotation() => _characterModel.ResetRotation();

    /// <summary>
    /// キャラクターの位置をセットする
    /// </summary>
    /// <param name="pos"></param>
    public void SetPosition(Vector3 pos) => _characterModel.SetPosition(pos);

    /// <summary>
    /// キャラクターの回転をセットする
    /// </summary>
    /// <param name="rot"></param>
    public void SetRotation(Quaternion rot) => _characterModel.SetRotation(rot);

    /// <summary>
    /// キャラクターを移動させる
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="time"></param>
    public void MovePosition(Vector3 pos, float time) => _characterModel.MovePosition(pos, time);

    /// <summary>
    /// キャラクターのアニメーションをリセットする
    /// </summary>
    public void ResetAnimation() => _characterModel.ResetAnimation();

    /// <summary>
    /// キャラクターのアニメーションを有効化
    /// </summary>
    public void EnableAnimation() => _characterModel.EnableAnimation();

    /// <summary>
    /// キャラクターのアニメーションを無効化
    /// </summary>
    public void DisableAnimation() => _characterModel.DisableAnimation();

    /// <summary>
    /// アニメーションを変更する
    /// </summary>
    /// <param name="animationName"></param>
    /// <returns></returns>
    public async UniTask ChangeAnimation(AnimationType animationType) => await _characterModel.ChangeAnimation(animationType);

    /// <summary>
    /// 表情を変更する
    /// </summary>
    /// <param name="facialType"></param>
    public async UniTask ChangeFacial(FacialType facialType, int msec)
    {
        // 自動目パチ停止
        _autoBlinkEX?.Pause();

        await _characterModel.ChangeFacial(facialType, msec);

        _autoBlinkEX?.Play();
    }

    /// <summary>
    /// 顔座標更新
    /// </summary>
    /// <param name="faceInfo"></param>
    public void FaceUpdate(FaceInfoManager.FaceInfo faceInfo) => _characterModel.FaceUpdate(faceInfo);

    /// <summary>
    /// 手動口パク開始
    /// </summary>
    public void StartAutoLipSync()
    {
        //Debug.Log("StartAutoLipSync");
        if (GlobalState.Instance.CurrentCharacterModel.Value == GlobalState.CharacterModel.Una2D)
        {
            _mouthController.MouthOpening = 0;
            _mouthController.enabled = true;
            _autoMouthInput.enabled = true;
        }
        else if (GlobalState.Instance.CurrentCharacterModel.Value == GlobalState.CharacterModel.Una3D)
        {
            _autoLipSync.enabled = true;
        }
    }

    /// <summary>
    /// 手動口パク停止
    /// </summary>
    public void StopAutoLipSync()
    {
        //Debug.Log("StopAutoLipSync");
        if (GlobalState.Instance.CurrentCharacterModel.Value == GlobalState.CharacterModel.Una2D)
        {
            _mouthController.enabled = false;
            _autoMouthInput.enabled = false;
            _mouthController.MouthOpening = 0;
        }
        else if (GlobalState.Instance.CurrentCharacterModel.Value == GlobalState.CharacterModel.Una3D)
        {
            _autoLipSync.enabled = false;
            _autoLipSync.ResetLipSync();
        }
    }

    public void SetMouseOpenYParameter(float value)
    {
        if (GlobalState.Instance.CurrentCharacterModel.Value == GlobalState.CharacterModel.Una2D)
        {
            _mouthOpenYValue = value;
        }
    }

    private void GetTransforms()
    {
        var transforms = gameObject.GetComponentsInChildren<Transform>();
        _characterPositions = new Vector3[transforms.Length];
        _characterRotations = new Quaternion[transforms.Length];
        for (var i = 0; i < transforms.Length; i++)
        {
            _characterPositions[i] = transforms[i].transform.position;
            _characterRotations[i] = transforms[i].transform.rotation;
        }
    }
}
