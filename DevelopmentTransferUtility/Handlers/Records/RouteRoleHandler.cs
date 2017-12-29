using NpoComputer.DevelopmentTransferUtility.Common;
using NpoComputer.DevelopmentTransferUtility.Models.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Records
{
  /// <summary>
  /// Обработчик ролей типовых маршрутов.
  /// </summary>
  internal class RouteRoleHandler : BaseRecordHandler
  {
    #region Константы

    /// <summary>
    /// Имя файла вычисления роли.
    /// </summary>
    protected const string CalculationFileName = "Calculation.isbl";

    /// <summary>
    /// Имя файла вычисления роли.
    /// </summary>
    protected const string CalculationFileNameTemplate = "RoleTM_РОЛ_{0}_ISBEvent.IMG";
    #endregion

    #region Поля и свойства

    /// <summary>
    /// Наименование входящего файла.
    /// </summary>
    private string InputFile;

    #endregion

    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "Roles"; } }

    protected override string ElementNameInGenitive { get { return "ролей типовых маршрутов"; } }

    protected override string DevelopmentElementsCommandText
    {
      get
      {
        return 
          "select MBAnalit." + this.DevelopmentElementKeyFieldName + " " +
          "from MBAnalit " +
          "inner join MBVidAn " +
          "on MBVidAn.Vid = MBAnalit.Vid " +
          "  and MBVidAn.Kod = 'РОЛ'"; 
      }
    }

    protected override string DevelopmentElementKeyFieldName { get { return "NameAn"; } }

    /// <summary>
    /// Получить модели соответствующие заданному обработчику.
    /// </summary>
    /// <param name = "rootModel" > Модель.</param>
    /// <returns>Модели записей.</returns>
    protected override List<RecordRefModel> GetComponentModelList(RootModel rootModel)
    {
      return rootModel.Records;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "Role";
    }

    /// <summary>
    /// Проверить, нужно ли удалить реквизит.
    /// </summary>
    /// <param name="requisites">Исходный список реквизитов.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    /// <returns>Признак того, что нужно удалять реквизит.</returns>
    protected override List<string> GetRequisitesToRemove(List<RequisiteModel> requisites, int detailIndex = 0)
    {
      var result = base.GetRequisitesToRemove(requisites, detailIndex);

      if (detailIndex == 0)
      {
        result.Add("ISBEvent");
      }
    
      return result;
    }

    protected override void InternalHandleExport(RootModel rootModel, XmlWriterSettings settings, XmlSerializerNamespaces namespaces, out int exportedCount)
    {
      var serializer = this.CreateSerializer();
      exportedCount = 0;
      var models = this.TakeComponentModels(rootModel);
      foreach (var model in models)
      {
        // Получаем путь к компоненте.
        var componentFolder = Path.Combine(this.ComponentsFolder, Utils.EscapeFilePath(model.Name));
        if (!Directory.Exists(componentFolder))
          Directory.CreateDirectory(componentFolder);

        // Выполнить обработку модели.
        this.HandleExportModel(componentFolder, model, settings, namespaces, serializer);
        exportedCount++;
      }
    }

    /// <summary>
    /// Обработать экспорт реквизита.
    /// </summary>
    /// <param name="path">Путь к папке с моделью</param>
    /// <param name="requisite">Обрабатываемый реквизит.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected override void ProcessRequisiteExport(string path, RequisiteModel requisite, int detailIndex)
    {
      if ((requisite.Name == "ISBEvent") && !(string.IsNullOrWhiteSpace(requisite.Value)))
      {
        var encodedText = File.ReadAllText(Path.Combine(Path.GetDirectoryName(InputFile), requisite.Value));
        var encoding = TransformerEnvironment.CurrentEncoding;
        var decodedText = encoding.GetString(Convert.FromBase64String(encodedText));
        this.ExportTextToFile(Path.Combine(path, CalculationFileName), decodedText);
      }
    }

    /// <summary>
    /// Импортировать реквизиты, которые надо импортировать.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="requisites">Список реквизитов.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected override void ImportRequisites(string path, List<RequisiteModel> requisites, int detailIndex = 0)
    {
      var commentRequisite = RequisiteModel.CreateFromFile("ISBEvent", Path.Combine(path, CalculationFileName));
      if (commentRequisite.Data != null)
      {
        string fileName = string.Format(CalculationFileNameTemplate, Path.GetFileName(path));
        this.ExportTextToFile(Path.Combine(Path.GetDirectoryName(InputFile), fileName), commentRequisite.Data.InnerText);
        commentRequisite.Value = fileName;
        commentRequisite.Data = null;
        requisites.Add(commentRequisite);
      }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="inputFile">Наименование входящего файла.</param>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public RouteRoleHandler(string inputFile, string developmentPath) : base(developmentPath)
    {
      this.InputFile = inputFile;
    }

    #endregion
  }
}
