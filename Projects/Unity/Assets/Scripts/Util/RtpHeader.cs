using System;


public class RtpHeader
{
    public static readonly int HeaderSize = 12;
    public static readonly byte RTPVersion = 2;

    public byte Version { get; private set; } = RTPVersion; // 2bits
    public byte Padding { get; private set; } = 0; // 1bit
    public byte Extension { get; private set; } = 0; // 1bit
    public byte CsrcCount { get; private set; } = 0; // 4bits
    public byte Marker { get; private set; } = 0; // 1bit
    public byte PayloadType { get; private set; } = 0; // 7bits
    public ushort SequenceNumber { get; private set; } = 0; // 16bits
    public uint TimeStamp { get; private set; } = 0; // 32bits
    public uint Ssrc { get; private set; } = 0; // 32bits

    private byte[] _header = new byte[HeaderSize];

    public RtpHeader()
    {
    }

    public RtpHeader(byte[] packet)
    {
        if (packet.Length < HeaderSize)
        {
            throw new ApplicationException("Invalid RTP packet");
        }

        var start = BitConverter.ToUInt16(packet, 0);
        SequenceNumber = BitConverter.ToUInt16(packet, 2);
        TimeStamp = BitConverter.ToUInt32(packet, 4);
        Ssrc = BitConverter.ToUInt32(packet, 8);

        if (BitConverter.IsLittleEndian)
        {
            start = NetworkHelper.ReverseEndian(start);
            SequenceNumber = NetworkHelper.ReverseEndian(SequenceNumber);
            TimeStamp = NetworkHelper.ReverseEndian(TimeStamp);
            Ssrc = NetworkHelper.ReverseEndian(Ssrc);
        }

        Version = (byte)(start >> 14);
        Padding = (byte)((start >> 13) & 0x1);
        Extension = (byte)((start >> 12) & 0x1);
        CsrcCount = (byte)(start & 0xf); // ?
        Marker = (byte)((start >> 9) & 0x1); // ?
        PayloadType = (byte)(start & 0x7f); 
    }

    public byte[] GetHeader(ushort sequenceNumber)
    {
        SequenceNumber = sequenceNumber;

        return GetHeader();
    }

    public byte[] GetHeader()
    {
        var start = Convert.ToUInt16(Version * 16384 + Padding * 8192 + Extension * 4096 + CsrcCount * 256 + Marker * 128 + PayloadType);
        if (BitConverter.IsLittleEndian)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(NetworkHelper.ReverseEndian(start)), 0, _header, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(NetworkHelper.ReverseEndian(SequenceNumber)), 0, _header, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(NetworkHelper.ReverseEndian(TimeStamp)), 0, _header, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(NetworkHelper.ReverseEndian(Ssrc)), 0, _header, 8, 4);
        }
        else
        {
            Buffer.BlockCopy(BitConverter.GetBytes(start), 0, _header, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(SequenceNumber), 0, _header, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(TimeStamp), 0, _header, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Ssrc), 0, _header, 8, 4);
        }
        return _header;
    }
}
