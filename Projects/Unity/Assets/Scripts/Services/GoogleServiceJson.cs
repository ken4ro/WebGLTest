using System;

using UnityEngine;

using Newtonsoft.Json;


public static partial class GoogleService
{
    [Serializable]
    public class GoogleServiceSettings : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string ApiKey;
        [NonSerialized]
        public string ServiceAccountFilePath;
        
        [SerializeField]
        private string api_key;
        [SerializeField]
        private string service_account_file_path;

        public void OnBeforeSerialize()
        {
            api_key = ApiKey;
            service_account_file_path = ServiceAccountFilePath;
        }

        public void OnAfterDeserialize()
        {
            ApiKey = api_key;
            ServiceAccountFilePath = service_account_file_path;
        }
    }

    [Serializable]
    public class GoogleSpeechToTextRequest
    {
        [JsonProperty("config")]
        public GoogleSpeechToTextRequestConfig Config;
        [JsonProperty("audio")]
        public GoogleSpeechToTextRequestAudio Audio;
    }

    [Serializable]
    public class GoogleSpeechToTextRequestConfig
    {
        [JsonProperty("encoding")]
        public string Encoding;
        [JsonProperty("sampleRateHertz")]
        public int SampleRateHertz;
        [JsonProperty("languageCode")]
        public string LanguageCode;
        [JsonProperty("enableWordTimeOffsets")]
        public bool EnableWordTimeOffsets;
    }

    [Serializable]
    public class GoogleSpeechToTextRequestAudio
    {
        [JsonProperty("content")]
        public string Content;
    }

    [Serializable]
    public class GoogleSpeechToTextResponse
    {
        [JsonProperty("results")]
        public GoogleSpeechToTextResponseResults[] Results;
    }

    [Serializable]
    public class GoogleSpeechToTextResponseResults
    {
        [JsonProperty("alternatives")]
        public GoogleSpeechToTextResponseResultsAlternatives[] Alternatives;
    }

    [Serializable]
    public class GoogleSpeechToTextResponseResultsAlternatives
    {
        [JsonProperty("transcript")]
        public string Transcript;
        [JsonProperty("confidence")]
        public float Confidence;
    }

    [Serializable]
    public class GoogleTextToSpeechRequest
    {
        [JsonProperty("input")]
        public GoogleTextToSpeechRequestInput Input;
        [JsonProperty("voice")]
        public GoogleTextToSpeechRequestVoice Voice;
        [JsonProperty("audioConfig")]
        public GoogleTextToSpeechRequestAudioConfig AudioConfig;
    }

    [Serializable]
    public class GoogleTextToSpeechRequestInput
    {
        [JsonProperty("text")]
        public string Text;
    }

    [Serializable]
    public class GoogleTextToSpeechRequestVoice
    {
        [JsonProperty("languageCode")]
        public string LanguageCode;
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("ssmlGender")]
        public string SsmlGender;
    }

    [Serializable]
    public class GoogleTextToSpeechRequestAudioConfig
    {
        [JsonProperty("audioEncoding")]
        public string AudioEncoding;
        [JsonProperty("sampleRateHertz")]
        public int SampleRateHertz;
    }

    [Serializable]
    public class GoogleTextToSpeechResponse
    {
        [JsonProperty("audioContent")]
        public string AudioContent;
    }

    [Serializable]
    public class GoogleTranslationRequest
    {
        [JsonProperty("q")]
        public string Q;
        [JsonProperty("target")]
        public string Target;
        [JsonProperty("format")]
        public string Format;
        [JsonProperty("source")]
        public string Source;
        [JsonProperty("model")]
        public string Model;
        [JsonProperty("key")]
        public string Key;
    }

    [Serializable]
    public class GoogleTranslationResponse
    {
        [JsonProperty("data")]
        public GoogleTranslationResponseData Data;
    }

    [Serializable]
    public class GoogleTranslationResponseData
    {
        [JsonProperty("translations")]
        public GoogleTranslationResponseDataTranslations[] Translations;
    }

    [Serializable]
    public class GoogleTranslationResponseDataTranslations
    {
        [JsonProperty("detectedSourceLanguage")]
        public string DetectedSourceLanguage;
        [JsonProperty("translatedText")]
        public string TranslatedText;
    }
}
