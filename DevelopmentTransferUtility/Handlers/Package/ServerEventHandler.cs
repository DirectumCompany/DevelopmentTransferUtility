using System.Collections.Generic;
using System.IO;

using NpoComputer.DevelopmentTransferUtility.Common;
using NpoComputer.DevelopmentTransferUtility.Models.Base;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Базовый обработчик серверных событий.
  /// </summary>
  internal class ServerEventHandler : BasePackageHandler
  {
    #region Методы

    /// <summary>
    /// Получить имя файла с комментарием.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Имя файла с комментарием.</returns>
    private static string GetCommentFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Comment.txt");
    }

    #endregion

    #region BasePackageHandler

    protected override string ElementNameInGenitive { get { return "серверных событий"; } }

    protected override string ComponentsFolderSuffix { get { return "ServerEvents"; } }

    protected override string DevelopmentElementKeyFieldName { get { return "Name"; } }

    protected override string DevelopmentElementTableName { get { return "SBServerEvent"; } }

    protected override bool NeedCheckTableExists { get { return true; } }

    protected override string GetElementName()
    {
      return "ServerEvent";
    }

    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.ServerEvents;
    }

    protected override List<string> GetRequisitesToRemove(List<RequisiteModel> requisites, int detailIndex = 0)
    {
      var result = base.GetRequisitesToRemove(requisites, detailIndex);

      if (detailIndex == 0)
      {
        if (TransformerEnvironment.IsRussianCodePage())
          result.Add("Примечание");
        if (TransformerEnvironment.IsEnglishCodePage())
          result.Add("Note");
      }

      return result;
    }

    protected override void ProcessRequisiteExport(string path, RequisiteModel requisite, int detailIndex)
    {
      if ((TransformerEnvironment.IsRussianCodePage() && (requisite.Code == "Примечание")) ||
         (TransformerEnvironment.IsEnglishCodePage() && (requisite.Code == "Note")))
        this.ExportTextToFile(GetCommentFileName(path), requisite.DecodedText);     
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
        var commentRequisiteCode = TransformerEnvironment.IsRussianCodePage() ? "Примечание" : "Note";
        var commentRequisite = RequisiteModel.CreateFromFile(commentRequisiteCode, GetCommentFileName(path));
        requisites.Add(commentRequisite);
      }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public ServerEventHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}
