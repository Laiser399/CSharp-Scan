using System;
using System.IO;
using System.Security;

namespace ScanService.Helpers
{
    public static class DirectoryInfoHelper
    {
        public static bool TryGetFiles(this DirectoryInfo directory, out FileInfo[] files)
        {
            try
            {
                files = directory.GetFiles();
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                files = Array.Empty<FileInfo>();
                return false;
            }
        }
        
        public static bool TryGetDirectories(this DirectoryInfo directory, out DirectoryInfo[] files)
        {
            try
            {
                files = directory.GetDirectories();
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                files = Array.Empty<DirectoryInfo>();
                return false;
            }
            catch (SecurityException)
            {
                files = Array.Empty<DirectoryInfo>();
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                files = Array.Empty<DirectoryInfo>();
                return false;
            }
        }
    }
}