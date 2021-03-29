using IECA.J1939;
using IECA.J1939.Messages;
using IECA.J1939.Messages.TransportProtocol;
using IECA.J1939.Configuration;
using IECA.J1939.Services;
using IECA.J1939.Utility;
using IECA.CANBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using IECA.Logging;
using IECA.J1939.Messages.Diagnostic;
using IECA.MQTT;

namespace IECA.Application
{
    public class IndustrialEcuCommunciationApp
    {
        #region Constants

        // 15 minutes is default
        const int DEFAULT_SAMPLING_TIME_IN_MS = 900_000;

        #endregion


        #region Fields

        public ICanInterface CanInterface;
        public bool AddressClaimSuccessfull;
        public byte ClaimedAddress = StandardData.NULL_ADDRESS;
        public MQTTClient MqttClient;

        List<MultiFrameMessage>? _mfMessagesBuffer;
        Dictionary<byte, EcuName>? _knownNetworkNodes;
        EcuName? _ecuName;
        IHost? _backgroundWorker;
        int _samplingTimeMs = DEFAULT_SAMPLING_TIME_IN_MS;

        readonly ApplicationConfiguration _appConfig;
        readonly ILogger _logger;

        #endregion Fields


        #region Constructors

        public IndustrialEcuCommunciationApp(ICanInterface canInterface, string appConfigurationPath)
        {
            var appConfig = ApplicationConfigurationDeserializer.GetConfigurationFromFile(appConfigurationPath);
            _appConfig = appConfig;
            CanInterface = canInterface;
            MqttClient = new MQTTClient(_appConfig.MqttClient.BrokerAddress,
                                         _appConfig.MqttClient.BrokerPort,
                                         _appConfig.MqttClient.ClientId,
                                         _appConfig.MqttClient.Username,
                                         _appConfig.MqttClient.Password,
                                         _appConfig.MqttClient.SamplingTimeTopic,
                                         _appConfig.MqttClient.EngineDataPayloadTopic,
                                         _appConfig.MqttClient.ConnectionRetryTimeMilliseconds);

            _logger = new MqttLogger(MqttClient);
        }

        #endregion Constructors


        #region Public Methods

        public void Initialize()
        {
            _logger.Initialize(_appConfig.EngineModel, _appConfig.EngineType, _appConfig.EngineDescription);
            MqttClient.ConnectToClientIfItIsNotConnected();
            MqttClient.SamplingTimeReceived += MqttClient_SamplingTimeReceived;

            _mfMessagesBuffer = new List<MultiFrameMessage>();
            _knownNetworkNodes = new Dictionary<byte, EcuName>();
            AddressClaimSuccessfull = false;
            _ecuName = new EcuName(_appConfig.EcuName.ArbitraryAddressCapable, _appConfig.EcuName.IndustryGroup,
                _appConfig.EcuName.VehicleSystemInstance, _appConfig.EcuName.VehicalSystem, reserved: false,
                _appConfig.EcuName.Function, _appConfig.EcuName.FunctionInstance, _appConfig.EcuName.EcuInstance,
                _appConfig.EcuName.ManufacturerCode, _appConfig.EcuName.IdentityNumber);

            CanInterface.Initialize();
            CanInterface.DataFrameReceived += OnCanMessageReceived;

            TryToClaimAddress();
            StartSendingRequestPgns(_samplingTimeMs);
        }

        public void StartJsonPayloadCreationForRequestId(long requestId)
        {
            _logger.StartLogCreationForRequestId(requestId);
        }

        public void LogInfoWithLogger(string info)
        {
            _logger.LogInfo(info);
        }

        public void CheckIfAnyMultiframeMessageIsReceivedCompletely()
        {
            if (_mfMessagesBuffer == null)
                return;

            if (_mfMessagesBuffer.Count != 0 && _mfMessagesBuffer.Any(msg => msg.IsMessageComplete == true))
            {
                try
                {
                    var receivedMFMessage = _mfMessagesBuffer.SingleOrDefault(msg => msg.IsMessageComplete == true);

                    if (receivedMFMessage is not MultiFrameMessage)
                        return;

                    HandleReceivedCompletedJ1939Message(receivedMFMessage);
                    RemoveCompletedMultiFrameMessageFromBuffer(receivedMFMessage);
                }
                catch
                {
                    foreach (var mfMsg in _mfMessagesBuffer.Where(msg => msg.IsMessageComplete == true).ToList())
                    {
                        HandleReceivedCompletedJ1939Message(mfMsg);
                        RemoveCompletedMultiFrameMessageFromBuffer(mfMsg);
                    }
                }
            }
        }

        #endregion Public Methods


        #region Event Handlers

        private void MqttClient_SamplingTimeReceived(object? sender, int samplingTimeMs)
        {
            _samplingTimeMs = samplingTimeMs;
            StopSendingRequestPgns();
            Task.Run(() => { StartSendingRequestPgns(_samplingTimeMs); });
        }

        private void OnCanMessageReceived(object? sender, CanMessage msg)
        {
            if (!msg.IsExtendedId || msg.Data == null)
                return;

            var receivedPdu = ProtocolDataUnit.FromCanExtIdentifierFormat(msg.ID);

            if (receivedPdu.ParameterGroupNumber == StandardPgns.TP_CM_PGN)
            {
                if (msg.DLC < StandardData.TP_CM_DATA_SIZE)
                    return;

                if (msg.Data[0] == (byte)ConnectionManagementMessageControlBytes.BAM)
                {
                    var broadcastAnnounceMsg = new BrodcastAnnounceMessage(msg.Data.ToList(), receivedPdu.SourceAddress);
                    HandleReceivedBroadcastAnnounceMessage(broadcastAnnounceMsg);
                }
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.TP_DT_PGN)
            {
                if (_mfMessagesBuffer == null || msg.DLC < StandardData.TP_DT_DATA_SIZE)
                    return;

                if (_mfMessagesBuffer.Exists(msg => msg.PDU.SourceAddress == receivedPdu.SourceAddress))
                {
                    var dataTransferMessage = new DataTransferMessage(msg.Data.ToList(), receivedPdu.Specific.Value, receivedPdu.SourceAddress);
                    HandleReceivedDataTransferMessage(dataTransferMessage);
                }
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.REQUEST_PGN)
            {
                if (Helpers.GetPgnFromRequestMessageData(msg.Data) == null)
                    return;

                var requestPgnMessage = new RequestPgnMessage((uint)Helpers.GetPgnFromRequestMessageData(msg.Data)!, receivedPdu.SourceAddress);
                HandleReceivedRequestPgnMessage(requestPgnMessage);
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.REQUEST_2_PGN)
            {
                // currently not in use
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.ACK_PGN)
            {
                if (msg.DLC < StandardData.ACK_DATA_SIZE)
                    return;

                var acknowledgementMessage = new AcknowledgementMessage(receivedPdu, msg.Data.ToList());
                HandleReceivedAcknowledgementMessage(acknowledgementMessage);
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.ADDR_CLAIMED_PGN)
            {
                if (msg.DLC < StandardData.ADDR_CLAIMED_DATA_SIZE)
                    return;

                var addressClaimedMessage = new AddressClaimedMessage(receivedPdu, msg.Data.ToList());
                HandleReceivedAddressClaimedMessage(addressClaimedMessage);
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.DM1_PGN || receivedPdu.ParameterGroupNumber == StandardPgns.DM2_PGN)
            {
                // Single-frame diagnostic messages
                var activeDiagnosticTroubleCodeMessage = new ActiveDiagnosticTroubleCodesMessage(receivedPdu, msg.Data.ToList());
                HandleReceivedActiveDiagnosticTroubleCodesMessage(activeDiagnosticTroubleCodeMessage);
            }
            else
            {
                var receivedMessage = new J1939Message(receivedPdu, msg.Data.ToList());
                HandleReceivedCompletedJ1939Message(receivedMessage);
            }
        }

        #endregion Event Handlers


        #region Private Methods

        private IHostBuilder BackgroundRequestSender(int samplingTimeMs) =>
               Host.CreateDefaultBuilder().ConfigureServices(services =>
               {
                   services.AddHostedService(x =>
                   new BackgroundRequestSendService(samplingTimeMs, this));
               });

        private void StartSendingRequestPgns(int samplingTimeMs)
        {
            _backgroundWorker = BackgroundRequestSender(samplingTimeMs).Build();
            _backgroundWorker.Run();
        }

        private void StopSendingRequestPgns()
        {
            _backgroundWorker?.Dispose();
        }

        private void TryToClaimAddress()
        {
            _ = Task.Run(() =>
            {
                CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(ConnectionProcedures.SendRequestForAddressClaimMessage()!));

                if (_knownNetworkNodes != null && _knownNetworkNodes.ContainsKey(_appConfig!.WantedAddress))
                {
                    for (byte i = _appConfig!.MinAddress; i <= _appConfig!.MaxAddress; i++)
                    {
                        if (!_knownNetworkNodes.ContainsKey(i))
                        {
                            ClaimedAddress = i;
                            break;
                        }
                    }
                }
                else
                    ClaimedAddress = _appConfig!.WantedAddress;

                if (ClaimedAddress != StandardData.NULL_ADDRESS)
                {
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new AddressClaimedMessage(_ecuName!.ToRawFormat(), ClaimedAddress)));
                    AddressClaimSuccessfull = true;
                }
                else
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new CannotClaimAddressMessage(_ecuName!.ToRawFormat())));
            });
            Thread.Sleep(_appConfig!.AddressClaimWaitPeriodMs);
        }

        private void HandleReceivedRequestPgnMessage(RequestPgnMessage requestedPgnMessage)
        {
            if (requestedPgnMessage.RequestedPgn == StandardPgns.ADDR_CLAIMED_PGN && AddressClaimSuccessfull)
            {
                if (ClaimedAddress != StandardData.NULL_ADDRESS)
                {
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new AddressClaimedMessage(_ecuName!.ToRawFormat(), ClaimedAddress)));
                    AddressClaimSuccessfull = true;
                }
                else
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new CannotClaimAddressMessage(_ecuName!.ToRawFormat())));
            }
        }

        private void HandleReceivedAddressClaimedMessage(AddressClaimedMessage addressClaimedMessage)
        {
            if (AddressClaimSuccessfull)
            {
                if (addressClaimedMessage.PDU.SourceAddress == ClaimedAddress)
                {
                    if (addressClaimedMessage.EcuName.ToRawFormat() < _ecuName!.ToRawFormat())
                    {
                        AddressClaimSuccessfull = false;
                        TryToClaimAddress();
                    }
                    else
                        CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new AddressClaimedMessage(_ecuName!.ToRawFormat(), ClaimedAddress)));
                }
            }
            else
            {
                _knownNetworkNodes?.TryAdd(addressClaimedMessage.PDU.SourceAddress, addressClaimedMessage.EcuName);
            }
        }

        private void HandleReceivedBroadcastAnnounceMessage(BrodcastAnnounceMessage rcvMsg)
        {
            // remove any message that in buffer with same source - only one PGN per transfer is allowed
            _ = _mfMessagesBuffer?.RemoveAll(msg => msg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress);

            _mfMessagesBuffer?.Add(new MultiFrameMessage(rcvMsg.Pgn, rcvMsg.PDU.SourceAddress, rcvMsg.TotalMessageSize));
        }

        private void HandleReceivedDataTransferMessage(DataTransferMessage rcvMsg)
        {
            _mfMessagesBuffer!.Single(msg => msg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress).AddPacketizedData(rcvMsg.PacketizedData);
        }

        private void RemoveCompletedMultiFrameMessageFromBuffer(MultiFrameMessage rcvMsg)
        {
            if (_mfMessagesBuffer == null)
                return;

            _mfMessagesBuffer.RemoveAll(mfMsg => mfMsg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress
                                                                && mfMsg.PDU.Specific.Value == rcvMsg.PDU.Specific.Value);
        }

        private void HandleReceivedAcknowledgementMessage(AcknowledgementMessage acknowledgementMessage)
        {
            _logger.LogInfo($"Received ACK Message for PGN: {acknowledgementMessage.PgnAcknowledged} - {acknowledgementMessage.Response.ToString()}");
        }

        private void HandleReceivedActiveDiagnosticTroubleCodesMessage(ActiveDiagnosticTroubleCodesMessage activeDiagnosticTroubleCodeMessage)
        {
            //TODO: pretify dtc logger
            foreach (var dtcRecord in activeDiagnosticTroubleCodeMessage.DtcRecords)
                _logger.LogInfo($"DM1 - stat: {dtcRecord.LampStatus}, spn: {dtcRecord.DTC.SuspectParameterNumber}, fmi: {dtcRecord.DTC.FailureModeIdentifier}, oc: {dtcRecord.DTC.OccurenceCount}");
        }

        private void HandleReceivedCompletedJ1939Message(J1939Message rcvMsg)
        {
            if (rcvMsg.PDU.ParameterGroupNumber == StandardPgns.DM1_PGN || rcvMsg.PDU.ParameterGroupNumber == StandardPgns.DM2_PGN)
            {
                // Multiple-frame diagnostic messages
                var activeDiagnosticTroubleCodeMessage = new ActiveDiagnosticTroubleCodesMessage(rcvMsg.PDU, rcvMsg.Data);
                HandleReceivedActiveDiagnosticTroubleCodesMessage(activeDiagnosticTroubleCodeMessage);
            }
            else
                _logger.AddJ1939MessageToLogHandler(rcvMsg);
        }

        #endregion
    }

    public enum CanChannel : byte
    {
        can0,
        can1,
        can2,
        can3
    }
}
