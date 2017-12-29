using System.Collections.Generic;
using System.IO;
using NpoComputer.DevelopmentTransferUtility.Common;
using NpoComputer.DevelopmentTransferUtility.Models.Base;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик сценариев.
  /// </summary>
  internal class ScriptHandler : BasePackageHandler
  {
    #region Методы

    /// <summary>
    /// Получить имя файла с текстом сценария.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Имя файла с текстом сценария.</returns>
    private static string GetTextFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Text.isbl");
    }

    /// <summary>
    /// Получить имя файла с комментарием.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Имя файла с комментарием.</returns>
    private static string GetCommentFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Comment.txt");
    }

    #endregion

    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "Scripts"; } }

    protected override string ElementNameInGenitive { get { return "сценариев"; } }

    protected override string DevelopmentElementsCommandText
    {
      get { return base.DevelopmentElementsCommandText + " where TypeRpt = 'Function'"; }
    }

    protected override string DevelopmentElementKeyFieldName { get { return "NameRpt"; } }

    protected override string DevelopmentElementTableName { get { return "MBReports"; } }

    protected override bool NeedCheckTableExists { get { return false; } }

    /// <summary>
    /// Получить модели, соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.Scripts;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "Script";
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
        if (TransformerEnvironment.IsRussianCodePage())
        {
          result.Add("Текст");
          result.Add("Примечание");
          result.Add("ИДМодуля");
        }
        if (TransformerEnvironment.IsEnglishCodePage())
        {
          result.Add("Text");
          result.Add("Note");
          result.Add("UnitID");
        }
      }

      return result;
    }

    /// <summary>
    /// Обработать экспорт реквизита.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="requisite">Обрабатываемый реквизит.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected override void ProcessRequisiteExport(string path, RequisiteModel requisite, int detailIndex)
    {
      if (TransformerEnvironment.IsRussianCodePage())
      {
        if (requisite.Code == "Текст")
          this.ExportTextToFile(GetTextFileName(path), requisite.DecodedText);
        if (requisite.Code == "Примечание")
          this.ExportTextToFile(GetCommentFileName(path), requisite.DecodedText);
      }
      if (TransformerEnvironment.IsEnglishCodePage())
      {
        if (requisite.Code == "Text")
          this.ExportTextToFile(GetTextFileName(path), requisite.DecodedText);
        if (requisite.Code == "Note")
          this.ExportTextToFile(GetCommentFileName(path), requisite.DecodedText);
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
      if (detailIndex == 0)
      {
        var textRequisiteCode = TransformerEnvironment.IsRussianCodePage() ? "Текст" : "Text";
        var textRequisite = RequisiteModel.CreateFromFile(textRequisiteCode, GetTextFileName(path));
        requisites.Add(textRequisite);

        var commentRequisiteCode = TransformerEnvironment.IsRussianCodePage() ? "Примечание" : "Note";
        var commentRequisite = RequisiteModel.CreateFromFile(commentRequisiteCode, GetCommentFileName(path));
        requisites.Add(commentRequisite);

        var unitIdRequisiteCode = TransformerEnvironment.IsRussianCodePage() ? "ИДМодуля" : "UnitID";
        var unitIdRequisite = new RequisiteModel();
        unitIdRequisite.Code = unitIdRequisiteCode;
        requisites.Add(unitIdRequisite);
      }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public ScriptHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}
