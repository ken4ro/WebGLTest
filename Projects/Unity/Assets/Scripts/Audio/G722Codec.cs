using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
public class G722Codec
{
    static readonly int Bitrate = 64000;

    static readonly G722CodecState _encoderState = new G722CodecState(Bitrate, G722Flags.None);
    static G722CodecState _decoderState = new G722CodecState(Bitrate, G722Flags.None);
    static NAudio.Codecs.G722Codec _codec = new NAudio.Codecs.G722Codec();

    public static byte[] Encode(byte[] data, int offset, int length)
    {
        if (offset != 0)
        {
            Debug.LogError("オフセット値は未サポート");
            return null;
        }

        var wb = new WaveBuffer(data);
        var encodedLength = length / 4;
        var outputBuffer = new byte[encodedLength];

        int encoded = _codec.Encode(_encoderState, outputBuffer, wb.ShortBuffer, length / 2);

        return outputBuffer;
    }

    public static byte[] Decode(byte[] data, int offset, int length)
    {
        if (offset != 0)
        {
            Debug.LogError("オフセット値は未サポート");
            return null;
        }

        int decodedLength = length * 4;
        var outputBuffer = new byte[decodedLength];
        var wb = new WaveBuffer(outputBuffer);
        int decoded = _codec.Decode(_decoderState, wb.ShortBuffer, data, length);

        return outputBuffer;
    }
}
*/

public class G722Codec
{

    public static byte[] Encode(byte[] data, int offset, int length)
    {
        return null;
    }

    public static byte[] Decode(byte[] data, int offset, int length)
    {
        return null;
    }
}