using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Парсер текста событий.
  /// </summary>
  internal static class EventTextParser
  {
    #region Внутренние классы

    /// <summary>
    /// Префиксы событий c гуидами.
    /// </summary>
    private static class EventGuidPrefix
    {
      public const string DataSet = "DATASET{9AFC8FC7-30C4-4076-9076-6E09A49B791C}";
      public const string Card = "CARD{2147B5A6-496E-4EFF-88D9-78970D889F1F}";
      public const string Operation = "OPERATION{C6CE6EDC-3645-4BBC-B00F-587BD2A54B4C}";
      public const string Form = "FORM{B28D55C1-651A-46C9-AD4E-50E73EF213A8}";
      public const string ListForm = "LISTFORM{EF850CF0-3135-4D1F-8726-74F95C4D08C7}";
      public const string Table = "TABLE{D402E843-74B2-4DC1-BFFD-DE677B48452C}";
      public const string Dialog = "DIALOG{3AA220D8-D906-4914-8586-F534A4C3767E}";
      public const string Requisite = "REQUISITE{CA900538-FC82-47D2-B574-001875901D2D}";
    }

    /// <summary>
    /// Префиксы событий без гуидов.
    /// </summary>
    private static class EventPrefix
    {
      public const string DataSet = "DATASET";
      public const string Card = "CARD";
      public const string Operation = "OPERATION";
      public const string Form = "FORM";
      public const string ListForm = "LISTFORM";
      public const string Table = "TABLE";
      public const string Dialog = "DIALOG";
      public const string Requisite = "REQUISITE";
    }

    /// <summary>
    /// Старые префиксы событий.
    /// </summary>
    private static class EventOldPrefix
    {
      public const string DataSet = "НАБОР ДАННЫХ";
      public const string Card = "КАРТОЧКА";
      public const string Operation = "ОПЕРАЦИЯ";
      public const string Form = "ФОРМА";
      public const string Table = "ТАБЛИЦА";
    }

    /// <summary>
    /// Постфиксы событий.
    /// </summary>
    private static class EventPostfix
    {
      public const string Open = "OPEN";
      public const string Close = "CLOSE";
      public const string Execution = "EXECUTION";
      public const string BeforeInsert = "BEFORE_INSERT";
      public const string AfterInsert = "AFTER_INSERT";
      public const string ValidUpdate = "VALID_UPDATE";
      public const string BeforeUpdate = "BEFORE_UPDATE";
      public const string AfterUpdate = "AFTER_UPDATE";
      public const string ValidDelete = "VALID_DELETE";
      public const string BeforeDelete = "BEFORE_DELETE";
      public const string AfterDelete = "AFTER_DELETE";
      public const string BeforeCancel = "BEFORE_CANCEL";
      public const string AfterCancel = "AFTER_CANCEL";
      public const string Show = "SHOW";
      public const string Hide = "HIDE";
      public const string Create = "CREATE";
      public const string ValidCloseWithResult = "VALID_CLOSE_WITH_RESULT";
      public const string CloseWithResult = "CLOSE_WITH_RESULT";
      public const string DialogShow = "DIALOG_SHOW";
      public const string DialogHide = "DIALOG_HIDE";
      public const string Select = "SELECT";
      public const string BeforeSelect = "BEFORE_SELECT";
      public const string AfterSelect = "AFTER_SELECT";
    }

    /// <summary>
    /// Постфиксы событий.
    /// </summary>
    private static class EventOldPostfix
    {
      public const string Open = "ОТКРЫТИЕ";
      public const string Close = "ЗАКРЫТИЕ";
      public const string Execution = "ВЫПОЛНЕНИЕ";
      public const string BeforeInsert = "ДОБАВЛЕНИЕ ДО";
      public const string AfterInsert = "ДОБАВЛЕНИЕ ПОСЛЕ";
      public const string ValidUpdate = "СОХРАНЕНИЕ ВОЗМОЖНОСТЬ";
      public const string BeforeUpdate = "СОХРАНЕНИЕ ДО";
      public const string AfterUpdate = "СОХРАНЕНИЕ ПОСЛЕ";
      public const string ValidDelete = "УДАЛЕНИЕ ВОЗМОЖНОСТЬ";
      public const string BeforeDelete = "УДАЛЕНИЕ ДО";
      public const string AfterDelete = "УДАЛЕНИЕ ПОСЛЕ";
      public const string BeforeCancel = "ОТМЕНА ДО";
      public const string AfterCancel = "ОТМЕНА ПОСЛЕ";
      public const string Show = "ПОКАЗ";
      public const string Hide = "СКРЫТИЕ";
    }

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Соответствие между именами событий и типами событий.
    /// </summary>
    private static readonly Dictionary<string, EventType> eventNameToType = new Dictionary<string, EventType>();

    /// <summary>
    /// Соответствие между типами событий и именами событий.
    /// </summary>
    private static readonly Dictionary<EventType, string> eventTypeToName = new Dictionary<EventType, string>();

    #endregion

    #region Методы

    /// <summary>
    /// Удалить перенос строки, завершающий исходную строку.
    /// </summary>
    /// <param name="sourceString">Исходная строка.</param>
    /// <returns>Исходная строка без завершающего переноса строки.</returns>
    private static string RemoveLastLineBreak(string sourceString)
    {
      const string LineBreak = "\r\n";
      if (sourceString.EndsWith(LineBreak))
        return sourceString.Substring(0, sourceString.LastIndexOf(LineBreak, StringComparison.InvariantCulture));
      else
        return sourceString;
    }

    /// <summary>
    /// Выполнить парсинг значения реквизита события.
    /// </summary>
    /// <param name="source">Значения реквизита события.</param>
    /// <returns>События.</returns>
    public static Dictionary<EventType, string> Parse(string source)
    {
      var result = new Dictionary<EventType, string>();
      if (string.IsNullOrEmpty(source))
        return result;

      var eventType = EventType.Unknown;
      var eventText = new StringBuilder();
      var sourceStrings = source.Split(new string[] {"\r\n"}, StringSplitOptions.None);
      for (var sourceStringIndex = 0; sourceStringIndex < sourceStrings.Length; sourceStringIndex++)
      {
        var newLine = sourceStrings[sourceStringIndex];
        if (eventNameToType.ContainsKey(newLine))
        {
          if (sourceStringIndex > 0)
            result.Add(eventType, eventText.ToString());
          eventType = eventNameToType[newLine];
          eventText = eventText.Clear();
        }
        else
        {
          if (sourceStringIndex != sourceStrings.Length - 1 || !string.IsNullOrEmpty(newLine) || eventType == EventType.Unknown)
            eventText = eventText.AppendFormat("{0}\r\n", newLine);
        }
      }
      result.Add(eventType, eventText.ToString());
      return result;
    }

    /// <summary>
    /// Соединить тексты обработчиков событий в значение реквизита.
    /// </summary>
    /// <param name="events">Список текстов обработчиков.</param>
    /// <returns>Значение реквизита.</returns>
    public static string Join(Dictionary<EventType, string> events)
    {
      var resultBuilder = new StringBuilder();
      if ((events.Count == 1) && (events.Keys.First() == EventType.Select))
        resultBuilder.Append(RemoveLastLineBreak(events.Values.First()));
      else
      {
        foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
        {
          if (events.ContainsKey(eventType))
          {
            string eventText;
            if (events.TryGetValue(eventType, out eventText))
            {
              var eventName = eventTypeToName[eventType];
              resultBuilder.AppendFormat("{0}\r\n", eventName);
              resultBuilder.Append(eventText);
            }
          }
        }
      }
      return resultBuilder.ToString();
    }

    /// <summary>
    /// Добавить имя и тип события.
    /// </summary>
    /// <param name="eventName">Имя события.</param>
    /// <param name="eventType">Тип события.</param>
    private static void AddEventNameAndType(string eventName, EventType eventType)
    {
      eventNameToType.Add(eventName, eventType);
      eventTypeToName.Add(eventType, eventName);
    }

    /// <summary>
    /// Сгенерировать имена событий c Guid.
    /// </summary>
    private static void GenerateGuidEventNames()
    {
      // Датасет
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.DataSet, EventPostfix.Open), EventType.OnDataSetOpen);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.DataSet, EventPostfix.Close), EventType.OnDataSetClose);

      // Запись
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.Open), EventType.OnOpenRecord);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.Close), EventType.OnCloseRecord);

      // Выполнение
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Operation, EventPostfix.Execution), EventType.OnUpdateRatifiedRecord);

      // Карточка
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.BeforeInsert), EventType.BeforeInsert);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.AfterInsert), EventType.AfterInsert);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.ValidUpdate), EventType.OnValidUpdate);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.BeforeUpdate), EventType.BeforeUpdate);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.AfterUpdate), EventType.AfterUpdate);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.ValidDelete), EventType.OnValidDelete);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.BeforeDelete), EventType.BeforeDelete);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.AfterDelete), EventType.AfterDelete);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.BeforeCancel), EventType.BeforeCancel);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Card, EventPostfix.AfterCancel), EventType.AfterCancel);

      // Форма карточки
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Form, EventPostfix.Show), EventType.FormShow);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Form, EventPostfix.Hide), EventType.FormHide);

      // Форма списка
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.ListForm, EventPostfix.Show), EventType.ListFormShow);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.ListForm, EventPostfix.Hide), EventType.ListFormHide);

      // Диалог
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Dialog, EventPostfix.Create), EventType.Create);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Dialog, EventPostfix.ValidCloseWithResult), EventType.OnValidCloseWithResult);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Dialog, EventPostfix.CloseWithResult), EventType.CloseWithResult);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Form, EventPostfix.DialogShow), EventType.DialogShow);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Form, EventPostfix.DialogHide), EventType.DialogHide);

      // Таблица
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Table, EventPostfix.BeforeInsert), EventType.TableBeforeInsert);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Table, EventPostfix.AfterInsert), EventType.TableAfterInsert);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Table, EventPostfix.BeforeDelete), EventType.TableBeforeDelete);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Table, EventPostfix.AfterDelete), EventType.TableAfterDelete);

      // Таблицы 2-24
      for (var i = 2; i <= 24; i++)
      {
        var beforeInsertEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}BeforeInsert", i));
        AddEventNameAndType(string.Format("{0}{1}.{2}", EventGuidPrefix.Table, i, EventPostfix.BeforeInsert), beforeInsertEnumValue);

        var afterInsertEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}AfterInsert", i));
        AddEventNameAndType(string.Format("{0}{1}.{2}", EventGuidPrefix.Table, i, EventPostfix.AfterInsert), afterInsertEnumValue);

        var beforeDeleteEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}BeforeDelete", i));
        AddEventNameAndType(string.Format("{0}{1}.{2}", EventGuidPrefix.Table, i, EventPostfix.BeforeDelete), beforeDeleteEnumValue);

        var afterDeleteEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}AfterDelete", i));
        AddEventNameAndType(string.Format("{0}{1}.{2}", EventGuidPrefix.Table, i, EventPostfix.AfterDelete), afterDeleteEnumValue);
      }

      // Реквизит
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Requisite, EventPostfix.Select), EventType.Select);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Requisite, EventPostfix.BeforeSelect), EventType.BeforeSelect);
      AddEventNameAndType(string.Format("{0}.{1}", EventGuidPrefix.Requisite, EventPostfix.AfterSelect), EventType.AfterSelect);
    }

    /// <summary>
    /// Сгенерировать имена событий без Guid.
    /// </summary>
    private static void GenerateEventNames()
    {
      // Датасет
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.DataSet, EventPostfix.Open), EventType.OnDataSetOpen);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.DataSet, EventPostfix.Close), EventType.OnDataSetClose);

      // Запись
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.Open), EventType.OnOpenRecord);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.Close), EventType.OnCloseRecord);

      // Выполнение.
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Operation, EventPostfix.Execution), EventType.OnUpdateRatifiedRecord);

      // Карточка
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.BeforeInsert), EventType.BeforeInsert);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.AfterInsert), EventType.AfterInsert);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.ValidUpdate), EventType.OnValidUpdate);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.BeforeUpdate), EventType.BeforeUpdate);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.AfterUpdate), EventType.AfterUpdate);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.ValidDelete), EventType.OnValidDelete);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.BeforeDelete), EventType.BeforeDelete);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.AfterDelete), EventType.AfterDelete);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.BeforeCancel), EventType.BeforeCancel);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Card, EventPostfix.AfterCancel), EventType.AfterCancel);

      // Форма карточки
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Form, EventPostfix.Show), EventType.FormShow);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Form, EventPostfix.Hide), EventType.FormHide);

      // Форма списка
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.ListForm, EventPostfix.Show), EventType.ListFormShow);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.ListForm, EventPostfix.Hide), EventType.ListFormHide);

      // Диалог
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Dialog, EventPostfix.Create), EventType.Create);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Dialog, EventPostfix.ValidCloseWithResult), EventType.OnValidCloseWithResult);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Dialog, EventPostfix.CloseWithResult), EventType.CloseWithResult);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Form, EventPostfix.DialogShow), EventType.DialogShow);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Form, EventPostfix.DialogHide), EventType.DialogHide);

      // Таблица
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Table, EventPostfix.BeforeInsert), EventType.TableBeforeInsert);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Table, EventPostfix.AfterInsert), EventType.TableAfterInsert);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Table, EventPostfix.BeforeDelete), EventType.TableBeforeDelete);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Table, EventPostfix.AfterDelete), EventType.TableAfterDelete);

      // Таблицы 2-24
      for (var i = 2; i <= 24; i++)
      {
        var beforeInsertEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}BeforeInsert", i));
        eventNameToType.Add(string.Format("{0}{1}.{2}", EventPrefix.Table, i, EventPostfix.BeforeInsert), beforeInsertEnumValue);

        var afterInsertEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}AfterInsert", i));
        eventNameToType.Add(string.Format("{0}{1}.{2}", EventPrefix.Table, i, EventPostfix.AfterInsert), afterInsertEnumValue);

        var beforeDeleteEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}BeforeDelete", i));
        eventNameToType.Add(string.Format("{0}{1}.{2}", EventPrefix.Table, i, EventPostfix.BeforeDelete), beforeDeleteEnumValue);

        var afterDeleteEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}AfterDelete", i));
        eventNameToType.Add(string.Format("{0}{1}.{2}", EventPrefix.Table, i, EventPostfix.AfterDelete), afterDeleteEnumValue);
      }

      // Реквизит
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Requisite, EventPostfix.Select), EventType.Select);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Requisite, EventPostfix.BeforeSelect), EventType.BeforeSelect);
      eventNameToType.Add(string.Format("{0}.{1}", EventPrefix.Requisite, EventPostfix.AfterSelect), EventType.AfterSelect);
    }

    /// <summary>
    /// Сгенерировать старые имена событий.
    /// </summary>
    private static void GenerateOldEventNames()
    {
      // Датасет
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.DataSet, EventOldPostfix.Open), EventType.OnDataSetOpen);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.DataSet, EventOldPostfix.Close), EventType.OnDataSetClose);

      // Запись
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.Open), EventType.OnOpenRecord);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.Close), EventType.OnCloseRecord);

      // Выполнение.
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Operation, EventOldPostfix.Execution), EventType.OnUpdateRatifiedRecord);

      // Карточка
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.BeforeInsert), EventType.BeforeInsert);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.AfterInsert), EventType.AfterInsert);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.ValidUpdate), EventType.OnValidUpdate);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.BeforeUpdate), EventType.BeforeUpdate);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.AfterUpdate), EventType.AfterUpdate);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.ValidDelete), EventType.OnValidDelete);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.BeforeDelete), EventType.BeforeDelete);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.AfterDelete), EventType.AfterDelete);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.BeforeCancel), EventType.BeforeCancel);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Card, EventOldPostfix.AfterCancel), EventType.AfterCancel);

      // Форма карточки
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Form, EventOldPostfix.Show), EventType.FormShow);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Form, EventOldPostfix.Hide), EventType.FormHide);

      // Таблица
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Table, EventOldPostfix.BeforeInsert), EventType.TableBeforeInsert);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Table, EventOldPostfix.AfterInsert), EventType.TableAfterInsert);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Table, EventOldPostfix.BeforeDelete), EventType.TableBeforeDelete);
      eventNameToType.Add(string.Format("{0}.{1}", EventOldPrefix.Table, EventOldPostfix.AfterDelete), EventType.TableAfterDelete);

      // Таблицы 2-24
      for (var i = 2; i <= 24; i++)
      {
        var beforeInsertEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}BeforeInsert", i));
        eventNameToType.Add(string.Format("{0}{1}.{2}", EventOldPrefix.Table, i, EventOldPostfix.BeforeInsert), beforeInsertEnumValue);

        var afterInsertEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}AfterInsert", i));
        eventNameToType.Add(string.Format("{0}{1}.{2}", EventOldPrefix.Table, i, EventOldPostfix.AfterInsert), afterInsertEnumValue);

        var beforeDeleteEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}BeforeDelete", i));
        eventNameToType.Add(string.Format("{0}{1}.{2}", EventOldPrefix.Table, i, EventOldPostfix.BeforeDelete), beforeDeleteEnumValue);

        var afterDeleteEnumValue = (EventType)Enum.Parse(typeof(EventType), string.Format("Table{0}AfterDelete", i));
        eventNameToType.Add(string.Format("{0}{1}.{2}", EventOldPrefix.Table, i, EventOldPostfix.AfterDelete), afterDeleteEnumValue);
      }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Статический конструктор.
    /// </summary>
    static EventTextParser()
    {
      GenerateGuidEventNames();
      GenerateEventNames();
      GenerateOldEventNames();
    }

    #endregion
  }
}
