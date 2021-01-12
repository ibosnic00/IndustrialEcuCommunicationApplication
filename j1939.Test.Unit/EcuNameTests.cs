using NUnit.Framework;
using IECA.J1939;

namespace j1939.Test.Unit
{
    class EcuNameTests
    {
        [Test]
        public void ToRawFormat_ArbitraryAddressCapableTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = true;
            byte industryGroup = 0;
            byte vehicleSystemInstance = 0;
            byte vehicalSystem = 0;
            bool reserved = false;
            byte function = 0;
            byte functionInstance = 0;
            byte ecuInstance = 0;
            uint manufacturerCode = 0;
            uint identityNumber = 0;

            var expected = 0b1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void ToRawFormat_IndustryGroupTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = false;
            byte industryGroup = 0b0111;
            byte vehicleSystemInstance = 0;
            byte vehicalSystem = 0;
            bool reserved = false;
            byte function = 0;
            byte functionInstance = 0;
            byte ecuInstance = 0;
            uint manufacturerCode = 0;
            uint identityNumber = 0;

            var expected = 0b0111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void ToRawFormat_VehicleSystemInstanceTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = false;
            byte industryGroup = 0;
            byte vehicleSystemInstance = 0b1111;
            byte vehicalSystem = 0;
            bool reserved = false;
            byte function = 0;
            byte functionInstance = 0;
            byte ecuInstance = 0;
            uint manufacturerCode = 0;
            uint identityNumber = 0;

            var expected = 0b0000_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void ToRawFormat_VehicalSystemTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = false;
            byte industryGroup = 0;
            byte vehicleSystemInstance = 0;
            byte vehicalSystem = 0b0111_1111;
            bool reserved = false;
            byte function = 0;
            byte functionInstance = 0;
            byte ecuInstance = 0;
            uint manufacturerCode = 0;
            uint identityNumber = 0;

            var expected = 0b0000_0000_1111_1110_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void ToRawFormat_ReservedTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = false;
            byte industryGroup = 0;
            byte vehicleSystemInstance = 0;
            byte vehicalSystem = 0;
            bool reserved = true;
            byte function = 0;
            byte functionInstance = 0;
            byte ecuInstance = 0;
            uint manufacturerCode = 0;
            uint identityNumber = 0;

            var expected = 0b0000_0000_0000_0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void ToRawFormat_FunctionTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = false;
            byte industryGroup = 0;
            byte vehicleSystemInstance = 0;
            byte vehicalSystem = 0;
            bool reserved = false;
            byte function = 0b1111_1111;
            byte functionInstance = 0;
            byte ecuInstance = 0;
            uint manufacturerCode = 0;
            uint identityNumber = 0;

            var expected = 0b0000_0000_0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void ToRawFormat_FunctionInstanceTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = false;
            byte industryGroup = 0;
            byte vehicleSystemInstance = 0;
            byte vehicalSystem = 0;
            bool reserved = false;
            byte function = 0;
            byte functionInstance = 0b1_1111;
            byte ecuInstance = 0;
            uint manufacturerCode = 0;
            uint identityNumber = 0;

            var expected = 0b0000_0000_0000_0000_0000_0000_1111_1000_0000_0000_0000_0000_0000_0000_0000_0000;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void ToRawFormat_EcuInstanceTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = false;
            byte industryGroup = 0;
            byte vehicleSystemInstance = 0;
            byte vehicalSystem = 0;
            bool reserved = false;
            byte function = 0;
            byte functionInstance = 0;
            byte ecuInstance = 0b0111;
            uint manufacturerCode = 0;
            uint identityNumber = 0;

            var expected = 0b0000_0000_0000_0000_0000_0000_0000_0111_0000_0000_0000_0000_0000_0000_0000_0000;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void ToRawFormat_ManufacturerCodeTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = false;
            byte industryGroup = 0;
            byte vehicleSystemInstance = 0;
            byte vehicalSystem = 0;
            bool reserved = false;
            byte function = 0;
            byte functionInstance = 0;
            byte ecuInstance = 0;
            uint manufacturerCode = 0b111_1111_1111;
            uint identityNumber = 0;

            var expected = 0b0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1110_0000_0000_0000_0000_0000;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void ToRawFormat_IdentityNumberTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = false;
            byte industryGroup = 0;
            byte vehicleSystemInstance = 0;
            byte vehicalSystem = 0;
            bool reserved = false;
            byte function = 0;
            byte functionInstance = 0;
            byte ecuInstance = 0;
            uint manufacturerCode = 0;
            uint identityNumber = 0b1_1111_1111_1111_1111_1111;

            var expected = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001_1111_1111_1111_1111_1111;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void ToRawFormat_MultipleParametersTest_SuccessfulConversion()
        {
            bool arbitraryAddressCapable = true;
            byte industryGroup = 0;
            byte vehicleSystemInstance = 0;
            byte vehicalSystem = 0b0111_1111;
            bool reserved = false;
            byte function = 0b1111_1111;
            byte functionInstance = 0;
            byte ecuInstance = 0b0111;
            uint manufacturerCode = 0;
            uint identityNumber = 0b1_1111_1111_1111_1111_1111;

            var expected = 0b1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            expected |= 0b0000_0000_1111_1110_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            expected |= 0b0000_0000_0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            expected |= 0b0000_0000_0000_0000_0000_0000_0000_0111_0000_0000_0000_0000_0000_0000_0000_0000;
            expected |= 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001_1111_1111_1111_1111_1111;

            var j1939Name = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
            vehicalSystem, reserved, function, functionInstance, ecuInstance,
            manufacturerCode, identityNumber);

            var received = j1939Name.ToRawFormat();

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_ArbitraryAddressCapableTest_SuccessfulConversion()
        {
            bool expected = true;

            var arbitraryAddressCapableRaw = 0b1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var received = EcuName.FromRawFormat(arbitraryAddressCapableRaw).ArbitraryAddressCapable;

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_IndustryGroupTest_SuccessfulConversion()
        {
            byte expected = 0b0111;

            ulong industryGroupRaw = 0b0111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var received = EcuName.FromRawFormat(industryGroupRaw).IndustryGroup;

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_VehicleSystemInstanceTest_SuccessfulConversion()
        {
            var expected = 0b1111;

            ulong vehicleSystemInstanceRaw = 0b0000_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var received = EcuName.FromRawFormat(vehicleSystemInstanceRaw).VehicleSystemInstance;

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_VehicalSystemTest_SuccessfulConversion()
        {
            byte expected = 0b0111_1111;

            ulong raw = 0b0000_0000_1111_1110_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var received = EcuName.FromRawFormat(raw).VehicalSystem;

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_ReservedTest_SuccessfulConversion()
        {
            bool expected = true;

            ulong raw = 0b0000_0000_0000_0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var received = EcuName.FromRawFormat(raw).Reserved;

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_FunctionTest_SuccessfulConversion()
        {
            byte expected = 0b1111_1111;

            ulong raw = 0b0000_0000_0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            var received = EcuName.FromRawFormat(raw).Function;

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_FunctionInstanceTest_SuccessfulConversion()
        {
            byte expected = 0b1_1111;

            ulong raw = 0b0000_0000_0000_0000_0000_0000_1111_1000_0000_0000_0000_0000_0000_0000_0000_0000;

            var received = EcuName.FromRawFormat(raw).FunctionInstance;

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_EcuInstanceTest_SuccessfulConversion()
        {
            byte expected = 0b0111;

            ulong raw = 0b0000_0000_0000_0000_0000_0000_0000_0111_0000_0000_0000_0000_0000_0000_0000_0000;

            var received = EcuName.FromRawFormat(raw).EcuInstance;

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_ManufacturerCodeTest_SuccessfulConversion()
        {
            uint expected = 0b111_1111_1111;

            ulong raw = 0b0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1110_0000_0000_0000_0000_0000;

            var received = EcuName.FromRawFormat(raw).ManufacturerCode;

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_IdentityNumberTest_SuccessfulConversion()
        {
            uint expected = 0b1_1111_1111_1111_1111_1111;

            ulong raw = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001_1111_1111_1111_1111_1111;

            var received = EcuName.FromRawFormat(raw).IdentityNumber;

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void FromRawFormat_MultipleParametersTest_SuccessfulConversion()
        {
            bool expectedArbitraryAddressCapable = true;
            byte expectedVehicalSystem = 0b0111_1111;
            byte expectedFunction = 0b1111_1111;
            byte expectedEcuInstance = 0b0111;
            uint expectedIdentityNumber = 0b1_1111_1111_1111_1111_1111;

            ulong raw = 0b1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            raw |= 0b0000_0000_1111_1110_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            raw |= 0b0000_0000_0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            raw |= 0b0000_0000_0000_0000_0000_0000_0000_0111_0000_0000_0000_0000_0000_0000_0000_0000;
            raw |= 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001_1111_1111_1111_1111_1111;

            var received = EcuName.FromRawFormat(raw);

            Assert.AreEqual(expectedArbitraryAddressCapable, received.ArbitraryAddressCapable);
            Assert.AreEqual(expectedVehicalSystem, received.VehicalSystem);
            Assert.AreEqual(expectedFunction, received.Function);
            Assert.AreEqual(expectedEcuInstance, received.EcuInstance);
            Assert.AreEqual(expectedIdentityNumber, received.IdentityNumber);
        }
    }
}
