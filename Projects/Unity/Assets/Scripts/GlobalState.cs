using System;
using UnityEngine;
using UniRx;

/// <summary>
/// グローバルステート管理
/// </summary>
public class GlobalState : SingletonBase<GlobalState>
{
    /// <summary>
    /// 状態一覧
    /// </summary>
    public enum State
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
    /// 接続種別
    /// </summary>
    public enum ConnectionType
    {
        WebRTC,
        Local,
    }

    /// <summary>
    /// Bot リクエストモード
    /// </summary>
    public enum BotRequestMode
    {
        Dictionary,
        Translation
    }

    /// <summary>
    /// Bot リクエスト手法
    /// </summary>
    public enum BotRequestMethod
    {
        Both,   // 音声＆ボタン
        Button, // ボタンのみ
    }

    /// <summary>
    /// 受付中アニメーション種別
    /// </summary>
    public enum AcceptableAnimationType
    {
        Simple,
        Pac,
        Cocoa,
    }

    /// <summary>
    /// 使用ボイス種別
    /// </summary>
    public enum VoiceType
    {
        Azure,
        Google,
        Hoya,
        Local
    }

    /// <summary>
    /// アプリケーション共通エラーコード
    /// </summary>
    public enum ErrorCode
    {
        Network = 100,              // ネットワーク
        ReplUnauthorized = 200,     // Repl 認証エラー
        ReplBadRequest,             // Repl 不正なリクエスト
        ReplUnrecognizedResponse,   // Repl レスポンス解析エラー
    }

    /// <summary>
    /// ネットワーク不良時エラーテキスト
    /// </summary>
    public static readonly string NetworkErrorText = "通信がタイムアウトしました。" + Environment.NewLine + "インターネット接続が有効であるか確認した後、リトライしてください。";

    /// <summary>
    /// Repl シナリオ関連エラーテキスト
    /// </summary>
    public static readonly string ReplErrorText = "シナリオの初期化に失敗しました。";

    /// <summary>
    /// ユーザー設定
    /// </summary>
    public UserSettings UserSettings { get; set; }

    /// <summary>
    /// 現在のキャラクターモデル
    /// </summary>
    public ReactiveProperty<CharacterModel> CurrentCharacterModel { get; set; } = new ReactiveProperty<CharacterModel>(CharacterModel.Una3D);

    /// <summary>
    /// 現在のモード
    /// </summary>
    public ReactiveProperty<BotRequestMode> CurrentBotRequestMode { get; set; } = new ReactiveProperty<BotRequestMode>(BotRequestMode.Dictionary);

    /// <summary>
    /// 現在の状態
    /// </summary>
    public ReactiveProperty<State> CurrentState { get; set; } = new ReactiveProperty<State>(State.Waiting);

    /// <summary>
    /// 現在の接続種別
    /// </summary>
    public ReactiveProperty<ConnectionType> CurrentConnectionType { get; set; }

    /// <summary>
    /// 現在の Bot リクエスト手法
    /// </summary>
    public BotRequestMethod CurrentBotRequestMethod { get; set; } = BotRequestMethod.Both;

    /// <summary>
    /// 現在の受付中アニメーション種別
    /// </summary>
    public AcceptableAnimationType CurrentAcceptableAnimationType { get; set; } = AcceptableAnimationType.Simple;

    /// <summary>
    /// 現在の音源
    /// </summary>
    public VoiceType CurrentVoiceType { get; set; } = VoiceType.Google;

    /// <summary>
    /// ビデオ解像度
    /// </summary>
    public VideoResolutionWH VideoResolution { get; set; }
    public struct VideoResolutionWH
    {
        public int Width;
        public int Height;
    }

    /// <summary>
    /// トップに戻れる状態か
    /// </summary>
    public bool CanReturnToTop { get; set; } = false;
}
