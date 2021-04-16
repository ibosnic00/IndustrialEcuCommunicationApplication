using IECA.CANBus;
using IECA.J1939.Configuration;
using IECA.J1939.Utility;
using System.Linq;

namespace IECA.J1939.Messages
{
    public class CannotClaimAddressMessage : J1939Message
    {
        private const int ADDR_CLAIM_PRIORITY = 6;
        private const int ADDR_CLAIM_DATA_PAGE = 0;
        private const int ADDR_CLAIM_FORMAT = 238;

        public CannotClaimAddressMessage(CanMessage canMessage)
            : base(ProtocolDataUnit.FromCanExtIdentifierFormat(canMessage.ID), canMessage.Data!.ToList())
        {
            EcuName = EcuName.FromRawFormat(Helpers.GetRawEcuName(canMessage.Data.ToList()) ?? 0);
        }

        public CannotClaimAddressMessage(ulong ecuName)
            : base(new ProtocolDataUnit(priority: ADDR_CLAIM_PRIORITY,
                                         dataPage: ADDR_CLAIM_DATA_PAGE,
                                         format: ADDR_CLAIM_FORMAT,
                                         specific: StandardData.GLOBAL_DESTINATION_ADDRESS,
                                         sourceAddress: StandardData.NULL_ADDRESS),
                                         Helpers.ConvertEcuNameToList(ecuName))
        {
            EcuName = EcuName.FromRawFormat(ecuName);
        }

        public EcuName EcuName { get; }
    }
}
