using NpoComputer.DevelopmentTransferUtility.Common;
using NpoComputer.DevelopmentTransferUtility.Models.Records;
using System.Collections.Generic;
using System.IO;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Records
{
  /// <summary>
  /// Обработчик групп типовых маршрутов.
  /// </summary>
  internal class WizardGroupHandler : BaseRecordHandler
  {
    #region Константы

    /// <summary>
    /// Имя справочника групп мастеров действий.
    /// </summary>
    protected const string WizardsGroupReferenceName = "WIZARD_GROUPS";

    #endregion

    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "WizardGroups"; } }

    protected override string ElementNameInGenitive { get { return "групп мастеров действий"; } }

    protected override string DevelopmentElementsCommandText
    {
      get
      {
        return
          "select MBAnalit." + this.DevelopmentElementKeyFieldName + " " +
          "from MBAnalit " +
          "inner join MBVidAn " +
          "on MBVidAn.Vid = MBAnalit.Vid " +
          "  and MBVidAn.Kod = 'WIZARD_GROUPS'";
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
      return rootModel.Records.FindAll(x => x.ReferenceName.Equals(WizardsGroupReferenceName));
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "WizardGroup";
    }

    /// <summary>
    /// Обработать импорт моделей (внутренний метод).
    /// </summary>
    /// <param name="rootModel">Модель пакета разработки.</param>
    /// <param name="importFilter">Фильтр импорта.</param>
    /// <param name="importedCount">Количество импортированных элементов.</param>
    protected override void InternalHandleImport(RootModel rootModel, ImportFilter importFilter, out int importedCount)
    {
      importedCount = 0;
      var serializer = this.CreateSerializer();
      var models = rootModel.Records;

      if (!Directory.Exists(this.ComponentsFolder))
        return;

      foreach (var componentFolder in Directory.EnumerateDirectories(this.ComponentsFolder))
        if (importFilter.NeedImport(componentFolder, this.DevelopmentPath))
        {
          models.Add(this.HandleImportModel(componentFolder, serializer));
          importedCount++;
        }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public WizardGroupHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}