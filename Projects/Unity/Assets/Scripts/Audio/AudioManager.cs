using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using static SignageSettings;
using Cysharp.Threading.Tasks;

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

    // リングバッファ関連
    private static readonly int Bit = 16;
    private static readonly int UnityBit = 32;
    private static readonly int DelayMilliseconds = 100;
    private static readonly float DelaySeconds = 0.1f;
    private static readonly int StartBufferingMilliseconds = 300;

    // 接続先のマイクサンプリングレート
    private int _targetMicSampleRate = 0;

    // キャラクター発音用オーディオソース
    private AudioSource _audioSourceForCharacter = null;

    // リングバッファ関連
    private IntPtr _ringAudioHandle = IntPtr.Zero;
    private bool _startSpeaker = false;
    private long _speakerTime = 0;
    private float[] _receiveBuffer = null;
    private float[] _tmpBuffer = null;
    private IntPtr _speakerPointer = IntPtr.Zero;
    private int _residueReadPosition = 0;
    private int _residueUseSize = 0;

    private bool _isInitializeAudio = false;

#region RingAudioLibrary

    [DllImport("RingAudioLibrary", CallingConvention = CallingConvention.Cdecl)]
    extern static IntPtr createRingAudio(int bitRate, int samplingRate, int sampleTime);

    [DllImport("RingAudioLibrary", CallingConvention = CallingConvention.Cdecl)]
    extern static bool releaseRingAudio(IntPtr handle);

    [DllImport("RingAudioLibrary", CallingConvention = CallingConvention.Cdecl)]
    extern static bool resetBuffer(IntPtr handle);

    [DllImport("RingAudioLibrary", CallingConvention = CallingConvention.Cdecl)]
    extern static bool writePcm(IntPtr handle, int bitRate, int samplingRate, long timeStamp, float sampleTime, IntPtr buffer);

    [DllImport("RingAudioLibrary", CallingConvention = CallingConvention.Cdecl)]
    extern static long getTopTimeStamp(IntPtr handle);

    [DllImport("RingAudioLibrary", CallingConvention = CallingConvention.Cdecl)]
    extern static long getBottomTimeStamp(IntPtr handle);

    [DllImport("RingAudioLibrary", CallingConvention = CallingConvention.Cdecl)]
    extern static bool readPcm(IntPtr handle, int bitRate, int samplingRate, long timeStamp, float sampleTime, IntPtr buffer);

#endregion RingAudioLibrary

    void Start()
    {
        CharacterManager.Instance.OnAudioFilterReadCallback += OnAudioFilterRead;

        // ボイス再生対象キャラクターオブジェクトをセット
        _audioSourceForCharacter = CharacterManager.Instance.gameObject.GetComponent<AudioSource>();
        _audioSourceForCharacter.volume = 0.1f;
    }

    protected override void OnDestroy()
    {
        // リングバッファ破棄
        ReleaseRingAudio();

        Marshal.FreeCoTaskMem(_speakerPointer);

        base.OnDestroy();
    }

    public void PlayTargetVoice()
    {
        if (_isInitializeAudio) return;

        // オーディオ設定リセット
        AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
        audioConfig.sampleRate = _targetMicSampleRate;
        AudioSettings.Reset(audioConfig);

        // リングバッファ生成
        CreateRingAudio();

        // オーディオデータ受信用バッファ(1秒分)
        _receiveBuffer = new float[_targetMicSampleRate];
        // オーディオデータ送信用バッファ
        _speakerPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(byte)) * 70000);

        // 口パクさせるためにキャラクターモデルにアタッチされている AudioSource を使用する
        _audioSourceForCharacter.clip = AudioClip.Create("Target Voice", _targetMicSampleRate, VoiceChannels, _targetMicSampleRate, true);
        _audioSourceForCharacter.loop = true;
        _audioSourceForCharacter.Play();

        _startSpeaker = false;
        _residueReadPosition = 0;
        _residueUseSize = 0;

        _isInitializeAudio = true;
    }

    public void StopTargetVoice()
    {
        _audioSourceForCharacter.Stop();

        // リングバッファリセット
        ResetRingAudio();

        _startSpeaker = false;
        _residueReadPosition = 0;
        _residueUseSize = 0;
    }

    /// <summary>
    /// オーディオデータ受信
    /// </summary>
    /// <param name="data"></param>
    public void AudioDataReceived(byte[] data)
    {
        // タイムスタンプを抽出
        var timeStamp = BitConverter.ToInt64(data, 0);
        var timeStampSize = 8;

        // サンプリングレート抽出
        _targetMicSampleRate = BitConverter.ToInt32(data, timeStampSize);
        var sampleRateSize = 4;
        Debug.Log($"AudioDataReceived: sampleRate = {_targetMicSampleRate}");

        if (!_isInitializeAudio) return;

        // オーディオデータ抽出
        var receivedAudioBuffer = new byte[data.Length - timeStampSize - sampleRateSize];
        Array.Copy(data, timeStampSize + sampleRateSize, receivedAudioBuffer, 0, receivedAudioBuffer.Length);

        // 指定したコーデックでデコード
        byte[] decoded;
        switch (GlobalState.Instance.ApplicationGlobalSettings.AudioCodec)
        {
            case ApplicationSettings.AudioCodecType.MuLaw:
                decoded = MuLawCodec.Decode(receivedAudioBuffer, 0, receivedAudioBuffer.Length);
                break;
            case ApplicationSettings.AudioCodecType.G722:
                decoded = G722Codec.Decode(receivedAudioBuffer, 0, receivedAudioBuffer.Length);
                break;
            default:
                decoded = MuLawCodec.Decode(receivedAudioBuffer, 0, receivedAudioBuffer.Length);
                break;
        }

        // 格納時間を抽出
        var samplingTime = (decoded.Length / VoiceChannels / 2) / (float)_targetMicSampleRate;

        // floatへコンバート
        var receiveBuffer = new float[decoded.Length * 2];
        for (int i = 0; i < decoded.Length / 2; i++)
        {
            receiveBuffer[i] = AudioClipMaker.ConvertByteToFloatData(decoded, i * 2, Bit);
        }

        // リングバッファ書き込み
        var receivePointer = Marshal.AllocHGlobal(Marshal.SizeOf(receiveBuffer[0]) * receiveBuffer.Length);
        Marshal.Copy(receiveBuffer, 0, receivePointer, receiveBuffer.Length);
        try
        {
            if (!WritePcm(timeStamp, samplingTime, receivePointer))
            {
                Debug.LogError($"WritePcm error.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"WritePcm exception: {ex.Message}");
        }
    }

    // オーディオバッファ要求Unityコールバック
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_isInitializeAudio) return;

        int requestSize = data.Length / channels;
        int writePosition = 0;

        // 残バッファがある場合はそれをセットする
        if (_residueUseSize - _residueReadPosition > 0)
        {
            int writeSize = _residueUseSize - _residueReadPosition < requestSize ? _residueUseSize - _residueReadPosition : requestSize;
            _tmpBuffer = new float[writeSize];
            Array.Copy(_receiveBuffer, _residueReadPosition, _tmpBuffer, 0, writeSize);
            var copyBuffer = CopyBufferByChannelNum(_tmpBuffer, channels);
            Array.Copy(copyBuffer, 0, data, 0, copyBuffer.Length);
            _residueReadPosition += writeSize;
            writePosition += writeSize;
            requestSize -= writeSize;
        }

        // 残バッファでは足りない場合
        // リングバッファから取得して書き込む
        if (requestSize > 0)
        {
            _residueReadPosition = 0;

            // 1コールあたりのバッファサイズ(0.1秒分)
            int speakerLength = (int)(_targetMicSampleRate * DelaySeconds) * VoiceChannels;
            var topTime = GetTopTimestamp();
            var bottomTime = GetBottomTimestamp();

            // 通話開始(再開)後初回
            if (!_startSpeaker)
            {
                // 0.3秒分バッファリングされてから処理を開始
                if (bottomTime - topTime < StartBufferingMilliseconds) return;

                // 初回は0.2秒分セット
                _speakerTime = topTime;
                if (!ReadPcm(_speakerTime, DelaySeconds * 2, _speakerPointer)) return;
                Marshal.Copy(_speakerPointer, _receiveBuffer, 0, speakerLength * 2);
                _residueUseSize = speakerLength * 2;
                _speakerTime += DelayMilliseconds * 2;
                _startSpeaker = true;
            }
            // 2週目以降
            else
            {
                // 収録時間を超過しそうな場合
                if (bottomTime < _speakerTime + DelayMilliseconds)
                {
                    // 伝送が不安定と予想できるので、0.5秒強制的に巻き戻す
                    if (topTime < _speakerTime - (DelayMilliseconds * 5))
                    {
                        _speakerTime = bottomTime - (DelayMilliseconds * 5);
                    }
                    else
                    {
                        _speakerTime = bottomTime - DelayMilliseconds;
                    }
                }

                // 収録時間に遅れ気味な場合
                if (topTime > _speakerTime)
                {
                    // 再生速度が遅いと予測できるので、強制的に収録後半にスキップする
                    if (topTime < bottomTime - (DelayMilliseconds * 3))
                    {
                        _speakerTime = bottomTime - (DelayMilliseconds * 3);
                    }
                    else
                    {
                        _speakerTime = topTime;
                    }
                }

                // 0.1秒分セット
                if (!ReadPcm(_speakerTime, DelaySeconds, _speakerPointer)) return;
                Marshal.Copy(_speakerPointer, _receiveBuffer, 0, speakerLength);
                _speakerTime += DelayMilliseconds;
                _residueUseSize = speakerLength;
            }

            // 書き込み
            if (_residueUseSize > 0)
            {
                int writeSize = requestSize < _residueUseSize ? requestSize : _residueUseSize;
                _tmpBuffer = new float[writeSize];
                Array.Copy(_receiveBuffer, _residueReadPosition, _tmpBuffer, 0, writeSize);
                var copyBuffer = CopyBufferByChannelNum(_tmpBuffer, channels);
                Array.Copy(copyBuffer, 0, data, writePosition * channels, copyBuffer.Length);
                _residueReadPosition += writeSize;
            }
        }
    }

    /// <summary>
    /// 指定された文字列からAudioClipを取得(音声合成orキャッシュ)
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async UniTask<AudioClip> GetAudioClip(string text)
    {
        AudioClip audioClip = null;
        switch (SignageSettings.CurrentTextToSpeechEngine)
        {
            //ローカルファイル
            case TextToSpeechEngine.Local:
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + text, AudioType.MPEG))
                {
                    while (!www.isDone)
                        continue;

                    audioClip = DownloadHandlerAudioClip.GetContent(www);

                }
                break;

            case TextToSpeechEngine.UnityAsset:
                audioClip = AssetBundleManager.Instance.LoadAudioClipFromResourcePack(text);
                break;

            //WEBサービス
            default:
                audioClip = await GetWebAudioClip(SignageSettings.CurrentTextToSpeechEngine, text);
                break;
        }

        return audioClip;
    }

    /// <summary>
    /// キャラクターボイスをリセットする
    /// </summary>
    public void ResetCharacterVoice()
    {
        _audioSourceForCharacter.Stop();
        _audioSourceForCharacter.clip = null;

        // リングバッファ解放
        ReleaseRingAudio();
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
        // 再生終了まで待つ
        var audioClipLengthMs = (int)TimeSpan.FromSeconds(audioClip.length).TotalMilliseconds;
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
    /// マイクミュート(トグル)
    /// </summary>
    public bool MuteMic()
    {
        MuteMic(!IsMicMute);

        return IsMicMute;
    }

    /// <summary>
    /// webサービスのオーディオクリップを取得
    /// </summary>
    /// <param name="type"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    private async UniTask<AudioClip> GetWebAudioClip(TextToSpeechEngine type, string text)
    {
        string key = $"{SignageSettings.CurrentTextToSpeechEngine.ToString()}_{text}";
        var audioCache = new TextToSpeechAudioCache(key);
#if UNITY_EDITOR || !UNITY_WEBGL
        if (audioCache != null && audioCache.IsCached())
        {
            Debug.Log($"GetWebAudioClip: from cache file path = {audioCache.GetFilePath()}");
            // キャッシュから取得
            return await audioCache.GetCacheFile();
        }
#else
        // WebGLの場合はファイル存在チェックが出来ないので、ファイルがある前提で...
        if (audioCache != null)
        {
            Debug.Log($"GetWebAudioClip: from StreamingAssets file path = {audioCache.GetFilePath()}");
            // キャッシュから取得
            return await audioCache.GetCacheFile();
        }
#endif
        else
        {
            Debug.Log($"GetWebAudioClip: from web file path = {audioCache.GetFilePath()}");
            // 新規で音声合成して取得
            AudioClip audioClip = null;
            byte[] audioData = null;
            int audioDataLength = 0;
            switch (type)
            {
                // Google Cloud Text-to-Speech
                case TextToSpeechEngine.Google:
                    audioData = await GoogleService.TextToSpeech(text, (GoogleService.Language)CurrentLanguage.Value);
                    audioDataLength = audioData.Length;
                    audioClip = AudioClipMaker.Create("clipname", audioData, 44, AudioClipMaker.BIT_16, (audioDataLength - 44) / 2, 1, 48000, false);
                    break;

                // Microsoft Azure Text to Speech
                case TextToSpeechEngine.Azure:
                    audioData = await AzureVoice.GetVoice(text, (AzureVoice.VoiceSelect)CurrentLanguage.Value);
                    audioDataLength = audioData.Length;
                    audioClip = AudioClipMaker.Create("clipname", audioData, 44, AudioClipMaker.BIT_16, (audioDataLength - 44) / 2, 1, 24000, false);
                    break;

                // Hoya Text to Speech
                case TextToSpeechEngine.Hoya:
                    audioData = await HoyaVoice.GetVoice(text);
                    audioDataLength = audioData.Length;
                    audioClip = AudioClipMaker.Create("clipname", audioData, 44, AudioClipMaker.BIT_16, (audioDataLength - 44) / 2, 1, 44100, false);
                    break;

                default:
                    break;
            }
#if UNITY_EDITOR || !UNITY_WEBGL
            // 音声ファイル保存
            audioCache.SaveFile(audioData);
#endif

            return audioClip;
        }
    }

    // マイクミュート
    private void MuteMic(bool mute)
    {
        // デバイスによって正常にミュート出来ないので、アプリ側で独自のミュート処理を実装するように変更

        // ミュートセット
        IsMicMute = mute;

        // イベント通知
        OnMicMute?.Invoke(mute);
    }

    private float[] CopyBufferByChannelNum(float[] buffer, int channel)
    {
        float[] ret = new float[buffer.Length * channel];

        for (int i = 0, j = 0; i < ret.Length; i++, j = i / channel)
        {
            ret[i] = buffer[j];
        }

        return ret;
    }

    // リングバッファ作成
    private void CreateRingAudio() => _ringAudioHandle = createRingAudio(UnityBit, _targetMicSampleRate, 5);

    // リングバッファ解放
    private void ReleaseRingAudio()
    {
        if (_ringAudioHandle != IntPtr.Zero)
        {
            releaseRingAudio(_ringAudioHandle);
            _ringAudioHandle = IntPtr.Zero;
        }
    }

    // リングバッファリセット
    private void ResetRingAudio()
    {
        if (_ringAudioHandle != IntPtr.Zero)
        {
            resetBuffer(_ringAudioHandle);
        }
    }

    private long GetTopTimestamp() => getTopTimeStamp(_ringAudioHandle);

    private long GetBottomTimestamp() => getBottomTimeStamp(_ringAudioHandle);

    private bool ReadPcm(long timeStamp, float sampleTime, IntPtr buffer) => readPcm(_ringAudioHandle, UnityBit, _targetMicSampleRate, timeStamp, sampleTime, buffer);

    private bool WritePcm(long timeStamp, float sampleTime, IntPtr buffer) => writePcm(_ringAudioHandle, UnityBit, _targetMicSampleRate, timeStamp, sampleTime, buffer);

}
