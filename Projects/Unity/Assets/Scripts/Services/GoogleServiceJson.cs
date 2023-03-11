using System;
using UnityEngine;

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
        public GoogleSpeechToTextRequestConfig config;
        public GoogleSpeechToTextRequestAudio audio;
    }

    [Serializable]
    public class GoogleSpeechToTextRequestConfig
    {
        public string encoding;
        public int sampleRateHertz;
        public string languageCode;
        public bool enableWordTimeOffsets;
    }

    [Serializable]
    public class GoogleSpeechToTextRequestAudio
    {
        public string content;
    }

    [Serializable]
    public class GoogleSpeechToTextResponse
    {
        public GoogleSpeechToTextResponseResults[] results;
    }

    [Serializable]
    public class GoogleSpeechToTextResponseResults
    {
        public GoogleSpeechToTextResponseResultsAlternatives[] alternatives;
    }

    [Serializable]
    public class GoogleSpeechToTextResponseResultsAlternatives
    {
        public string transcript;
        public float confidence;
    }

    [Serializable]
    public class GoogleTextToSpeechRequest
    {
        public GoogleTextToSpeechRequestInput input;
        public GoogleTextToSpeechRequestVoice voice;
        public GoogleTextToSpeechRequestAudioConfig audioConfig;
    }

    [Serializable]
    public class GoogleTextToSpeechRequestInput
    {
        public string text;
    }

    [Serializable]
    public class GoogleTextToSpeechRequestVoice
    {
        public string languageCode;
        public string name;
        public string ssmlGender;
    }

    [Serializable]
    public class GoogleTextToSpeechRequestAudioConfig
    {
        public string audioEncoding;
        public int sampleRateHertz;
    }

    [Serializable]
    public class GoogleTextToSpeechResponse
    {
        public string audioContent;
    }

    [Serializable]
    public class GoogleTranslationRequest
    {
        public string q;
        public string target;
        public string format;
        public string source;
        public string model;
        public string key;
    }

    [Serializable]
    public class GoogleTranslationResponse
    {
        public GoogleTranslationResponseData data;
    }

    [Serializable]
    public class GoogleTranslationResponseData
    {
        public GoogleTranslationResponseDataTranslations[] translations;
    }

    [Serializable]
    public class GoogleTranslationResponseDataTranslations
    {
        public string detectedSourceLanguage;
        public string translatedText;
    }
}
