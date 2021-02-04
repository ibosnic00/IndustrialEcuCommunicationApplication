using CommandLine;
using IECA.Application;
using IECA.CANBus;
using IECA.Logging;
using System.Collections.Generic;

namespace IECA
{
    public class CommandLineOptions
    {
        [Option('c', "canChannel", Required = true, HelpText = "Provide CAN channel which will be used. Only one allowed from list: can0, can1, can2, can3")]
        public CanChannel SelectedCanChannel { get; set; }

        [Option('f', "appCfgFile", Required = true, HelpText = "Provide path to app configuration file (.json) which will be parsed. Eg. \"home\\pi\\someFile.json\"")]
        public string CfgFilePath { get; set; } = null!;
    }

    class Program
    {
        static void Main(string[] args)
        {
            CommandLineOptions cmdLineOptions = null!;
            _ = Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithNotParsed(HandleParseError)
                .WithParsed(parsedOptions => cmdLineOptions = parsedOptions);

            var mainApp = new IndustrialEcuCommunciationApp(canInterface: new SocketCanInterface(cmdLineOptions.SelectedCanChannel),
                                                            appConfigurationPath: cmdLineOptions.CfgFilePath,
                                                            logger: new SerilogLogger());
            mainApp.Initialize();

            while (true)
            {
                // running until user explicitly stops process
            };
        }

        //in case of errors or --help or --version
        static void HandleParseError(IEnumerable<Error> errs)
        {
            System.Console.WriteLine("Unable to start application. Check command line arguments");
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
