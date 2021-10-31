using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ScanService;
using ScanService.Scan;

namespace ScanUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Arguments count must be equals 2.");
                return;
            }

            var command = args[0];

            if (command == "scan")
            {
                OnScanCommand(args[1]);
            }
            else if (command == "status")
            {
                if (int.TryParse(args[1], out var taskId))
                {
                    OnStatusCommand(taskId);
                }
                else
                {
                    Console.WriteLine("Error on parse second argument - taskId.");
                }
            }
            else
            {
                Console.WriteLine($"Unknown command: {command}");
            }
        }

        private static void OnScanCommand(string dirPath)
        {
            using var client = new TcpClient();
            client.Connect(IPAddress.Loopback, ScanWorker.ConnectionPort);

            using var reader = new StreamReader(client.GetStream());
            using var writer = new StreamWriter(client.GetStream());
            
            writer.WriteLine("scan");
            writer.WriteLine(dirPath);
            writer.Flush();
            
            var responseStatus = reader.ReadLine();
            if (responseStatus == "Error")
            {
                var errorMessage = reader.ReadLine();
                Console.WriteLine($"Error: {errorMessage}");
            }
            else if (responseStatus == "Ok")
            {
                var response = reader.ReadLine();
                if (response != "TaskStarted")
                {
                    Console.WriteLine($"Received unknown response from service: \"{response}\"");
                    return;
                }

                var taskId = reader.ReadLine();
                if (taskId == null)
                {
                    Console.WriteLine("Error on receive task id from service");
                    return;
                }
                
                Console.WriteLine($"Scan task was created with ID: {taskId}");
            }
            else
            {
                Console.WriteLine($"Received unknown response status from service: \"{responseStatus}\"");
            }
        }

        private static void OnStatusCommand(int taskId)
        {
            using var client = new TcpClient();
            client.Connect(IPAddress.Loopback, ScanWorker.ConnectionPort);

            using var reader = new StreamReader(client.GetStream());
            using var writer = new StreamWriter(client.GetStream());
            
            writer.WriteLine("status");
            writer.WriteLine(taskId);
            writer.Flush();
            
            var responseStatus = reader.ReadLine();
            if (responseStatus == "Error")
            {
                var errorMessage = reader.ReadLine();
                Console.WriteLine($"Error: {errorMessage}");
            }
            else if (responseStatus == "Ok")
            {
                var response = reader.ReadLine();
                if (response == "InProgress")
                {
                    Console.WriteLine("Scan task in progress, please wait");
                }
                else if (response == "TaskError")
                {
                    var errorMessage = reader.ReadLine();
                    Console.WriteLine($"Task failed. Message: {errorMessage}");
                }
                else if (response == "Done")
                {
                    var resultSerialized = reader.ReadLine();
                    if (resultSerialized == null 
                        || !DirectoryScanResult.TryParse(resultSerialized, out var scanResult))
                    {
                        Console.WriteLine("Error on read scan result from service");
                        return;
                    }

                    Console.WriteLine(scanResult);
                }
                else
                {
                    Console.WriteLine($"Received unknown response status from service: \"{responseStatus}\"");
                }
            }
            else
            {
                Console.WriteLine($"Received unknown response status from service: \"{responseStatus}\"");
            }
        }
    }
}