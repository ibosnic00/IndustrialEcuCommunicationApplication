using IECA.J1939.Configuration;
using IECA.J1939.Utility;
using System.Collections.Generic;

namespace IECA.J1939.Messages
{
    public class AddressClaimedMessage : J1939Message
    {
        private const int ADDR_CLAIM_PRIORITY = 6;
        private const int ADDR_CLAIM_DATA_PAGE = 0;
        private const int ADDR_CLAIM_FORMAT = 238;

        public AddressClaimedMessage(ProtocolDataUnit receivedPDU, List<byte> data)
            : base(receivedPDU, data)
        {
            EcuName = EcuName.FromRawFormat(Helpers.GetRawEcuName(data) ?? 0);
        }

        public AddressClaimedMessage(ulong ecuName, byte sourceAddress)
            : base(new ProtocolDataUnit(priority: ADDR_CLAIM_PRIORITY,
                                         dataPage: ADDR_CLAIM_DATA_PAGE,
                                         format: ADDR_CLAIM_FORMAT,
                                         specific: StandardData.GLOBAL_DESTINATION_ADDRESS,
                                         sourceAddress: sourceAddress),
                                         Helpers.ConvertEcuNameToList(ecuName))
        {
            EcuName = EcuName.FromRawFormat(ecuName);
        }

        public EcuName EcuName { get; }
    }
}
