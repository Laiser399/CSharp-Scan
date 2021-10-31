namespace ScanService.Scan
{
    public class SimpleFileContentScanner : IFileContentScanner
    {
        private readonly string _stringForCheck;
        private readonly FileScanResult _resultOnContains;

        public SimpleFileContentScanner(string stringForCheck, FileScanResult resultOnContains)
        {
            _stringForCheck = stringForCheck;
            _resultOnContains = resultOnContains;
        }

        public FileScanResult Scan(string fileContent)
        {
            return fileContent.Contains(_stringForCheck) 
                ? _resultOnContains 
                : FileScanResult.ProblemsNotFound;
        }
    }
}