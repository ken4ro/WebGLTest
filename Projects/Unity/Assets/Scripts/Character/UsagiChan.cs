using System;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;

/// <summary>
/// うさぎちゃんモデル管理クラス
/// </summary>
public class UsagiChan : ALive2DCharacterModel
{
    // うさぎちゃんパラメーターリスト
    public enum UsagiParameter
    {
        ANGLE_X,        // 顔の向き(横)
        ANGLE_Y,        // 顔の向き(縦)
        ANGLE_Z,        // 首の傾き
        MOUTH_FORM,     // 口の形状？
        MOUTH_OPEN_Y,   // 口の開き具合
        CHEEK,
        BROW_L_FORM,    // 左眉毛の形状？
        BROW_R_FORM,    // 右眉毛の形状？
        EYE_BALL_X,     // 視線(横)
        EYE_BALL_Y,     // 視線(縦)
        EYE_R_SMILE,
        EYE_R_OPEN,     // 右目の開き具合
        EYE_L_SMILE,
        EYE_L_OPEN,     // 左目の開き具合
        HAIR_FRONT,     // 前髪の揺れ具合
        HAIR_SIDE,
        HAIR_BACK,      // 後髪の揺れ具合
        BODY_ANGLE_X,   // 体の向き(横)
        BODY_ANGLE_Y,
        BODY_ANGLE_Z,   // 体の傾き
        BODY_ANGLE_XZ,
        BREATH,         // 呼吸
        ARM_L,
        ARM_R,          // 右手
        PARAM,          // ?
    }

    public override int LeftEyeKey => throw new NotImplementedException();

    public override int RightEyeKey => throw new NotImplementedException();

    public override int MouthKey => throw new NotImplementedException();

    private CubismModel _model = null;

    public UsagiChan(GameObject parent, CubismModel model, Animator animator, RuntimeAnimatorController motionController, RuntimeAnimatorController facialController, Transform parameters, CubismEyeBlinkController eyeBlinkController) : base(parent, model, animator, motionController, facialController, parameters, eyeBlinkController)
    {
        _model = model;
    }

    public override void FaceUpdate(FaceInfoManager.FaceInfo faceInfo)
    {
        // 顔情報取得
        var yaw = faceInfo.Yaw;
        var pitch = faceInfo.Pitch;
        var roll = faceInfo.Roll;
        var bodyYaw = faceInfo.BodyYaw;
        var bodyPitch = faceInfo.BodyPitch;
        var bodyRoll = faceInfo.BodyRoll;

        _model.Parameters[(int)UsagiParameter.ANGLE_X].BlendToValue(CubismParameterBlendMode.Override, (float)yaw);
        _model.Parameters[(int)UsagiParameter.ANGLE_Y].BlendToValue(CubismParameterBlendMode.Override, (float)pitch);
        _model.Parameters[(int)UsagiParameter.ANGLE_Z].BlendToValue(CubismParameterBlendMode.Override, (float)roll);
        _model.Parameters[(int)UsagiParameter.BODY_ANGLE_X].BlendToValue(CubismParameterBlendMode.Override, (float)bodyYaw);
        _model.Parameters[(int)UsagiParameter.BODY_ANGLE_Y].BlendToValue(CubismParameterBlendMode.Override, (float)bodyPitch);
        _model.Parameters[(int)UsagiParameter.BODY_ANGLE_Z].BlendToValue(CubismParameterBlendMode.Override, (float)bodyRoll);
    }

#if false
    private void Smile()
    {
        SetValues("ParamEyeLOpen", 0f);
        SetValues("ParamEyeROpen", 0f);
        SetValues("ParamEyeLSmile", 1f);
        SetValues("ParamEyeRSmile", 1f);
        SetValues("ParamMouthForm", 1f);

        _eyeBlinkController.enabled = false;

        ModelUpdate();
    }

    private void Komaru()
    {
        SetValues("ParamEyeLOpen", 1f);
        SetValues("ParamEyeROpen", 1f);
        SetValues("ParamBrowLForm", -1f);
        SetValues("ParamBrowRForm", -1f);
        SetValues("ParamMouthForm", -1f);

        _eyeBlinkController.enabled = true;

        ModelUpdate();
    }

    private void Normal()
    {
        SetValues("ParamEyeLOpen", 1f);
        SetValues("ParamEyeROpen", 1f);
        SetValues("ParamEyeLSmile", 0f);
        SetValues("ParamEyeRSmile", 0f);
        SetValues("ParamBrowLForm", 0f);
        SetValues("ParamBrowRForm", 0f);
        SetValues("ParamMouthForm", 1f);

        _eyeBlinkController.enabled = true;

        ModelUpdate();
    }
#endif
}
