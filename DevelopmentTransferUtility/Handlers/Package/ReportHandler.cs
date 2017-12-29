using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;
using System.Linq;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик отчетов.
  /// </summary>
  internal class ReportHandler : BaseReportHandler
  {
    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "Reports"; } }

    protected override string ElementNameInGenitive { get { return "отчетов"; } }

    protected override string DevelopmentElementsCommandText
    {
      get { return base.DevelopmentElementsCommandText + " where TypeRpt = 'MBAnAccRpt'"; }
    }

    protected override string DevelopmentElementKeyFieldName { get { return "NameRpt"; } }

    protected override string DevelopmentElementTableName { get { return "MBReports"; } }

    protected override bool NeedCheckTableExists { get { return false; } }

    /// <summary>
    /// Получить модели соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override IEnumerable<ComponentModel> TakeComponentModels(ComponentsModel packageModel)
    {
      return this.GetComponentModelList(packageModel)
        .Where(m => m.Card.Requisites.First(r => r.Code == "Тип").DecodedText == "MBAnAccRpt");
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "Report";
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    internal ReportHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}
