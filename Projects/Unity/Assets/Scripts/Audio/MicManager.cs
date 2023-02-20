using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class MicManager : SingletonBase<MicManager>
{
    /// <summary>
    /// マイク入力データを受け取る
    /// </summary>
    public Action<byte[]> OnMicDataAvailable = null;

    /// <summary>
    /// 録音中かどうか
    /// </summary>
    public bool IsRecording { get; private set; } = false;

    private static readonly int Channels = 1;
    private static readonly int DelayMilliseconds = 100;

    private bool _isInitialized = false;
    private WaveInEvent _waveIn = null;

    private long _sendTimeStamp = 0;
    private int _sampleRate = 0;

    /// <summary>
    /// マイク初期化
    /// </summary>
    public void Initialize()
    {
        Microphone.GetDeviceCaps(Microphone.devices[0], out int minFreq, out int maxFreq);
        Debug.Log($"Microphone device name = {Microphone.devices[0]}, min freq = {minFreq}, max freq = {maxFreq}");
        _sampleRate = minFreq;

        _waveIn = new WaveInEvent
        {
            DeviceNumber = 0,
            WaveFormat = new WaveFormat(_sampleRate, Channels)
        };
        _waveIn.BufferMilliseconds = DelayMilliseconds;
        _waveIn.DataAvailable += MicDataAvailable;
        _isInitialized = true;
    }

    /// <summary>
    /// マイク処理終了
    /// </summary>
    public void Quit()
    {
        if (!_isInitialized) return;

        _waveIn.DataAvailable -= MicDataAvailable;
        _waveIn.Dispose();
        _isInitialized = false;
    }

    /// <summary>
    /// 録音開始
    /// </summary>
    public void StartRecording()
    {
        if (!_isInitialized) return;

        IsRecording = true;
        _sendTimeStamp = 0;

        _waveIn.StartRecording();
    }

    /// <summary>
    /// 録音終了
    /// </summary>
    public void StopRecording()
    {
        if (!_isInitialized) return;

        IsRecording = false;

        _waveIn.StopRecording();
    }

    private void MicDataAvailable(object sender, WaveInEventArgs args)
    {
        if (!_isInitialized) return;

        // タイムスタンプ付与
        _sendTimeStamp = _sendTimeStamp <= 0 ? DateTimeOffset.Now.ToUnixTimeMilliseconds() : _sendTimeStamp + DelayMilliseconds;
        byte[] timeStamp = BitConverter.GetBytes(_sendTimeStamp);

        // サンプリングレート付与
        byte[] sampleRate = BitConverter.GetBytes(_sampleRate);

        // 指定したコーデックでエンコード
        byte[] encoded;
        switch (GlobalState.Instance.ApplicationGlobalSettings.AudioCodec)
        {
            case ApplicationSettings.AudioCodecType.MuLaw:
                encoded = MuLawCodec.Encode(args.Buffer, 0, args.BytesRecorded);
                break;
            case ApplicationSettings.AudioCodecType.G722:
                encoded = G722Codec.Encode(args.Buffer, 0, args.BytesRecorded);
                break;
            default:
                encoded = MuLawCodec.Encode(args.Buffer, 0, args.BytesRecorded);
                break;
        }

        // 送信バッファ生成
        byte[] micBuffer = new byte[timeStamp.Length + sampleRate.Length + encoded.Length];
        Array.Copy(timeStamp, 0, micBuffer, 0, timeStamp.Length);
        Array.Copy(sampleRate, 0, micBuffer, timeStamp.Length, sampleRate.Length);
        Array.Copy(encoded, 0, micBuffer, timeStamp.Length + sampleRate.Length, encoded.Length);

        OnMicDataAvailable?.Invoke(micBuffer);
    }
}
*/

public class MicManager : SingletonBase<MicManager>
{
    /// <summary>
    /// マイク入力データを受け取る
    /// </summary>
    public Action<byte[]> OnMicDataAvailable = null;

    /// <summary>
    /// 録音中かどうか
    /// </summary>
    public bool IsRecording { get; private set; } = false;

    /// <summary>
    /// マイク初期化
    /// </summary>
    public void Initialize()
    {
    }

    /// <summary>
    /// マイク処理終了
    /// </summary>
    public void Quit()
    {
    }

    /// <summary>
    /// 録音開始
    /// </summary>
    public void StartRecording()
    {
    }

    /// <summary>
    /// 録音終了
    /// </summary>
    public void StopRecording()
    {
    }
}