using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Cysharp.Threading.Tasks;
using System.IO;

public abstract class ALive2DCharacterModel : ICharacterModel
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

    private GameObject _parent = null;
    private CubismModel _model = null;
    private Animator _animator = null;
    private Transform _parameters = null;
    private CubismEyeBlinkController _eyeBlinkController = null;
    private RuntimeAnimatorController _motionController = null;
    private RuntimeAnimatorController _facialController = null;
    private RuntimeAnimatorController _preController = null;
    private Dictionary<AnimationType, float> _motionClipsTimeTable = new Dictionary<AnimationType, float>();
    private Dictionary<FacialType, float> _facialClipsTimeTable = new Dictionary<FacialType, float>();

    public ALive2DCharacterModel(GameObject parent, CubismModel model, Animator animator, RuntimeAnimatorController motionController, RuntimeAnimatorController facialController, Transform parameters, CubismEyeBlinkController eyeBlinkController)
    {
        _parent = parent;
        _model = model;
        _animator = animator;
        _motionController = motionController;
        _facialController = facialController;
        _parameters = parameters;
        _eyeBlinkController = eyeBlinkController;

        _preController = _motionController;

        DefaultPosition = _parent.transform.position;
        DefaultRotation = _parent.transform.rotation;

        // アニメーション時間を取得しておく
        var motionClips = _motionController.animationClips;
        for (var i = 0; i < motionClips.Length; i++)
        {
            if (Enum.TryParse(Path.GetFileNameWithoutExtension(motionClips[i].name), true, out AnimationType result))
            {
                _motionClipsTimeTable[(AnimationType)i] = motionClips[i].length;
            }
            else
            {
                Debug.LogError($"Invalid motion name: {motionClips[i].name}");
            }
        }
        var facialClips = _facialController.animationClips;
        for (var i = 0; i < facialClips.Length; i++)
        {
            if (Enum.TryParse(Path.GetFileNameWithoutExtension(facialClips[i].name), true, out FacialType result))
            {
                _facialClipsTimeTable[(FacialType)i] = facialClips[i].length;
            }
            else
            {
                Debug.LogError($"Invalid facial name: {facialClips[i].name}");
            }
        }
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
        _eyeBlinkController.enabled = false;

        if (_animator.runtimeAnimatorController != _facialController)
        {
            SetAnimatorController(_facialController);
        }

        // アニメーション遷移
        _animator.SetTrigger(_facialControllerTriggerTable[facialType]);

        // 遷移後のアニメーションが終了するまで待機
        var delayTime = (int)(_facialClipsTimeTable[facialType] * 1000);
        await UniTask.Delay(delayTime);

        _eyeBlinkController.enabled = true;
    }

    public void ResetFacial()
    {
    }

    public void Dispose()
    {
    }

    public void ResetAnimation()
    {
    }

    public void EnableAnimation()
    {
        _animator.runtimeAnimatorController = _preController;
        _eyeBlinkController.BlendMode = CubismParameterBlendMode.Multiply;
    }

    public void DisableAnimation()
    {
        _animator.runtimeAnimatorController = null;
        _eyeBlinkController.BlendMode = CubismParameterBlendMode.Override;
    }

    public void SetAnimatorController(RuntimeAnimatorController animatorController)
    {
        _animator.runtimeAnimatorController = animatorController;
        _preController = animatorController;
    }

    public abstract void FaceUpdate(FaceInfoManager.FaceInfo faceInfo);

    public void MovePosition(Vector3 pos, float time)
    {
        var currentPos = _parent.transform.position;
        DOTween.To(() => _parent.transform.position, value => _parent.transform.position = value, pos, time);
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

    private void SetValues(string paramName, float value)
    {
        var param = _parameters.transform.Find(paramName);
        var paramScript = param.GetComponent<CubismParameter>();
        paramScript.Value = value;
    }

    private void ModelUpdate()
    {
        // アップデート
        _model.ForceUpdateNow();
    }
}
