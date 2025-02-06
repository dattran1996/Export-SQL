using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;

class FileCopyScript
{
    static void Main()
    {
        // Đọc các giá trị từ app.config
        string filelistPath = ConfigurationManager.AppSettings["FileListPath"];
        string sourcedir = ConfigurationManager.AppSettings["SourceDirectory"];

        // Đặt thư mục đích là thư mục cùng nơi với file .exe, với tên "Database"
        string destdir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
        string logfilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "copy_log.txt");

        try
        {
            // Xóa file log cũ nếu tồn tại
            if (File.Exists(logfilePath))
            {
                File.Delete(logfilePath);
            }

            // Xóa thư mục đích nếu tồn tại và tạo lại
            if (Directory.Exists(destdir))
            {
                Directory.Delete(destdir, true); // Tham số 'true' cho phép xóa cả các thư mục con
            }
            Directory.CreateDirectory(destdir);

            // Kiểm tra sự tồn tại của file danh sách
            if (!File.Exists(filelistPath))
            {
                LogError(logfilePath, $"[ERROR] File danh sách '{filelistPath}' không tồn tại!");
                Console.WriteLine("File danh sách không tồn tại. Kiểm tra lại đường dẫn tới file danh sách.");
                return;
            }

            // Đọc từng dòng trong file danh sách
            IEnumerable<string> lines = File.ReadLines(filelistPath);
            foreach (var line in lines)
            {
                string trimmedLine = line.Replace('/', '\\').Trim();

                // Kiểm tra xem dòng có chứa file .sql hay không và không phải dòng bình luận
                if (!trimmedLine.StartsWith(";") && trimmedLine.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                {
                    string sourceFilePath = Path.Combine(sourcedir, trimmedLine);
                    string destFilePath = Path.Combine(destdir, trimmedLine);

                    if (File.Exists(sourceFilePath))
                    {
                        // Tạo thư mục con cần thiết trong thư mục đích
                        string destDirectory = Path.GetDirectoryName(destFilePath);
                        if (!Directory.Exists(destDirectory))
                        {
                            Directory.CreateDirectory(destDirectory);
                        }

                        try
                        {
                            File.Copy(sourceFilePath, destFilePath, true);
                            LogInfo(logfilePath, $"Copied: {sourceFilePath}");
                        }
                        catch (Exception ex)
                        {
                            LogError(logfilePath, $"[ERROR] Failed to copy: {sourceFilePath}. Error: {ex.Message}");
                        }
                    }
                    else
                    {
                        LogWarning(logfilePath, $"[WARNING] File not found: {sourceFilePath}");
                    }
                }
            }

            Console.WriteLine("Completed copying SQL files. Check {0} for details.", logfilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
        }
    }
    // ghi log 
    static void LogInfo(string logfilePath, string message)
    {
        using (StreamWriter writer = new StreamWriter(logfilePath, true))
        {
            writer.WriteLine($"[{DateTime.Now}] {message}");
        }
    }

    static void LogWarning(string logfilePath, string message)
    {
        LogInfo(logfilePath, message);
    }

    static void LogError(string logfilePath, string message)
    {
        LogInfo(logfilePath, message);
    }
}
