using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик модулей.
  /// </summary>
  internal class ModuleHandler : BasePackageHandler
  {
    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "Modules"; } }

    protected override string ElementNameInGenitive { get { return "модулей"; } }

    protected override string DevelopmentElementKeyFieldName { get { return "Name"; } }

    protected override string DevelopmentElementTableName { get { return "MBRegUnit"; } }

    protected override bool NeedCheckTableExists { get { return false; } }

    /// <summary>
    /// Получить модели, соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.Modules;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "Module";
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public ModuleHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}
