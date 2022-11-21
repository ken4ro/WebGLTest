using System;

using UnityEngine;


/// <summary>
/// アプリケーション設定クラス
/// </summary>
[Serializable]
public class ApplicationSettings : ISerializationCallbackReceiver
{
    /// <summary>
    /// アプリケーション設定ファイルパス
    /// </summary>
    [NonSerialized]
    public static readonly string ApplicationSettingFilePath = "ApplicationSettings.json";

    public enum AudioCodecType
    {
        MuLaw,
        G722,
    }

    public enum VideoCodecType
    {
        MJPEG,
        MWEBP,
        H264,
    }

    [NonSerialized]
    public int StartOffsetSec;
    [NonSerialized]
    public string CompanyCode;
    [NonSerialized]
    public string FileServer;
    [NonSerialized]
    public string SignalingServer;
    [NonSerialized]
    public string SignalingUserName;
    [NonSerialized]
    public string SignalingUserPassword;
    [NonSerialized]
    public string ConnectionType;
    [NonSerialized]
    public string WebRtcApiKey;
    [NonSerialized]
    public string BotRequestMethod;
    [NonSerialized]
    public int FontSize;
    [NonSerialized]
    public string AcceptableAnimation;
    [NonSerialized]
    public string EnableRecordingAgreement;
    [NonSerialized]
    public AudioCodecType AudioCodec;
    [NonSerialized]
    public VideoCodecType VideoCodec;
    [NonSerialized]
    public string VideoResolution;

    [SerializeField]
    private int start_offset_sec;
    [SerializeField]
    private string character_model;
    [SerializeField]
    private string company_code;
    [SerializeField]
    private string file_server;
    [SerializeField]
    private string signaling_server;
    [SerializeField]
    private string signaling_user_name;
    [SerializeField]
    private string signaling_user_password;
    [SerializeField]
    private string connection_type;
    [SerializeField]
    private string webrtc_api_key;
    [SerializeField]
    private string bot_request_method;
    [SerializeField]
    private int font_size;
    [SerializeField]
    private string acceptable_animation;
    [SerializeField]
    private string enable_recording_agreement;
    [SerializeField]
    private string audio_codec;
    [SerializeField]
    private string video_codec;
    [SerializeField]
    private string video_resolution;

    public void OnBeforeSerialize()
    {
        start_offset_sec = StartOffsetSec;
        company_code = CompanyCode;
        file_server = FileServer;
        signaling_server = SignalingServer;
        signaling_user_name = SignalingUserName;
        signaling_user_password = SignalingUserPassword;
        connection_type = ConnectionType;
        webrtc_api_key = WebRtcApiKey;
        bot_request_method = BotRequestMethod;
        font_size = FontSize;
        acceptable_animation = AcceptableAnimation;
        enable_recording_agreement = EnableRecordingAgreement;
        audio_codec = AudioCodec.ToString().ToLower();
        video_codec = VideoCodec.ToString().ToLower();
    }

    public void OnAfterDeserialize()
    {
        StartOffsetSec = start_offset_sec;
        CompanyCode = company_code;
        FileServer = file_server;
        SignalingServer = signaling_server;
        SignalingUserName = signaling_user_name;
        SignalingUserPassword = signaling_user_password;
        ConnectionType = connection_type;
        WebRtcApiKey = webrtc_api_key;
        BotRequestMethod = bot_request_method;
        FontSize = font_size;
        AcceptableAnimation = acceptable_animation;
        EnableRecordingAgreement = enable_recording_agreement;
        if (Enum.TryParse(audio_codec, true, out AudioCodecType audioCodec))
        {
            AudioCodec = audioCodec;
        }
        if (Enum.TryParse(video_codec, true, out VideoCodecType videoCodec))
        {
            VideoCodec = videoCodec;
        }
        VideoResolution = video_resolution?.ToString().ToLower();
    }

    public ApplicationSettings Clone() => (ApplicationSettings)MemberwiseClone();
}