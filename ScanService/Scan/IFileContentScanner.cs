namespace ScanService.Scan
{
    public interface IFileContentScanner
    {
        FileScanResult Scan(string fileContent);
    }
}