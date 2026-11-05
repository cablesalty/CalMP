using System;
using System.IO;

namespace CalMP_Melon
{
    public enum PacketType : byte
    {
        Movement = 0,
    }

    public struct CatPartPosition
    {
        public float X;
        public float Y;
        public bool IsPresent;
    }

    public struct PlayerStatePacket
    {
        public const int MaxCatParts = 12;
        public const int MinSerializedSize = 1 + 1 + sizeof(ushort) + 1 + 1;

        private const byte JumpingFlag = 1;
        private const byte LiquidFlag = 2;

        public CatPartPosition[] CatParts;
        public sbyte MovementDirection;
        public bool IsJumping;
        public bool IsInLiquidForm;

        public byte[] Serialize()
        {
            CatPartPosition[] parts = CatParts ?? Array.Empty<CatPartPosition>();
            int partCount = Math.Min(parts.Length, MaxCatParts);
            ushort presentParts = 0;
            int presentPartCount = 0;

            for (int i = 0; i < partCount; i++)
            {
                if (!parts[i].IsPresent)
                    continue;

                presentParts |= (ushort)(1 << i);
                presentPartCount++;
            }

            using (MemoryStream stream = new MemoryStream(MinSerializedSize + presentPartCount * 8))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketType.Movement);
                writer.Write((byte)partCount);
                writer.Write(presentParts);

                for (int i = 0; i < partCount; i++)
                {
                    if (!parts[i].IsPresent)
                        continue;

                    writer.Write(parts[i].X);
                    writer.Write(parts[i].Y);
                }

                writer.Write(MovementDirection);
                byte flags = 0;
                if (IsJumping)
                    flags |= JumpingFlag;
                if (IsInLiquidForm)
                    flags |= LiquidFlag;
                writer.Write(flags);
                return stream.ToArray();
            }
        }

        public static PlayerStatePacket Deserialize(byte[] data)
        {
            if (data == null || data.Length < MinSerializedSize)
            {
                throw new ArgumentException(
                    $"Movement packet must be at least {MinSerializedSize} bytes, got {data?.Length ?? 0}.",
                    nameof(data));
            }

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                if (reader.ReadByte() != (byte)PacketType.Movement)
                    throw new ArgumentException("Unsupported movement packet type.", nameof(data));

                int partCount = reader.ReadByte();
                if (partCount > MaxCatParts)
                    throw new ArgumentException($"Movement packet contains {partCount} cat parts.", nameof(data));

                ushort presentParts = reader.ReadUInt16();
                ushort validPartMask = (ushort)((1 << partCount) - 1);
                if ((presentParts & ~validPartMask) != 0)
                    throw new ArgumentException("Movement packet contains an invalid cat-part mask.", nameof(data));

                int presentPartCount = CountSetBits(presentParts);
                int expectedSize = MinSerializedSize + presentPartCount * 8;
                if (data.Length != expectedSize)
                {
                    throw new ArgumentException(
                        $"Movement packet must be {expectedSize} bytes, got {data.Length}.",
                        nameof(data));
                }

                CatPartPosition[] parts = new CatPartPosition[MaxCatParts];
                for (int i = 0; i < partCount; i++)
                {
                    if ((presentParts & (1 << i)) == 0)
                        continue;

                    parts[i] = new CatPartPosition
                    {
                        X = reader.ReadSingle(),
                        Y = reader.ReadSingle(),
                        IsPresent = true,
                    };
                }

                sbyte movementDirection = reader.ReadSByte();
                if (movementDirection < -1 || movementDirection > 1)
                    throw new ArgumentException("Movement direction must be -1, 0, or 1.", nameof(data));

                byte flags = reader.ReadByte();
                if ((flags & ~(JumpingFlag | LiquidFlag)) != 0)
                    throw new ArgumentException("Movement packet contains unknown control flags.", nameof(data));

                return new PlayerStatePacket
                {
                    CatParts = parts,
                    MovementDirection = movementDirection,
                    IsJumping = (flags & JumpingFlag) != 0,
                    IsInLiquidForm = (flags & LiquidFlag) != 0,
                };
            }
        }

        private static int CountSetBits(ushort value)
        {
            int count = 0;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }

            return count;
        }
    }
}
