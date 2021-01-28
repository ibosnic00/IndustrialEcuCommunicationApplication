using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IECA.J1939.Configuration
{
    public static class DataConfigurationDeserializer
    {
        public static List<ConfigurationPgn> GetConfigurationFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception($"IECA Data configuration file doesn't exist: {filePath}");

            return JsonConvert.DeserializeObject<List<ConfigurationPgn>>(File.ReadAllText(filePath));
        }
    }
}
