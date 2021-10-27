namespace ScanService
{
    public class SimpleFileContentScanner : IFileContentScanner
    {
        private readonly string _stringForCheck;
        private readonly ScanResult _resultOnContains;

        public SimpleFileContentScanner(string stringForCheck, ScanResult resultOnContains)
        {
            _stringForCheck = stringForCheck;
            _resultOnContains = resultOnContains;
        }

        public ScanResult Scan(string fileContent)
        {
            return fileContent.Contains(_stringForCheck) 
                ? _resultOnContains 
                : ScanResult.ProblemsNotFound;
        }
    }
}