using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;
using System.Linq;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик интегрированных отчетов.
  /// </summary>
  internal class IntegratedReportHandler : BaseReportHandler
  {
    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "IntegratedReports"; } }

    protected override string ElementNameInGenitive { get { return "интегрированных отчетов"; } }

    protected override string DevelopmentElementsCommandText
    {
      get { return base.DevelopmentElementsCommandText + " where TypeRpt = 'MBAnalitV'"; }
    }

    protected override string DevelopmentElementKeyFieldName { get { return "NameRpt"; } }

    protected override string DevelopmentElementTableName { get { return "MBReports"; } }

    protected override bool NeedCheckTableExists { get { return false; } }

    /// <summary>
    /// Получить модели, соответствующие заданному обработчику c необходимой фильтрацией.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override IEnumerable<ComponentModel> TakeComponentModels(ComponentsModel packageModel)
    {
      return this.GetComponentModelList(packageModel)
        .Where(m => m.Card.Requisites.First(r => r.Code == "Тип").DecodedText == "MBAnalitV");
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "IntegratedReport";
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    internal IntegratedReportHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}
