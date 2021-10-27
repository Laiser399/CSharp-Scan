using System;

namespace ScanService
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("No directory in arguments.");
            }

            var scanner = new DirectoryScanner();
            scanner.Go(args[0]);
        }
    }
}