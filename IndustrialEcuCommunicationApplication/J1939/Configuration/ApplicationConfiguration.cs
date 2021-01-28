using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Configuration
{
    public class ApplicationConfiguration
    {
        public ApplicationConfiguration(byte wantedAddress, byte minAddress, byte maxAddress,
            string dataConfigurationPath, int addressClaimWaitPeriodMs, bool arbitraryAddressCapable, byte industryGroup,
            byte vehicleSystemInstance, byte vehicalSystem, byte function, byte functionInstance,
            byte ecuInstance, uint manufacturerCode, uint identityNumber,
            List<ConfigurationRequestPgn> pgnsForRequesting)
        {
            WantedAddress = wantedAddress;
            MinAddress = minAddress;
            MaxAddress = maxAddress;
            DataConfigurationPath = dataConfigurationPath ?? throw new ArgumentNullException(nameof(dataConfigurationPath));
            AddressClaimWaitPeriodMs = addressClaimWaitPeriodMs;
            ArbitraryAddressCapable = arbitraryAddressCapable;
            IndustryGroup = industryGroup;
            VehicleSystemInstance = vehicleSystemInstance;
            VehicalSystem = vehicalSystem;
            Function = function;
            FunctionInstance = functionInstance;
            EcuInstance = ecuInstance;
            ManufacturerCode = manufacturerCode;
            IdentityNumber = identityNumber;
            PgnsForRequesting = pgnsForRequesting ?? throw new ArgumentNullException(nameof(pgnsForRequesting));
        }

        public byte WantedAddress { get; }
        public byte MinAddress { get; }
        public byte MaxAddress { get; }
        public string DataConfigurationPath { get; }
        public int AddressClaimWaitPeriodMs { get; }
        public bool ArbitraryAddressCapable { get; }
        public byte IndustryGroup { get; }
        public byte VehicleSystemInstance { get; }
        public byte VehicalSystem { get; }
        public byte Function { get; }
        public byte FunctionInstance { get; }
        public byte EcuInstance { get; }
        public uint ManufacturerCode { get; }
        public uint IdentityNumber { get; }
        public List<ConfigurationRequestPgn> PgnsForRequesting { get; }
    }
}
