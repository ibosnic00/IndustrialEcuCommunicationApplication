using IECA.J1939.Utility;
using System;
using System.Collections.Generic;

namespace IECA.J1939.Messages.Diagnostic
{
    public class DtcRecord
    {
        public const uint DEF_VALUE = 0;
        public const uint SPN_START_INDEX = 16;
        public const uint SPN_LEN = 19;
        public const uint FMI_START_INDEX = 35;
        public const uint FMI_LEN = 5;
        public const uint OC_START_INDEX = 41;
        public const uint OC_LEN = 7;

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
            var spnValue = Helpers.GetNumberValueFromBitArray(dataConvertedToBits, SPN_START_INDEX, SPN_LEN, reverseEndian: false);
            var fmiValue = Helpers.GetNumberValueFromBitArray(dataConvertedToBits, FMI_START_INDEX, FMI_LEN);
            var ocValue = Helpers.GetNumberValueFromBitArray(dataConvertedToBits, OC_START_INDEX, OC_LEN);

            return new DtcRecord(status, new DiagnosticTroubleCode(spnValue ?? DEF_VALUE, (byte)(fmiValue ?? DEF_VALUE), (byte)(ocValue ?? DEF_VALUE)));
        }
    }
}
