using System;
using System.Collections.Generic;
using System.Text;

namespace IECA
{
    public readonly struct CanMessage : IEquatable<CanMessage>
    {
        public const uint MAX_11_BIT_CAN_ID_LEN = 3;
        public const uint MAX_29_BIT_CAN_ID = 0x1fffffff;

        public const uint MAX_CAN_MESSAGE_SIZE = 8u;

        public CanMessage(uint id, uint idLen, byte dlc, byte[]? data, CanMessageType messageType)
        {
            if (dlc > MAX_CAN_MESSAGE_SIZE)
                throw new ArgumentOutOfRangeException(nameof(dlc));
            else if (id > MAX_29_BIT_CAN_ID)
                throw new ArgumentOutOfRangeException(nameof(id));

            ID = id;
            DLC = dlc;
            Data = data;
            MessageType = messageType;
            IsExtendedId = idLen > MAX_11_BIT_CAN_ID_LEN;
        }

        public uint ID { get; }

        public byte DLC { get; }

        public byte[]? Data { get; }

        public CanMessageType MessageType { get; }

        public bool IsExtendedId { get; }

        #region IEquatable implementation

        public bool Equals(CanMessage other)
        {
            return ID == other.ID && DLC == other.DLC && Equals(Data, other.Data) && MessageType == other.MessageType;
        }

        public override bool Equals(object? obj)
        {
            return obj is CanMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)ID;
                hashCode = (hashCode * 397) ^ DLC.GetHashCode();
                hashCode = (hashCode * 397) ^ (Data != null ? Data.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)MessageType;
                return hashCode;
            }
        }

        public static bool operator ==(CanMessage left, CanMessage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CanMessage left, CanMessage right)
        {
            return !left.Equals(right);
        }

        #endregion
    }

    public enum CanMessageType : byte
    {
        Data,
        RemoteRequest,
        Error
    }
}
