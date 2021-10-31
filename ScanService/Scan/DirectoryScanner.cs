using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ScanService.Helpers;

namespace ScanService.Scan
{
    public class DirectoryScanner
    {
        private readonly IReadOnlyCollection<IFileContentScanner> _jsScanners;
        private readonly IReadOnlyCollection<IFileContentScanner> _otherScanners;

        public DirectoryScanner()
        {
            var jsScanner = new SimpleFileContentScanner(@"<script>evil_script()</script>", FileScanResult.Js);
            var rmRfScanner = new SimpleFileContentScanner(@"rm -rf %userprofile% \Documents", FileScanResult.RmRf);
            var runDll32Scanner = new SimpleFileContentScanner(@"Rundll32 sus.dll SusEntry", FileScanResult.RunDll32);

            _jsScanners = new[] { jsScanner, rmRfScanner, runDll32Scanner };
            _otherScanners = new[] { rmRfScanner, runDll32Scanner };
        }

        public DirectoryScanResult ScanDirectory(DirectoryInfo directory)
        {
            var sw = Stopwatch.StartNew();
            var results = Scan(directory).ToList();
            sw.Stop();

            var resultsCounts = results
                .GroupBy(x => x)
                .ToDictionary(
                    x => x.Key, 
                    x => x.Count());

            return new DirectoryScanResult(
                directory.FullName,
                results.Count,
                resultsCounts.GetValueOrDefault(FileScanResult.Js, 0),
                resultsCounts.GetValueOrDefault(FileScanResult.RmRf, 0),
                resultsCounts.GetValueOrDefault(FileScanResult.RunDll32, 0),
                resultsCounts.GetValueOrDefault(FileScanResult.ScanError, 0),
                sw.Elapsed);
        }
        
        private IEnumerable<FileScanResult> Scan(DirectoryInfo directory)
        {
            if (!directory.TryGetFiles(out var files)
                || !directory.TryGetDirectories(out var subDirectories))
            {
                return new[] { FileScanResult.ScanError };
            }
            
            var currentDirResults = files.Select(Scan);

            return subDirectories
                .Aggregate(
                    currentDirResults,
                    (current, subDirectory)
                        => current.Concat(Scan(subDirectory)));
        }

        private FileScanResult Scan(FileInfo file)
        {
            var scanners = file.Extension.ToLower() == ".js"
                ? _jsScanners
                : _otherScanners;

            if (!TryReadContent(file.FullName, out var content))
            {
                return FileScanResult.ScanError;
            }

            return scanners
                .Select(s => s.Scan(content))
                .Where(r => r != FileScanResult.ProblemsNotFound)
                .DefaultIfEmpty(FileScanResult.ProblemsNotFound)
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