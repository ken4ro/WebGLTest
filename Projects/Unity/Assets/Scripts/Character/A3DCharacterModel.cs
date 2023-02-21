using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

/// <summary>
/// 3Dキャラクターモデル抽象基底クラス
/// </summary>
public abstract class A3DCharacterModel : ICharacterModel
{
    /// <summary>
    /// 初期配置位置
    /// </summary>
    public Vector3 DefaultPosition { get; set; }

    /// <summary>
    /// 初期配置クォータニオン
    /// </summary>
    public Quaternion DefaultRotation { get; set; }

    /// <summary>
    /// 左目のブレンドシェイプキー
    /// </summary>
    public abstract int LeftEyeKey { get; }

    /// <summary>
    /// 右目のブレンドシェイプキー
    /// </summary>
    public abstract int RightEyeKey { get; }

    /// <summary>
    /// 口のブレンドシェイプキー
    /// </summary>
    public abstract int MouthKey { get; }

    // モーションコントローラー用トリガーテーブル
    protected static readonly Dictionary<AnimationType, string> _motionControllerTriggerTable = new Dictionary<AnimationType, string>
    {
        [AnimationType.Idle] = "",
        [AnimationType.Ojigi] = "Ojigi",
        [AnimationType.Nod] = "Nod",
        [AnimationType.What] = "What",
        [AnimationType.Here] = "Here",
        [AnimationType.HereIdle] = "",
        [AnimationType.HereReturn] = "HereReturn",
    };

    // フェイシャルコントローラー用トリガーテーブル
    protected static readonly Dictionary<FacialType, string> _facialControllerTriggerTable = new Dictionary<FacialType, string>
    {
        [FacialType.Normal] = "",
        [FacialType.Smile] = "Smile",
        [FacialType.Shy] = "Shy",
        [FacialType.Angry] = "Angry",
        [FacialType.Sulky] = "Sulky",
        [FacialType.Sad] = "Sad",
        [FacialType.Cry] = "Cry",
        [FacialType.Surprised] = "Surprised",
        [FacialType.Panicked] = "Panicked",
        [FacialType.Puzzled] = "Puzzled",
        [FacialType.AngryAnime] = "AngryAnime",
        [FacialType.CryAnime] = "CryAnime",
        [FacialType.SurprisedAnime] = "SurprisedAnime",
    };

    // ブレンドシェイプキー
    protected virtual int[] NormalKey { get; } = { 0 };
    protected abstract int[] SmileKey { get; }
    protected abstract int[] ShyKey { get; }
    protected abstract int[] AngryKey { get; }
    protected abstract int[] SulkyKey { get; }
    protected abstract int[] SadKey { get; }
    protected abstract int[] CryKey { get; }
    protected abstract int[] SurprisedKey { get; }
    protected abstract int[] PanickedKey { get; }
    protected abstract int[] PuzzledKey { get; }
    protected abstract int[] AngryAnimeKey { get; }
    protected abstract int[] CryAnimeKey { get; }
    protected abstract int[] SurprisedAnimeKey { get; }

    // 首ボーンの初期回転角
    protected abstract Vector3 DefaultNeckRotation { get; set; }

    // 身体ボーンの初期回転角
    protected abstract Vector3 DefaultBodyRotation { get; set; }

    private Dictionary<FacialType, int[]> _facialTable = null;
    private GameObject _parent = null;
    private Animator _animator = null;
    private RuntimeAnimatorController _motionController = null;
    private RuntimeAnimatorController _facialController = null;
    private RuntimeAnimatorController _preController = null;
    private Dictionary<AnimationType, float> _motionClipsTimeTable = new Dictionary<AnimationType, float>();
    private Dictionary<FacialType, float> _facialClipsTimeTable = new Dictionary<FacialType, float>();
    private SkinnedMeshRenderer _facial = null;
    private Transform _neck = null;
    private Transform _body = null;
    private Vector3 _neckRotateAngles = new Vector3();
    private Vector3 _bodyRotateAngles = new Vector3();
    private Vector3 _diffNeckRotateAngles = new Vector3();
    private Vector3 _diffBodyRotateAngles = new Vector3();
    private IDisposable _rotateFaceAnimation = null;

    public A3DCharacterModel(GameObject parent, Animator animator, RuntimeAnimatorController motionController, RuntimeAnimatorController facialController, SkinnedMeshRenderer facial, Transform neck, Transform body)
    {
        _parent = parent;
        _animator = animator;
        _facial = facial;
        _neck = neck;
        _body = body;

        _motionController = motionController;
        _facialController = facialController;

        _preController = _motionController;

        DefaultPosition = _parent.transform.position;
        DefaultRotation = _parent.transform.rotation;

        _facialTable = new Dictionary<FacialType, int[]>
        {
            [FacialType.Normal] = NormalKey,
            [FacialType.Smile] = SmileKey,
            [FacialType.Shy] = ShyKey,
            [FacialType.Angry] = AngryKey,
            [FacialType.Sulky] = SulkyKey,
            [FacialType.Sad] = SadKey,
            [FacialType.Cry] = CryKey,
            [FacialType.Surprised] = SurprisedKey,
            [FacialType.Panicked] = PanickedKey,
            [FacialType.Puzzled] = PuzzledKey,
            [FacialType.AngryAnime] = AngryAnimeKey,
            [FacialType.CryAnime] = CryAnimeKey,
            [FacialType.SurprisedAnime] = SurprisedAnimeKey,
        };

        // アニメーション時間を取得しておく
        var motionClips = _motionController.animationClips;
        for (var i = 0; i < motionClips.Length; i++)
        {
            _motionClipsTimeTable[(AnimationType)i] = motionClips[i].length;
        }
        var facialClips = _facialController.animationClips;
        for (var i = 0; i < facialClips.Length; i++)
        {
            _facialClipsTimeTable[(FacialType)i] = facialClips[i].length;
        }

        // 各ボーンの回転角を初期位置にセット
        _diffNeckRotateAngles.x = DefaultNeckRotation.x;
        _diffNeckRotateAngles.y = DefaultNeckRotation.y;
        _diffNeckRotateAngles.z = DefaultNeckRotation.z;
        _diffBodyRotateAngles.x = DefaultBodyRotation.x;
        _diffBodyRotateAngles.y = DefaultBodyRotation.y;
        _diffBodyRotateAngles.z = DefaultBodyRotation.z;
    }

    public void Dispose()
    {
        _rotateFaceAnimation?.Dispose();
    }

    public void ResetPosition()
    {
        _parent.transform.position = DefaultPosition;
    }

    public void ResetRotation()
    {
        _parent.transform.rotation = DefaultRotation;
    }

    public void SetPosition(Vector3 pos)
    {
        _parent.transform.position = pos;
    }

    public void SetRotation(Quaternion rot)
    {
        _parent.transform.rotation = rot;
    }

    public void MovePosition(Vector3 pos, float time)
    {
        var currentPos = _parent.transform.position;
        DOTween.To(() => _parent.transform.position, value => _parent.transform.position = value, pos, time);
    }

    public void ResetAnimation()
    {
    }

    public void EnableAnimation()
    {
        _animator.runtimeAnimatorController = _preController;
    }

    public void DisableAnimation()
    {
        _animator.runtimeAnimatorController = null;
    }

    public void SetAnimatorController(RuntimeAnimatorController animatorController)
    {
        _animator.runtimeAnimatorController = animatorController;
        _preController = animatorController;
    }

    public async UniTask ChangeAnimation(AnimationType animationType)
    {
        if (_animator.runtimeAnimatorController != _motionController)
        {
            SetAnimatorController(_motionController);
        }

        if (!_motionControllerTriggerTable.ContainsKey(animationType) || !_motionClipsTimeTable.ContainsKey(animationType))
        {
            Debug.Log($"Character animation not found: {animationType.ToString()}");
            return;
        }

        // アニメーション遷移
        _animator.SetTrigger(_motionControllerTriggerTable[animationType]);

        // 遷移後のアニメーションが終了するまで待機
        var delayTime = (int)(_motionClipsTimeTable[animationType] * 1000);
        await UniTask.Delay(delayTime);
    }

    public async UniTask ChangeFacial(FacialType facialType, int msec)
    {
        ResetFacialKey();

        if (facialType == FacialType.Angry)
        {
            facialType = FacialType.AngryAnime;
        }
        else if (facialType == FacialType.Sad)
        {
            facialType = FacialType.CryAnime;
        }

        SetFacialKey(facialType);

        await UniTask.Delay(msec);

        ResetFacialKey();
    }

    public void ResetFacial()
    {
        ResetFacialKey();
    }

    public void FaceUpdate(FaceInfoManager.FaceInfo faceInfo)
    {
        // 顔情報取得
        var pitch = faceInfo.Pitch;
        var yaw = faceInfo.Yaw;
        var roll = faceInfo.Roll;
        var bodyPitch = faceInfo.BodyPitch;
        var bodyYaw = faceInfo.BodyYaw;
        var bodyRoll = faceInfo.BodyRoll;

        // 顔回転
        _neckRotateAngles.x = (float)yaw;
        _neckRotateAngles.z = (float)pitch;
        _neckRotateAngles.y = (float)roll;
        _neck.DOComplete();
        _neck.DOLocalRotateQuaternion(Quaternion.Euler(_neckRotateAngles), 0.1f);

        // 身体回転
        _bodyRotateAngles.x = (float)bodyYaw;
        _bodyRotateAngles.z = (float)bodyPitch;
        _bodyRotateAngles.y = (float)bodyRoll;
        _body.DOComplete();
        _body.DOLocalRotateQuaternion(Quaternion.Euler(_bodyRotateAngles), 0.1f);
    }

    private void ResetFacialKey()
    {
        foreach (var keys in _facialTable.Values)
        {
            foreach (var key in keys)
            {
                _facial.SetBlendShapeWeight(key, 0);
            }
        }
    }

    private void SetFacialKey(FacialType type)
    {
        var keys = _facialTable[type];
        if (keys.Length == 0)
        {
            // 未実装 or 設定漏れ
            Debug.LogError($"Facial key \"{type.ToString()}\" is not implemented.");
            return;
        }
        foreach (var key in keys)
        {
            _facial.SetBlendShapeWeight(key, 100);
        }
    }
}
