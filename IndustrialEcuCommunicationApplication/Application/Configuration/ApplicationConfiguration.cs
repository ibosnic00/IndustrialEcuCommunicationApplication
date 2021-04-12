using IECA.J1939;
using IECA.J1939.Configuration;
using IECA.MQTT.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.Application
{
    public class ApplicationConfiguration
    {
        public ApplicationConfiguration(byte wantedAddress, byte minAddress, byte maxAddress, int addressClaimWaitPeriodMs, EcuName ecuName, MqttClientConfiguration mqttClient,
             string engineModel, string engineType, string engineDescription)
        {
            WantedAddress = wantedAddress;
            MinAddress = minAddress;
            MaxAddress = maxAddress;
            AddressClaimWaitPeriodMs = addressClaimWaitPeriodMs;
            EcuName = ecuName;
            MqttClient = mqttClient;
            EngineModel = engineModel;
            EngineType = engineType;
            EngineDescription = engineDescription;
        }

        public byte WantedAddress { get; }
        public byte MinAddress { get; }
        public byte MaxAddress { get; }
        public int AddressClaimWaitPeriodMs { get; }
        public EcuName EcuName { get; }
        public MqttClientConfiguration MqttClient { get; }
        public string EngineModel { get; }
        public string EngineType { get; }
        public string EngineDescription { get; }

    }
}
