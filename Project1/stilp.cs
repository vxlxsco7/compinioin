using System;
using System.IO;
using System.IO.Compression;
using System.Security.Principal;
using System.Drawing; 
using System.Windows.Forms; 
using System.Management; 
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        string userName = GetCurrentUserName();

        if (string.IsNullOrEmpty(userName))
        {
            Console.WriteLine("Не удалось получить имя пользователя.");
            return;
        }

        string sourcePath = Path.Combine("C:\\Users", userName, "Documents", "NSD");
        string destinationDirectory = Path.Combine("C:\\Users", userName, "Desktop", "copy");

        Directory.CreateDirectory(destinationDirectory);

        try
        {
            CopyDirectory(sourcePath, destinationDirectory);
            Console.WriteLine("Копирование завершено.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при копировании: {ex.Message}");
            return;
        }

    
        string screenshotPath = Path.Combine(destinationDirectory, $"{DateTime.Now:dd.MM.yyyy_HH.mm.ss}_{userName}_screenshot.png");
        CaptureScreenshot(screenshotPath);
        Console.WriteLine($"Скриншот сохранен: {screenshotPath}");

  
        string infoFilePath = Path.Combine(destinationDirectory, "info_pc.txt");
        SaveComputerInfo(infoFilePath);
        Console.WriteLine($"Информация о компьютере сохранена: {infoFilePath}");

        string opiomDirectory = Path.Combine(destinationDirectory, "opiom");
        Directory.CreateDirectory(opiomDirectory);

        string timestamp = DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss");
        string zipPath = Path.Combine(opiomDirectory, $"{timestamp}_{userName}.zip");

        if (File.Exists(zipPath))
        {
            try
            {
                File.Delete(zipPath);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при удалении файла: {ex.Message}");
                return;
            }
        }

        try
        {
            ZipFile.CreateFromDirectory(destinationDirectory, zipPath);
            Console.WriteLine("Архив успешно создан.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании архива: {ex.Message}");
        }

        try
        {
            DeleteFilesAndDirectoriesExcept(destinationDirectory, "opiom");
            Console.WriteLine("Все файлы в папке 'copy', кроме 'opiom', успешно удалены.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении файлов: {ex.Message}");
        }
    }

    static void CaptureScreenshot(string filePath)
    {

        Rectangle bounds = Screen.GetBounds(Point.Empty);
        using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }
            bitmap.Save(filePath); 
        }
    }

    static void SaveComputerInfo(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Информация о компьютере:");
            writer.WriteLine($"Имя компьютера: {Environment.MachineName}");
            writer.WriteLine($"Имя пользователя: {Environment.UserName}");
            writer.WriteLine($"Версия ОС: {Environment.OSVersion}");
            writer.WriteLine($"Архитектура: {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");
            writer.WriteLine($"Количество процессоров: {Environment.ProcessorCount}");
            writer.WriteLine($"Текущая директория: {Environment.CurrentDirectory}");


            writer.WriteLine(new string('-', 15));
            writer.WriteLine(); 

  
            try
            {
                var searcher = new ManagementObjectSearcher("select Name from Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    writer.WriteLine($"Имя процессора: {obj["Name"]}");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"Ошибка при получении информации о процессоре: {ex.Message}");
            }

            try
            {
                var searcher = new ManagementObjectSearcher("select Capacity, Manufacturer, PartNumber from Win32_PhysicalMemory");
                foreach (ManagementObject obj in searcher.Get())
                {
                    long capacityInMB = Convert.ToInt64(obj["Capacity"]) / (1024 * 1024);
                    writer.WriteLine($"Оперативная память: {capacityInMB} МБ, Производитель: {obj["Manufacturer"]}, Номер детали: {obj["PartNumber"]}");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"Ошибка при получении информации о оперативной памяти: {ex.Message}");
            }

            try
            {
                var searcher = new ManagementObjectSearcher("select Name, AdapterRAM from Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    long adapterRAMInMB = Convert.ToInt64(obj["AdapterRAM"]) / (1024 * 1024);
                    writer.WriteLine($"Видеокарта: {obj["Name"]}, Видеопамять: {adapterRAMInMB} МБ");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"Ошибка при получении информации о видеокарте: {ex.Message}");
            }

         
            try
            {
                var searcher = new ManagementObjectSearcher("select Name from Win32_DesktopMonitor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    writer.WriteLine($"Монитор: {obj["Name"]}");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"Ошибка при получении информации о мониторе: {ex.Message}");
            }


            writer.WriteLine();
            writer.WriteLine(new string('-', 15)); 
            writer.WriteLine("Информация о системе Windows:");

            try
            {
                var searcher = new ManagementObjectSearcher("SELECT Caption, Version, BuildNumber, OSArchitecture FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    writer.WriteLine($"Название ОС: {obj["Caption"]}");
                    writer.WriteLine($"Версия: {obj["Version"]}");
                    writer.WriteLine($"Номер сборки: {obj["BuildNumber"]}");
                    writer.WriteLine($"Архитектура: {obj["OSArchitecture"]}");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"Ошибка при получении информации о системе Windows: {ex.Message}");
            }

            


          
            writer.WriteLine(); 
            writer.WriteLine(new string('-', 15)); 
            writer.WriteLine(); 

            try
            {
                var searcher = new ManagementObjectSearcher("select Name from Win32_PnPEntity");
                foreach (ManagementObject obj in searcher.Get())
                {
                    writer.WriteLine($"Устройство: {obj["Name"]}");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"Ошибка при получении информации о устройствах: {ex.Message}");
            }

            
            writer.WriteLine(); 
            writer.WriteLine(new string('-', 15));
            writer.WriteLine("Текущие процессы:");

            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    writer.WriteLine($"Процесс: {process.ProcessName}, ID: {process.Id}, Память: {process.WorkingSet64 / (1024 * 1024)} МБ");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"Ошибка при получении информации о процессах: {ex.Message}");
            }

            
            writer.WriteLine(); 
            writer.WriteLine(new string('-', 15)); 
            writer.WriteLine("Информация о системе Windows:");

            

        }
    }

    static void CopyDirectory(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(sourceDir))
        {
            throw new DirectoryNotFoundException($"Исходная директория не найдена: {sourceDir}");
        }

        Directory.CreateDirectory(destinationDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destinationDir, fileName);
            File.Copy(file, destFile, overwrite: true);
            Console.WriteLine($"Скопирован файл: {fileName}");
        }

        foreach (string directory in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(directory);
            string destDir = Path.Combine(destinationDir, dirName);
            CopyDirectory(directory, destDir);
        }
    }

    static void DeleteFilesAndDirectoriesExcept(string directory, string exceptionFolderName)
    {
        foreach (string file in Directory.GetFiles(directory))
        {
            File.Delete(file);
            Console.WriteLine($"Удален файл: {Path.GetFileName(file)}");
        }

        foreach (string dir in Directory.GetDirectories(directory))
        {
            string dirName = Path.GetFileName(dir);
            if (dirName != exceptionFolderName)
            {
                Directory.Delete(dir, true);
                Console.WriteLine($"Удалена папка: {dirName}");
            }
        }
    }

    static string GetCurrentUserName()
    {
        return WindowsIdentity.GetCurrent().Name.Split('\\')[1];
    }
}

