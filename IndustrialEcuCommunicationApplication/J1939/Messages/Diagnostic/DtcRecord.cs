using IECA.J1939.Utility;
using System;
using System.Collections.Generic;

namespace IECA.J1939.Messages.Diagnostic
{
    public class DtcRecord
    {
        public DtcRecord(byte lampStatus, DiagnosticTroubleCode dTC)
        {
            LampStatus = lampStatus;
            DTC = dTC ?? throw new ArgumentNullException(nameof(dTC));
        }

        public byte LampStatus { get; }

        public DiagnosticTroubleCode DTC { get; }

        public static DtcRecord FromRawBytes(List<byte> rawData)
        {
            var status = rawData[0];
            var dataConvertedToBits = Helpers.ConvertByteListToStringOfBits(rawData);
            var spnValue = Helpers.GetNumberValueFromBitArray(dataConvertedToBits, 16, 19, reverseEndian: false);
            var fmiValue = Helpers.GetNumberValueFromBitArray(dataConvertedToBits, 35, 5);
            var ocValue = Helpers.GetNumberValueFromBitArray(dataConvertedToBits, 41, 7);

            return new DtcRecord(status, new DiagnosticTroubleCode((uint)spnValue, (byte)fmiValue, (byte)ocValue));
        }
    }
}
