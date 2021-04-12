using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IECA.J1939.Configuration;
using IECA.J1939.Services;
using IECA.J1939.Utility;
using Microsoft.Extensions.Hosting;

namespace IECA.Application
{
    internal class BackgroundRequestSendService : IHostedService, IDisposable
    {
        private readonly List<Timer>? _timers;
        private bool _disposed;
        private readonly List<uint> _requestPgns;
        private readonly IndustrialEcuCommunciationApp _app;
        private readonly int _samplingTimeMs;

        public BackgroundRequestSendService(int samplingTimeMs, IndustrialEcuCommunciationApp app)
        {
            _timers = new List<Timer>();
            _requestPgns = new List<uint> {
            61443, 61444, 64988, 65101, 65132,
            65207, 65244, 65247, 65253, 65257,
            65262, 65263, 65266, 65269,
            65270, 65271, 65272, 65276 };
            _app = app;
            _samplingTimeMs = samplingTimeMs;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            _timers!.Add(new Timer(x =>
            {
                try
                {
                    _app.CheckIfAnyMultiframeMessageIsReceivedCompletely();
                }
                catch (Exception ex)
                {
                    _app.LogInfoWithLogger(ex.Message);
                }
            }, null, dueTime: 0, period: 100));

            _timers!.Add(new Timer(x =>
            {
                _app.MqttClient.ConnectToClientIfItIsNotConnected();
            }, null, dueTime: _app.MqttClient.ConnectionRetryMillis, period: _app.MqttClient.ConnectionRetryMillis));

            _timers!.Add(new Timer(x =>
            {
                _app.StartJsonPayloadCreationForRequestId(DateTime.Now.Ticks);
            }, null, dueTime: 0, _samplingTimeMs));

            foreach (var requestPgn in _requestPgns)
            {
                _timers!.Add(new Timer(x =>
               {
                   var j1939MessageForSending = ConnectionProcedures.SendRequestForPgnMessage(requestPgn, _app.ClaimedAddress);

                   if (j1939MessageForSending?.Data != null && _app.AddressClaimSuccessfull)
                   {
                       _app.CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(j1939MessageForSending));
                   }

               }, null, dueTime: 0, _samplingTimeMs));

                // HACK: SocketCAN is not able to send large amount of data in a row, 
                // this can be removed for other can devices
                Thread.Sleep(55);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timers?.ForEach(timr => timr.Change(Timeout.Infinite, 0));
            return Task.CompletedTask;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_timers != null)
                    foreach (var _timer in _timers)
                        _timer?.Dispose();
            }
            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
