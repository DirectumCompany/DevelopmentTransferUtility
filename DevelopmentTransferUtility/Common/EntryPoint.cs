using CommandLine;
using Common.Logging;
using System;
using System.IO;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Точка входа.
  /// </summary>
  internal class EntryPoint
  {
    #region Поля и свойства

    /// <summary>
    /// Логгер.
    /// </summary>
    private static ILog Log = LogManager.GetLogger<EntryPoint>();

    #endregion

    #region Методы

    /// <summary>
    /// Обработать экспорт.
    /// </summary>
    /// <param name="options">Ключи командной строки.</param>
    private static void DebugProcess(ICommandLineOptions options)
    {
      options.Mode = "import";
      options.Type = "wizards";
      options.DevelopmentFolderName = @"D:\TFSProjects\ISBVersionControlSystem\Dev\DevelopmentTransferUtility\bin\Debug\Test\export";
      options.DevelopmentPackageFileName = "D:\\TFSProjects\\ISBVersionControlSystem\\Dev\\DevelopmentTransferUtility\\bin\\Debug\\Test\\import\\Wizards.xml";
      //options.DevelopmentPackageFileName = @"D:\TEMP\тестирование импорта ТМ и МД в tfs 2\Wizards.xml";

      switch (options.Mode.ToLower())
      {
        default:
        case "export":
          ProcessExport(options);
          break;
        case "import":
          ProcessImport(options);
          break;
      }
    }

    /// <summary>
    /// Проверить, что параметр с именем файла пакета разработки пуст.
    /// </summary>
    /// <param name="options">Параметры командной строки.</param>
    /// <returns>Истина, если параметр командной строки с именем пакета разработки пуст.</returns>
    private static bool IsDevelopmentPackageParamClear(ICommandLineOptions options)
    {
      return string.IsNullOrEmpty(options.DevelopmentPackageFileName);
    }

    /// <summary>
    /// Проверить, что разработку необходимо экспортировать.
    /// </summary>
    /// <param name="options">Параметры командной строки.</param>
    /// <returns>Истина, если разработку необходимо экспортировать.</returns>
    private static bool NeedExportDevelopment(ICommandLineOptions options)
    {
      return IsDevelopmentPackageParamClear(options);
    }

    /// <summary>
    /// Проверить, что разработку необходимо импортировать.
    /// </summary>
    /// <param name="options">Параметры командной строки.</param>
    /// <returns>Истина, если разработку необходимо импортировать.</returns>
    private static bool NeedImportDevelopment(ICommandLineOptions options)
    {
      return IsDevelopmentPackageParamClear(options);
    }

    /// <summary>
    /// Получить имя файла с разработкой.
    /// </summary>
    /// <param name="options">Параметры командной строки.</param>
    /// <param name="tempFolderName">Имя временной папки.</param>
    /// <returns>Имя файла с разработкой.</returns>
    private static string GetDevelopmentFileName(ICommandLineOptions options, out string tempFolderName)
    {
      string developmentFileName;
      
      if (NeedExportDevelopment(options))
      {
        string exportFileName;
        switch (options.Type)
        {
          default:
          case "standard":
            exportFileName = DevelopmentTransformer.StandartFileName;
            break;
          case "wizards":
            exportFileName = DevelopmentTransformer.WizardsFileName;
            break;
          case "routes":
            exportFileName = DevelopmentTransformer.RouteFileName;
            break;
        }
        tempFolderName = Utils.GetUniqueTempFolderName();
        developmentFileName = Path.Combine(tempFolderName, exportFileName);
        Directory.CreateDirectory(tempFolderName);
      }
      else
      {
        tempFolderName = string.Empty;
        developmentFileName = options.DevelopmentPackageFileName;
      }

      return developmentFileName;
    }

    /// <summary>
    /// Выполнить экспорт разработки.
    /// </summary>
    /// <param name="options">Параметры командной строки.</param>
    /// <param name="developmentFileName">Имя файла с разработкой.</param>
    private static void ExportDevelopment(ICommandLineOptions options, string developmentFileName)
    {
      try
      {
        Log.Info("Экспорт из базы данных: запущен.");
        switch (options.Type)
        {
          default:
          case "standard":
            var exportDevelopmentRunner = new TransferDevelopmentRunner();
            exportDevelopmentRunner.Options = options;
            exportDevelopmentRunner.DevelopmentPackageFileName = developmentFileName;
            exportDevelopmentRunner.Export();
            break;
          case "wizards":
          case "routes":
            var exportRecordsRunner = new TransferRecordsRunner();
            exportRecordsRunner.Options = options;
            exportRecordsRunner.OutputFileName = developmentFileName;
            exportRecordsRunner.Export();
            break;
        }
        Log.Info("Экспорт из базы данных: завершен.");
      }
      catch (Exception ex)
      {
        Log.Error("Экспорт из базы данных: ошибка.", ex);
        throw;
      }
    }

    /// <summary>
    /// Выполнить импорт разработки.
    /// </summary>
    /// <param name="options">Параметры командной строки.</param>
    /// <param name="developmentFileName">Имя файла с разработкой.</param>
    private static void ImportDevelopment(ICommandLineOptions options, string developmentFileName)
    {
      try
      {
        Log.Info("Импорт в базу данных: запущен.");
        switch (options.Type)
        {
          default:
          case "standard":
            var importDevelopmentRunner = new TransferDevelopmentRunner();
            importDevelopmentRunner.Options = options;
            importDevelopmentRunner.DevelopmentPackageFileName = developmentFileName;
            importDevelopmentRunner.Import();
            break;
          case "wizards":
          case "routes":
            var importRecordsRunner = new TransferRecordsRunner();
            importRecordsRunner.Options = options;
            importRecordsRunner.OutputFileName = developmentFileName;
            importRecordsRunner.Import();
            break;
        }

        Log.Info("Импорт в базу данных: завершен.");
      }
      catch (Exception ex)
      {
        Log.Error("Импорт в базу данных: ошибка.", ex);
        throw;
      }
    }

    /// <summary>
    /// Выполнить преобразование пакета разработки в структуру папок.
    /// </summary>
    /// <param name="options">Параметры командной строки.</param>
    /// <param name="developmentFileName">Имя файла с разработкой.</param>
    private static void TransformPackageToFolder(ICommandLineOptions options, string developmentFileName)
    {
      try
      {
        Log.Info("Преобразование пакета в структуру папок: запущено.");
        var developmentTransformer = new DevelopmentTransformer(developmentFileName, options);
        developmentTransformer.TransformPackageToFolder();
        Log.Info("Преобразование пакета в структуру папок: завершено.");
      }
      catch (Exception ex)
      {
        Log.Error("Преобразование пакета в струкуру папок: ошибка.", ex);
        throw;
      }
    }

    /// <summary>
    /// Выполнить преобразование структуры папок в пакет разработки.
    /// </summary>
    /// <param name="options">Параметры командной строки.</param>
    /// <param name="developmentFileName">Имя файла с разработкой.</param>
    private static void TransformFolderToPackage(ICommandLineOptions options, string developmentFileName)
    {
      try
      {
        Log.Info("Преобразование структуры папок в пакет: запущено.");
        var developmentTransformer = new DevelopmentTransformer(developmentFileName, options);
        developmentTransformer.TransformFolderToPackage();
        Log.Info("Преобразование структуры папок в пакет: завершено.");
    }
      catch (Exception ex)
      {
        Log.Error("Преобразование струкуры папок в пакет: ошибка.", ex);
        throw;
      }
}

    /// <summary>
    /// Обработать экспорт.
    /// </summary>
    /// <param name="options">Ключи командной строки.</param>
    private static void ProcessExport(ICommandLineOptions options, string processType="")
    {
      string tempFolder; 
      string developmentFileName = GetDevelopmentFileName(options, out tempFolder);
      
      if (!string.IsNullOrEmpty(processType))
        options.Type = processType;

      try
      {
        if (NeedExportDevelopment(options))
          ExportDevelopment(options, developmentFileName);

       TransformPackageToFolder(options, developmentFileName);

      }
      finally
      {
        if (NeedExportDevelopment(options))
          Directory.Delete(tempFolder, true);
      }
    }

    /// <summary>
    /// Обработать импорт.
    /// </summary>
    /// <param name="options">Ключи командной строки.</param>
    private static void ProcessImport(ICommandLineOptions options)
    {
      string tempFolder;
      string developmentFileName = GetDevelopmentFileName(options, out tempFolder);
      try
      {
        TransformFolderToPackage(options, developmentFileName);

        if (NeedImportDevelopment(options))
          ImportDevelopment(options, developmentFileName);
      }
      finally
      {
        if (NeedImportDevelopment(options))
          Directory.Delete(tempFolder, true);
      }
    }

    /// <summary>
    /// Точка входа в программу.
    /// </summary>
    /// <param name="args">Параметры командной строки.</param>
    public static void Main(string[] args)
    {

            bool needCloseApplicationWindow = false;

      try
      {
        var options = new CommandLineOptions();

        if (Parser.Default.ParseArguments(args, options))
        {
          needCloseApplicationWindow = options.CloseWindow;
          switch (options.Mode.ToLower())
          {
            default:
            case "export":
              if (options.Type == "all")
              {
                ProcessExport(options, "standard");
                ProcessExport(options, "routes");
                ProcessExport(options, "wizards");
               }
              
              else
                ProcessExport(options);
              
              if (options.convertToUTF8)
              {
                 Console.Write("Конвертация в UTF-8.. ");
                 filestoutf8.convert(options.DevelopmentFolderName);
                 Console.Write("Done");
                 Console.WriteLine();
              }
              
              break;
            case "import":
              ProcessImport(options);
              break;
          }
        }
        Log.Info("Готово!");
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Возникла ошибка: {0}", ex.Message);
      }

      if (!needCloseApplicationWindow) 
        Console.ReadKey();
    }

    #endregion
  }
}
