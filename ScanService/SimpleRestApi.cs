using System.IO;
using System.Threading.Tasks;

namespace ScanService
{
    public static class SimpleRestApi
    {
        public static async Task OkAsync(TextWriter writer, params string[] lines)
        {
            await writer.WriteLineAsync("Ok");
            
            foreach (var line in lines)
            {
                await writer.WriteLineAsync(line);
            }

            await writer.FlushAsync();
        }

        public static async Task ErrorAsync(TextWriter writer, string errorMessage)
        {
            await writer.WriteLineAsync("Error");
            await writer.WriteLineAsync(errorMessage);
            await writer.FlushAsync();
        }
    }
}