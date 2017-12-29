using NpoComputer.DevelopmentTransferUtility.Common;
using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик диалогов.
  /// </summary>
  internal class DialogHandler : BasePackageHandler
  {
    #region Методы

    /// <summary>
    /// Получить путь до папки с методами.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Путь до папки с методами.</returns>
    private static string GetMethodsPath(string modelPath)
    {
      return Path.Combine(modelPath, "Methods");
    }

    /// <summary>
    /// Получить путь до папки с действиями.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Путь до папки с действиями.</returns>
    private static string GetActionsPath(string modelPath)
    {
      return Path.Combine(modelPath, "Actions");
    }

    /// <summary>
    /// Получить имя файла с описанием формы-карточки.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Путь к файлу с описанием формы-карточки.</returns>
    private static string GetCardFormFileName(string modelPath)
    {
      return Path.Combine(modelPath, "CardForm.dfm");
    }

    /// <summary>
    /// Получить имя файла с комментарием.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Имя файла с комментарием.</returns>
    private static string GetCommentFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Comment.txt");
    }

    /// <summary>
    /// Получить имя папки с обработчиками событий.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Имя папки с обработчиками событий.</returns>
    private static string GetEventsPath(string modelPath)
    {
      return Path.Combine(modelPath, "Events");
    }

    /// <summary>
    /// Получить имя файла с текстом события на изменение реквизита.
    /// </summary>
    /// <param name="requisitePath">Путь к папке с событиями реквизита.</param>
    /// <returns>Имя файла с текстом события на изменение реквизита.</returns>
    private static string GetRequisiteChangeEventFileName(string requisitePath)
    {
      return Path.Combine(requisitePath, "Requisite.Change.isbl");
    }

    /// <summary>
    /// Получить имя папки с событиями реквизита.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <param name="requisiteCode">Код реквизита.</param>
    /// <returns>Имя папки с событиями реквизита.</returns>
    private static string GetRequsitePath(string modelPath, string requisiteCode)
    {
      return Path.Combine(modelPath, "Requisites", requisiteCode);
    }

    /// <summary>
    /// Экспортировать реквизиты карточки.
    /// </summary>
    /// <param name="path">Путь к файлам модели.</param>
    /// <param name="requisites">Список реквизитов.</param>
    private void ExportCardRequisites(string path, List<RequisiteModel> requisites)
    {
      foreach (var requisite in requisites)
      {
        if (requisite.Code == "ISBDialogForm")
          this.ExportTextToFile(GetCardFormFileName(path), requisite.DecodedText);

        if (requisite.Code == "ISBDialogComment")
          this.ExportTextToFile(GetCommentFileName(path), requisite.DecodedText);

        if (requisite.Code == "ISBDialogEventText")
        {
          if (requisite.DecodedText != null)
          {
            var events = EventTextParser.Parse(requisite.DecodedText);
            if (events.Count > 0)
            {
              // Получаем путь к событиям.
              var eventsPath = GetEventsPath(path);
              if (!Directory.Exists(eventsPath))
                Directory.CreateDirectory(eventsPath);

              // Сохраняем события.
              foreach (var eventType in events.Keys)
              {
                var eventFileName = EventTypeFileNames.GetFileName(eventType);
                var eventFilePath = Path.Combine(eventsPath, eventFileName);
                this.ExportTextToFile(eventFilePath, events[eventType]);
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Экспортировать детальный раздел событий реквизитов.
    /// </summary>
    /// <param name="path">Путь к файлам модели.</param>
    /// <param name="requisites">Список реквизитов.</param>
    private void ExportRequisiteEvents(string path, List<RequisiteModel> requisites)
    {
      var changeEventReq = requisites.FirstOrDefault(r => r.Code == "ISBDialogReqOnChange");
      var selectEventReq = requisites.FirstOrDefault(r => r.Code == "ISBDialogReqOnSelect");

      if (changeEventReq.DecodedText != null || selectEventReq.DecodedText != null)
      {
        // Получаем путь к реквизиту.
        var requisiteCodeReq = requisites.FirstOrDefault(r => r.Code == "ISBDialogReqCode");
        var requisitePath = GetRequsitePath(path, requisiteCodeReq.DecodedText);

        // Сохраняем событие изменения.
        if (changeEventReq.DecodedText != null)
        {
          var changeEventFilePath = GetRequisiteChangeEventFileName(requisitePath);
          this.ExportTextToFile(changeEventFilePath, changeEventReq.DecodedText);
        }

        // Сохраняем события.
        if (selectEventReq.DecodedText != null)
        {
          var events = EventTextParser.Parse(selectEventReq.DecodedText);
          foreach (var eventType in events.Keys)
          {
            var eventFileName = "Requisite.Select.isbl";
            if (eventType != EventType.Unknown)
              eventFileName = EventTypeFileNames.GetFileName(eventType);
            var eventFilePath = Path.Combine(requisitePath, eventFileName);
            this.ExportTextToFile(eventFilePath, events[eventType]);
          }
        }
      }
    }

    /// <summary>
    /// Импортировать реквизиты карточки.
    /// </summary>
    /// <param name="path">Путь к файлам модели.</param>
    /// <param name="requisites">Список реквизитов.</param>
    private void ImportCardRequisites(string path, List<RequisiteModel> requisites)
    {
      var formRequisite = RequisiteModel.CreateFromFile("ISBDialogForm", GetCardFormFileName(path));
      requisites.Add(formRequisite);
      var commentRequisite = RequisiteModel.CreateFromFile("ISBDialogComment", GetCommentFileName(path));
      requisites.Add(commentRequisite);

      var eventsPath = GetEventsPath(path);
      var eventTextValue = this.LoadEventsFromFolder(eventsPath);
      var eventTextRequisite = RequisiteModel.CreateFromText("ISBDialogEventText", eventTextValue);
      requisites.Add(eventTextRequisite);
    }

    /// <summary>
    /// Импортировать детальный раздел событий реквизитов.
    /// </summary>
    /// <param name="path">Путь к файлам модели.</param>
    /// <param name="requisites">Список реквизитов.</param>
    private void ImportRequisiteEvents(string path, List<RequisiteModel> requisites)
    {
      var requisiteCodeRequisite = requisites.FirstOrDefault(r => r.Code == "ISBDialogReqCode");
      var requisiteCode = requisiteCodeRequisite.DecodedText;
      var requisitePath = GetRequsitePath(path, requisiteCode);

      var changeEventRequisite = RequisiteModel.CreateFromFile("ISBDialogReqOnChange", GetRequisiteChangeEventFileName(requisitePath));
      requisites.Add(changeEventRequisite);

      var selectEventRequisiteValue = this.LoadEventsFromFolder(requisitePath);
      var selectEventRequisite = RequisiteModel.CreateFromText("ISBDialogReqOnSelect", selectEventRequisiteValue);
      requisites.Add(selectEventRequisite);
    }

    #endregion

    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "Dialogs"; } }

    protected override string ElementNameInGenitive { get { return "диалогов"; } }

    protected override string DevelopmentElementKeyFieldName { get { return "Code"; } }

    protected override string DevelopmentElementTableName { get { return "SBDialog"; } }

    protected override bool NeedCheckTableExists { get { return true; } }

    /// <summary>
    /// Получить модели, соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.Dialogs;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "Dialog";
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
        result.Add("ISBDialogForm");
        result.Add("ISBDialogComment");
        result.Add("ISBDialogEventText");
      }

      if (detailIndex == 1)
      {
        result.Add("ISBDialogReqOnChange");
        result.Add("ISBDialogReqOnSelect");
      }

      if (detailIndex == 2 && !IsDetailDataSetExists(7))
        result.Add("ISBDialogActOnExecute");

      if (detailIndex == 7)
        result.Add("Calculation");

      return result;
    }

    /// <summary>
    /// Экспортировать реквизиты, которые надо экспортировать.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="requisites">Список реквизитов.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected override void ExportRequisites(string path, List<RequisiteModel> requisites, int detailIndex = 0)
    {
      base.ExportRequisites(path, requisites, detailIndex);

      if (detailIndex == 0)
        this.ExportCardRequisites(path, requisites);

      if (detailIndex == 1)
        this.ExportRequisiteEvents(path, requisites);

      if (detailIndex == 2 && !IsDetailDataSetExists(7))
        this.ExportTextRequisiteToFolder(requisites, "ISBDialogActCode", "ISBDialogActOnExecute", GetActionsPath(path));

      if (detailIndex == 7)
        this.ExportTextRequisiteToFolder(requisites, "Name", "Calculation", GetMethodsPath(path));
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
        this.ImportCardRequisites(path, requisites);

      if (detailIndex == 1)
        this.ImportRequisiteEvents(path, requisites);

      if (detailIndex == 2 && !IsDetailDataSetExists(7))
        this.ImportTextRequisiteFromFolder(requisites, "ISBDialogActCode", "ISBDialogActOnExecute", GetActionsPath(path));

      if (detailIndex == 7)
        this.ImportTextRequisiteFromFolder(requisites, "Name", "Calculation", GetMethodsPath(path));
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public DialogHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}