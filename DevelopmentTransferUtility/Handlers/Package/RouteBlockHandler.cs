using NpoComputer.DevelopmentTransferUtility.Common;
using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Package
{
  /// <summary>
  /// Обработчик блоков типовых маршрутов.
  /// </summary>
  internal class RouteBlockHandler : BasePackageHandler
  {
    #region Константы

    /// <summary>
    /// Уникальный идентификатор-признак для маркировки текста, закодированного в MIME.
    /// </summary>
    private const string MimeCodedTextGuid = "{5314B05F-CF9F-4F66-99EC-24992A5FB114}";

    /// <summary>
    /// XPath для свойств.
    /// </summary>
    private const string PropertiesXPath = "/Settings/Blocks/Block/Properties/Property";

    /// <summary>
    /// XPath для действий.
    /// </summary>
    private const string ActionsXPath = "/Settings/Blocks/Block/Actions/Action";

    /// <summary>
    /// XPath для риббона.
    /// </summary>
    private const string RibbonXPath = "/Settings/Blocks/Block/Ribbon";

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Имена файлов событий.
    /// </summary>
    private readonly Dictionary<string, string> eventFileNames = new Dictionary<string, string>
    {
      { "BeforeStart", "BeforeStart.isbl" },
      { "AfterFinish", "AfterFinish.isbl" },
      { "OnCreateJobs", "OnCreateJobs.isbl" },
      { "SubTaskCreate", "SubTaskCreate.isbl" },
      { "SubtaskInit", "SubtaskInit.isbl" },
      { "SubtaskStart", "SubtaskStart.isbl" },
      { "BeforeQueryParams", "BeforeQueryParams.isbl" },
      { "AfterQueryParams", "AfterQueryParams.isbl" },
      { "OnCreateNotices", "OnCreateNotices.isbl" },
      { "OnFormShow", "OnFormShow.isbl" },
      { "OnFormHide", "OnFormHide.isbl" },
      { "SearchScript", "Monitoring.isbl" },
      { "Script", "Calculation.isbl" },
      { "ISBL", "Condition.isbl" }
    };

    #endregion

    #region Методы

    /// <summary>
    /// Получить путь к папке с событиями блока.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Путь к папке с событиями блока.</returns>
    private static string GetEventsPath(string modelPath)
    {
      return Path.Combine(modelPath, "Events");
    }

    /// <summary>
    /// Получить путь к папке действий.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Путь к папке действий.</returns>
    private static string GetActionsPath(string modelPath)
    {
      return Path.Combine(modelPath, "Actions");
    }

    /// <summary>
    /// Получить путь к описанию ленты.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Путь к описанию ленты.</returns>
    private static string GetRibbonFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Ribbon.dfm");
    }

    /// <summary>
    /// Получить путь к файлу свойств.
    /// </summary>
    /// <param name="modelPath">Путь к папке с моделью.</param>
    /// <returns>Путь к файлу свойств.</returns>
    private static string GetPropertiesFileName(string modelPath)
    {
      return Path.Combine(modelPath, "Properties.xml");
    }

    /// <summary>
    /// Вставить постфикс после соответствующей подстроки.
    /// </summary>
    /// <param name="source">Исходная строка.</param>
    /// <param name="substringRegex">Регулярное выражение подстроки.</param>
    /// <param name="postfix">Постфикс.</param>
    /// <returns>Результат вставки постфикса после соответствующей подстроки.</returns>
    private static string InsertPostfixAfterMatch(string source, string substringRegex, string postfix)
    {
      var regex = new Regex(substringRegex);
      var match = regex.Match(source);
      if (match.Success)
        return regex.Replace(source, match.Groups[0].Value + postfix, 1);
      else
        return source;
    }

    /// <summary>
    /// Возвращает строку, содержащую отформатированный xml.
    /// </summary>
    /// <param name="document">XML-документ.</param>
    /// <returns>Строка, содержащая отформатированный xml-документ.</returns>
    private static string GetFormattedXml(XmlNode document)
    {
      using (var stream = new StringWriter())
      {
        var settings = new XmlWriterSettings
        {
          ConformanceLevel = ConformanceLevel.Document,
          Encoding = TransformerEnvironment.CurrentEncoding,
          Indent = true,
          NewLineHandling = NewLineHandling.Entitize
        };
        using (var writer = XmlWriter.Create(stream, settings))
        {
          document.WriteTo(writer);
          writer.Flush();
          return stream.ToString();
        }
      }
    }

    /// <summary>
    /// Возвращает строку, содержащую xml-документ, отформатированный в стиле IS-Builder.
    /// </summary>
    /// <param name="document">XML-документ.</param>
    /// <returns>Строка, содержащая xml-документ, отформатированный в стиле IS-Builder.</returns>
    private static string GetIsBuilderFormattedXml(XmlNode document)
    {
      string result;
      using (var stream = new StringWriter())
      {
        var settings = new XmlWriterSettings
        {
          ConformanceLevel = ConformanceLevel.Document,
          Encoding = TransformerEnvironment.CurrentEncoding,
          Indent = false,
          NewLineHandling = NewLineHandling.None,
        };
        using (var writer = SpacelessClosingTagXmlWriter.Create(stream, settings))
        {
          document.WriteTo(writer);
          writer.Flush();
          result = stream.ToString();
        }
      }
      result = InsertPostfixAfterMatch(result, "<Settings>", "\r\n\t");
      result = InsertPostfixAfterMatch(result, @"<\?xml.*\?>", "\r\n");
      result = result + "\r\n";
      return result;
    }

    /// <summary>
    /// Закодировать значение свойства в MIME.
    /// </summary>
    /// <param name="decodedValue">"Сырое" значение свойства.</param>
    /// <returns>Закодированное значение свойства.</returns>
    private string MimeEncodePropertyValue(string decodedValue)
    {
      var encoding = TransformerEnvironment.CurrentEncoding;
      return MimeCodedTextGuid + Convert.ToBase64String(encoding.GetBytes(decodedValue));
    }

    /// <summary>
    /// Раскодировать значение свойства из MIME.
    /// </summary>
    /// <param name="encodedValue">Закодированное значение свойства.</param>
    /// <returns>Раскодированное значение свойства.</returns>
    private string MimeDecodePropertyValue(string encodedValue)
    {
      if (encodedValue.StartsWith(MimeCodedTextGuid))
      {
        var encoding = TransformerEnvironment.CurrentEncoding;
        var encodedValueWithoutGuid = encodedValue.Substring(MimeCodedTextGuid.Length);
        return encoding.GetString(Convert.FromBase64String(encodedValueWithoutGuid));
      }
      else
        return null;
    }

    /// <summary>
    /// Экспортировать узлы со свойствами (события).
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Узел со свойствами.</param>
    private void ExportPropertyNodes(string componentPath, XmlNodeList nodes)
    {
      var eventsPath = GetEventsPath(componentPath);
      foreach (XmlElement element in nodes)
      {
        var propertyName = element.GetAttribute("Name");
        if (this.eventFileNames.ContainsKey(propertyName))
        {
          var eventTextNode = element.SelectSingleNode("Value/Value/text()");
          if (eventTextNode == null || string.IsNullOrEmpty(eventTextNode.Value))
            continue;

          var eventText = this.MimeDecodePropertyValue(eventTextNode.Value);
          if (eventText != null)
          {
            var eventFilePath = Path.Combine(eventsPath, this.eventFileNames[propertyName]);
            this.ExportTextToFile(eventFilePath, eventText);
          }

          eventTextNode.Value = string.Empty;
        }
        else
        {
          var valueNodes = element.SelectNodes("Value/Value");
          if (valueNodes == null)
            continue;

          foreach(XmlElement valueNode in valueNodes)
          {
            var valueTextNode = valueNode.SelectSingleNode("text()");
            if (valueTextNode == null || string.IsNullOrEmpty(valueTextNode.Value))
              continue;

            var valueText = this.MimeDecodePropertyValue(valueTextNode.Value);
            if (valueText != null)
              valueTextNode.Value = valueText;
          }
        }
      }
    }

    /// <summary>
    /// Экспортировать действия.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Список узлов действий</param>
    private void ExportActionNodes(string componentPath, XmlNodeList nodes)
    {
      var actionsPath = GetActionsPath(componentPath); 
      var encoding = TransformerEnvironment.CurrentEncoding;
      foreach (XmlElement element in nodes)
      {
        var actionCode = element.GetAttribute("Code");
        var actionIsblNode = element.SelectSingleNode("ISBLText/text()");
        if (actionIsblNode == null || string.IsNullOrEmpty(actionIsblNode.Value))
          continue;

        var actionIsblText = actionIsblNode.Value;
        actionIsblText = encoding.GetString(Convert.FromBase64String(actionIsblText));

        var actionFileName = $"{actionCode}.isbl";
        var actionFilePath = Path.Combine(actionsPath, actionFileName);
        this.ExportTextToFile(actionFilePath, actionIsblText);

        actionIsblNode.Value = string.Empty;
      }
    }

    /// <summary>
    /// Экспортировать узел риббона.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="node">Узел риббона.</param>
    private void ExportRibbonNode(string componentPath, XmlNode node)
    {
      var ribbonTextNode = node.SelectSingleNode("text()");
      if (ribbonTextNode == null || string.IsNullOrEmpty(ribbonTextNode.Value))
        return;

      var ribbonFilePath = GetRibbonFileName(componentPath);
      this.ExportTextToFile(ribbonFilePath, ribbonTextNode.Value);

      ribbonTextNode.Value = string.Empty;
    }

    /// <summary>
    /// Экспортировать свойства блока типового маршрута.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="node">Xml узел свойств блока.</param>
    private void ExportSettingsNode(string componentPath, XmlNode node)
    {
      var propertiesNodes = node.SelectNodes(PropertiesXPath);
      if (propertiesNodes != null)
        this.ExportPropertyNodes(componentPath, propertiesNodes);

      var actionsNodes = node.SelectNodes(ActionsXPath);
      if (actionsNodes != null)
        this.ExportActionNodes(componentPath, actionsNodes);

      var ribbonNode = node.SelectSingleNode(RibbonXPath);
      if (ribbonNode != null)
        this.ExportRibbonNode(componentPath, ribbonNode);
    }

    /// <summary>
    /// Импортировать узлы со свойствами (события).
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Узел со свойствами.</param>
    private void ImportPropertyNodes(string componentPath, XmlNodeList nodes)
    {
      var eventsPath = GetEventsPath(componentPath);
      foreach (XmlElement element in nodes)
      {
        var propertyName = element.GetAttribute("Name");
        if (this.eventFileNames.ContainsKey(propertyName))
        {
          var eventFilePath = Path.Combine(eventsPath, this.eventFileNames[propertyName]);
          if (!File.Exists(eventFilePath))
            continue;
          var eventText = this.LoadTextFromFile(eventFilePath);
          var eventEncodedText = this.MimeEncodePropertyValue(eventText);
          var eventTextNode = element.SelectSingleNode("Value/Value/text()");
          if (eventTextNode != null)
            eventTextNode.Value = eventEncodedText;
        }
        else
        {
          var valueNodes = element.SelectNodes("Value/Value");
          if (valueNodes == null)
            continue;

          foreach (XmlElement valueNode in valueNodes)
          {
            var valueTextNode = valueNode.SelectSingleNode("text()");
            if (valueTextNode == null)
              continue;

            var valueText = this.MimeEncodePropertyValue(valueTextNode.Value);
            valueTextNode.Value = valueText;
          }
        }
      }
    }

    /// <summary>
    /// Импортировать действия.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Список узлов действий</param>
    private void ImportActionNodes(string componentPath, XmlNodeList nodes)
    {
      var actionsPath = GetActionsPath(componentPath);
      foreach (XmlElement actionNode in nodes)
      {
        var actionCode = actionNode.GetAttribute("Code");
        var actionFileName = $"{actionCode}.isbl";
        var actionFilePath = Path.Combine(actionsPath, actionFileName);
        if (!File.Exists(actionFilePath))
          continue;
        var actionText = this.LoadTextFromFile(actionFilePath);
        var encoding = TransformerEnvironment.CurrentEncoding;
        var encodedActionText = Convert.ToBase64String(encoding.GetBytes(actionText));
        var actionIsblNode = actionNode.SelectSingleNode("ISBLText/text()");
        if (actionIsblNode != null)
          actionIsblNode.Value = encodedActionText;
      }
    }

    /// <summary>
    /// Импортировать узел риббона.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="node">Узел риббона.</param>
    private void ImportRibbonNode(string componentPath, XmlNode node)
    {
      var ribbonFilePath = GetRibbonFileName(componentPath);
      var ribbonTextNode = node.SelectSingleNode("text()");
      if (ribbonTextNode != null)
        ribbonTextNode.Value = this.LoadTextFromFile(ribbonFilePath);
    }

    /// <summary>
    /// Импортировать свойства блока типового маршрута.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="node">Xml узел свойств блока.</param>
    private void ImportSettingsNode(string componentPath, XmlNode node)
    {
      var propertiesNodes = node.SelectNodes(PropertiesXPath);
      if (propertiesNodes != null)
        this.ImportPropertyNodes(componentPath, propertiesNodes);

      var actionsNodes = node.SelectNodes(ActionsXPath);
      if (actionsNodes != null)
        this.ImportActionNodes(componentPath, actionsNodes);

      var ribbonNode = node.SelectSingleNode(RibbonXPath);
      if (ribbonNode != null)
        this.ImportRibbonNode(componentPath, ribbonNode);
    }

    /// <summary>
    /// Импортировать XML-описание блока.
    /// </summary>
    /// <param name="path">Путь к папке с моделью</param>
    /// <param name="requisites">Список реквизитов.</param>
    private void ImportProperties(string path, List<RequisiteModel> requisites)
    {
      var workflowBlockPropertiesFileName = GetPropertiesFileName(path);
      var workflowBlockProperties = new XmlDocument();
      workflowBlockProperties.Load(workflowBlockPropertiesFileName);
      this.ImportSettingsNode(path, workflowBlockProperties);
      var workflowBlockPropertiesText = GetIsBuilderFormattedXml(workflowBlockProperties);
      var workflowBlockPropertiesRequisite = RequisiteModel.CreateFromText("Properties", workflowBlockPropertiesText);
      requisites.Add(workflowBlockPropertiesRequisite);
    }

    #endregion

    #region BasePackageHandler

    protected override string ComponentsFolderSuffix { get { return "RouteBlocks"; } }

    protected override string ElementNameInGenitive { get { return "блоков типовых маршрутов"; } }

    protected override string DevelopmentElementKeyFieldName { get { return "Name"; } }

    protected override string DevelopmentElementTableName { get { return "SBRouteBlock"; } }

    protected override bool NeedCheckTableExists { get { return false; } }

    /// <summary>
    /// Получить модели соответствующие заданному обработчику.
    /// </summary>
    /// <param name="packageModel">Модель пакета.</param>
    /// <returns>Модели компонент.</returns>
    protected override List<ComponentModel> GetComponentModelList(ComponentsModel packageModel)
    {
      return packageModel.RouteBlocks;
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
      return "RouteBlock";
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
        result.Add("Properties");

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
      if (requisite.Code != "Properties")
        return;
      var workFlowBlockProperties = new XmlDocument();
      workFlowBlockProperties.LoadXml(requisite.DecodedText);
      this.ExportSettingsNode(path, workFlowBlockProperties);
      this.ExportTextToFile(GetPropertiesFileName(path), GetFormattedXml(workFlowBlockProperties));
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
        this.ImportProperties(path, requisites);
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public RouteBlockHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }
}