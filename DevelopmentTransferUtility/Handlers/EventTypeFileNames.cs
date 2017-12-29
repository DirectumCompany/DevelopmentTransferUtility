using NpoComputer.DevelopmentTransferUtility.Common;
using System;
using System.Collections.Generic;

namespace NpoComputer.DevelopmentTransferUtility.Handlers
{
  /// <summary>
  /// Генератор имен файлов.
  /// </summary>
  internal static class EventTypeFileNames
  {
    #region Внутренние классы

    /// <summary>
    /// Префиксы событий.
    /// </summary>
    private static class EventFilePrefix
    {
      public const string Unknown = "Unknown";
      public const string DataSet = "DataSet";
      public const string Card = "Card";
      public const string Operation = "Operation";
      public const string Form = "Form";
      public const string ListForm = "ListForm";
      public const string Table = "Table";
      public const string Dialog = "Dialog";
      public const string Requisite = "Requisite";
    }

    /// <summary>
    /// Постфиксы событий.
    /// </summary>
    private static class EventFilePostfix
    {
      public const string Open = "Open";
      public const string Close = "Close";
      public const string Execution = "Execution";
      public const string BeforeInsert = "BeforeInsert";
      public const string AfterInsert = "AfterInsert";
      public const string ValidUpdate = "ValidUpdate";
      public const string BeforeUpdate = "BeforeUpdate";
      public const string AfterUpdate = "AfterUpdate";
      public const string ValidDelete = "ValidDelete";
      public const string BeforeDelete = "BeforeDelete";
      public const string AfterDelete = "AfterDelete";
      public const string BeforeCancel = "BeforeCancel";
      public const string AfterCancel = "AfterCancel";
      public const string Show = "Show";
      public const string Hide = "Hide";
      public const string Create = "Create";
      public const string ValidCloseWithResult = "ValidCloseWithResult";
      public const string CloseWithResult = "CloseWithResult";
      public const string DialogShow = "DialogShow";
      public const string DialogHide = "DialogHide";
      public const string Select = "Select";
      public const string BeforeSelect = "BeforeSelect";
      public const string AfterSelect = "AfterSelect";
    }

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Имена файлов.
    /// </summary>
    private static Dictionary<EventType, string> fileNames = new Dictionary<EventType, string>();

    #endregion

    #region Методы

    /// <summary>
    /// Получить имя файла.
    /// </summary>
    /// <param name="type">Тип события.</param>
    /// <returns>Имя файла для события.</returns>
    public static string GetFileName(EventType type)
    {
      return fileNames[type];
    }

    /// <summary>
    /// Сгенерировать имена файлов.
    /// </summary>
    private static void GenerateFileNames()
    {
      // Неизвестно
      fileNames.Add(EventType.Unknown, string.Format("{0}.isbl", EventFilePrefix.Unknown));

      // Датасет
      fileNames.Add(EventType.OnDataSetOpen, string.Format("{0}.{1}.isbl", EventFilePrefix.DataSet, EventFilePostfix.Open));
      fileNames.Add(EventType.OnDataSetClose, string.Format("{0}.{1}.isbl", EventFilePrefix.DataSet, EventFilePostfix.Close));

      // Запись
      fileNames.Add(EventType.OnOpenRecord, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.Open));
      fileNames.Add(EventType.OnCloseRecord, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.Close));

      // Операция
      fileNames.Add(EventType.OnUpdateRatifiedRecord, string.Format("{0}.{1}.isbl", EventFilePrefix.Operation, EventFilePostfix.Execution));

      // Карточка
      fileNames.Add(EventType.BeforeInsert, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.BeforeInsert));
      fileNames.Add(EventType.AfterInsert, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.AfterInsert));
      fileNames.Add(EventType.OnValidUpdate, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.ValidUpdate));
      fileNames.Add(EventType.BeforeUpdate, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.BeforeUpdate));
      fileNames.Add(EventType.AfterUpdate, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.AfterUpdate));
      fileNames.Add(EventType.OnValidDelete, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.ValidDelete));
      fileNames.Add(EventType.BeforeDelete, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.BeforeDelete));
      fileNames.Add(EventType.AfterDelete, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.AfterDelete));
      fileNames.Add(EventType.BeforeCancel, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.BeforeCancel));
      fileNames.Add(EventType.AfterCancel, string.Format("{0}.{1}.isbl", EventFilePrefix.Card, EventFilePostfix.AfterCancel));

      // Форма карточки
      fileNames.Add(EventType.FormShow, string.Format("{0}.{1}.isbl", EventFilePrefix.Form, EventFilePostfix.Show));
      fileNames.Add(EventType.FormHide, string.Format("{0}.{1}.isbl", EventFilePrefix.Form, EventFilePostfix.Hide));

      // Форма списка
      fileNames.Add(EventType.ListFormShow, string.Format("{0}.{1}.isbl", EventFilePrefix.ListForm, EventFilePostfix.Show));
      fileNames.Add(EventType.ListFormHide, string.Format("{0}.{1}.isbl", EventFilePrefix.ListForm, EventFilePostfix.Hide));

      // Диалог
      fileNames.Add(EventType.Create, string.Format("{0}.{1}.isbl", EventFilePrefix.Dialog, EventFilePostfix.Create));
      fileNames.Add(EventType.OnValidCloseWithResult, string.Format("{0}.{1}.isbl", EventFilePrefix.Dialog, EventFilePostfix.ValidCloseWithResult));
      fileNames.Add(EventType.CloseWithResult, string.Format("{0}.{1}.isbl", EventFilePrefix.Dialog, EventFilePostfix.CloseWithResult));
      fileNames.Add(EventType.DialogShow, string.Format("{0}.{1}.isbl", EventFilePrefix.Form, EventFilePostfix.DialogShow));
      fileNames.Add(EventType.DialogHide, string.Format("{0}.{1}.isbl", EventFilePrefix.Form, EventFilePostfix.DialogHide));

      // Таблица
      fileNames.Add(EventType.TableBeforeInsert, string.Format("{0}.{1}.isbl", EventFilePrefix.Table, EventFilePostfix.BeforeInsert));
      fileNames.Add(EventType.TableAfterInsert, string.Format("{0}.{1}.isbl", EventFilePrefix.Table, EventFilePostfix.AfterInsert));
      fileNames.Add(EventType.TableBeforeDelete, string.Format("{0}.{1}.isbl", EventFilePrefix.Table, EventFilePostfix.BeforeDelete));
      fileNames.Add(EventType.TableAfterDelete, string.Format("{0}.{1}.isbl", EventFilePrefix.Table, EventFilePostfix.AfterDelete));

      // Таблицы 2-24
      for (var i = 2; i <= 24; i++)
      {
        var beforeInsertEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}BeforeInsert", i));
        fileNames.Add(beforeInsertEnumValue, string.Format("{0}{1}.{2}.isbl", EventFilePrefix.Table, i, EventFilePostfix.BeforeInsert));

        var afterInsertEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}AfterInsert", i));
        fileNames.Add(afterInsertEnumValue, string.Format("{0}{1}.{2}.isbl", EventFilePrefix.Table, i, EventFilePostfix.AfterInsert));

        var beforeDeleteEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}BeforeDelete", i));
        fileNames.Add(beforeDeleteEnumValue, string.Format("{0}{1}.{2}.isbl", EventFilePrefix.Table, i, EventFilePostfix.BeforeDelete));

        var afterDeleteEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}AfterDelete", i));
        fileNames.Add(afterDeleteEnumValue, string.Format("{0}{1}.{2}.isbl", EventFilePrefix.Table, i, EventFilePostfix.AfterDelete));
      }

      // Реквизит
      fileNames.Add(EventType.Select, string.Format("{0}.{1}.isbl", EventFilePrefix.Requisite, EventFilePostfix.Select));
      fileNames.Add(EventType.BeforeSelect, string.Format("{0}.{1}.isbl", EventFilePrefix.Requisite, EventFilePostfix.BeforeSelect));
      fileNames.Add(EventType.AfterSelect, string.Format("{0}.{1}.isbl", EventFilePrefix.Requisite, EventFilePostfix.AfterSelect));
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Статический конструктор.
    /// </summary>
    static EventTypeFileNames()
    {
      GenerateFileNames();
    }

    #endregion
  }
}
