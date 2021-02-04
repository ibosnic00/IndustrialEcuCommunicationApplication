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
        private readonly List<ConfigurationRequestPgn> _requestPgns;
        private readonly IndustrialEcuCommunciationApp _app;

        public BackgroundRequestSendService(List<ConfigurationRequestPgn> requestPgns, IndustrialEcuCommunciationApp app)
        {
            _timers = new List<Timer>();
            _requestPgns = requestPgns;
            _app = app;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var requestPgn in _requestPgns)
            {
                _timers!.Add(new Timer(x =>
               {
                   var j1939MessageForSending = ConnectionProcedures.SendRequestForPgnMessage(requestPgn.ParameterGroupNumber, _app.ClaimedAddress);

                   if (j1939MessageForSending?.Data != null && _app.AddressClaimSuccessfull)
                   {
                       _app.CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(j1939MessageForSending));
                   }

               }, null, dueTime: 0, requestPgn.RequestRateMilliseconds));

                // HACK: SocketCAN is not able to send large amount of data in a row, 
                // this can be removed for other can devices
                Thread.Sleep(50);
            }

            _timers!.Add(new Timer(x =>
            {
                _app.CheckIfAnyMultiframeMessageIsReceivedCompletely();
            }, null, dueTime: 0, period: 100));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
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
