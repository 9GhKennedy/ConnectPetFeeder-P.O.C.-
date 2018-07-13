using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManualConnectedCiotola
{
    class Program
    {
        private static bool _releSensor;
        private static bool _foto;
        private DeviceClient _client;
        private static int _dose;
        private static DateTime _dateFoto;
        private static bool _getDose;
        
        static async Task Main(string[] args)
        {
            // per Azure
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("configuration.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();

            var deviceId = configuration["deviceId"];
            var authenticationMethod = new DeviceAuthenticationWithRegistrySymmetricKey(
                    deviceId, configuration["deviceKey"]);
            var transportType = TransportType.Mqtt;
            if (!string.IsNullOrWhiteSpace(configuration["transportType"]))
            {
                transportType = (TransportType)
                    Enum.Parse(typeof(TransportType),
                    configuration["transportType"], true);

            }
            var client = DeviceClient.Create(
                configuration["hostName"],
                authenticationMethod,
                transportType
            );

           
            // per il progetto
            _releSensor = false;
            _foto = false;
            _getDose = false;
            _dose = 50;
            

            while (true)
            {
                // ricevo 
                var message = await client.ReceiveAsync();
                if (message == null) continue;

                var bytes = message.GetBytes();
                if (bytes == null) continue;

                var text = Encoding.UTF8.GetString(bytes);
                var textParts = text.Split();
                switch (textParts[0].ToLower())
                {
                    case "dose":
                        _getDose = true;
                        break;
                    case "foto":
                        _foto = true;
                        break;
                    default:
                        Console.WriteLine("Error ");
                        break;
                }
                
                if (_getDose == true)
                {
                    NotifyDose(client);
                }
                if(_foto== true)
                {
                    TakeAPhoto(client);
                }
                await client.CompleteAsync(message);

                Task.Delay(1000);
            }

        }
        
        private static void NotifyDose(DeviceClient client)
        {
            if (_dose > 0)
            {
                _dose--;
                Console.WriteLine($"dosi decrementate, dosi rimanenti {_dose}");
                _getDose = false;
            }

        }

        private static async Task TakeAPhoto(DeviceClient client)
        {
            _dateFoto = DateTime.Now.ToUniversalTime();
            _foto = false;
        }

    }
}
