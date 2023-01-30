using System;
using UnityEngine;

/// <summary>
/// グローバルステート管理
/// </summary>
public class GlobalState : SingletonBase<GlobalState>
{
    /// <summary>
    /// 処理一覧
    /// </summary>
    public enum Process
    {
        Waiting,
        Starting,
        Loading,
        LoadingComplete,
        LoadingError,
        Speakable,
        Speaking,
        SpeakingComplete,
        DisConnect,
        PreOperating,
        Operating,
    }

    /// <summary>
    /// キャラクターモデル種別
    /// </summary>
    public enum CharacterModel
    {
        Maru,
        Usagi,
        Una3D,
        Una2D,
        Una2D_Rugby,
    }

    /// <summary>
    /// アプリケーション設定
    /// </summary>
    public ApplicationSettings ApplicationGlobalSettings
    {
        get { return _applicationGlobalSettings; }
        set
        {
            if (value == null)
            {
                Debug.LogError("設定ファイルが正しく読み込まれませんでした");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
                UnityEngine.Application.Quit();
#endif
                return;
            }

            _applicationGlobalSettings = value;
        }
    }
    private ApplicationSettings _applicationGlobalSettings;

    /// <summary>
    /// 現在のキャラクターモデル
    /// </summary>
    public CharacterModel CurrentCharacterModel { get; set; } = CharacterModel.Una2D;
}
