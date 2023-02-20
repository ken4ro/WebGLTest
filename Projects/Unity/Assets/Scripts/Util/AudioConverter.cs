using System;

using UnityEngine;


public static class AudioConverter
{
    /// <summary>
    /// オーディオバッファ(float)をバイト配列へ変換
    /// </summary>
    /// <param name="clip"></param>
    /// <returns></returns>
    public static byte[] ConvertToLinear16(AudioClip clip)
    {
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        byte[] ret = new byte[clip.samples * 2];    //16Bit

        int idx = 0;
        for (int i = 0; i < clip.samples; i++)
        {
            int d = Math.Min((int)(samples[i] * 32768), 32767);
            ret[idx++] = (byte)(d & 255);
            d >>= 8;
            ret[idx++] = (byte)(d & 255);
        }

        return ret;
    }
}
