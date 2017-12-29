using NpoComputer.DevelopmentTransferUtility.Common;
using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик типов справочников.
  /// </summary>
  internal class ReferenceTypeHandler : BasePackageHandler
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
    /// Получить имя файла комментария.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Имя файла комментария.</returns>
    private static string GetCommentFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Comment.txt");
    }

    /// <summary>
    /// Получить имя файла с параметрами.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Имя файла с параметрами.</returns>
    private static string GetParamsFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Params.ini");
    }

    /// <summary>
    /// Получить имя файла с настройками по умолчанию.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Имя файла с настройками по умолчанию.</returns>
    private static string GetSettingsFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Settings.xml");
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
    /// Получить путь к папке с событиями реквизита.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <param name="requisiteCode">Код реквизита.</param>
    /// <returns>Путь к папке с событиями реквизита.</returns>
    private static string GetRequisitePath(string modelPath, string requisiteCode)
    {
      return Path.Combine(modelPath, "Requisites", requisiteCode);
    }

    /// <summary>
    /// Получить имя файла с событием на изменение реквизита.
    /// </summary>
    /// <param name="requisitePath">Имя папки с событиями реквизита.</param>
    /// <returns>Имя файла с событиями на изменение реквизита.</returns>
    private static string GetRequisiteChangeEventFileName(string requisitePath)
    {
      return Path.Combine(requisitePath, "Requisite.Change.isbl");
    }

    /// <summary>
    /// Получить имя папки с данными представления.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <param name="viewCode">Код представления.</param>
    /// <returns>Имя папки с данными представления.</returns>
    private static string GetViewPath(string modelPath, string viewCode)
    {
      return Path.Combine(modelPath, "Views", viewCode);
    }

    /// <summary>
    /// Получить имя файла с описанием формы-карточки.
    /// </summary>
    /// <param name="viewPath">Путь к папке с данными представления.</param>
    /// <returns>Имя файла с описанием формы-карточки.</returns>
    private static string GetCardFormFileName(string viewPath)
    {
      return Path.Combine(viewPath, "CardForm.dfm");
    }

    /// <summary>
    /// Получить имя файла с описанием формы-списка.
    /// </summary>
    /// <param name="viewPath">Путь к папке с данными представления.</param>
    /// <returns>Имя файла с описанием формы-карточки.</returns>
    private static string GetListFormFileName(string viewPath)
    {
      return Path.Combine(viewPath, "ListForm.dfm");
    }

    /// <summary>
    /// Получить имя файла с комментарием к представлению.
    /// </summary>
    /// <param name="viewPath">Путь к папке с данными представления.</param>
    /// <returns>Имя файла с комментарием к представлению.</returns>
    private static string GetViewCommentFileName(string viewPath)
    {
      return Path.Combine(viewPath, "Comment.txt");
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
        if (requisite.Code == "ISBRefTypeComment")
          this.ExportTextToFile(GetCommentFileName(path), requisite.DecodedText);

        if (requisite.Code == "ISBRefTypeAddParams")
          this.ExportTextToFile(GetParamsFileName(path), requisite.DecodedText);

        if (requisite.Code == "ISBRefTypeCommonSettings")
          this.ExportTextToFile(GetSettingsFileName(path), requisite.DecodedText);

        if (requisite.Code == "ISBRefTypeEventText")
        {
          if (requisite.DecodedText != null)
          {
            var events = EventTextParser.Parse(requisite.DecodedText);
            if (events.Count > 0)
            {
              // Получаем путь к событиям.
              var eventsPath = GetEventsPath(path);

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
      var changeEventReq = requisites.FirstOrDefault(r => r.Code == "ISBRefTypeReqOnChange");
      var selectEventReq = requisites.FirstOrDefault(r => r.Code == "ISBRefTypeReqOnSelect");

      if (changeEventReq.DecodedText != null || selectEventReq.DecodedText != null)
      {
        // Получаем путь к реквизиту.
        var requisiteCodeReq = requisites.FirstOrDefault(r => r.Code == "ISBRefTypeReqCode");
        var requisitePath = GetRequisitePath(path, requisiteCodeReq.DecodedText);

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
    /// Экспортировать детальный раздел представлений.
    /// </summary>
    /// <param name="path">Путь к файлам модели.</param>
    /// <param name="requisites">Список реквизитов.</param>
    private void ExportViews(string path, List<RequisiteModel> requisites)
    {
      var cardFormReq = requisites.FirstOrDefault(r => r.Code == "ISBRefTypeViewCardForm");
      var listFormReq = requisites.FirstOrDefault(r => r.Code == "ISBRefTypeViewListForm");
      var viewCommentReq = requisites.FirstOrDefault(r => r.Code == "ISBRefTypeViewComment");

      string listFormReqDecodedText = listFormReq?.DecodedText;
      if (cardFormReq.DecodedText != null || listFormReqDecodedText != null || viewCommentReq.DecodedText != null)
      {
        // Получаем путь к представлению.
        var viewCodeReq = requisites.FirstOrDefault(r => r.Code == "ISBRefTypeViewCode");
        var viewPath = GetViewPath(path, viewCodeReq.DecodedText);

        // Сохраняем форму карточки.
        if (cardFormReq.DecodedText != null)
        {
          var cardFormFilePath = GetCardFormFileName(viewPath);
          this.ExportTextToFile(cardFormFilePath, cardFormReq.DecodedText);
        }

        // Сохраняем форму списка.
        if (listFormReqDecodedText != null)
        {
          var listFormFilePath = GetListFormFileName(viewPath);
          this.ExportTextToFile(listFormFilePath, listFormReqDecodedText);
        }

        // Сохраняем комментарий представления.
        if (viewCommentReq.DecodedText != null)
        {
          var viewCommentFilePath = GetViewCommentFileName(viewPath);
          this.ExportTextToFile(viewCommentFilePath, viewCommentReq.DecodedText);
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
      var commentRequisite = RequisiteModel.CreateFromFile("ISBRefTypeComment", GetCommentFileName(path));
      requisites.Add(commentRequisite);
      var paramsRequisite = RequisiteModel.CreateFromFile("ISBRefTypeAddParams", GetParamsFileName(path));
      requisites.Add(paramsRequisite);
      var commonSettingsRequisite = RequisiteModel.CreateFromFile("ISBRefTypeCommonSettings", GetSettingsFileName(path));
      requisites.Add(commonSettingsRequisite);

      var eventsPath = GetEventsPath(path);
      var eventTextValue = this.LoadEventsFromFolder(eventsPath);
      var eventTextRequisite = RequisiteModel.CreateFromText("ISBRefTypeEventText", eventTextValue);
      requisites.Add(eventTextRequisite);
    }

    /// <summary>
    /// Экспортировать детальный раздел событий реквизитов.
    /// </summary>
    /// <param name="path">Путь к файлам модели.</param>
    /// <param name="requisites">Список реквизитов.</param>
    private void ImportRequisiteEvents(string path, List<RequisiteModel> requisites)
    {
      var requisiteCodeRequisite = requisites.FirstOrDefault(r => r.Code == "ISBRefTypeReqCode");
      var requisiteCode = requisiteCodeRequisite.DecodedText;
      var requisitePath = GetRequisitePath(path, requisiteCode); 

      var changeEventRequisite = RequisiteModel.CreateFromFile("ISBRefTypeReqOnChange", GetRequisiteChangeEventFileName(requisitePath));
      requisites.Add(changeEventRequisite);

      var selectEventRequisiteValue = this.LoadEventsFromFolder(requisitePath);
      var selectEventRequisite = RequisiteModel.CreateFromText("ISBRefTypeReqOnSelect", selectEventRequisiteValue);
      requisites.Add(selectEventRequisite);
    }

    /// <summary>
    /// Импортировать детальный раздел представлений.
    /// </summary>
    /// <param name="path">Путь к файлам модели.</param>
    /// <param name="requisites">Список реквизитов.</param>
    private void ImportViews(string path, List<RequisiteModel> requisites)
    {
      var viewCodeRequisite = requisites.FirstOrDefault(r => r.Code == "ISBRefTypeViewCode");
      var viewPath = GetViewPath(path, viewCodeRequisite.DecodedText);

      var cardFormFilePath = GetCardFormFileName(viewPath);
      var cardFormRequisite = RequisiteModel.CreateFromFile("ISBRefTypeViewCardForm", cardFormFilePath);
      requisites.Add(cardFormRequisite);

      var listFormFilePath = GetListFormFileName(viewPath);
      var listFormRequisite = RequisiteModel.CreateFromFile("ISBRefTypeViewListForm", listFormFilePath);
      requisites.Add(listFormRequisite);

      var viewCommentFilePath = GetViewCommentFileName(viewPath);
      var viewCommentRequisite = RequisiteModel.CreateFromFile("ISBRefTypeViewComment", viewCommentFilePath);
      requisites.Add(viewCommentRequisite);
    }

    #endregion

    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "ReferenceTypes"; } }

    protected override string ElementNameInGenitive { get { return "типов справочников"; } }

    protected override string DevelopmentElementKeyFieldName { get { return "Kod"; } }

    protected override string DevelopmentElementTableName { get { return "MBVidAn"; } }

    protected override bool NeedCheckTableExists { get { return false; } }


    /// <summary>
    /// Получить модели соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.ReferenceTypes;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "ReferenceType";
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
        result.Add("ISBRefTypeComment");
        result.Add("ISBRefTypeAddParams");
        result.Add("ISBRefTypeCommonSettings");
        result.Add("ISBRefTypeEventText");
      }

      if (detailIndex == 1)
      {
        result.Add("ISBRefTypeReqOnChange");
        result.Add("ISBRefTypeReqOnSelect");
      }

      if (detailIndex == 2 && !IsDetailDataSetExists(7))
        result.Add("ISBRefTypeActOnExecute");

      if (detailIndex == 3)
      {
        result.Add("ISBRefTypeViewCardForm");
        result.Add("ISBRefTypeViewListForm");
        result.Add("ISBRefTypeViewComment");
      }

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
    protected override void ExportRequisites(string path, List<RequisiteModel> requisites, int detailIndex)
    {
      base.ExportRequisites(path, requisites, detailIndex);

      if (detailIndex == 0)
        this.ExportCardRequisites(path, requisites);

      if (detailIndex == 1)
        this.ExportRequisiteEvents(path, requisites);

      if (detailIndex == 2 && !IsDetailDataSetExists(7))
        this.ExportTextRequisiteToFolder(requisites, "ISBRefTypeActCode", "ISBRefTypeActOnExecute", GetActionsPath(path));

      if (detailIndex == 3)
        this.ExportViews(path, requisites);

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
        this.ImportTextRequisiteFromFolder(requisites, "ISBRefTypeActCode", "ISBRefTypeActOnExecute", GetActionsPath(path));

      if (detailIndex == 3)
        this.ImportViews(path, requisites);

      if (detailIndex == 7)
        this.ImportTextRequisiteFromFolder(requisites, "Name", "Calculation", GetMethodsPath(path));
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public ReferenceTypeHandler(string developmentPath): base(developmentPath)
    {
    }

    #endregion
  }
}
