using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ScanService
{
    public class DirectoryScanner
    {
        private readonly IReadOnlyCollection<IFileContentScanner> _jsScanners;
        private readonly IReadOnlyCollection<IFileContentScanner> _otherScanners;

        public DirectoryScanner()
        {
            var jsScanner = new SimpleFileContentScanner(@"<script>evil_script()</script>", ScanResult.Js);
            var rmRfScanner = new SimpleFileContentScanner(@"rm -rf %userprofile% \Documents", ScanResult.RmRf);
            var runDll32Scanner = new SimpleFileContentScanner(@"Rundll32 sus.dll SusEntry", ScanResult.RunDll32);

            _jsScanners = new[] { jsScanner, rmRfScanner, runDll32Scanner };
            _otherScanners = new[] { rmRfScanner, runDll32Scanner };
        }

        public void Go(string dirPath)
        {
            var rootDirectory = new DirectoryInfo(dirPath);

            if (!rootDirectory.Exists)
            {
                Console.WriteLine($"Directory \"{rootDirectory.FullName}\" does not exists.");
                return;
            }
            
            var sw = Stopwatch.StartNew();
            var results = Scan(rootDirectory);
            sw.Stop();

            var resultsCounts = results
                .GroupBy(x => x)
                .ToDictionary(
                    x => x.Key, 
                    x => x.Count());

            Console.WriteLine("====== Scan result ======");
            Console.WriteLine($"Directory: {rootDirectory.FullName}");
            Console.WriteLine($"Processed files: {results.Count}");
            Console.WriteLine($"JS detects: {resultsCounts.GetValueOrDefault(ScanResult.Js, 0)}");
            Console.WriteLine($"rm -rf detects: {resultsCounts.GetValueOrDefault(ScanResult.RmRf, 0)}");
            Console.WriteLine($"Rundll32 detects: {resultsCounts.GetValueOrDefault(ScanResult.RunDll32, 0)}");
            Console.WriteLine($"Errors: {resultsCounts.GetValueOrDefault(ScanResult.ScanError, 0)}");
            Console.WriteLine($"Execution time: {sw.Elapsed}");
            Console.WriteLine("=========================");
        }

        private IReadOnlyCollection<ScanResult> Scan(DirectoryInfo directory)
        {
            var currentDirResults = directory
                .GetFiles()
                .Select(Scan);

            return directory
                .GetDirectories()
                .Aggregate(
                    currentDirResults, 
                    (current, subDirectory) 
                        => current.Concat(Scan(subDirectory)))
                .ToList();
        }

        private ScanResult Scan(FileInfo file)
        {
            var scanners = file.Extension.ToLower() == ".js"
                ? _jsScanners
                : _otherScanners;

            if (!TryReadContent(file.FullName, out var content))
            {
                return ScanResult.ScanError;
            }

            return scanners
                .Select(s => s.Scan(content))
                .Where(r => r != ScanResult.ProblemsNotFound)
                .DefaultIfEmpty(ScanResult.ProblemsNotFound)
                .First();
        }

        private static bool TryReadContent(string filePath, out string content)
        {
            try
            {
                content = File.ReadAllText(filePath);
                return true;
            }
            catch (Exception)
            {
                content = string.Empty;
                return false;
            }
        }
    }
}