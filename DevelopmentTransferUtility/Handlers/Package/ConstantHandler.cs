using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик констант.
  /// </summary>
  internal class ConstantHandler : BasePackageHandler
  {
    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "Constants"; } }

    protected override string ElementNameInGenitive { get { return "констант"; } }

    protected override string DevelopmentElementKeyFieldName { get { return "Const"; } }

    protected override string DevelopmentElementTableName { get { return "MBConstLst"; } }

    protected override bool NeedCheckTableExists { get { return false; } }

    /// <summary>
    /// Получить модели, соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.Constants;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "Constant";
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="workspacePath">Путь к рабочему пространству.</param>
    public ConstantHandler(string workspacePath) : base(workspacePath)
    {
    }

    #endregion
  }
}
