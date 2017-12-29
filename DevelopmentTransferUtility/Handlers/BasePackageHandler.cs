using System;
using NpoComputer.DevelopmentTransferUtility.Common;
using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Common.Logging;

namespace NpoComputer.DevelopmentTransferUtility.Handlers
{
  /// <summary>
  /// Базовый обработчик.
  /// </summary>
  internal abstract class BasePackageHandler : IPackageHandler
  {
    #region Константы

    /// <summary>
    /// Имя файла карточки компоненты.
    /// </summary>
    protected const string CardFileName = "Card.xml";

    #endregion

    #region Поля и свойства.

    /// <summary>
    /// Логгер.
    /// </summary>
    private static ILog Log = LogManager.GetLogger<BasePackageHandler>();

    /// <summary>
    /// Список имен выходных файлов.
    /// </summary>
    protected readonly Collection<string> outputFileNames = new Collection<string>();

    /// <summary>
    /// Список имен выходных папок.
    /// </summary>
    protected readonly Collection<string> outputFolderNames = new Collection<string>();

    /// <summary>
    /// Папка с разработкой.
    /// </summary>
    protected string DevelopmentPath { get; }

    /// <summary>
    /// Суффикс имени папки моделей компонент.
    /// </summary>
    protected abstract string ComponentsFolderSuffix { get; }

    /// <summary>
    /// Папка моделей компонент.
    /// </summary>
    protected string ComponentsFolder => Path.Combine(this.DevelopmentPath, this.ComponentsFolderSuffix);

    /// <summary>
    /// Имя элемента разработки в родительном падеже.
    /// </summary>
    protected abstract string ElementNameInGenitive { get; }

    /// <summary>
    /// SQL-запрос на получение элементов разработки.
    /// </summary>
    protected virtual string DevelopmentElementsCommandText => 
      $"select {this.DevelopmentElementKeyFieldName} from {this.DevelopmentElementTableName}";

    /// <summary>
    /// Имя ключевого поля элемента разработки.
    /// </summary>
    protected abstract string DevelopmentElementKeyFieldName { get; }

    /// <summary>
    /// Имя таблицы элемента разработки.
    /// </summary>
    protected abstract string DevelopmentElementTableName { get; }

    /// <summary>
    /// Признак необходимости проверять существование таблицы.
    /// </summary>
    protected abstract bool NeedCheckTableExists { get; }

    /// <summary>
    /// Текущая модель компоненты, для которой выполняется экспорт/импорт.
    /// </summary>
    private ComponentModel currentComponentModel;

    #endregion

    #region Методы

    /// <summary>
    /// Сформировать имя файла карточки.
    /// </summary>
    /// <param name="path">Путь к папке с файлами.</param>
    /// <returns>Имя файла карточки.</returns>
    private static string GetCardFileName(string path)
    {
      return Path.Combine(path, CardFileName);
    }

    /// <summary>
    /// Импортировать модель из файла.
    /// </summary>
    /// <param name="fileName">Файл.</param>
    /// <param name="serializer">Сериализатор XML.</param>
    private static ComponentModel ImportModelFromFile(string fileName, XmlSerializer serializer)
    {
      using (var fileStream = new FileStream(fileName, FileMode.Open))
        using (var xmlReader = XmlReader.Create(fileStream))
          return (ComponentModel) serializer.Deserialize(xmlReader);
    }

    /// <summary>
    /// Удалить в папке файлы, которые не являются выходными. 
    /// </summary>
    /// <param name="path">Путь к файлам.</param>
    protected void DeleteUnwantedFiles(string path)
    {
      foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories))
      {
        if (File.Exists(fileSystemEntry))
        {
          if (!this.outputFileNames.Contains(fileSystemEntry))
            File.Delete(fileSystemEntry);
        }
        if (Directory.Exists(fileSystemEntry))
        {
          bool needDelete = true;
          foreach (var outputFolderName in this.outputFolderNames)
            if (outputFolderName.StartsWith(fileSystemEntry))
            {
              needDelete = false;
              break;
            }
          if (needDelete)
            Directory.Delete(fileSystemEntry, true);
          else
            this.DeleteUnwantedFiles(fileSystemEntry);
        }
      }
    }

    /// <summary>
    /// Обработать экспорт моделей (внутренний метод).
    /// </summary>
    /// <param name="packageModel">Модель пакета разработки.</param>
    /// <param name="settings">Настройки сериализации XML.</param>
    /// <param name="namespaces">Пространства имен сериализации.</param>
    /// <param name="exportedCount">Количество экспортированных элементов.</param>
    protected virtual void InternalHandleExport(ComponentsModel packageModel, XmlWriterSettings settings, XmlSerializerNamespaces namespaces, out int exportedCount)
    {
      var serializer = this.CreateSerializer();
      var models = this.TakeComponentModels(packageModel);
      exportedCount = 0;
      foreach (var model in models)
      {
        // Получаем путь к компоненте.
        var componentFolder = Path.Combine(this.ComponentsFolder, Utils.EscapeFilePath(model.KeyValue));
        if (!Directory.Exists(componentFolder))
          Directory.CreateDirectory(componentFolder);

        // Выполнить обработку модели.
        this.HandleExportModel(componentFolder, model, settings, namespaces, serializer);
        exportedCount++;
      }
    }

    /// <summary>
    /// Создать сериализатор модели.
    /// </summary>
    /// <returns>Сериализатор модели.</returns>
    private XmlSerializer CreateSerializer()
    {
      var overrides = new XmlAttributeOverrides();

      var attributes = new XmlAttributes();
      var attribute = new XmlRootAttribute();
      attribute.ElementName = this.GetElementName();
      attributes.XmlRoot = attribute;
      
      overrides.Add(typeof(ComponentModel), attributes);
      return new XmlSerializer(typeof(ComponentModel), overrides);
    }

    /// <summary>
    /// Обработать импорт модели.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="serializer">Сериализатор XML.</param>
    /// <returns>Модель.</returns>
    private ComponentModel HandleImportModel(string path, XmlSerializer serializer)
    {
      var cardFileName = GetCardFileName(path);
      this.currentComponentModel = ImportModelFromFile(cardFileName, serializer);
      this.PrepareForImport(path, this.currentComponentModel);
      return this.currentComponentModel;
    }

    /// <summary>
    /// Получить список детальных разделов модели.
    /// </summary>
    /// <param name="model">Модель.</param>
    /// <returns>Список детальных разделов модели.</returns>
    private static DataSetModel[] GetDetailDataSets(ComponentModel model)
    {
      return new[]
      {
        model.DetailDataSets.DetailDataSet1,
        model.DetailDataSets.DetailDataSet2,
        model.DetailDataSets.DetailDataSet3,
        model.DetailDataSets.DetailDataSet4,
        model.DetailDataSets.DetailDataSet5,
        model.DetailDataSets.DetailDataSet6,
        model.DetailDataSets.DetailDataSet7,
        model.DetailDataSets.DetailDataSet8,
      };
    }

    /// <summary>
    /// Получить соответствующий список моделей.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Соответствующий список моделей.</returns>
    protected abstract List<ComponentModel> GetComponentModelList(ComponentsModel packageModel);

    /// <summary>
    /// Получить соответствующий список моделей с необходимой фильтрацией.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Соответствующий список моделей.</returns>
    protected virtual IEnumerable<ComponentModel> TakeComponentModels(ComponentsModel packageModel)
    {
      return this.GetComponentModelList(packageModel);
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected abstract string GetElementName();

    /// <summary>
    /// Экспортировать модель в файл.
    /// </summary>
    /// <param name="model">Модель.</param>
    /// <param name="fileName">Файл.</param>
    /// <param name="settings">Настройки сохранение XML.</param>
    /// <param name="namespaces">Пространства имен XML.</param>
    /// <param name="serializer">Сериализатор XML.</param>
    protected void ExportModelToFile(object model, string fileName, XmlWriterSettings settings,
      XmlSerializerNamespaces namespaces, XmlSerializer serializer)
    {
      using (var memoryStream = new MemoryStream())
      {
        using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
          serializer.Serialize(xmlWriter, model, namespaces);
        this.ExportTextToFile(fileName, 
          TransformerEnvironment.CurrentEncoding.GetString(memoryStream.ToArray()));
      }
    }

    /// <summary>
    /// Преобразовать модель компоненты к формату для экспорта.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="model">Модель.</param>
    /// <returns>Выходной формат.</returns>
    protected virtual void PrepareForExport(string path, ComponentModel model)
    {
      if (model.Card != null && model.Card.Requisites.Count > 0)
      {
        this.ExportRequisites(path, model.Card.Requisites);
        this.RemoveRequisites(model.Card.Requisites);

        // Конвертировать реквизиты.
        foreach (var requisite in model.Card.Requisites)
          requisite.PrepareForExport();
      }

      if (model.DetailDataSets != null)
      {
        DataSetModel[] detailDataSets = GetDetailDataSets(model);
        for (int detailIndex = 0; detailIndex < detailDataSets.Length; detailIndex++)
        {
          var detailDataSet = detailDataSets[detailIndex];
          if (detailDataSet == null)
            continue;

          foreach (var row in detailDataSet.Rows)
          {
            if (row.Requisites.Count > 0)
            {
              this.ExportRequisites(path, row.Requisites, detailIndex + 1);
              this.RemoveRequisites(row.Requisites, detailIndex + 1);

              // Конвертировать реквизиты.
              foreach (var requisite in row.Requisites)
                requisite.PrepareForExport();
            }
          }
        }
      }
    }

    /// <summary>
    /// Преобразовать модель компоненты к формату для импорта.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="model">Модель.</param>
    /// <returns>Выходной формат.</returns>
    protected virtual void PrepareForImport(string path, ComponentModel model)
    {
      if (model.Card != null && model.Card.Requisites.Count > 0)
      {
        // Конвертировать реквизиты.
        foreach (var requisite in model.Card.Requisites)
          requisite.PrepareForImport();

        this.ImportRequisites(path, model.Card.Requisites);
      }

      if (model.DetailDataSets != null)
      {
        DataSetModel[] detailDataSets = GetDetailDataSets(model);
        for (int detailIndex = 0; detailIndex < detailDataSets.Length; detailIndex++)
        {
          var detailDataSet = detailDataSets[detailIndex];
          if (detailDataSet == null)
            continue;

          foreach (var row in detailDataSet.Rows)
          {
            if (row.Requisites.Count > 0)
            {
              // Конвертировать реквизиты.
              foreach (var requisite in row.Requisites)
                requisite.PrepareForImport();

              this.ImportRequisites(path, row.Requisites, detailIndex + 1);
            }
          }
        }
      }
    }

    /// <summary>
    /// Обработать экспорт модели.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="model">Модель компоненты.</param>
    /// <param name="settings">Настройки записи XML.</param>
    /// <param name="namespaces">Пространства имен сериализации.</param>
    /// <param name="serializer">Сериализатор модели.</param>
    protected virtual void HandleExportModel(string path, ComponentModel model, XmlWriterSettings settings, 
      XmlSerializerNamespaces namespaces, XmlSerializer serializer)
    {
      this.currentComponentModel = model;
      var cardFilePath = GetCardFileName(path);

      this.outputFileNames.Clear();
      this.outputFolderNames.Clear();
      
      this.PrepareForExport(path, model);
      this.ExportModelToFile(model, cardFilePath, settings, namespaces, serializer);

      this.DeleteUnwantedFiles(path);
    }

    /// <summary>
    /// Проверить, нужно ли удалить реквизит.
    /// </summary>
    /// <param name="requisites">Исходный список реквизитов.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    /// <returns>Признак того, что нужно удалять реквизит.</returns>
    protected virtual List<string> GetRequisitesToRemove(List<RequisiteModel> requisites, int detailIndex = 0)
    {
      var result = new List<string>();
      if (detailIndex == 0)
      {
        if (TransformerEnvironment.IsRussianCodePage())
        {
          result.Add("ИД");
          result.Add("LastUpdate");
        }
        if (TransformerEnvironment.IsEnglishCodePage())
        {
          result.Add("ISBID");
          result.Add("LastUpdate");
        }
      }

      if (detailIndex >= 1)
      {
        if (TransformerEnvironment.IsRussianCodePage())
        {
          result.Add("ИД");
          result.Add("ИДЗапГлавРазд");
        }
        if (TransformerEnvironment.IsEnglishCodePage())
        {
          result.Add("ISBID");
          result.Add("MainSectionRecordID");
        }
      }

      return result;
    }

    /// <summary>
    /// Удалить реквизиты, которые надо удалить.
    /// </summary>
    /// <param name="requisites">Список реквизитов.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected virtual void RemoveRequisites(List<RequisiteModel> requisites, int detailIndex = 0)
    {
      var removeList = this.GetRequisitesToRemove(requisites, detailIndex);
      for (int requisiteIndex = requisites.Count - 1; requisiteIndex >= 0; requisiteIndex--)
        foreach (var requisiteCode in removeList)
          if (requisites[requisiteIndex].Code == requisiteCode)
          {
            requisites.Remove(requisites[requisiteIndex]);
            break;
          }
    }

    /// <summary>
    /// Экспортировать реквизиты, которые надо экспортировать.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="requisites">Список реквизитов.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected virtual void ExportRequisites(string path, List<RequisiteModel> requisites, int detailIndex = 0)
    {
      foreach (var requisite in requisites)
        this.ProcessRequisiteExport(path, requisite, detailIndex);
    }

    /// <summary>
    /// Импортировать реквизиты, которые надо импортировать.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="requisites">Список реквизитов.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected virtual void ImportRequisites(string path, List<RequisiteModel> requisites, int detailIndex = 0)
    {
      // Пустой метод.
    }

    /// <summary>
    /// Обработать экспорт реквизита.
    /// </summary>
    /// <param name="path">Путь к папке с моделью</param>
    /// <param name="requisite">Обрабатываемый реквизит.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected virtual void ProcessRequisiteExport(string path, RequisiteModel requisite, int detailIndex)
    {
      // Пустой метод.
    }

    /// <summary>
    /// Экспортировать текст в файл.
    /// </summary>
    /// <param name="fileName">Имя файла.</param>
    /// <param name="text">Текст файла.</param>
    protected void ExportTextToFile(string fileName, string text)
    {
      byte[] bytes = null;
      if (!string.IsNullOrEmpty(text))
        bytes = TransformerEnvironment.CurrentEncoding.GetBytes(text);
      this.ExportBytesToFile(fileName, bytes);
    }

    /// <summary>
    /// Загрузить текст из файла.
    /// </summary>
    /// <param name="fileName">Имя файла.</param>
    protected string LoadTextFromFile(string fileName)
    {
      var bytes = File.ReadAllBytes(fileName);
      return TransformerEnvironment.CurrentEncoding.GetString(bytes);
    }

    /// <summary>
    /// Экспортировать текст в файл.
    /// </summary>
    /// <param name="fileName">Имя файла.</param>
    /// <param name="fileBytes">Массив байт.</param>
    protected void ExportBytesToFile(string fileName, byte[] fileBytes)
    {
      var fileInfo = new FileInfo(fileName);

      var directoryInfo = fileInfo.Directory;
      if (directoryInfo != null)
      {
        if (!directoryInfo.Exists)
          Directory.CreateDirectory(directoryInfo.FullName);
        if (!this.outputFolderNames.Contains(directoryInfo.FullName))
          this.outputFolderNames.Add(directoryInfo.FullName);
      }

      byte[] newFileBytes = null;
      if (fileBytes != null)
        newFileBytes = fileBytes;
      else   
        newFileBytes = new byte[0];

      byte[] currentFileBytes = null;
      if (File.Exists(fileName))
        currentFileBytes = File.ReadAllBytes(fileName);

      if (currentFileBytes == null || !currentFileBytes.SequenceEqual(newFileBytes))
        File.WriteAllBytes(fileName, newFileBytes);

      if (!this.outputFileNames.Contains(fileName))
        this.outputFileNames.Add(fileName);
    }

    /// <summary>
    /// Загрузить все тексты обработчиков событий из заданной папки.
    /// </summary>
    /// <param name="path">Папка с обработчиками событий.</param>
    /// <returns>Тексты обработчиков событий в виде значения реквизита.</returns>
    protected string LoadEventsFromFolder(string path)
    {
      var eventTexts = new Dictionary<EventType, string>();
      foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
      {
        var eventFileName = EventTypeFileNames.GetFileName(eventType);
        var eventFilePath = Path.Combine(path, eventFileName);
        if (File.Exists(eventFilePath))
        {
          var eventText = this.LoadTextFromFile(eventFilePath);
          eventTexts.Add(eventType, eventText);
        }
      }
      return EventTextParser.Join(eventTexts);
    }

    /// <summary>
    /// Внутренний метод обработки удаления элементов разработки.
    /// </summary>
    /// <param name="connectionParams">Параметры соединения.</param>
    /// <param name="settings">Настройки экспорта в XML.</param>
    /// <param name="namespaces">Пространства имен XML.</param>
    /// <param name="deletedCount">Количество удаленных элементов</param>
    protected virtual void InternalHandleDelete(ConnectionParams connectionParams, XmlWriterSettings settings, XmlSerializerNamespaces namespaces,
      out int deletedCount)
    {
      DeleteProcessor.ProcessDelete(connectionParams, this.DevelopmentElementsCommandText, this.DevelopmentElementKeyFieldName, this.DevelopmentElementTableName,
        this.NeedCheckTableExists, this.ComponentsFolder, out deletedCount);
    }

    /// <summary>
    /// Обработать импорт моделей (внутренний метод).
    /// </summary>
    /// <param name="packageModel">Модель пакета разработки.</param>
    /// <param name="importFilter">Фильтр импорта.</param>
    /// <param name="importedCount">Количество импортированных элементов.</param>
    protected virtual void InternalHandleImport(ComponentsModel packageModel, ImportFilter importFilter, out int importedCount)
    {
      importedCount = 0;
      var serializer = this.CreateSerializer();
      var models = this.GetComponentModelList(packageModel);

      if (!Directory.Exists(this.ComponentsFolder))
        return;

      foreach (var componentFolder in Directory.EnumerateDirectories(this.ComponentsFolder))
        if (importFilter.NeedImport(componentFolder, this.DevelopmentPath))
        {
          models.Add(this.HandleImportModel(componentFolder, serializer));
          importedCount++;
        }
    }
        
    /// <summary>
    /// Экспортировать текстовый реквизит в папку.
    /// </summary>
    /// <param name="requisites">Список реквизитов.</param>
    /// <param name="codeRequisiteCode">Код реквизита с кодом.</param>
    /// <param name="textRequisiteCode">Код экспортируемого реквизита с текстом.</param>
    /// <param name="outputFolderName">Папка, в которую выполняется экспорт.</param>
    protected void ExportTextRequisiteToFolder(List<RequisiteModel> requisites, String codeRequisiteCode, String textRequisiteCode, 
      String outputFolderName)
    {
      var codeRequisite = requisites.FirstOrDefault(r => r.Code == codeRequisiteCode);
      var fileName = string.Format("{0}.isbl", codeRequisite.DecodedText);
      var filePath = Path.Combine(outputFolderName, fileName);
      var textRequisite = requisites.FirstOrDefault(r => r.Code == textRequisiteCode);
      if (textRequisite.DecodedText != null)
        this.ExportTextToFile(filePath, textRequisite.DecodedText);
    }
        
    /// <summary>
    /// Импортировать текстовый реквизит из папки.
    /// </summary>
    /// <param name="requisites">Список реквизитов.</param>
    /// <param name="codeRequisiteCode">Код реквизита с кодом.</param>
    /// <param name="textRequisiteCode">Код импортируемого реквизита с текстом.</param>
    /// <param name="inputFolderName">Папка, из которой выполняется импорт.</param>
    protected void ImportTextRequisiteFromFolder(List<RequisiteModel> requisites, String codeRequisiteCode, String textRequisiteCode,
      String inputFolderName)
    {
      var codeRequisite = requisites.FirstOrDefault(r => r.Code == codeRequisiteCode);
      var fileName = string.Format("{0}.isbl", codeRequisite.DecodedText);
      var filePath = Path.Combine(inputFolderName, fileName);
      var textRequisite = RequisiteModel.CreateFromFile(textRequisiteCode, filePath);
      requisites.Add(textRequisite);
    }

    /// <summary>
    /// Проверить существование детального набора данных.
    /// </summary>
    /// <param name="datasetIndex">Индекс набора данных.</param>
    /// <returns>True - если набор данных существует, иначе False.</returns>
    protected bool IsDetailDataSetExists(int datasetIndex)
    {
      return this.currentComponentModel?.DetailDataSets?.GetDataSetByIndex(datasetIndex) != null;
    }

   #endregion

    #region IPackageHandler

    public void HandleExport(ComponentsModel packageModel, XmlWriterSettings settings, XmlSerializerNamespaces namespaces)
    {
      const string ExportStageName = "Экспорт";
      try
      {
        int exportedCount;
        this.InternalHandleExport(packageModel, settings, namespaces, out exportedCount);
        Log.Info(string.Format("  {0} {1}: завершен (экспортировано {2})", ExportStageName, this.ElementNameInGenitive, exportedCount));
      }
      catch (Exception ex)
      {
        Log.Error(string.Format("  {0} {1}: ошибка. Подробнее: {2}", ExportStageName, this.ElementNameInGenitive, ex.Message));
        throw;
      }
    }

    public virtual void HandleDelete(ConnectionParams connectionParams, XmlWriterSettings settings, XmlSerializerNamespaces namespaces)
    {
      const string DeleteStageName = "Удаление";
      try
      {
        int deletedCount;
        this.InternalHandleDelete(connectionParams, settings, namespaces, out deletedCount);
        Log.Info(string.Format("  {0} {1}: завершено (удалено {2})", DeleteStageName, this.ElementNameInGenitive, deletedCount));
      }
      catch (Exception ex)
      {
        Log.Error(string.Format("  {0} {1}: ошибка. Подробнее: {2}", DeleteStageName, this.ElementNameInGenitive, ex.Message));
        throw;
      }
    }

    public void HandleImport(ComponentsModel packageModel, ImportFilter importFilter)
    {
      const string ImportStageName = "Импорт";
      try
      {
        int importedCount;
        this.InternalHandleImport(packageModel, importFilter, out importedCount);
        Log.Info(string.Format("  {0} {1}: завершен (импортировано {2})", ImportStageName, this.ElementNameInGenitive, importedCount));
      }
      catch (Exception ex)
      {
        Log.Error(string.Format("  {0} {1}: ошибка. Подробнее: {2}", ImportStageName, this.ElementNameInGenitive, ex.Message));
        throw;
      }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    protected BasePackageHandler(string developmentPath)
    {
      this.DevelopmentPath = Path.GetFullPath(developmentPath);
    }

    #endregion
  }
}
