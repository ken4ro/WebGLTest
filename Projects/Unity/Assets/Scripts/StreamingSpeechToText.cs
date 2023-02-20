/*
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;
using NAudio.Wave;
using Grpc.Core;
using Grpc.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using static SignageSettings;

public class StreamingSpeechToText : SingletonBase<StreamingSpeechToText>
{
    /// <summary>
    /// 録音開始時コールバック
    /// </summary>
    public Action OnStartRecording = null;

    /// <summary>
    /// ストリーミング音声認識結果受信時コールバック
    /// </summary>
    public Action<string> OnStreamingDataAvailable = null;

    /// <summary>
    /// ストリーミング音声認識結果完了時コールバック
    /// </summary>
    public Action<string> OnStreamingDataComplete = null;

    /// <summary>
    /// ストリーミング音声認識結果格納用キュー
    /// </summary>
    public ConcurrentQueue<string> RecognitionTextQueue { get; private set; } = new ConcurrentQueue<string>();

    /// <summary>
    /// ストリーミング音声認識最終結果
    /// </summary>
    public string RecognitionCompleteText { get; private set; } = "";

    /// <summary>
    /// 音声認識言語コード
    /// </summary>
    public string SpeechToTextLanguageCode { get; set; } = "ja-JP";

    /// <summary>
    /// 音声認識エンコーディング
    /// </summary>
    public RecognitionConfig.Types.AudioEncoding SpeechToTextAudioEncoding { get; set; } = RecognitionConfig.Types.AudioEncoding.Linear16;

    /// <summary>
    /// 音声認識サンプリングレート
    /// </summary>
    public int SpeechToTextSampleRateHertz { get; set; } = 16000;

    // ストリーミング音声認識用タスク
    private Task<string> RecognitionTask = null;

    // タスクキャンセル用
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private GoogleCredential _credential = null;
    private Channel _channel = null;

    private WaveInEvent _waveIn = null;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
        // サービスアカウントファイルで認証
        //_credential = GoogleCredential.FromFile(GoogleService.CredentialFilePath);
        using (var stream = new MemoryStream(GoogleService.CredentialFileData))
        {
            _credential = GoogleCredential.FromStream(stream);
        }

        // チャンネル作成
        CreateChannel();

        // 音声入力初期化
        _waveIn = new WaveInEvent
        {
            DeviceNumber = 0,
            WaveFormat = new WaveFormat(16000, 1)
        };
    }

    /// <summary>
    /// 解放処理
    /// </summary>
    /// <returns></returns>
    public async Task Dispose()
    {
        try
        {
            await CancelRecognition();
            await _channel.ShutdownAsync();
            //await GrpcEnvironment.ShutdownChannelsAsync();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    /// <summary>
    /// ストリーミング音声認識を専用タスクで実行する
    /// </summary>
    public async Task RunOneShotTask()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        RecognitionCompleteText = "";

        while (string.IsNullOrEmpty(RecognitionCompleteText) && !_cancellationTokenSource.IsCancellationRequested)
        {
            // スレッドプールで実行
            await Task.Run(async () =>
            {
                RecognitionTask = OneShot();
                RecognitionCompleteText = await RecognitionTask;
            });
        }

        // イベント通知
        if (!string.IsNullOrEmpty(RecognitionCompleteText))
        {
            OnStreamingDataComplete?.Invoke(RecognitionCompleteText);
        }
    }

    /// <summary>
    /// ストリーミング音声認識をキャンセルする
    /// </summary>
    /// <returns></returns>
    public async Task CancelRecognition()
    {
        if (RecognitionTask != null)
        {
            _cancellationTokenSource.Cancel();
            await Task.Run(async () =>
            {
                await RecognitionTask;
            });
            RecognitionTask = null;
        }
    }

    /// <summary>
    /// ストリーミング音声認識完了時コールバック
    /// テキスト入力にも対応するため public メソッドとする
    /// </summary>
    /// <param name="text"></param>
    public void SetRecognitionCompleteText(string text)
    {
        // 結果を格納
        RecognitionCompleteText = text;

        // イベント通知
        OnStreamingDataComplete?.Invoke(RecognitionCompleteText);
    }

    // ストリーミング音声認識を一度実行
    private async Task<string> OneShot(int recognitionTimeMs = 50000)
    {
        // 言語切り替え
        switch (CurrentLanguage.Value)
        {
            case Language.Japanese:
                SpeechToTextLanguageCode = "ja-JP";
                break;
            case Language.English:
                SpeechToTextLanguageCode = "en";
                break;
            case Language.Chinese:
                SpeechToTextLanguageCode = "zh";
                break;
            case Language.Russian:
                SpeechToTextLanguageCode = "ru-RU";
                break;
            case Language.Arabic:
                SpeechToTextLanguageCode = "ar-EG";
                break;
            case Language.Vietnamese:
                SpeechToTextLanguageCode = "vi-VN";
                break;

        }

        // ストリーミング開始
        var speech = SpeechClient.Create(_channel);
        var streamingCall = speech.StreamingRecognize();
        try
        {
            await streamingCall.WriteAsync(new StreamingRecognizeRequest()
            {
                StreamingConfig = new StreamingRecognitionConfig()
                {
                    Config = new RecognitionConfig()
                    {
                        LanguageCode = SpeechToTextLanguageCode,
                        Encoding = SpeechToTextAudioEncoding,
                        SampleRateHertz = SpeechToTextSampleRateHertz,
                    },
                    InterimResults = true,
                    SingleUtterance = true,
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            streamingCall.GrpcCall.Dispose();
            await _channel.ShutdownAsync();
            CreateChannel();
            return null;
        }
        var finalPhrase = "";
        Task recognitionTask = Task.Run(async () =>
        {
            try
            {
                while (await streamingCall.ResponseStream.MoveNext())
                {
                    var phrase = new StringBuilder();
                    var isFinal = false;
                    // デフォルトなら返ってくる変換候補は１つのみだが、念の為 foreach で回す
                    foreach (var result in streamingCall.ResponseStream.Current.Results)
                    {
                        // ミュート時は無視する
                        if (AudioManager.Instance.IsMicMute) break;

                        isFinal = result.IsFinal;
                        foreach (var alternative in result.Alternatives)
                        {
                            var transcript = alternative.Transcript;
                            //Debug.Log(transcript);
                            phrase.Append(transcript);
                            finalPhrase = phrase.ToString();
                            StreamingDataAvailable(finalPhrase);
                        }
                    }
                    if (streamingCall.ResponseStream.Current.SpeechEventType == StreamingRecognizeResponse.Types.SpeechEventType.EndOfSingleUtterance)
                    {
                        // ストリーミング終了
                        // SingleUtterance = true の場合、一切の発話が無くても、なぜか約9秒おきに EndOfSingleUtterance となり音声認識が終了してしまう
                        // その際は IsFinal = true とはならない
                        // 正常に発話を検知した場合、次のループで IsFinal = true となる謎仕様
                        //Debug.Log($"EndOfSingleUtterance: {finalPhrase.ToString()}");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        });
        object writeLock = new object();
        bool writeMore = true;
        void handler(object sender, WaveInEventArgs args)
        {
            lock (writeLock)
            {
                if (!writeMore) return;

                try
                {
                    streamingCall.WriteAsync(new StreamingRecognizeRequest()
                    {
                        AudioContent = RecognitionAudio.FromBytes(args.Buffer, 0, args.BytesRecorded).Content
                    }).Wait();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
        _waveIn.DataAvailable += handler;
        StartRecording();
        try
        {
            var delayTask = Task.Delay(recognitionTimeMs, _cancellationTokenSource.Token);
            await Task.WhenAny(recognitionTask, delayTask);
        }
        catch (Exception)
        {
            //Debug.Log($"Task.Delay Exception: {e.Message}");
        }
        _waveIn.StopRecording();
        _waveIn.DataAvailable -= handler;
        lock (writeLock) writeMore = false;
        try
        {
            await streamingCall.WriteCompleteAsync();
        }
        catch (Exception e)
        {
            Debug.Log($"WriteCompleteAsync Exception: {e.Message}");
            streamingCall.GrpcCall.Dispose();
            await _channel.ShutdownAsync();
            CreateChannel();
            return null;
        }
        try
        {
            await recognitionTask;
        }
        catch (Exception e)
        {
            Debug.Log($"RecognitionTask Exception: {e.Message}");
        }
        //await SpeechClient.ShutdownDefaultChannelsAsync();
        streamingCall.GrpcCall.Dispose();

        return finalPhrase;
    }

    private void StartRecording()
    {
        // 録音開始
        _waveIn.StartRecording();

        // イベント通知
        OnStartRecording?.Invoke();
    }

    private void StreamingDataAvailable(string text)
    {
        //Debug.Log($"MicTextAvailable text: {text}");

        // 発話テキストをキューに追加
        try
        {
            RecognitionTextQueue.Enqueue(text);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        // イベント通知
        OnStreamingDataAvailable?.Invoke(text);
    }

    private void CreateChannel()
    {
        //ChannelOption[] channelOptions =
        //{
        //    new ChannelOption("grpc.keepalive_permit_without_calls", 1),
        //    new ChannelOption("grpc.keepalive_time_ms", 100),
        //    new ChannelOption("grpc.keepalive_timeout_ms", 100),
        //    new ChannelOption("grpc.http2.min_time_between_pings_ms", 300),
        //};
        //_channel = new Channel(SpeechClient.DefaultEndpoint.Host, _credential.ToChannelCredentials(), channelOptions);
        _channel = new Channel(SpeechClient.DefaultEndpoint.Host, _credential.ToChannelCredentials());
    }
}
*/

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;
using static SignageSettings;
using Cysharp.Threading.Tasks;

public class StreamingSpeechToText : SingletonBase<StreamingSpeechToText>
{
    /// <summary>
    /// 録音開始時コールバック
    /// </summary>
    public Action OnStartRecording = null;

    /// <summary>
    /// ストリーミング音声認識結果受信時コールバック
    /// </summary>
    public Action<string> OnStreamingDataAvailable = null;

    /// <summary>
    /// ストリーミング音声認識結果完了時コールバック
    /// </summary>
    public Action<string> OnStreamingDataComplete = null;

    /// <summary>
    /// ストリーミング音声認識結果格納用キュー
    /// </summary>
    public ConcurrentQueue<string> RecognitionTextQueue { get; private set; } = new ConcurrentQueue<string>();

    /// <summary>
    /// ストリーミング音声認識最終結果
    /// </summary>
    public string RecognitionCompleteText { get; private set; } = "";

    /// <summary>
    /// 音声認識言語コード
    /// </summary>
    public string SpeechToTextLanguageCode { get; set; } = "ja-JP";

    /// <summary>
    /// 音声認識エンコーディング
    /// </summary>
    //public RecognitionConfig.Types.AudioEncoding SpeechToTextAudioEncoding { get; set; } = RecognitionConfig.Types.AudioEncoding.Linear16;

    /// <summary>
    /// 音声認識サンプリングレート
    /// </summary>
    public int SpeechToTextSampleRateHertz { get; set; } = 16000;

    /*
    // ストリーミング音声認識用タスク
    private Task<string> RecognitionTask = null;

    // タスクキャンセル用
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private GoogleCredential _credential = null;
    private Channel _channel = null;

    private WaveInEvent _waveIn = null;
    */

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
    }

    /// <summary>
    /// 解放処理
    /// </summary>
    /// <returns></returns>
    public async UniTask Dispose()
    {
    }

    /// <summary>
    /// ストリーミング音声認識を専用タスクで実行する
    /// </summary>
    public async UniTask RunOneShotTask()
    {
    }

    /// <summary>
    /// ストリーミング音声認識をキャンセルする
    /// </summary>
    /// <returns></returns>
    public async UniTask CancelRecognition()
    {
    }

    /// <summary>
    /// ストリーミング音声認識完了時コールバック
    /// テキスト入力にも対応するため public メソッドとする
    /// </summary>
    /// <param name="text"></param>
    public void SetRecognitionCompleteText(string text)
    {
    }

    // ストリーミング音声認識を一度実行
    private async UniTask<string> OneShot(int recognitionTimeMs = 50000)
    {
        return null;
    }

    private void StartRecording()
    {
    }

    private void StreamingDataAvailable(string text)
    {
    }

    private void CreateChannel()
    {
    }
}