using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;
using System.IO;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик функций.
  /// </summary>
  internal class FunctionHandler : BasePackageHandler
  {
    #region Методы

    /// <summary>
    /// Получить имя файла с текстом функции.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Имя файла с текстом функции.</returns>
    private static string GetTextFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Text.isbl");
    }

    /// <summary>
    /// Получить имя файла со справкой по функции.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Имя файла со справкой по функции.</returns>
    private static string GetHelpFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Help.xml");
    }

    #endregion

    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "Functions"; } }

    protected override string ElementNameInGenitive { get { return "функций"; } }

    protected override string DevelopmentElementKeyFieldName { get { return "FName"; } }

    protected override string DevelopmentElementTableName { get { return "MBFunc"; } }

    protected override bool NeedCheckTableExists { get { return false; } }

    /// <summary>
    /// Получить модели соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.Functions;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "Function";
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
        result.Add("ISBFuncText");
        result.Add("ISBFuncHelp");
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
      if (requisite.Code == "ISBFuncText")
        this.ExportTextToFile(GetTextFileName(path), requisite.DecodedText);

      if (requisite.Code == "ISBFuncHelp")
        this.ExportTextToFile(GetHelpFileName(path), requisite.DecodedText);
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
        var funcTextRequisite = RequisiteModel.CreateFromFile("ISBFuncText", GetTextFileName(path));
        requisites.Add(funcTextRequisite);

        var funcHelpRequisite = RequisiteModel.CreateFromFile("ISBFuncHelp", GetHelpFileName(path));
        requisites.Add(funcHelpRequisite);
      }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public FunctionHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}
