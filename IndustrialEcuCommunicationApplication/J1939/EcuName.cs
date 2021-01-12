using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939
{
    public class EcuName
    {
        public EcuName(bool arbitraryAddressCapable, byte industryGroup, byte vehicleSystemInstance,
            byte vehicalSystem, bool reserved, byte function, byte functionInstance, byte ecuInstance,
            uint manufacturerCode, uint identityNumber)
        {
            ArbitraryAddressCapable = arbitraryAddressCapable;
            IndustryGroup = industryGroup;
            VehicleSystemInstance = vehicleSystemInstance;
            VehicalSystem = vehicalSystem;
            Reserved = reserved;
            Function = function;
            FunctionInstance = functionInstance;
            EcuInstance = ecuInstance;
            ManufacturerCode = manufacturerCode;
            IdentityNumber = identityNumber;
        }

        public bool ArbitraryAddressCapable { get; }
        public byte IndustryGroup { get; }
        public byte VehicleSystemInstance { get; }
        public byte VehicalSystem { get; }
        public bool Reserved { get; }
        public byte Function { get; }
        public byte FunctionInstance { get; }
        public byte EcuInstance { get; }
        public uint ManufacturerCode { get; }
        public uint IdentityNumber { get; }

        public ulong ToRawFormat()
        {
            ulong result = Convert.ToUInt64(ArbitraryAddressCapable) << 63;
            result |= Convert.ToUInt64(IndustryGroup) << 60;
            result |= Convert.ToUInt64(VehicleSystemInstance) << 56;
            result |= Convert.ToUInt64(VehicalSystem) << 49;
            result |= Convert.ToUInt64(Reserved) << 48;
            result |= Convert.ToUInt64(Function) << 40;
            result |= Convert.ToUInt64(FunctionInstance) << 35;
            result |= Convert.ToUInt64(EcuInstance) << 32;
            result |= Convert.ToUInt64(ManufacturerCode) << 21;
            result |= Convert.ToUInt64(IdentityNumber);

            return result;
        }

        public static EcuName FromRawFormat(ulong rawFromat)
        {

            bool arbitraryAddressCapable = Convert.ToBoolean(rawFromat >> 63);
            byte industryGroup = Convert.ToByte((rawFromat >> 60) & 0b0111);
            byte vehicleSystemInstance = Convert.ToByte((rawFromat >> 56) & 0b1111);
            byte vehicalSystem = Convert.ToByte((rawFromat >> 49) & 0b0111_1111);
            bool reserved = Convert.ToBoolean((rawFromat >> 48) & 0b0001);
            byte function = Convert.ToByte((rawFromat >> 40) & 0b1111_1111);
            byte functionInstance = Convert.ToByte((rawFromat >> 35) & 0b0001_1111);
            byte ecuInstance = Convert.ToByte((rawFromat >> 32) & 0b0111);
            uint manufacturerCode = Convert.ToUInt32((rawFromat >> 21) & 0b0111_1111_1111);
            uint identityNumber = Convert.ToUInt32(rawFromat & 0b0001_1111_1111_1111_1111_1111);

            return new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);
        }

    }
}
