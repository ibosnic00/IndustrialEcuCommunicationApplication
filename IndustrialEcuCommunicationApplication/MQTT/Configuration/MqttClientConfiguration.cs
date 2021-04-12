using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.MQTT.Configuration
{
    public class MqttClientConfiguration
    {
        public MqttClientConfiguration(string brokerAddress, int brokerPort, string clientId, string username, string password, string samplingTimeTopic, string engineDataPayloadTopic, int connectionRetryTimeMilliseconds)
        {
            BrokerAddress = brokerAddress ?? throw new ArgumentNullException(nameof(brokerAddress));
            BrokerPort = brokerPort;
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
            SamplingTimeTopic = samplingTimeTopic ?? throw new ArgumentNullException(nameof(samplingTimeTopic));
            EngineDataPayloadTopic = engineDataPayloadTopic ?? throw new ArgumentNullException(nameof(engineDataPayloadTopic));
            ConnectionRetryTimeMilliseconds = connectionRetryTimeMilliseconds;
        }

        public string BrokerAddress { get; }
        public int BrokerPort { get; }
        public string ClientId { get; }
        public string Username { get; }
        public string Password { get; }
        public string SamplingTimeTopic { get; }
        public string EngineDataPayloadTopic { get; }
        public int ConnectionRetryTimeMilliseconds { get; }
    }
}
