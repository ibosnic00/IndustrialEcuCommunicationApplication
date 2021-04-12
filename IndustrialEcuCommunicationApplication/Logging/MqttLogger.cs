using IECA.J1939;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IECA.MQTT;

namespace IECA.Logging
{
    public class MqttLogger : ILogger
    {
        const double LOG_WAIT_TIMEOUT_MS = 5000;
        const int LOG_QUERRY_INTERVAL_MS = 50;

        private readonly List<uint> _pgnsOfInterest = new List<uint> {
            61443, 61444, 64988, 65101, /*65132,*/
            65207, 65244, 65247, 65253, 65257,
            65262, 65263, 65266, /*65269,*/
            65270, 65271, 65272, 65276 };

        public MqttLogger(MQTTClient mQTTClient)
        {
            receivedJ1939MessagesOfInterestToReqIdMap = new Dictionary<long, List<J1939Message>>();
            logsForRequestIdInProcess = new List<long>();
            this.mQTTClient = mQTTClient;
        }

        #region Properties

        private Dictionary<long, List<J1939Message>> receivedJ1939MessagesOfInterestToReqIdMap;
        private List<long> logsForRequestIdInProcess;
        private MQTTClient mQTTClient;
        private string? engineModel;
        private string? engineType;
        private string? engineDescription;

        #endregion


        #region Public  Methods

        public void Initialize(string engineModel, string engineType, string engineDescription)
        {
            this.engineModel = engineModel;
            this.engineType = engineType;
            this.engineDescription = engineDescription;
        }

        public void LogInfo(string lineToAdd) { Console.WriteLine(lineToAdd); }

        public void AddJ1939MessageToLogHandler(J1939Message j1939Message)
        {
            if (logsForRequestIdInProcess.Count != 0)
            {
                if (CheckIfMessageIsOfInterest(j1939Message))
                    foreach (var requestId in logsForRequestIdInProcess)
                        SaveMessageToLog(j1939Message, requestId);
            }
        }

        public void StartLogCreationForRequestId(long requestId)
        {
            Task.Run(() =>
            {
                var startTimestamp = DateTime.Now;
                bool shouldReceiveBeAlive = true;

                if (logsForRequestIdInProcess.Count > 10)
                    EmptyWaitQue();

                AddReqIdToInterestList(requestId);

                try
                {

                    while ((DateTime.Now - startTimestamp).TotalMilliseconds <= LOG_WAIT_TIMEOUT_MS && shouldReceiveBeAlive)
                    {
                        if (ReceivedMapContainsAllMessagesOfInterest(receivedJ1939MessagesOfInterestToReqIdMap[requestId]))
                            shouldReceiveBeAlive = false;
                        Thread.Sleep(LOG_QUERRY_INTERVAL_MS);
                    }
                }
                catch (Exception ex) { LogInfo($"Error in while: {ex.Message}"); }
                finally
                {
                    LogInfo($"Time needed for message composal: {(startTimestamp - DateTime.Now).TotalMilliseconds}");
                    LogInfo($"Messages in buffer {logsForRequestIdInProcess.Count}");
                    ComposeAndSendMqttMessage(receivedJ1939MessagesOfInterestToReqIdMap[requestId]);
                    RemoveReqIdFromInterestList(requestId);
                }
            });
        }

        #endregion


        #region Private Methods

        private bool ReceivedMapContainsAllMessagesOfInterest(List<J1939Message> receivedMessagesForThisRequest)
        {
            foreach (var pgnOfInterest in _pgnsOfInterest)
            {
                if (!receivedMessagesForThisRequest.Any(msg => msg.PDU.ParameterGroupNumber == pgnOfInterest))
                {
                    LogInfo($" Missing pgn {pgnOfInterest}");
                    return false;
                }

            }

            return true;
        }

        private void EmptyWaitQue()
        {
            try
            {
                receivedJ1939MessagesOfInterestToReqIdMap = new Dictionary<long, List<J1939Message>>();
                logsForRequestIdInProcess = new List<long>();
            }
            catch { }
        }

        private bool CheckIfMessageIsOfInterest(J1939Message j1939Message)
        {
            return _pgnsOfInterest.Contains(j1939Message.PDU.ParameterGroupNumber);
        }

        private void SaveMessageToLog(J1939Message j1939Message, long requestId)
        {
            try
            {
                if (!receivedJ1939MessagesOfInterestToReqIdMap[requestId].Any(loggedMsg => loggedMsg.PDU.ParameterGroupNumber == j1939Message.PDU.ParameterGroupNumber))
                    receivedJ1939MessagesOfInterestToReqIdMap[requestId].Add(j1939Message);
            }
            catch (Exception ex)
            {
                LogInfo($"Error in saving message: {ex.Message}");
            }
        }

        private void AddReqIdToInterestList(long reqId)
        {
            logsForRequestIdInProcess.Add(reqId);
            receivedJ1939MessagesOfInterestToReqIdMap.Add(reqId, new List<J1939Message>());
        }
        private void RemoveReqIdFromInterestList(long reqId)
        {
            _ = logsForRequestIdInProcess.Remove(reqId);
            _ = receivedJ1939MessagesOfInterestToReqIdMap.Remove(reqId);
        }

        private void ComposeAndSendMqttMessage(List<J1939Message> receivedJ1939MessagesOfInterest)
        {
            string jsonPayload = $"{{\"timestamp\" : {DateTime.Now.Ticks}, \"shipId\" : \"{mQTTClient.ClientId}\",\"engineDataList\" : [{{";

            foreach (var pgn in _pgnsOfInterest)
                if (receivedJ1939MessagesOfInterest.Any(msg => msg.PDU.ParameterGroupNumber == pgn))
                    jsonPayload += J1939ToMqttPayloadConverter.ConvertJ1939MessageToJsonFormat(receivedJ1939MessagesOfInterest.Single(msg => msg.PDU.ParameterGroupNumber == pgn));
                else
                    jsonPayload += J1939ToMqttPayloadConverter.ConvertJ1939MessageToJsonFormat(new J1939Message(new ProtocolDataUnit(pgn, 0xFF), new List<byte>()));
            jsonPayload += $"\"engineModel\" : \"{engineModel}\",\"engineType\" : \"{engineType}\",\"engineDescription\" : \"{engineDescription}\"}}]}}";

            mQTTClient.PublishEngineData(jsonPayload);
        }

        #endregion
    }
}
