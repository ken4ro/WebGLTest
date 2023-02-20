using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Motion;

public class UnaChan2DModel : ALive2DCharacterModel
{
    // うなちゃんパラメーターリスト
    public enum UnaParameter
    {
        Param13,
        ParamAngleX,
        ParamAngleY,
        ParamAngleZ,
        SMILE,
        MAYU_SAD,
        MAYU_ANGRY,
        ParamEyeLOpen,
        ParamEyeLSmile,
        ParamEyeROpen,
        ParamEyrRSmile,
        ParamCheek,
        ParamEyeBallX,
        ParamEyeBallY,
        ParamBrowLY,
        ParamBrowRY,
        ParamBrowLAngle,
        ParamBrowRAngle,
        ParamMouthForm,
        ParamMouthOpenY,
        MOUTH_UP_DOWN,
        ParamBodyAngleX,
        ParamBodyAngleY,
        ParamBodyAngleZ,
        ParamBreath,
    }

    public override int LeftEyeKey => throw new System.NotImplementedException();

    public override int RightEyeKey => throw new System.NotImplementedException();

    public override int MouthKey => throw new System.NotImplementedException();

    private CubismModel _model = null;

    public UnaChan2DModel(GameObject parent, CubismModel model, Animator animator, RuntimeAnimatorController motionController, RuntimeAnimatorController facialController, Transform parameters, CubismEyeBlinkController eyeBlinkController) : base(parent, model, animator, motionController, facialController, parameters, eyeBlinkController)
    {
        _model = model;
    }

    public override void FaceUpdate(FaceInfoManager.FaceInfo faceInfo)
    {
        // 顔情報取得
        var yaw = faceInfo.Yaw;
        var pitch = -1 * faceInfo.Pitch;
        var roll = faceInfo.Roll;
        var bodyYaw = faceInfo.BodyYaw;
        var bodyPitch = -1 * faceInfo.BodyPitch;
        var bodyRoll = faceInfo.BodyRoll;

        _model.Parameters[(int)UnaParameter.ParamAngleX].BlendToValue(CubismParameterBlendMode.Override, (float)yaw);
        _model.Parameters[(int)UnaParameter.ParamAngleY].BlendToValue(CubismParameterBlendMode.Override, (float)pitch);
        _model.Parameters[(int)UnaParameter.ParamAngleZ].BlendToValue(CubismParameterBlendMode.Override, (float)roll);
        _model.Parameters[(int)UnaParameter.ParamBodyAngleX].BlendToValue(CubismParameterBlendMode.Override, (float)bodyYaw);
        _model.Parameters[(int)UnaParameter.ParamBodyAngleY].BlendToValue(CubismParameterBlendMode.Override, (float)bodyPitch);
        _model.Parameters[(int)UnaParameter.ParamBodyAngleZ].BlendToValue(CubismParameterBlendMode.Override, (float)bodyRoll);
    }
}
