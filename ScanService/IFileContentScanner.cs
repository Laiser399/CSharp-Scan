namespace ScanService
{
    public interface IFileContentScanner
    {
        ScanResult Scan(string fileContent);
    }
}