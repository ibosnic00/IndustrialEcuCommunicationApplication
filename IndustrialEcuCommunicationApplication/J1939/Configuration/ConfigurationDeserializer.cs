using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IECA.J1939
{
    public static class ConfigurationDeserializer
    {
        public static List<ConfigurationPgn> GetConfigurationFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception($"IECA configuration file doesn't exist: {filePath}");

            return JsonConvert.DeserializeObject<List<ConfigurationPgn>>(File.ReadAllText(filePath));
        }
    }
}
