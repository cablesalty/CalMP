using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CalMP_Melon
{
    public enum PacketType : byte
    {
        Movement = 0,
        // Add chat and other stuff later
    }

    public struct PlayerStatePacket
    {
        public const int MinSerializedSize = 1 + (sizeof(float) * 4) + sizeof(bool);

        public float X;
        public float Y;
        public float VelocityX;
        public float VelocityY;
        public bool IsInLiquidForm;

        // I have no idea, don't ask me, this is completely written by gemini
        // Convert Struct -> Byte Array
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    // Write packet type first so the receiver knows what this data is
                    writer.Write((byte)PacketType.Movement);

                    writer.Write(X);
                    writer.Write(Y);
                    writer.Write(VelocityX);
                    writer.Write(VelocityY);
                    writer.Write(IsInLiquidForm);
                    return ms.ToArray();
                }
            }
        }

        // Convert Byte Array -> Struct (Skip the first byte because it's the PacketType)
        public static PlayerStatePacket Deserialize(byte[] data)
        {
            if (data == null || data.Length < MinSerializedSize)
            {
                throw new ArgumentException($"Movement packet must be at least {MinSerializedSize} bytes, got {data?.Length ?? 0}.", nameof(data));
            }

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    reader.ReadByte(); // Skip PacketType byte

                    PlayerStatePacket packet = new PlayerStatePacket();
                    packet.X = reader.ReadSingle();
                    packet.Y = reader.ReadSingle();
                    packet.VelocityX = reader.ReadSingle();
                    packet.VelocityY = reader.ReadSingle();
                    packet.IsInLiquidForm = reader.ReadBoolean();
                    return packet;
                }
            }
        }
    }
}
