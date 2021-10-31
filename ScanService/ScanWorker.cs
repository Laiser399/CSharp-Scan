using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScanService.Scan;

namespace ScanService
{
    public class ScanWorker : BackgroundService
    {
        public const int ConnectionPort = 3993;
        
        private readonly ILogger<ScanWorker> _logger;
        private readonly DirectoryScanner _scanner;

        private readonly IList<Task<DirectoryScanResult>> _tasks;

        public ScanWorker(ILogger<ScanWorker> logger)
        {
            _logger = logger;
            _scanner = new DirectoryScanner();

            _tasks = new List<Task<DirectoryScanResult>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var listener = new TcpListener(IPAddress.Loopback, ConnectionPort);
            listener.Start();
            _logger.Log(LogLevel.Information, "Listening started");

            await using (stoppingToken.Register(() => listener.Stop()))
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var client = await listener.AcceptTcpClientAsync();
                        using var reader = new StreamReader(client.GetStream());
                        await using var writer = new StreamWriter(client.GetStream());
                        _logger.Log(LogLevel.Information, "New client accepted");
                        
                        await HandleClient(reader, writer);
                        _logger.Log(LogLevel.Information, "Client handle done");
                        
                        client.Close();
                        _logger.Log(LogLevel.Information, "Client closed");
                    }
                    catch (InvalidCastException e)
                    {
                        _logger.Log(LogLevel.Error, e, "Error on accept or handle");
                    }
                }
            }
        }

        private async Task HandleClient(TextReader reader, TextWriter writer)
        {
            var command = await reader.ReadLineAsync();
            switch (command)
            {
                case "scan":
                    await HandleScanCommand(reader, writer);
                    break;
                case "status":
                    await HandleStatusCommand(reader, writer);
                    break;
                default:
                    await SimpleRestApi.ErrorAsync(writer, $"Unknown command: \"{command}\"");
                    break;
            }
        }

        private async Task HandleScanCommand(TextReader reader, TextWriter writer)
        {
            var dirPath = await reader.ReadLineAsync();
            if (dirPath == null)
            {
                await SimpleRestApi.ErrorAsync(writer, "Error read directory for scan");
                return;
            }

            var directory = new DirectoryInfo(dirPath);
            if (!directory.Exists)
            {
                await SimpleRestApi.ErrorAsync(writer, "Specified directory does not exists");
                return;
            }
                
            var task = Task.Run(() => _scanner.ScanDirectory(directory));
            _tasks.Add(task);
            await SimpleRestApi.OkAsync(writer, "TaskStarted",
                $"{_tasks.Count - 1}");
        }

        private async Task HandleStatusCommand(TextReader reader, TextWriter writer)
        {
            var idStr = await reader.ReadLineAsync();
            if (!int.TryParse(idStr, out var taskId))
            {
                await SimpleRestApi.ErrorAsync(writer, $"Error parse task id: \"{idStr}\"");
                return;
            }

            if (taskId < 0 || taskId >= _tasks.Count)
            {
                await SimpleRestApi.ErrorAsync(writer, $"Task with id {taskId} not found");
                return;
            }

            var task = _tasks[taskId];
            if (!task.IsCompleted)
            {
                await SimpleRestApi.OkAsync(writer, "InProgress");
                return;
            }

            if (task.IsFaulted)
            {
                await SimpleRestApi.OkAsync(writer, "TaskError",
                    task.Exception?.Message ?? string.Empty);
                return;
            }

            var scanResult = await task;
            await SimpleRestApi.OkAsync(writer, "Done",
                scanResult.Serialize());
        }
    }
}
