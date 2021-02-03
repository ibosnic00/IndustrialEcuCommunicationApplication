using System;
using System.Collections.Generic;

namespace IECA.J1939
{
    public class J1939Message : IEquatable<J1939Message>
    {
        public J1939Message(ProtocolDataUnit pdu, List<byte> data)
        {
            PDU = pdu ?? throw new ArgumentNullException(nameof(pdu));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public ProtocolDataUnit PDU { get; }
        public List<byte> Data { get; private set; }

        public void AddPacketizedData(List<byte> packetizedData)
        {
            Data.AddRange(packetizedData);
        }

        #region IEquatable implementation

        public bool Equals(J1939Message? other)
        {
            return PDU.ParameterGroupNumber == other?.PDU.ParameterGroupNumber
                && Equals(Data, other.Data);
        }

        public override bool Equals(object? obj)
        {
            return obj is J1939Message other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)PDU.ParameterGroupNumber;
                hashCode = (hashCode * 397) ^ (Data != null ? Data.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(J1939Message left, J1939Message right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(J1939Message left, J1939Message right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
