using NUnit.Framework;
using IECA;
using IECA.J1939;

namespace j1939.Test.Unit
{
    public class J1939ToJsonConverterTest
    {
        J1939ToJsonConverter? J1939ToJsonConverter;
        private readonly string CONFIGURATION_PATH = @"C:\Users\ivan.bosnic\source\repos\IndustrialEcuCommunicationApplication\IndustrialEcuCommunicationApplication\Resources\ieca_configuration.json";

        [SetUp]
        public void Setup()
        {
            var configuration = ConfigurationDeserializer.GetConfigurationFromFile(CONFIGURATION_PATH);
            J1939ToJsonConverter = new J1939ToJsonConverter(configuration);
        }

        [Test]
        public void ConvertJ1939MessageToHumanReadableFormat_Pdu0_SuccessfullConversion()
        {
            var pdu = new ProtocolDataUnit(0, 0, 0, 0, 0);
            var j1939Message = new J1939Message(pdu,
                new System.Collections.Generic.List<byte> { 0xFF, 0xFF, 0xFF, 0xFF });
            var parsedMessage = J1939ToJsonConverter.ConvertJ1939MessageToHumanReadableFormat(j1939Message);
            var expectedMessage = "Torque/Speed Control 1\nOverride Control Mode:[11]\nRequested Speed Control Conditions: [11]\nOverride Control Mode Priority: [11]\nRequested Speed/Speed Limit: 8191.875\nRequested Torque/Torque Limit: 130";

            Assert.IsTrue(string.Compare(expectedMessage, parsedMessage) == 1);
        }

        [Test]
        public void ConvertJ1939MessageToHumanReadableFormat_Pdu0xCF00_SuccessfullConversion()
        {
            var pdu = new ProtocolDataUnit(6, 0, 0xCF, 0, 0x21);
            var j1939Message = new J1939Message(pdu,
                new System.Collections.Generic.List<byte> { 0x01, 0x55, 0x12, 0xAA, 0x02, 0x55, 0x0A, 0xEE });
            var parsedMessage = J1939ToJsonConverter.ConvertJ1939MessageToHumanReadableFormat(j1939Message);
            var expectedMessage = @"Continuous Torque & Speed Limit Request
Minimum Continuous Engine Speed Limit Request: 32
Maximum Continuous Engine Speed Limit Request: 2720
Minimum Continuous Engine Torque Limit Request: -107
Maximum Continuous Engine Torque Limit Request: 45
Minimum Continuous Retarder Speed Limit Request: 64
Maximum Continuous Retarder Speed Limit Request: 2720
Minimum Continuous Retarder Torque Limit Request: -115
Maximum Continuous Retarder Torque Limit Request: 113";

            Assert.IsTrue(parsedMessage.StartsWith(expectedMessage));
        }
    }
}
