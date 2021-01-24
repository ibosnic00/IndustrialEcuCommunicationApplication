using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939
{

    public class ProtocolDataUnit
    {
        public ProtocolDataUnit(byte priority, byte dataPage, byte format,
            byte specific, byte sourceAddress, bool reserved = false)
        {
            Priority = priority;
            DataPage = dataPage;
            Format = format;
            Specific = SolvePDUSpecific(specific);
            SourceAddress = sourceAddress;
            ParameterGroupNumber = CalculatePgn();
            Reserved = reserved;
        }


        #region Properties

        public byte Priority { get; }
        public byte DataPage { get; }
        public byte Format { get; }
        public IPDUSpecific Specific { get; }
        public byte SourceAddress { get; }
        public uint ParameterGroupNumber { get; }
        public bool Reserved { get; } // Extended data page. J1939 devices must set to 0.

        #endregion


        #region Public Methods

        public uint ToCanExtIdentifierFormat()
        {
            uint canExtIdentifier = Convert.ToUInt32(Priority) << 26;
            canExtIdentifier |= Convert.ToUInt32(Reserved) << 25;
            canExtIdentifier |= Convert.ToUInt32(DataPage) << 24;
            canExtIdentifier |= Convert.ToUInt32(Format) << 16;
            canExtIdentifier |= Convert.ToUInt32(Specific.Value) << 8;
            canExtIdentifier |= Convert.ToUInt32(SourceAddress);

            return canExtIdentifier;
        }

        public static ProtocolDataUnit FromCanExtIdentifierFormat(uint canExtId)
        {
            byte priority = Convert.ToByte(canExtId >> 26 & 0b0111);
            byte dataPage = Convert.ToByte(canExtId >> 24 & 0b0001);
            byte format = Convert.ToByte(canExtId >> 16 & 0b1111_1111);
            byte specific = Convert.ToByte(canExtId >> 8 & 0b1111_1111);
            byte sourceAddress = Convert.ToByte(canExtId & 0b1111_1111);

            return new ProtocolDataUnit(priority, dataPage, format, specific, sourceAddress);
        }

        #endregion


        #region Private Methods

        private uint CalculatePgn()
        {
            uint result;
            if (Format < 240)
                result = (uint)((DataPage << 16) + (Format << 8));
            else
                result = (uint)((DataPage << 16) + (Format << 8) + Specific.Value);

            return result;
        }

        private IPDUSpecific SolvePDUSpecific(byte value)
        {
            IPDUSpecific result;
            if (Format < 240)
                result = new DestinationAddress(value);
            else
                result = new GroupExtension(value);

            return result;
        }

        #endregion
    }

    /// <summary>
    /// PDU format:
    ///     < 240, IPDUSpecific is destination address. (PDU1 format)
    ///     >= 240, IPDUSpecific is group extension. (PDU2 format)
    /// </summary>
    public interface IPDUSpecific
    {
        public byte Value { get; }
    }

    public class DestinationAddress : IPDUSpecific
    {
        public DestinationAddress(byte value)
        {
            Value = value;
        }

        public byte Value { get; }
    }

    public class GroupExtension : IPDUSpecific
    {
        public GroupExtension(byte value)
        {
            Value = value;
        }

        public byte Value { get; }
    }
}
