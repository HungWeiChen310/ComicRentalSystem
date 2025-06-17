using System;
using System.IO;

namespace ComicRentalSystem_14Days.Helpers
{
    public static class DatabaseConfig
    {
        private const string DbName = "comic_rental.db";
        private const string AppFolderName = "ComicRentalApp";

        public static string GetConnectionString()
        {
            var customPath = Environment.GetEnvironmentVariable("COMIC_DB_PATH");
            if (!string.IsNullOrWhiteSpace(customPath))
            {
                var dir = Path.GetDirectoryName(customPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                return $"Data Source={customPath}";
            }

            // Default to a database file located in the application's base directory
            string baseDir = AppContext.BaseDirectory;
            string dbPath = Path.Combine(baseDir, DbName);

            return $"Data Source={dbPath}";
        }
    }
}
