using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Motion;

public class UnaChan2DRugbyModel : ALive2DCharacterModel
{
    // うなちゃんパラメーターリスト
    public enum UnaRugbyParameter
    {
        ParamBreath,
        SMILE,
        MAYU_SAD,
        MAYU_ANGRY,
        ParamEyeLOpen,
        ParamEyeROpen,
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
        Param45,
        Param13,
        ParamAngleX,
        ParamAngleY,
        ParamAngleZ,
        Param46,
        ParamBodyAngleX,
        ParamBodyAngleY,
        ParamBodyAngleZ,
        Param38,
        Param28,
        Param39,
        Param42,
        Param40,
        Param41,
        Param49,
        TOUMEI_ARM_R,
        Param18,
        Param19,
        Param43,
        Param44,
        Param26,
        PARAM_ARM_R,
        Param20,
        Param21,
        Param32,
        Param16,
        Param11,
        Param17,
        Param22,
        Param24,
        Param47,
        Param54,
        TOUMEI_ARM_L,
        Param14,
        Param15,
        Param2,
        Param3,
        Param27,
        PARAM_ARM_L,
        Param33,
        Param34,
        Param29,
        Param35,
        Param36,
        Param37,
        Param30,
        Param31,
        Param48,
        Param53,
        Param12,
        Param23,
        Param8,
        Param9,
        Param10,
        ParamHairFront,
        Param,
        ParamHairSide,
        Param4,
        Param5,
        ParamHairBack,
        Param6,
        Param7,
        Param25,
    }

    public override int LeftEyeKey => throw new System.NotImplementedException();

    public override int RightEyeKey => throw new System.NotImplementedException();

    public override int MouthKey => throw new System.NotImplementedException();

    private CubismModel _model = null;

    public UnaChan2DRugbyModel(GameObject parent, CubismModel model, Animator animator, RuntimeAnimatorController motionController, RuntimeAnimatorController facialController, Transform parameters, CubismEyeBlinkController eyeBlinkController) : base(parent, model, animator, motionController, facialController, parameters, eyeBlinkController)
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

        _model.Parameters[(int)UnaRugbyParameter.ParamAngleX].BlendToValue(CubismParameterBlendMode.Override, (float)yaw);
        _model.Parameters[(int)UnaRugbyParameter.ParamAngleY].BlendToValue(CubismParameterBlendMode.Override, (float)pitch);
        _model.Parameters[(int)UnaRugbyParameter.ParamAngleZ].BlendToValue(CubismParameterBlendMode.Override, (float)roll);
        _model.Parameters[(int)UnaRugbyParameter.ParamBodyAngleX].BlendToValue(CubismParameterBlendMode.Override, (float)bodyYaw);
        _model.Parameters[(int)UnaRugbyParameter.ParamBodyAngleY].BlendToValue(CubismParameterBlendMode.Override, (float)bodyPitch);
        _model.Parameters[(int)UnaRugbyParameter.ParamBodyAngleZ].BlendToValue(CubismParameterBlendMode.Override, (float)bodyRoll);
    }
}
