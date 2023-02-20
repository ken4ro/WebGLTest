using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class MuLawCodec
{
    public static byte[] Encode(byte[] data, int offset, int length)
    {
        var encoded = new byte[length / 2];
        int outIndex = 0;
        for (var n = 0; n < length; n += 2)
        {
            encoded[outIndex++] = MuLawEncoder.LinearToMuLawSample(BitConverter.ToInt16(data, offset + n));
        }
        return encoded;
    }

    public static byte[] Decode(byte[] data, int offset, int length)
    {
        var decoded = new byte[length * 2];
        int outIndex = 0;
        for (var n = 0; n < length; n++)
        {
            var decodedSample = MuLawDecoder.MuLawToLinearSample(data[n + offset]);
            decoded[outIndex++] = (byte)(decodedSample & 0xFF);
            decoded[outIndex++] = (byte)(decodedSample >> 8);
        }
        return decoded;
    }
}
*/

public class MuLawCodec
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