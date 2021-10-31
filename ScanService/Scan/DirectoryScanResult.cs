using System;

namespace ScanService.Scan
{
    public class DirectoryScanResult
    {
        private const string SerializeSeparator = "|";
        
        public string Directory { get; }
        public int TotalFiles { get; }
        public int JsDetects { get; }
        public int RmRfDetects { get; }
        public int RunDll32Detects { get; }
        public int Errors { get; }
        public TimeSpan Elapsed { get; }

        public DirectoryScanResult(
            string directory,
            int totalFiles,
            int jsDetects,
            int rmRfDetects,
            int runDll32Detects,
            int errors,
            TimeSpan elapsed)
        {
            Directory = directory;
            TotalFiles = totalFiles;
            JsDetects = jsDetects;
            RmRfDetects = rmRfDetects;
            RunDll32Detects = runDll32Detects;
            Errors = errors;
            Elapsed = elapsed;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine,
                "====== Scan result ======",
                $"Directory: {Directory}",
                $"Processed files: {TotalFiles}",
                $"JS detects: {JsDetects}",
                $"rm -rf detects: {RmRfDetects}",
                $"Rundll32 detects: {RunDll32Detects}",
                $"Errors: {Errors}",
                $"Execution time: {Elapsed}",
                "=========================");
        }

        public string Serialize()
        {
            return string.Join(SerializeSeparator, 
                Directory, 
                TotalFiles.ToString(), 
                JsDetects.ToString(), 
                RmRfDetects.ToString(),
                RunDll32Detects.ToString(), 
                Errors.ToString(), 
                Elapsed.Milliseconds.ToString());
        }

        public static bool TryParse(string serialized, out DirectoryScanResult? result)
        {
            var elements = serialized.Split(SerializeSeparator);
            if (elements.Length != 7)
            {
                result = null;
                return false;
            }

            if (!int.TryParse(elements[1], out var totalFiles)
                || !int.TryParse(elements[2], out var jsDetects)
                || !int.TryParse(elements[3], out var rmRfDetects)
                || !int.TryParse(elements[4], out var runDll32Detects)
                || !int.TryParse(elements[5], out var errors)
                || !int.TryParse(elements[6], out var elapsedMs))
            {
                result = null;
                return false;
            }

            result = new DirectoryScanResult(
                elements[0],
                totalFiles,
                jsDetects,
                rmRfDetects,
                runDll32Detects,
                errors,
                TimeSpan.FromMilliseconds(elapsedMs));
            return true;
        }
    }
}