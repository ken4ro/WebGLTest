using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アニメーション種別
/// </summary>
public enum AnimationType
{
    Idle,
    Ojigi,
    Nod,
    What,
    Here,
    HereIdle,
    HereReturn,
}

/// <summary>
/// 表情種別
/// </summary>
public enum FacialType
{
    Normal,
    Smile,
    Angry,
    Sad,
    Shy,
    Sulky,
    Cry,
    Surprised,
    Panicked,
    Puzzled,
    AngryAnime,
    CryAnime,
    SurprisedAnime,
}

public interface ICharacterModel
{
    // 初期配置位置
    Vector3 DefaultPosition { get; }

    // 初期配置クォータニオン
    Quaternion DefaultRotation { get; }

    // 左目のブレンドシェイプキー
    int LeftEyeKey { get; }

    // 右目のブレンドシェイプキー
    int RightEyeKey { get; }

    // 口のブレンドシェイプキー
    int MouthKey { get; }

    /// <summary>
    /// 解放
    /// </summary>
    void Dispose();

    /// <summary>
    /// 位置リセット
    /// </summary>
    void ResetPosition();

    /// <summary>
    /// 回転リセット
    /// </summary>
    void ResetRotation();

    /// <summary>
    /// 位置セット
    /// </summary>
    void SetPosition(Vector3 pos);

    /// <summary>
    /// 回転セット
    /// </summary>
    void SetRotation(Quaternion rot);

    /// <summary>
    /// 位置移動
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="time"></param>
    void MovePosition(Vector3 pos, float time);

    /// <summary>
    /// アニメーションリセット
    /// </summary>
    void ResetAnimation();

    /// <summary>
    /// アニメーション有効化
    /// </summary>
    void EnableAnimation();

    /// <summary>
    /// アニメーション無効化
    /// </summary>
    void DisableAnimation();

    /// <summary>
    /// アニメーターコントローラーをセット
    /// </summary>
    /// <param name="animatorController"></param>
    void SetAnimatorController(RuntimeAnimatorController animatorController);

    /// <summary>
    /// アニメーション切り替え
    /// </summary>
    UniTask ChangeAnimation(AnimationType animationType);

    /// <summary>
    /// フェイシャル切り替え
    /// </summary>
    UniTask ChangeFacial(FacialType facialType, int msec);

    /// <summary>
    /// フェイシャル初期化
    /// </summary>
    void ResetFacial();

    /// <summary>
    /// 顔アニメーション更新
    /// </summary>
    void FaceUpdate(FaceInfoManager.FaceInfo faceInfo);
}
