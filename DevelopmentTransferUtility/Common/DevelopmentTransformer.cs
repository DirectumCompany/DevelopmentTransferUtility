using Common.Logging;
using NpoComputer.DevelopmentTransferUtility.Handlers;
using NpoComputer.DevelopmentTransferUtility.Handlers.Package;
using NpoComputer.DevelopmentTransferUtility.Handlers.Records;
using NpoComputer.DevelopmentTransferUtility.Models.Base;
using NpoComputer.DevelopmentTransferUtility.Models.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Преобразователь разработки.
  /// </summary>
  internal class DevelopmentTransformer
  {
    #region Константы 

    /// <summary>
    /// Имя файла со стандартной разработкой.
    /// </summary>
    public const string StandartFileName = "dev.isx";

    /// <summary>
    /// Имя файла со стандартной разработкой.
    /// </summary>
    public const string WizardsFileName = "Wizards.xml";

    /// <summary>
    /// Имя файла с типовыми маршрутами.
    /// </summary>
    public const string RouteFileName = "TipMarsh.xml";

    /// <summary>
    /// Имя файла с группами типовых маршрутов.
    /// </summary>
    protected const string RouteGroupFileName = "GroupTM.dat";

    /// <summary>
    /// Имя файла с ролями типовых маршрутов.
    /// </summary>
    protected const string RouteRoleFileName = "RoleTM.dat";

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Логгер.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger<DevelopmentTransformer>();

    /// <summary>
    /// Обработчики пакета.
    /// </summary>
    private readonly List<IPackageHandler> packageHandlers;

    /// <summary>
    /// Обработчик типовых маршрутов.
    /// </summary>
    private readonly Dictionary<string, IRecordHandler> routeHandlers;

    /// <summary>
    /// Обработчик мастеров действий.
    /// </summary>
    private readonly List<IRecordHandler> wizardHandlers;

    /// <summary>
    /// Фильтр импорта.
    /// </summary>
    private ImportFilter importFilter;

    /// <summary>
    /// Имя файла пакета.
    /// </summary>
    public string PackageFileName { get; private set; }

    /// <summary>
    /// Папка с разработкой.
    /// </summary>
    public string DevelopmentPath { get; private set; }

    /// <summary>
    /// Параметры командной строки.
    /// </summary>
    public ICommandLineOptions CommandLineOptions { get; private set; }

    #endregion

    #region Методы

    /// <summary>
    /// Получить настройки записи XML.
    /// </summary>
    /// <returns>Настройки записи XML.</returns>
    private static XmlWriterSettings GetXmlWriterSettings()
    {
      var writerSettings = new XmlWriterSettings();
      writerSettings.Encoding = TransformerEnvironment.CurrentEncoding;
      writerSettings.Indent = true;
      writerSettings.NewLineHandling = NewLineHandling.Entitize;
      return writerSettings;
    }

    /// <summary>
    /// Получить коллекцию пространств имен сериализации.
    /// </summary>
    /// <returns>Коллекция пространств имен сериализации.</returns>
    private static XmlSerializerNamespaces GetXmlSerializerNamespaces()
    {
      var ns = new XmlSerializerNamespaces();
      ns.Add(string.Empty, string.Empty);
      return ns;
    }

    /// <summary>
    /// Получить имя файла информации о пакете.
    /// </summary>
    /// <returns>Имя файла информации о пакете.</returns>
    private string GetPackageInfoFileName()
    {
      return Path.Combine(this.DevelopmentPath, "PackageInfo.xml");
    }

    /// <summary>
    /// Получить кодировку из XML-файла.
    /// </summary>
    /// <param name="fileName">Имя файла.</param>
    /// <returns>Кодировка XML-файла.</returns>
    private Encoding GetEncodingFromFile(string fileName)
    {
      using (var reader = new XmlTextReader(fileName))
      {
        reader.MoveToContent();
        return reader.Encoding;
      }
    }

    /// <summary>
    /// Сохранить информацию о пакете.
    /// </summary>
    /// <param name="packageInfoModel">Модель информации о пакете.</param>
    private void SavePackageInfoModel(PackageInfoModel packageInfoModel)
    {
      Log.Info("  Сохранение информации о пакете: запущено");
      var packageInfoSerializer = new XmlSerializer(typeof(PackageInfoModel));
      using (var packageInfoStream = new FileStream(this.GetPackageInfoFileName(), FileMode.Create))
        using (var writer = XmlWriter.Create(packageInfoStream, GetXmlWriterSettings()))
          packageInfoSerializer.Serialize(writer, packageInfoModel, GetXmlSerializerNamespaces());
      Log.Info("  Сохранение информации о пакете: завершено");
    }

    /// <summary>
    /// Загрузить информацию о пакете.
    /// </summary>
    /// <returns>Инфорация о пакете.</returns>
    private PackageInfoModel LoadPackageInfoModel()
    {
      Log.Info("  Загрузка информации о пакете: запущена");
      PackageInfoModel result = null;
      var packageInfoSerializer = new XmlSerializer(typeof(PackageInfoModel));
      using (var packageInfoStream = new FileStream(this.GetPackageInfoFileName(), FileMode.Open))
        using (var reader = XmlReader.Create(packageInfoStream))
          result = (PackageInfoModel)packageInfoSerializer.Deserialize(reader);
      Log.Info("  Загрузка информации о пакете: завершена");
      return result;
    }

    /// <summary>
    /// Выполнить преобразование структуры папок в пакет разработки для импорта.
    /// </summary>
    private void HandleImportPackage()
    {
      var componentsModel = new ComponentsModel();

      var packageInfoModel = this.LoadPackageInfoModel();
      componentsModel.SetupPackageInfo(packageInfoModel);

      Parallel.ForEach<IPackageHandler>(this.packageHandlers,
        (handler) => handler.HandleImport(componentsModel, this.importFilter));

      componentsModel.ClearEmptyLists();

      using (var fileStream = new FileStream(this.PackageFileName, FileMode.Create))
      {
        var packageSerializer = new XmlSerializer(typeof(ComponentsModel));
        using (var writer = XmlWriter.Create(fileStream, GetXmlWriterSettings()))
        {
          try
          {
            Log.Info("  Запись пакета разработки: запущена.");
            packageSerializer.Serialize(writer, componentsModel, GetXmlSerializerNamespaces());
            Log.Info("  Запись пакета разработки: завершена.");
          }
          catch (Exception ex)
          {
            Log.Error("  Запись пакета разработки: ошибка.", ex);
            throw;
          }
        }
      }
    }

    /// <summary>
    /// Выполнить преобразование структуры папок в разработку мастеров действий для импорта.
    /// </summary>
    private void HandleImportWizards()
    {
      var rootModel = new RootModel();

      Parallel.ForEach<IRecordHandler>(this.wizardHandlers,
        (handler) => handler.HandleImport(rootModel, this.importFilter));

      using (var fileStream = new FileStream(this.PackageFileName, FileMode.Create))
      {
        var packageSerializer = new XmlSerializer(typeof(RootModel));
        using (var writer = XmlWriter.Create(fileStream, GetXmlWriterSettings()))
        {
          try
          {
            Log.Info("  Запись пакета разработки МД: запущена.");
            packageSerializer.Serialize(writer, rootModel, GetXmlSerializerNamespaces());
            Log.Info("  Запись пакета разработки МД: завершена.");
          }
          catch (Exception ex)
          {
            Log.Error("  Запись пакета разработки МД: ошибка.", ex);
            throw;
          }
        }
      }
    }

    /// <summary>
    /// Выполнить преобразование структуры папок в разработку типовых маршрутов для импорта.
    /// </summary>
    private void HandleImportRoute()
    {
      foreach (var routeHandlerType in this.routeHandlers)
      {
        IRecordHandler routeHandler = routeHandlerType.Value;
        string filePath = Path.Combine(Path.GetDirectoryName(this.PackageFileName), routeHandlerType.Key);
        var rootModel = new RootModel();
        routeHandler.HandleImport(rootModel, this.importFilter);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
          var recordsSerializer = new XmlSerializer(typeof(RootModel));
          using (var writer = XmlWriter.Create(fileStream, GetXmlWriterSettings()))
          {
            try
            {
              Log.Info("  Запись пакета разработки ТМ: запущена.");
              recordsSerializer.Serialize(writer, rootModel, GetXmlSerializerNamespaces());
              Log.Info("  Запись пакета разработки ТМ: завершена.");
            }
            catch (Exception ex)
            {
              Log.Error("  Запись пакета разработки ТМ: ошибка.", ex);
              throw;
            }
          }
        }
      }
    }

    /// <summary>
    /// Выполнить преобразование разработки типовых маршрутов в структуру папок.
    /// </summary>
    private void HandleExportRoute(XmlReaderSettings packageReaderSettings)
    {
      foreach (var routeHandlerType in this.routeHandlers)
      {
        IRecordHandler routeHandler = routeHandlerType.Value;
        string filePath = Path.Combine(Path.GetDirectoryName(this.PackageFileName), routeHandlerType.Key);
        if (File.Exists(filePath))
        {
          using (var fileStream = File.OpenRead(filePath))
          {
            var recordsSerializer = new XmlSerializer(typeof(RootModel));
            using (var reader = XmlReader.Create(fileStream, packageReaderSettings))
            {
              RootModel rootModel = null;
              try
              {
                Log.Info("  Чтение пакета ТМ: запущено.");
                rootModel = (RootModel)recordsSerializer.Deserialize(reader);
                Log.Info("  Чтение пакета ТМ: завершено.");
              }
              catch (Exception ex)
              {
                Log.Error("  Чтение пакета ТМ: ошибка. " + ex.Message, ex);
                throw;
              }

              var settings = GetXmlWriterSettings();
              var namespaces = GetXmlSerializerNamespaces();
              routeHandler.HandleExport(rootModel, settings, namespaces);

              if (this.CommandLineOptions.AuthType == AuthenticationType.Windows)
              {
                Log.Warn("  Обработка удаления типовых маршрутов не будет выполнена, так как используется Windows-аутентификация.");
                continue;
              }

              if (string.IsNullOrEmpty(this.CommandLineOptions.Server) ||
                  string.IsNullOrEmpty(this.CommandLineOptions.Database) ||
                  string.IsNullOrEmpty(this.CommandLineOptions.UserName))
              {
                Log.Warn("  Обработка удаления типовых маршрутов не будет выполнена, так как не заданы параметры подключения к БД.");
                continue;
              }

              var connectionParams = new ConnectionParams(
                this.CommandLineOptions.Server,
                this.CommandLineOptions.Database,
                this.CommandLineOptions.UserName,
                this.CommandLineOptions.Password);
              routeHandler.HandleDelete(connectionParams);
            }
          }
        }
      }
    }

    /// <summary>
    /// Выполнить экспорт из пакета разработки в структуру папок.
    /// </summary>
    private void HandleExportPackage(XmlReaderSettings packageReaderSettings)
    {
      using (var fileStream = File.OpenRead(this.PackageFileName))
      {
        var packageSerializer = new XmlSerializer(typeof(ComponentsModel));
        using (var reader = XmlReader.Create(fileStream, packageReaderSettings))
        {
          ComponentsModel packageModel = null;
          try
          {
            Log.Info("  Чтение пакета разработки: запущено.");
            packageModel = (ComponentsModel)packageSerializer.Deserialize(reader);
            Log.Info("  Чтение пакета разработки: завершено.");
          }
          catch (Exception ex)
          {
            Log.Error("  Чтение пакета разработки: ошибка.", ex);
            throw;
          }

          var settings = GetXmlWriterSettings();
          var namespaces = GetXmlSerializerNamespaces();

          Parallel.ForEach<IPackageHandler>(this.packageHandlers,
            (handler) => handler.HandleExport(packageModel, settings, namespaces));

          var packageInfoModel = PackageInfoModel.CreateFromComponentsModel(packageModel);
          this.SavePackageInfoModel(packageInfoModel);

          if (this.CommandLineOptions.AuthType == AuthenticationType.Windows)
          {
            Log.Warn("  Обработка удаления элементов разработки не будет выполнена, так как используется Windows-аутентификация.");
            return;
          }

          if (string.IsNullOrEmpty(this.CommandLineOptions.Server) ||
              string.IsNullOrEmpty(this.CommandLineOptions.Database) ||
              string.IsNullOrEmpty(this.CommandLineOptions.UserName))
          {
            Log.Warn("  Обработка удаления элементов разработки не будет выполнена, так как не заданы параметры подключения к БД.");
            return;
          }

          var connectionParams = new ConnectionParams(
            this.CommandLineOptions.Server,
            this.CommandLineOptions.Database,
            this.CommandLineOptions.UserName,
            this.CommandLineOptions.Password);
          Parallel.ForEach<IPackageHandler>(this.packageHandlers,
            (handler) => handler.HandleDelete(connectionParams, settings, namespaces));
        }
      }
    }

    /// <summary>
    /// Выполнить преобразование разработки мастеров действий в структуру папок.
    /// </summary>
    /// <param name="packageReaderSettings">Настройки чтения из XML.</param>
    private void HandleExportWizards(XmlReaderSettings packageReaderSettings)
    {
      using (var fileStream = File.OpenRead(this.PackageFileName))
      {
        var packageSerializer = new XmlSerializer(typeof(RootModel));
        using (var reader = XmlReader.Create(fileStream, packageReaderSettings))
        {
          RootModel rootModel = null;
          try
          {
            Log.Info("  Чтение пакета мастеров действий: запущено.");
            rootModel = (RootModel)packageSerializer.Deserialize(reader);
            Log.Info("  Чтение пакета мастеров действий: завершено.");
          }
          catch (Exception ex)
          {
            Log.Error("  Чтение пакета мастеров действий: ошибка.", ex);
            throw;
          }

          var settings = GetXmlWriterSettings();
          var namespaces = GetXmlSerializerNamespaces();

          Parallel.ForEach<IRecordHandler>(this.wizardHandlers,
            (handler) => handler.HandleExport(rootModel, settings, namespaces));

          if (this.CommandLineOptions.AuthType == AuthenticationType.Windows)
          {
            Log.Warn("  Обработка удаления мастеров действий не будет выполнена, так как используется Windows-аутентификация.");
            return;
          }

          if (string.IsNullOrEmpty(this.CommandLineOptions.Server) ||
              string.IsNullOrEmpty(this.CommandLineOptions.Database) ||
              string.IsNullOrEmpty(this.CommandLineOptions.UserName))
          {
            Log.Warn("  Обработка удаления мастеров действий не будет выполнена, так как не заданы параметры подключения к БД.");
            return;
          }

          var connectionParams = new ConnectionParams(
            this.CommandLineOptions.Server,
            this.CommandLineOptions.Database,
            this.CommandLineOptions.UserName,
            this.CommandLineOptions.Password);
          Parallel.ForEach<IRecordHandler>(this.wizardHandlers,
            (handler) => handler.HandleDelete(connectionParams));
        }
      }
    }

    /// <summary>
    /// Выполнить преобразование пакета в структуру папок.
    /// </summary>
    public void TransformPackageToFolder()
    {
      if (!File.Exists(this.PackageFileName))
        return;

      var packageReaderSettings = new XmlReaderSettings();
      packageReaderSettings.ConformanceLevel = ConformanceLevel.Document;

      TransformerEnvironment.CurrentEncoding = this.GetEncodingFromFile(this.PackageFileName);

      switch (this.CommandLineOptions.Type)
      {
        default:
        case "standard":
          this.HandleExportPackage(packageReaderSettings);
          break;
        case "wizards":
          this.HandleExportWizards(packageReaderSettings);
          break;
        case "routes":
          this.HandleExportRoute(packageReaderSettings);
          break;
      }
    }

    /// <summary>
    /// Выполнить преобразование структуры папок в пакет.
    /// </summary>
    public void TransformFolderToPackage()
    {
      if (!string.IsNullOrEmpty(this.CommandLineOptions.Changesets) ||
          !string.IsNullOrEmpty(this.CommandLineOptions.FromDateFilter) ||
          !string.IsNullOrEmpty(this.CommandLineOptions.ToDateFilter) ||
          !string.IsNullOrEmpty(this.CommandLineOptions.UserFilter))
        this.importFilter = new ImportFilter(
          this.CommandLineOptions.TfsCollection,
          this.CommandLineOptions.TfsDevelopmentPath,
          this.CommandLineOptions.Changesets,
          this.CommandLineOptions.FromDateFilter,
          this.CommandLineOptions.ToDateFilter,
          this.CommandLineOptions.UserFilter);
      else if (!string.IsNullOrEmpty(this.CommandLineOptions.IncludedImportFolders))
        this.importFilter = new ImportFilter(this.CommandLineOptions.IncludedImportFolders);
      else
        this.importFilter = new ImportFilter();

      switch (this.CommandLineOptions.Type)
      {
        default:
        case "standard":
          TransformerEnvironment.CurrentEncoding = this.GetEncodingFromFile(this.GetPackageInfoFileName());

          this.HandleImportPackage();
          break;
        case "wizards":
          // Обработать импорт мастеров действий.
          this.HandleImportWizards();
          break;
        case "routes":
          // Обработать импорт типовых маршрутов.
          this.HandleImportRoute();
          break;
      }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="inputFile">Входной файл пакета.</param>
    /// <param name="options">Аргументы командной строки.</param>
    public DevelopmentTransformer(string inputFile, ICommandLineOptions options)
    {
      this.PackageFileName = inputFile;
      this.DevelopmentPath = options.DevelopmentFolderName.Trim();
      this.CommandLineOptions = options;

      this.routeHandlers = new Dictionary<string, IRecordHandler>
      {
        { RouteFileName, new RouteRecordHandler(inputFile, this.DevelopmentPath) },
        { RouteGroupFileName, new RouteGroupHandler(this.DevelopmentPath) },
        { RouteRoleFileName, new RouteRoleHandler(inputFile, this.DevelopmentPath) }
      };

      this.wizardHandlers = new List<IRecordHandler>
      {
        new WizardGroupHandler(this.DevelopmentPath),
        new WizardRecordHandler(this.DevelopmentPath)
      };

      this.packageHandlers = new List<IPackageHandler>
      {
        new ConstantHandler(this.DevelopmentPath),
        new DialogHandler(this.DevelopmentPath),
        new DialogRequisiteHandler(this.DevelopmentPath),
        new DocumentCardTypeHandler(this.DevelopmentPath),
        new DocumentRequisiteHandler(this.DevelopmentPath),
        new FunctionGroupHandler(this.DevelopmentPath),
        new FunctionHandler(this.DevelopmentPath),
        new IntegratedReportHandler(this.DevelopmentPath),
        new LocalizationStringHandler(this.DevelopmentPath),
        new ModuleHandler(this.DevelopmentPath),
        new ReferenceRequisiteHandler(this.DevelopmentPath),
        new ReferenceTypeHandler(this.DevelopmentPath),
        new ReportHandler(this.DevelopmentPath),
        new RouteBlockGroupHandler(this.DevelopmentPath),
        new RouteBlockHandler(this.DevelopmentPath),
        new ScriptHandler(this.DevelopmentPath),
        new ServerEventHandler(this.DevelopmentPath),
        new ViewerHandler(this.DevelopmentPath)
      };
    }

    #endregion
  }
}
