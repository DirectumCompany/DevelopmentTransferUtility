using NpoComputer.DevelopmentTransferUtility.Common;
using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;
using System.IO;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Базовый обработчик отчетов.
  /// </summary>
  internal abstract class BaseReportHandler : BasePackageHandler
  {
    #region BasePackageHandler

    /// <summary>
    /// Получить путь к файлу с вычислением.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <returns>Путь к файлу с вычислением.</returns>
    private static string GetCalculationPath(string path)
    {
      return Path.Combine(path, "Calculation.isbl");
    }

    /// <summary>
    /// Получить путь к файлу с комментарием.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <returns>Путь к файлу с комментарием.</returns>
    private static string GetCommentPath(string path)
    {
      return Path.Combine(path, "Comment.txt");
    }

    /// <summary>
    /// Получить путь к файлу с шаблоном.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <returns>Путь к файлу с шаблоном.</returns>
    private static string GetTemplatePath(string path)
    {
      return Path.Combine(path, "Template");
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
          result.Add("Расчет");
          result.Add("Примечание");
          result.Add("Шаблон");
          result.Add("ИДМодуля");
        }

        if (TransformerEnvironment.IsEnglishCodePage())
        {
          result.Add("Script");
          result.Add("Note");
          result.Add("Template");
          result.Add("UnitID");
        }
      }

      return result;
    }

    /// <summary>
    /// Обработать экспорт реквизита.
    /// </summary>
    /// <param name="path">Путь к папке с моделью</param>
    /// <param name="requisite">Обрабатываемый реквизит.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected override void ProcessRequisiteExport(string path, RequisiteModel requisite, int detailIndex)
    {
      if (TransformerEnvironment.IsRussianCodePage())
      {
        if (requisite.Code == "Расчет")
          this.ExportTextToFile(GetCalculationPath(path), requisite.DecodedText);

        if (requisite.Code == "Примечание")
          this.ExportTextToFile(GetCommentPath(path), requisite.DecodedText);

        if (requisite.Code == "Шаблон")
          this.ExportTextToFile(GetTemplatePath(path), requisite.DecodedText);
      }

      if (TransformerEnvironment.IsEnglishCodePage())
      {
        if (requisite.Code == "Script")
          this.ExportTextToFile(GetCalculationPath(path), requisite.DecodedText);

        if (requisite.Code == "Note")
          this.ExportTextToFile(GetCommentPath(path), requisite.DecodedText);

        if (requisite.Code == "Template")
          this.ExportTextToFile(GetTemplatePath(path), requisite.DecodedText);
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
        var calculationRequisiteCode = TransformerEnvironment.IsRussianCodePage() ? "Расчет" : "Script";
        var calculationRequisite = RequisiteModel.CreateFromFile(calculationRequisiteCode, GetCalculationPath(path));
        requisites.Add(calculationRequisite);

        var commentRequisiteCode = TransformerEnvironment.IsRussianCodePage() ? "Примечание" : "Note";
        var commentRequisite = RequisiteModel.CreateFromFile(commentRequisiteCode, GetCommentPath(path));
        requisites.Add(commentRequisite);

        var templateRequisiteCode = TransformerEnvironment.IsRussianCodePage() ? "Шаблон" : "Template";
        var templateRequisite = RequisiteModel.CreateFromFile(templateRequisiteCode, GetTemplatePath(path));
        requisites.Add(templateRequisite);

        var unitIdRequisiteCode = TransformerEnvironment.IsRussianCodePage() ? "ИДМодуля" : "UnitID";
        var unitIdRequisite = new RequisiteModel();
        unitIdRequisite.Code = unitIdRequisiteCode;
        requisites.Add(unitIdRequisite);
      }
    }

    /// <summary>
    /// Получить модели, соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.Reports;
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    protected BaseReportHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}
