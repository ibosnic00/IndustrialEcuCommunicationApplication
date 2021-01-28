using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IECA.J1939.Configuration
{
    public static class ApplicationConfigurationDeserializer
    {
        public static ApplicationConfiguration GetConfigurationFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception($"IECA App configuration file doesn't exist: {filePath}");

            return JsonConvert.DeserializeObject<ApplicationConfiguration>(File.ReadAllText(filePath));
        }
    }
}
