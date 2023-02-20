using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public static partial class SkyWayService
{
    #region Peers

    public class CreatePeerRequest
    {
        [JsonProperty("key")]
        public string Key;
        [JsonProperty("domain")]
        public string Domain;
        [JsonProperty("peer_id")]
        public string PeerId;
        [JsonProperty("turn")]
        public bool Turn;
    }

    public class CreatePeerResponse
    {
        [JsonProperty("command_type")]
        public string CommandType;
        [JsonProperty("params")]
        public CreatePeerResponseParams Params;
    }

    public class CreatePeerResponseParams
    {
        [JsonProperty("peer_id")]
        public string PeerId;
        [JsonProperty("token")]
        public string Token;
    }

    public class GetPeerEventResponse
    {
        [JsonProperty("event")]
        public string Event;
        [JsonProperty("params")]
        public CreatePeerResponseParams Params;
        [JsonProperty("call_params")]
        public GetPeerEventResponseCallParams CallParams;
        [JsonProperty("data_params")]
        public GetPeerEventResponseDataParams DataParams;
    }

    public class GetPeerEventResponseCallParams
    {
        [JsonProperty("media_connection_id")]
        public string MediaConnectionId;
    }

    public class GetPeerEventResponseDataParams
    {
        [JsonProperty("data_connection_id")]
        public string DataConnectionId;
    }

    public class GetMediaStreamEventResponse
    {
        [JsonProperty("event")]
        public string Event;
        [JsonProperty("stream_options")]
        public GetMediaStreamEventResponseStreamOptions StreamOptions;
        [JsonProperty("close_options")]
        public object CloseOptions;
        [JsonProperty("error_message")]
        public string ErrorMessage;
    }

    public class GetMediaStreamEventResponseStreamOptions
    {
        [JsonProperty("is_video")]
        public bool IsVideo;
        [JsonProperty("stream_params")]
        public OpenPortForMediaStreamResponse StreamParams;
    }

    #endregion Peers

    #region DataChannel

    public class OpenPortForDataChannelResponse
    {
        [JsonProperty("data_id")]
        public string DataId;
        [JsonProperty("port")]
        public int Port;
        [JsonProperty("ip_v4")]
        public string Ipv4;
        [JsonProperty("ip_v6")]
        public string Ipv6;
    }

    public class ConnectRequest
    {
        [JsonProperty("peer_id")]
        public string PeerId;
        [JsonProperty("token")]
        public string Token;
        [JsonProperty("options")]
        public ConnectRequestOptions Options;
        [JsonProperty("target_id")]
        public string TargetId;
        [JsonProperty("params")]
        public ChangeDataChannelSettingRequestFeedParams Params;
        [JsonProperty("redirect_params")]
        public ChangeDataChannelSettingRequestRedirectParams RedirectParams;
    }

    public class ConnectRequestOptions
    {
        [JsonProperty("metadata")]
        public string Metadata;
        [JsonProperty("serialization")]
        public string Serialization;
        [JsonProperty("dcInit")]
        public ConnectRequestOptionsDcInit DcInit;
    }

    public class ConnectRequestOptionsDcInit
    {
        [JsonProperty("ordered")]
        public bool Ordered;
        //[JsonProperty("maxPacketLifeTime")]
        //public int MaxPacketLifeTime;
        [JsonProperty("maxRetransmits")]
        public int MaxRetransmits;
        //[JsonProperty("protocol")]
        //public string Protocol;
        //[JsonProperty("negotiated")]
        //public bool Negotiated;
        //[JsonProperty("id")]
        //public int Id;
        [JsonProperty("priority")]
        public string Priority;
    }

    public class ConnectResponse
    {
        [JsonProperty("command_type")]
        public string CommandType;
        [JsonProperty("params")]
        public GetPeerEventResponseDataParams Params;
    }

    public class ChangeDataChannelSettingRequest
    {
        [JsonProperty("feed_params")]
        public ChangeDataChannelSettingRequestFeedParams FeedParams;
        [JsonProperty("redirect_params")]
        public ChangeDataChannelSettingRequestRedirectParams RedirectParams;
    }

    public class ChangeDataChannelSettingRequestFeedParams
    {
        [JsonProperty("data_id")]
        public string DataId;
    }

    public class ChangeDataChannelSettingRequestRedirectParams
    {
        [JsonProperty("ip_v4")]
        public string Ipv4;
        [JsonProperty("ip_v6")]
        public string Ipv6;
        [JsonProperty("port")]
        public int Port;
    }

    public class GetDataChannelEventResponse
    {
        [JsonProperty("event")]
        public string Event;
        [JsonProperty("error_message")]
        public string ErrorMessage;
    }

    #endregion DataChannel

    #region MediaStream

    public class OpenPortForMediaStreamResponse
    {
        [JsonProperty("media_id")]
        public string MediaId;
        [JsonProperty("port")]
        public int Port;
        [JsonProperty("ip_v4")]
        public string Ipv4;
        [JsonProperty("ip_v6")]
        public string Ipv6;
    }

    public class CallRequest
    {
        [JsonProperty("peer_id")]
        public string PeerId;
        [JsonProperty("token")]
        public string Token;
        [JsonProperty("target_id")]
        public string TargetId;
        [JsonProperty("constraints")]
        public Constraints Constraints;
        [JsonProperty("redirect_params")]
        public AnswerRequestRedirectParams RedirectParams;
    }

    public class CallResponse
    {
        [JsonProperty("command_type")]
        public string CommandType;
        [JsonProperty("params")]
        public GetPeerEventResponseCallParams Params;
    }

    public class AnswerRequest
    {
        [JsonProperty("constraints")]
        public Constraints Constraints;
        [JsonProperty("redirect_params")]
        public AnswerRequestRedirectParams RedirectParams;
    }

    public class Constraints
    {
        [JsonProperty("video")]
        public bool Video;
        [JsonProperty("videoReceiveEnabled")]
        public bool VideoReceiveEnabled;
        [JsonProperty("audio")]
        public bool Audio;
        [JsonProperty("audioReceiveEnabled")]
        public bool AudioReceiveEnabled;
        [JsonProperty("video_params")]
        public ConstraintsMediaParams VideoParams;
        [JsonProperty("audio_params")]
        public ConstraintsMediaParams AudioParams;
    }

    public class ConstraintsMediaParams
    {
        [JsonProperty("band_width")]
        public int BandWidth;
        [JsonProperty("codec")]
        public string Codec;
        [JsonProperty("media_id")]
        public string MediaId;
        [JsonProperty("payload_type")]
        public int PayloadType;
        [JsonProperty("sampling_rate")]
        public int SamplingRate;
    }

    public class AnswerRequestRedirectParams
    {
        [JsonProperty("video")]
        public AnswerRequestRedirectParamsMedia Video;
        [JsonProperty("audio")]
        public AnswerRequestRedirectParamsMedia Audio;
    }

    public class AnswerRequestRedirectParamsMedia
    {
        [JsonProperty("ip_v4")]
        public string Ipv4;
        [JsonProperty("ip_v6")]
        public string Ipv6;
        [JsonProperty("port")]
        public int Port;
    }

    public class AnswerResponse
    {
        [JsonProperty("command_type")]
        public string CommandType;
        [JsonProperty("params")]
        public AnswerResponseParams Params;
    }

    public class AnswerResponseParams
    {
        [JsonProperty("video_port")]
        public int VideoPort;
        [JsonProperty("video_id")]
        public string VideoId;
        [JsonProperty("audio_port")]
        public int AudioPort;
        [JsonProperty("audio_id")]
        public string AudioId;
    }

    #endregion MediaStream
}
