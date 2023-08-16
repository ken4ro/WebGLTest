using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using static SignageSettings;
using static ApiServerManager;

public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
    /// <summary>
    /// SE再生用オーディオソース
    /// </summary>
    [SerializeField]
    private AudioSource _audioSourceForSE = null;

    /// <summary>
    /// SE種別
    /// </summary>
    public enum SEType
    {
        VoiceIn,
        VoiceOut,
        CallingStart,
        CallingEnd,
        SelectSentence,
        NextPage,
    }

    /// <summary>
    /// 音声合成エンジン
    /// </summary>
    public enum TextToSpeechEngine
    {
        Google,
        Azure,
        Hoya,
        Local,
        UnityAsset
    }

    /// <summary>
    /// マイクのミュートがオン/オフされた
    /// </summary>
    public Action<bool> OnMicMute = null;

    /// <summary>
    /// マイクのミュート状態
    /// </summary>
    public bool IsMicMute { get; private set; } = false;

    // ボイス設定
    private static readonly int VoiceChannels = 1;

    // 接続先のマイクサンプリングレート
    private int _targetMicSampleRate = 0;

    // キャラクター発音用オーディオソース
    private AudioSource _audioSourceForCharacter = null;

    private bool _isInitializeAudio = false;

    public void SetAudioSourceForCharacter()
    {
        // ボイス再生対象キャラクターオブジェクトをセット
        _audioSourceForCharacter = CharacterManager.Instance.gameObject.GetComponent<AudioSource>();
        _audioSourceForCharacter.volume = 0.1f;
    }

    public void PlayTargetVoice()
    {
        if (_isInitializeAudio) return;

        // オーディオ設定リセット
        AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
        audioConfig.sampleRate = _targetMicSampleRate;
        AudioSettings.Reset(audioConfig);

        // 口パクさせるためにキャラクターモデルにアタッチされている AudioSource を使用する
        _audioSourceForCharacter.clip = AudioClip.Create("Target Voice", _targetMicSampleRate, VoiceChannels, _targetMicSampleRate, true);
        _audioSourceForCharacter.loop = true;
        _audioSourceForCharacter.Play();

        _isInitializeAudio = true;
    }

    public void StopTargetVoice()
    {
        _audioSourceForCharacter.Stop();
    }

    /// <summary>
    /// 指定された文字列からAudioClipを取得(音声合成orキャッシュ)
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async UniTask<AudioClip> GetAudioClip(string text, bool nomatch = false)
    {
        AudioClip audioClip = await GetWebAudioClip(GlobalState.Instance.UserSettings.Bot.VoiceType, text, nomatch);

        return audioClip;
    }

    /// <summary>
    /// キャラクターボイスをリセットする
    /// </summary>
    public void ResetCharacterVoice()
    {
        _audioSourceForCharacter.Stop();
        _audioSourceForCharacter.clip = null;
    }

    /// <summary>
    /// AudioClipを指定して再生
    /// </summary>
    /// <param name="audioClip"></param>
    /// <returns></returns>
    public async UniTask Play(AudioClip audioClip)
    {
        // オートリップシンク開始
        CharacterManager.Instance.StartAutoLipSync();
        // AudioClipをセット
        _audioSourceForCharacter.clip = audioClip;
        // 再生
        _audioSourceForCharacter.loop = false;
        _audioSourceForCharacter.Play();
        while (audioClip.loadState != AudioDataLoadState.Loaded)
        {
            //Debug.Log($"audioClip loadState = {audioClip.loadState}");
            await UniTask.Delay(10);
        }
        // 再生終了まで待つ
        var audioClipLengthMs = (int)TimeSpan.FromSeconds(audioClip.length).TotalMilliseconds;
        //Debug.Log($"audioClip length = {audioClip.length}, samples = {audioClip.samples}, msec = = {audioClipLengthMs}");
        await UniTask.Delay(audioClipLengthMs);
        // オートリップシンク終了
        CharacterManager.Instance.StopAutoLipSync();
    }

    /// <summary>
    /// SE再生
    /// </summary>
    /// <param name="seType"></param>
    /// <returns></returns>
    public async UniTask PlaySE(SEType seType, bool isLoop = false)
    {
        // AudioClipをセット
        AudioClip audioClip = null;
        switch (seType)
        {
            case SEType.VoiceIn:
                audioClip = Resources.Load<AudioClip>("Audio/VoiceIn");
                break;
            case SEType.VoiceOut:
                audioClip = Resources.Load<AudioClip>("Audio/VoiceOut");
                break;
            case SEType.CallingStart:
                audioClip = Resources.Load<AudioClip>("Audio/se_calling_echo");
                break;
            case SEType.CallingEnd:
                audioClip = Resources.Load<AudioClip>("Audio/se_calling_end");
                break;
            case SEType.SelectSentence:
                audioClip = Resources.Load<AudioClip>("Audio/se_select_sentence");
                break;
            case SEType.NextPage:
                audioClip = Resources.Load<AudioClip>("Audio/se_page");
                break;
            default:
                break;
        }
        _audioSourceForSE.clip = audioClip;
        // ループ設定
        _audioSourceForSE.loop = isLoop;
        // 再生
        _audioSourceForSE.Play();
        // 再生終了まで待つ
        var audioClipLengthMs = (int)TimeSpan.FromSeconds(audioClip.length).TotalMilliseconds;
        await UniTask.Delay(audioClipLengthMs);
    }

    /// <summary>
    /// SE再生終了
    /// </summary>
    public void StopSE()
    {
        _audioSourceForSE.Stop();
    }

    /// <summary>
    /// webサービスのオーディオクリップを取得
    /// </summary>
    /// <param name="type"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    private async UniTask<AudioClip> GetWebAudioClip(string type, string text, bool nomatch = false)
    {
        string key = $"{type}_{text}";
        var audioCache = new TextToSpeechAudioCache(key);
#if UNITY_EDITOR || !UNITY_WEBGL
        //if (audioCache != null && audioCache.IsCached()) // いったんサーバキャッシュを使う
        if (false)
        {
            Debug.Log($"GetWebAudioClip: from cache file path = {audioCache.GetFilePath()}");
            // キャッシュから取得
            return await audioCache.GetCacheFile();
        }
#else
        // WebGLの場合はファイル存在チェックが出来ないので、ファイルがある前提で...
        //if (audioCache != null)
        if (false)
        {
            Debug.Log($"GetWebAudioClip: from StreamingAssets file path = {audioCache.GetFilePath()}");
            // キャッシュから取得
            return await audioCache.GetCacheFile();
        }
#endif
        else
        {
            Debug.Log($"GetWebAudioClip: from web type = {type}");
            // 新規で音声合成して取得
            AudioClip audioClip = null;
            byte[] audioData = null;
            int audioDataLength = 0;
            switch (type)
            {
                // Google Cloud Text-to-Speech
                case "google":
                    audioData = await GoogleService.TextToSpeech(text, (GoogleService.Language)SignageSettings.CurrentLanguage.Value);
                    audioDataLength = audioData.Length;
                    audioClip = AudioClipMaker.Create("clipname", audioData, 44, AudioClipMaker.BIT_16, (audioDataLength - 44) / 2, 1, 48000, false);
                    break;
                // Server Cache
                case "xrccgCache":
                default:
                    var userTokenBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(GlobalState.Instance.UserSettings.UserToken));
                    var json = new RequestNodeVoiceJson()
                    {
                        source_type = "node",
                        language = CurrentLanguage.Value.ToString().ToLower()
                    };
                    if (nomatch)
                    {
                        json.source_type = "notFound";
                    }
                    audioData = await ApiServerManager.Instance.RequestNodeVoice(userTokenBase64, JsonUtility.ToJson(json));
                    audioDataLength = audioData.Length;
                    if (CurrentLanguage.Value == Language.Japanese)
                    {
                        // 日本語はVoiceVoxデフォルト=24kHz
                        audioClip = AudioClipMaker.Create("clipname", audioData, 44, AudioClipMaker.BIT_16, (audioDataLength - 44) / 2, 1, 24000, false);
                    }
                    else if (CurrentLanguage.Value == Language.English)
                    {
                        // 英語はGoogleデフォルト=48kHz
                        audioClip = AudioClipMaker.Create("clipname", audioData, 44, AudioClipMaker.BIT_16, (audioDataLength - 44) / 2, 1, 48000, false);
                    }
                    break;
            }
#if UNITY_EDITOR || !UNITY_WEBGL
            // 音声ファイル保存
            audioCache.SaveFile(audioData);
#endif

            return audioClip;
        }
    }

}
