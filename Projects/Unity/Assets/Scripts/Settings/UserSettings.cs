using System;
using UnityEngine;

public enum AvatarType
{
    Una2D,
    Una3D,
}

/// <summary>
/// ユーザー設定クラス
/// </summary>
public class UserSettings
{
    public string LoginId { get; set; }
    public string LoginType { get; set; }
    public string Password { get; set; }
    public string UserToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string GoogleKey { get; set; }
    public UserSettingsUI UI { get; set; }
    public UserSettingsBot Bot { get; set; }
    public UserSettingsRtc Rtc { get; set; }

    public UserSettings() { }
}

public class UserSettingsUI
{
    public string RequestType;
    public int FontSize;
    public string WaitAnimationType;
    public string RecordingAgreementEnable;
    public string ScreensaverEnable;
    public int TextSpeed;
    public int InputLimitSec;
    public string[] Languages;
}

public class UserSettingsBot
{
    public string ServiceType;
    public int StartDelaySec;
    public int RestartSec;
    public int ReturnSec;
    public int ActionDelaySec;
    public string VoiceType;
    public string CcgFlowId;
}

public class UserSettingsRtc
{
    public string ServiceType;
}