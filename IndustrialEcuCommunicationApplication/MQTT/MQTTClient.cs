using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace IECA.MQTT
{
    public class MQTTClient
    {
        public readonly int ConnectionRetryMillis;

        readonly MqttClient _client;
        readonly string _username;
        readonly string _password;
        readonly string _samplingTimeTopic;
        readonly string _engineDataPayloadTopic;
        public readonly string ClientId;

        public event EventHandler<int>? SamplingTimeReceived;

        public MQTTClient(string broker, int port, string clientId, string username, string password, string samplingTimeTopic, string engineDataPayloadTopic, int connectionRetryMillis)
        {
            ClientId = clientId;
            _username = username;
            _password = password;
            _samplingTimeTopic = samplingTimeTopic;
            _engineDataPayloadTopic = engineDataPayloadTopic;
            ConnectionRetryMillis = connectionRetryMillis;
            _client = new MqttClient(broker, port, false, MqttSslProtocols.None, null, null);
            _client.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
        }

        public MqttConnectionResult ConnectToClientIfItIsNotConnected()
        {
            MqttConnectionResult result = MqttConnectionResult.ServerUnavailable;
            if (_client.IsConnected)
                result = MqttConnectionResult.ConnectionAccepted;
            else
            {
                try
                {
                    result = (MqttConnectionResult)_client.Connect(ClientId, _username, _password);
                }
                catch
                {
                    // TODO: add logging
                }
            }


            if (result == MqttConnectionResult.ConnectionAccepted)
                SubscribeToTopic(_samplingTimeTopic);

            return result;
        }

        public void SubscribeToTopic(string topic)
        {
            _client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        public void PublishEngineData(string engineDataJson)
        {
            _client.Publish(_engineDataPayloadTopic, Encoding.UTF8.GetBytes(engineDataJson));
        }

        public void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                var samplingTimeMs = int.Parse(Encoding.ASCII.GetString(e.Message));
                SamplingTimeReceived?.Invoke(this, samplingTimeMs);
            }
            catch
            {
                // TODO: log exception here
            }
        }

    }

    public enum MqttConnectionResult
    {
        ConnectionAccepted,
        UnacceptableProtocolVersion,
        IdentifierRejected,
        ServerUnavailable,
        BadUserNameOrPassword,
        NotAuthorized
    }
}
