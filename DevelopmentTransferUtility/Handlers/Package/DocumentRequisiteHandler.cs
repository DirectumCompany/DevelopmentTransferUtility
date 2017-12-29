using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик реквизитов документов.
  /// </summary>
  internal class DocumentRequisiteHandler : BasePackageHandler
  {
    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "DocumentRequisites"; } }

    protected override string ElementNameInGenitive { get { return "реквизитов документов"; } }

    protected override string DevelopmentElementKeyFieldName { get { return "Kod"; } }

    protected override string DevelopmentElementTableName { get { return "MBRecvEDoc"; } }

    protected override bool NeedCheckTableExists { get { return false; } }

    /// <summary>
    /// Получить модели соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.DocumentRequisites;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "DocumentRequisite";
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public DocumentRequisiteHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}
