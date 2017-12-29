using NpoComputer.DevelopmentTransferUtility.Models.Base;
using NpoComputer.DevelopmentTransferUtility.Models.ExportModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using NpoComputer.DevelopmentTransferUtility.Common;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Компаратор строк локализации (для сортировки по KeyValue).
  /// </summary>
  internal class LocalizationStringComparer : IEqualityComparer<ComponentModel>
  {
    #region IEqualityComparer<T>

    public bool Equals(ComponentModel x, ComponentModel y)
    {
      if (object.ReferenceEquals(x, y)) return true;
      return x != null && y != null && x.KeyValue.Equals(y.KeyValue);
    }

    public int GetHashCode(ComponentModel obj)
    {
      return obj.KeyValue?.GetHashCode() ?? 0;
    }

    #endregion
  }
  
  /// <summary>
  /// Обработчик строк локализации.
  /// </summary>
  internal class LocalizationStringHandler : BasePackageHandler
  {
    #region Константы

    /// <summary>
    /// Имя поля с кодом группы строки локализации.
    /// </summary>
    private const string GroupCodeFieldName = "GroupCode";

    /// <summary>
    /// Имя поля с кодом строки локализации.
    /// </summary>
    private const string CodeFieldName = "Code";

    #endregion

    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "LocalizationStrings"; } }

    protected override string ElementNameInGenitive { get { return "строк локализации"; } }

    protected override string DevelopmentElementsCommandText { get { return "select " + GroupCodeFieldName + ", " + CodeFieldName + " from " + this.DevelopmentElementTableName;  } }

    protected override string DevelopmentElementKeyFieldName { get { throw new NotImplementedException(); } }

    protected override string DevelopmentElementTableName { get { return "SBLocalizedData"; } }

    protected override bool NeedCheckTableExists { get { return false; } }

    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.LocalizationStrings;
    }

    protected override IEnumerable<ComponentModel> TakeComponentModels(ComponentsModel packageModel)
    {
      throw new NotImplementedException();
    }

    protected override string GetElementName()
    {
      throw new NotSupportedException("Method not supported in this class!");
    }

    protected override List<string> GetRequisitesToRemove(List<RequisiteModel> requisites, int detailIndex = 0)
    {
      var result = base.GetRequisitesToRemove(requisites, detailIndex);

      if (detailIndex == 1)
        result.Add("LastUpdate");

      return result;
    }

    protected override void InternalHandleExport(ComponentsModel packageModel, XmlWriterSettings settings, XmlSerializerNamespaces namespaces, out int exportedCount)
    {
      var models = packageModel.LocalizationStrings;
      exportedCount = models.Count;

      var stringsGrouppedByCode =
        models.GroupBy(
          model => model.DetailDataSets.DetailDataSet1.Rows[0].Requisites
            .Find(x => x.Code == "ISBGroupCode").DecodedText.ToUpper());

      var stringsToExport = new Dictionary<string, LocalizationStringsExportModel>();
      foreach (var stringsGroup in stringsGrouppedByCode)
      {
        var oldLocalizationStrings = this.LoadLocalizationStringsByGroup(stringsGroup.Key);

        Predicate<RequisiteModel> getLanguageRequisite = x => x.Code == "ISBLanguage";
        // Компаратор для строк локализации. 
        // В нашем случае всегда будет идти сначала "ru", потом "en".
        Comparison<RowModel> languageCodeComparer = 
          (reqA, reqB) => -1 * string.Compare(
            reqA.Requisites.Find(getLanguageRequisite).Value,
            reqB.Requisites.Find(getLanguageRequisite).Value,
            StringComparison.OrdinalIgnoreCase);
        var newLocalizationStrings = new List<ComponentModel>();
        foreach (var localizationString in stringsGroup)
        {
          this.PrepareForExport("notRequired", localizationString);
          // Сортируем локализацию внутри строки локализации, т.к. иногда в isx-файлах 
          // порядок следования "ru" и "en" локализаций для одной строки может различаться.
          localizationString.DetailDataSets.DetailDataSet1.Rows.Sort(languageCodeComparer);
          newLocalizationStrings.Add(localizationString);
        }
        // Найдём дельту, которую допишем к новым данным - это старые данные за исключением новых с таким же KeyValue.
        var unchanged = oldLocalizationStrings?.Except(newLocalizationStrings, new LocalizationStringComparer()).ToList() ?? new List<ComponentModel>();
        stringsToExport.Add(stringsGroup.Key, new LocalizationStringsExportModel());
        stringsToExport[stringsGroup.Key].LocalizationStrings.AddRange(newLocalizationStrings.Union(unchanged).OrderBy(x => x.KeyValue));
      }

      foreach (var model in stringsToExport)
      {
        // Получаем путь к компоненте.
        var componentFolder = Path.Combine(this.ComponentsFolder, model.Key);
        if (!Directory.Exists(componentFolder))
          Directory.CreateDirectory(componentFolder);
        // Выполнить обработку модели.
        var serializer = new XmlSerializer(typeof(LocalizationStringsExportModel));
        this.HandleExportModel(componentFolder, model.Value, settings, namespaces, serializer);
      }
    }

    protected override void InternalHandleDelete(ConnectionParams connectionParams, XmlWriterSettings settings, XmlSerializerNamespaces namespaces, out int deletedCount)
    {
      deletedCount = 0;
      if (!Directory.Exists(this.ComponentsFolder))
        return;
      using (var connection = ConnectionManager.GetConnection(connectionParams))
      { 
        connection.Open();
        var adapter = new SqlDataAdapter(this.DevelopmentElementsCommandText, connection);
        var dataSet = new DataSet();
        adapter.Fill(dataSet);
        var stringsTable = dataSet.Tables[0];

        var groupFolders = Directory.EnumerateDirectories(this.ComponentsFolder);
        foreach (var groupFolder in groupFolders)
        {
          var groupCode = new DirectoryInfo(groupFolder).Name;
          var localizationStrings = this.LoadLocalizationStringsByGroup(groupCode);
          if (localizationStrings == null)
            continue;
          var resultLocalizationStrings = new LocalizationStringsExportModel();
          var localizationStringIndex = 0;
          while (localizationStringIndex < localizationStrings.Count)
          {
            var localizationString = localizationStrings[localizationStringIndex];
            var localizationStringCode = localizationString.KeyValue;

            var stringExistsQuery =
              from stringsTableItem in stringsTable.AsEnumerable()
              where
                ((string) stringsTableItem[GroupCodeFieldName]).Equals(groupCode,
                  StringComparison.CurrentCultureIgnoreCase) &&
                ((string) stringsTableItem[CodeFieldName]).Equals(localizationStringCode,
                  StringComparison.CurrentCultureIgnoreCase)
              select 1;

            if (stringExistsQuery.Any())
              resultLocalizationStrings.LocalizationStrings.Add(localizationString);
            else
              deletedCount++;

            localizationStringIndex = localizationStringIndex + 1;
          }
          var serializer = new XmlSerializer(typeof (LocalizationStringsExportModel));
          this.HandleExportModel(groupFolder, resultLocalizationStrings, settings, namespaces, serializer);
        }
      }
    }

    protected override void InternalHandleImport(ComponentsModel packageModel, ImportFilter importFilter, out int importedCount)
    {
      importedCount = 0;
      if (!Directory.Exists(this.ComponentsFolder))
        return;
      var resultLocalizationStrings = this.GetComponentModelList(packageModel);
      var groupFolders = Directory.EnumerateDirectories(this.ComponentsFolder);
      foreach (var groupFolder in groupFolders)
      {
        if (importFilter.NeedImport(groupFolder, this.DevelopmentPath))
        {
          var groupCode = new DirectoryInfo(groupFolder).Name;
          var localizationStrings = this.LoadLocalizationStringsByGroup(groupCode);
          foreach (var localizationString in localizationStrings)
          {
            this.PrepareForImport(groupFolder, localizationString);
            resultLocalizationStrings.Add(localizationString);
            importedCount++;
          }
        }
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Обработать экспорт выходной модели.
    /// </summary>
    /// <param name="path">Выходная папка.</param>
    /// <param name="model">Выходная модель.</param>
    /// <param name="settings">Настройки XML.</param>
    /// <param name="namespaces">Пространства имен XML.</param>
    /// <param name="serializer">Сериализатор модели.</param>
    private void HandleExportModel(string path, LocalizationStringsExportModel model, XmlWriterSettings settings,
      XmlSerializerNamespaces namespaces, XmlSerializer serializer)
    {
      var cardFilePath = Path.Combine(path, CardFileName);

      this.outputFileNames.Clear();
      this.outputFolderNames.Clear();

      foreach (var lsModel in model.LocalizationStrings)
        this.PrepareForExport(path, lsModel);
      this.ExportModelToFile(model, cardFilePath, settings, namespaces, serializer);

      this.DeleteUnwantedFiles(path);
    }

    /// <summary>
    /// Считать все описания строк локализации заданной группы.
    /// </summary>
    /// <param name="groupCode">Код группы.</param>
    /// <returns>Описания строк локализации.</returns>
    private List<ComponentModel> LoadLocalizationStringsByGroup(string groupCode)
    {
      var serializer = new XmlSerializer(typeof(LocalizationStringsExportModel));
      var componentPath = Path.Combine(this.ComponentsFolder, groupCode, CardFileName);

      List<ComponentModel> result = null;
      if (File.Exists(componentPath))
        using (var fileStream = File.OpenRead(componentPath))
          using (var reader = XmlReader.Create(fileStream, new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Document }))
            result =
              ((LocalizationStringsExportModel)serializer.Deserialize(reader)).LocalizationStrings;

      return result;
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public LocalizationStringHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}