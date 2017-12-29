using NpoComputer.DevelopmentTransferUtility.Models.Records;
using System.Collections.Generic;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Records
{
  /// <summary>
  /// Обработчик групп типовых маршрутов.
  /// </summary>
  internal class RouteGroupHandler : BaseRecordHandler
  {
    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "RouteGroups"; } }

    protected override string ElementNameInGenitive { get { return "групп типовых маршрутов"; } }

    protected override string DevelopmentElementsCommandText
    {
      get
      {
        return
          "select MBAnalit." + this.DevelopmentElementKeyFieldName + " " +
          "from MBAnalit " +
          "inner join MBVidAn " +
          "on MBVidAn.Vid = MBAnalit.Vid " +
          "  and MBVidAn.Kod = 'ГТМ'";
      }
    }

    protected override string DevelopmentElementKeyFieldName { get { return "Kod"; } }

    /// <summary>
    /// Получить модели, соответствующие заданному обработчику.
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
      return "RouteGroup";
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public RouteGroupHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}