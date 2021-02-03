using IECA.Application;
using IECA.CANBus;
using System.Threading.Tasks;

namespace IECA
{
    class Program
    {
        private const string APP_CFG_PATH = @"/home/pi/Desktop/ieca_app_configuration.json";

        static void Main(string[] args)
        {
            var mainApp = new IndustrialEcuCommunciationApp(canInterface: new SocketCanInterface(CanChannel.can0),
                                                            appConfigurationPath: APP_CFG_PATH);
            mainApp.Initialize();

            while (true)
            {
                _ = Task.Run(() => { mainApp.InvokeEventIfAnyMultiframeMessageIsReceivedCompletely(); });
            };
        }
    }

    public enum CanChannel : byte
    {
        can0,
        can1,
        can2,
        can3
    }
}