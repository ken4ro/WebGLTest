using System;
using System.Collections.Concurrent;
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
    public string RecognitionCompleteText { get; set; } = "";

    /// <summary>
    /// 音声認識言語コード
    /// </summary>
    public string SpeechToTextLanguageCode { get; set; } = "ja-JP";

    /// <summary>
    /// 音声認識サンプリングレート
    /// </summary>
    public int SpeechToTextSampleRateHertz { get; set; } = 16000;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
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

}