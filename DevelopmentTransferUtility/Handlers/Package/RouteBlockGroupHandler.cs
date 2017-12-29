using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;
using System.Threading;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик групп блоков типовых маршрутов.
  /// </summary>
  internal class RouteBlockGroupHandler : BasePackageHandler
  {
    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "RouteBlockGroups"; } }

    protected override string ElementNameInGenitive { get { return "групп блоков типовых маршрутов"; } }

    protected override string DevelopmentElementKeyFieldName { get { return "Name"; } }

    protected override string DevelopmentElementTableName { get { return "SBRouteBlockGroup"; } }

    protected override bool NeedCheckTableExists { get { return false; } }

    /// <summary>
    /// Получить модели, соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.RouteBlockGroups;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "RouteBlockGroup";
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public RouteBlockGroupHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}
