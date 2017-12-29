using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик реквизитов диалогов.
  /// </summary>
  internal class DialogRequisiteHandler : BasePackageHandler
  {
    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "DialogRequisites"; } }

    protected override string ElementNameInGenitive { get { return "реквизитов диалогов"; } }

    protected override string DevelopmentElementKeyFieldName { get { return "Code"; } }

    protected override string DevelopmentElementTableName { get { return "SBDialogRequisite"; } }

    protected override bool NeedCheckTableExists { get { return true; } }

    /// <summary>
    /// Получить модели, соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.DialogRequisites;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "DialogRequisite";
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public DialogRequisiteHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}
