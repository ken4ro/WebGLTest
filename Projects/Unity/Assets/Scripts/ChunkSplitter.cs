using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class ChunkSplitter
{
    private uint packetId;

    public enum EventType
    {
        None = -1,
        FrameCompleted = 0,
        PacketLoss = 1,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ChunkHeader
    {
        public uint Id;
        public int Type;
        public int TotalSize;
        public int ChunkCount;
        public int ChunkIndex;
        public int ChunkSize;
    }

    public int MaxPacketSize { get; set; } = 9000;

    public ChunkSplitter()
    {
    }

    public List<byte[]> Split(byte[] data)
    {
        var ret = new List<byte[]>();
        var chunkCount = data.Length / MaxPacketSize + 1;
        for (var i = 0; i < chunkCount; i++)
        {
            // チャンクヘッダ作成
            int chunkSize;
            byte[] chunkData;
            if (chunkCount == 1)
            {
                chunkSize = data.Length;
                chunkData = data;
            }
            else if (i == chunkCount - 1)
            {
                chunkSize = data.Length % MaxPacketSize;
                chunkData = new byte[chunkSize];
                Buffer.BlockCopy(data, i * MaxPacketSize, chunkData, 0, chunkSize);
            }
            else
            {
                chunkSize = MaxPacketSize;
                chunkData = new byte[chunkSize];
                Buffer.BlockCopy(data, i * MaxPacketSize, chunkData, 0, chunkSize);
            }
            var chunkHeader = new ChunkHeader()
            {
                Id = packetId,
                TotalSize = data.Length,
                ChunkCount = chunkCount,
                ChunkIndex = i,
                ChunkSize = chunkSize
            };
            //UnityEngine.Debug.Log($"chunk header id = {chunkHeader.Id}, index = {chunkHeader.ChunkIndex}");

            // チャンクヘッダをbyte配列へ変換
            var chunkHeaderBytes = ChunkHeaderToBytes(chunkHeader);
            var chunkHeaderSize = chunkHeaderBytes.Length;

            // チャンク作成
            var chunk = new byte[chunkHeaderSize + chunkSize];
            Buffer.BlockCopy(chunkHeaderBytes, 0, chunk, 0, chunkHeaderSize);
            Buffer.BlockCopy(chunkData, 0, chunk, chunkHeaderSize, chunkSize);
            ret.Add(chunk);
        }

        packetId++;
        return ret;
    }

    public void ParseChunkHeader(byte[] data, ref ChunkHeader chunkHeader)
    {
        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        chunkHeader = (ChunkHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ChunkHeader));
        handle.Free();
    }

    public byte[] GetChunkData(byte[] data)
    {
        var chunkHeaderSize = Marshal.SizeOf(typeof(ChunkHeader));
        var ret = new byte[data.Length - chunkHeaderSize];
        Buffer.BlockCopy(data, chunkHeaderSize, ret, 0, ret.Length);
        return ret;
    }

    private byte[] ChunkHeaderToBytes(ChunkHeader ch)
    {
        var size = Marshal.SizeOf(typeof(ChunkHeader));
        var ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(ch, ptr, false);

        var bytes = new byte[size];
        Marshal.Copy(ptr, bytes, 0, size);
        Marshal.FreeHGlobal(ptr);
        return bytes;
    }
}
