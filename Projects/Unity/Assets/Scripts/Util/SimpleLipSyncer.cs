using UnityEngine;

using Live2D.Cubism.Core;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 口パクを行うクラス
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SimpleLipSyncer : MonoBehaviour
{
    [SerializeField] AudioSource audioSource = null;
    [SerializeField] CubismParameter MouthOpenParameter = null;
    [SerializeField] float Power = 20f;
    [SerializeField, Range(0f, 1f)] float Threshold = 0.1f;

    private float velocity = 0.0f;
    private float currentVolume = 0.0f;
    private float[] buffer = new float[256];

    private List<float> _audioBuffer = new List<float>();

    void Start()
    {
        if (MouthOpenParameter == null)
        {
            Debug.LogError("MouthOpenParameterが設定されていません");
        }
    }
    void LateUpdate()
    {
        //float targetVolume = GetAveragedVolume() * Power;
        float targetVolume = GetAveragedVolumeFromQueue() * Power;
        targetVolume = targetVolume < Threshold ? 0 : targetVolume;
        currentVolume = Mathf.SmoothDamp(currentVolume, targetVolume, ref velocity, 0.05f);

        // CubismParameterの更新はLateUpdate()内で行う必要がある点に注意
        MouthOpenParameter.Value = Mathf.Clamp01(currentVolume);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        _audioBuffer.AddRange(data);
    }

    private float GetAveragedVolume()
    {
        ClearBuffer();
        audioSource.GetOutputData(buffer, 0);
        float a = 0;
        foreach (float s in buffer)
        {
            a += Mathf.Abs(s);
        }
        return a / 255.0f;
    }

    private float GetAveragedVolumeFromQueue()
    {
        if (_audioBuffer.Count == 0) return 0;

        float a = 0;
        for (var i = 0; i < _audioBuffer.Count; i++)
        {
            a += Mathf.Abs(_audioBuffer[i]);
        }
        _audioBuffer.Clear();

        return a / 255.0f;
    }

    private void ClearBuffer()
    {
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = 0.0f;
        }
    }
}