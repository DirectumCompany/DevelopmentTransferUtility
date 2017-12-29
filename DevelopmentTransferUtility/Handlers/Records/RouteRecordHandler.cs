using NpoComputer.DevelopmentTransferUtility.Common;
using NpoComputer.DevelopmentTransferUtility.Models.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Records
{
  /// <summary>
  /// Обработчик типовых маршрутов.
  /// </summary>
  internal class RouteRecordHandler : BaseRecordHandler
  {
    #region Константы

    /// <summary>
    /// Папка событий.
    /// </summary>
    private const string EventsFolder = "Events";

    /// <summary>
    /// Папка действий.
    /// </summary>
    private const string ActionsFolder = "Actions";

    /// <summary>
    /// Папка блоков.
    /// </summary>
    private const string BlocksFolder = "Blocks";

    /// <summary>
    /// Имя файла вычисления роли.
    /// </summary>
    private const string CalculationFileName = "Properties.xml";

    /// <summary>
    /// Имя файла с описанием ленты блока.
    /// </summary>
    private const string RibbonFileName = "Ribbon.dfm";

    /// <summary>
    /// Уникальный идентификатор-признак для маркировки текста, закодированного в MIME.
    /// </summary>
    private const string MimeCodedTextGuid = "{5314B05F-CF9F-4F66-99EC-24992A5FB114}";

    /// <summary>
    /// XPath для свойств.
    /// </summary>
    private const string PropertiesXPath = "Properties/Property";

    /// <summary>
    /// XPath для действий.
    /// </summary>
    private const string ActionsXPath = "RouteActions/Action";

    /// <summary>
    /// XPath для событий.
    /// </summary>
    private const string EventXPath = "Event";

    /// <summary>
    /// XPath для риббона блока.
    /// </summary>
    private const string RibbonXPath = "Ribbon";

    /// <summary>
    /// XPath для риббона типового маршрута.
    /// </summary>
    private const string RouteRibbonXPath = "RouteRibbon";

    /// <summary>
    /// XPath для блоков.
    /// </summary>
    private const string BlockXPath = "Blocks/Block";

    /// <summary>
    /// Шаблон наименования файла со свойствами ТМ.
    /// </summary>
    private const string CalculationFileNameTemplate = "TipMarsh_ТМТ_{0}_ISBSearchCondition.IMG";

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Наименование входящего файла.
    /// </summary>
    private readonly string inputFile;

    #endregion

    #region BaseRecordHandler

    protected override string ComponentsFolderSuffix { get { return "Routes"; } }

    protected override string ElementNameInGenitive { get { return "типовых маршрутов"; } }

    protected override string DevelopmentElementsCommandText
    {
      get
      {
        return
          "select MBAnalit." + this.DevelopmentElementKeyFieldName + " " +
          "from MBAnalit " +
          "inner join MBVidAn " +
          "on MBVidAn.Vid = MBAnalit.Vid " +
          "  and MBVidAn.Kod = 'ТМТ'";
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
      return "RouteRecord";
    }

    /// <summary>
    /// Возвращает строку, содержащую отформатированный xml.
    /// </summary>
    private string GetFormattedXml(XmlNode document, Encoding encoding)
    {
      using (var stream = new StringWriter())
      {
        var settings = new XmlWriterSettings
        {
          ConformanceLevel = ConformanceLevel.Document,
          Encoding = encoding,
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
    /// Обработать экспорт реквизита.
    /// </summary>
    /// <param name="path">Путь к папке с моделью</param>
    /// <param name="requisite">Обрабатываемый реквизит.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected override void ProcessRequisiteExport(string path, RequisiteModel requisite, int detailIndex)
    {
      if (CheckNesting(requisite))
      {
        var encodedText = File.ReadAllText(Path.Combine(Path.GetDirectoryName(this.inputFile), requisite.Value));
        requisite.Value = string.Empty;
        var encoding = TransformerEnvironment.CurrentEncoding;
        var decodedText = encoding.GetString(Convert.FromBase64String(encodedText));

        var model = TryDesirializeText(decodedText);

        if (model != null)
        {
          PropertiesExport(path, model);
          this.ExportTextToFile(Path.Combine(path, CalculationFileName), GetFormattedXml(model, encoding));
          return;
        }

        this.ExportTextToFile(Path.Combine(path, CalculationFileName), decodedText);
      }
    }

    /// <summary>
    /// Импортировать реквизиты, которые надо импортировать.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="requisites">Список реквизитов.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected override void ImportRequisites(string path, List<RequisiteModel> requisites, int detailIndex = 0)
    {
      var commentRequisite = RequisiteModel.CreateFromFile("ISBSearchCondition", Path.Combine(path, CalculationFileName));
      if (commentRequisite.Data != null)
      {
        string fileName = string.Format(CalculationFileNameTemplate, Path.GetFileName(path));
        commentRequisite.Value = fileName;

        var model = TryDesirializeText(commentRequisite.DecodedValue);

        if (model != null)
        {
          PropertiesImport(path, model);
        }
        string encodedXML = Convert.ToBase64String(Encoding.Default.GetBytes(GetFormattedXml(model, Encoding.Default)));
        this.ExportTextToFile(Path.Combine(Path.GetDirectoryName(this.inputFile), fileName), encodedXML);

        commentRequisite.Data = null;
        requisites.Add(commentRequisite);
      }
    }

    /// <summary>
    /// Попытаться десериализовать текст.
    /// </summary>
    /// <param name="requisite">Обрабатываемый реквизит.</param>
    private XmlNode TryDesirializeText(string inputText)
    {
      XmlNode result = null;
      var recordsSerializer = new XmlSerializer(typeof(XmlNode));
      try
      {
        var stringReader = new StringReader(inputText);
        var deserializeText = recordsSerializer.Deserialize(stringReader);
        result = (XmlNode)deserializeText;
      }
      catch
      {
        return null;
      }
      return result;
    }

    /// <summary>
    /// Проверить вложенность.
    /// </summary>
    /// <param name="requisite">Обрабатываемый реквизит.</param>
    private static bool CheckNesting(RequisiteModel requisite) =>
      ((requisite.Name == "ISBEvent") || (requisite.Name == "ISBSearchCondition")) &&
        !string.IsNullOrWhiteSpace(requisite.Value);

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
        result.Add("ISBSearchCondition");
      }

      return result;
    }

    #endregion

    #region Обработка блоков
    /// <summary>
    /// Имена файлов событий блока.
    /// </summary>
    private readonly Dictionary<string, string> eventFileNamesBlocks = new Dictionary<string, string>
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


    /// <summary>
    /// Обработать узлы со свойствами.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Узел со свойствами.</param>
    private void ExportPropertyNodes(string componentPath, XmlNodeList nodes)
    {
      var eventsPath = Path.Combine(componentPath, EventsFolder);
      var encoding = TransformerEnvironment.CurrentEncoding;
      foreach (XmlElement element in nodes)
      {
        var propertyName = element.GetAttribute("Name");
        if (this.eventFileNamesBlocks.ContainsKey(propertyName))
        {
          var eventTextNode = element.SelectSingleNode("Value/Value");
          if (eventTextNode == null || string.IsNullOrEmpty(eventTextNode.InnerText))
            continue;

          var eventText = eventTextNode.InnerText;
          if (eventText.StartsWith(MimeCodedTextGuid))
          {
            eventText = eventText.Substring(MimeCodedTextGuid.Length);
            eventText = encoding.GetString(Convert.FromBase64String(eventText));

            var eventFilePath = Path.Combine(eventsPath, this.eventFileNamesBlocks[propertyName]);
            this.ExportTextToFile(eventFilePath, eventText);
          }

          eventTextNode.InnerText = string.Empty;
        }
        else
        {
          var valueNodes = element.SelectNodes("Value/Value");
          if (valueNodes == null)
            continue;

          foreach (XmlElement valueNode in valueNodes)
          {
            var valueTextNode = valueNode.SelectSingleNode("text()");
            if (valueTextNode == null || string.IsNullOrEmpty(valueTextNode.Value))
              continue;

            var valueText = valueTextNode.Value;
            if (valueText.StartsWith(MimeCodedTextGuid))
            {
              valueText = valueText.Substring(MimeCodedTextGuid.Length);
              valueText = encoding.GetString(Convert.FromBase64String(valueText));
              valueTextNode.Value = valueText;
            }
          }
        }
      }
    }

    /// <summary>
    /// Обработать узлы со свойствами.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Узел со свойствами.</param>
    private void ImportPropertyNodes(string componentPath, XmlNodeList nodes)
    {
      var eventsPath = Path.Combine(componentPath, EventsFolder);
      var encoding = TransformerEnvironment.CurrentEncoding;
      foreach (XmlElement element in nodes)
      {
        var propertyName = element.GetAttribute("Name");
        if (this.eventFileNamesBlocks.ContainsKey(propertyName) && Directory.Exists(eventsPath))
        {
          var eventTextNode = element.SelectSingleNode("Value/Value");
          var eventFilePath = Path.Combine(eventsPath, this.eventFileNamesBlocks[propertyName]);
          if (eventTextNode == null || !File.Exists(eventFilePath))
            continue;

          var bytes = File.ReadAllBytes(eventFilePath);
          var encodedActionText = string.Format("{0}{1}", MimeCodedTextGuid, Convert.ToBase64String(bytes));
          XmlDocument doc = eventTextNode.OwnerDocument;
          var cData = doc.CreateCDataSection(encodedActionText);
          eventTextNode.AppendChild(cData);
        }
        else
        {
          var valueNodes = element.SelectNodes("Value/Value");
          if (valueNodes == null)
            continue;

          foreach (XmlElement valueNode in valueNodes)
          {
            var valueTextNode = valueNode.SelectSingleNode("text()");
            if (valueTextNode == null || string.IsNullOrEmpty(valueTextNode.Value))
              continue;

            var valueText = valueTextNode.Value;
            var encodedActionText = string.Format("{0}{1}", MimeCodedTextGuid, Convert.ToBase64String(encoding.GetBytes(valueText)));
            valueTextNode.Value = encodedActionText;
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
      var actionsPath = Path.Combine(componentPath, ActionsFolder);
      var encoding = TransformerEnvironment.CurrentEncoding;
      foreach (XmlElement element in nodes)
      {
        var actionCode = element.GetAttribute("Code");
        var actionIsblNode = element.SelectSingleNode("ISBLText/text()");
        if (actionIsblNode == null || string.IsNullOrEmpty(actionIsblNode.Value))
          continue;

        var actionIsblText = actionIsblNode.Value;
        actionIsblText = encoding.GetString(Convert.FromBase64String(actionIsblText));

        var actionFileName = string.Format("{0}.isbl", actionCode);
        var actionFilePath = Path.Combine(actionsPath, actionFileName);
        this.ExportTextToFile(actionFilePath, actionIsblText);

        actionIsblNode.Value = string.Empty;
      }
    }

    /// <summary>
    /// Импортировать действия.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Список узлов действий</param>
    private void ImportActionNodes(string componentPath, XmlNodeList nodes)
    {
      var actionsPath = Path.Combine(componentPath, ActionsFolder);
      if (Directory.Exists(actionsPath))
      {
        var encoding = TransformerEnvironment.CurrentEncoding;
        foreach (XmlElement element in nodes)
        {
          var actionCode = element.GetAttribute("Code");
          var actionIsblNode = element.SelectSingleNode("ISBLText/text()");
          var actionFileName = Path.Combine(actionsPath, string.Format("{0}.isbl", actionCode));
          if (actionIsblNode == null || !File.Exists(actionFileName))
            continue;

          var bytes = File.ReadAllBytes(actionFileName);
          var encodedActionText = Convert.ToBase64String(bytes);
          actionIsblNode.Value = encodedActionText;
        }
      }
    }


    /// <summary>
    /// Имена файлов событий ТМ.
    /// </summary>
    private readonly Dictionary<string, string> eventFileNamesTM = new Dictionary<string, string>
    {
      { "InitScript", "TaskBeforeSelection.isbl" },
      { "Script", "TaskAfterSelection.isbl" },
      { "TaskStart", "TaskStartPossibility.isbl" },
    };

    /// <summary>
    /// Экспортировать события.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Список узлов действий</param>
    private void ExportEventNodes(string componentPath, XmlNodeList nodes)
    {
      var eventPath = Path.Combine(componentPath, EventsFolder);
      var encoding = TransformerEnvironment.CurrentEncoding;
      foreach (XmlElement node in nodes)
      {
        foreach (var element in node.ChildNodes.Cast<XmlNode>())
        {

          var eventIsblText = element.InnerText;
          if (!eventIsblText.StartsWith(MimeCodedTextGuid))
            continue;

          eventIsblText = eventIsblText.Substring(MimeCodedTextGuid.Length);
          eventIsblText = encoding.GetString(Convert.FromBase64String(eventIsblText));

          var eventFileName =
           (eventFileNamesTM.ContainsKey(element.Name))
           ? eventFileNamesTM[element.Name]
           : string.Format("{0}.isbl", element.Name);

          var eventFilePath = Path.Combine(eventPath, eventFileName);
          this.ExportTextToFile(eventFilePath, eventIsblText);

          element.InnerText = string.Empty;
        }
      }
    }

    /// <summary>
    /// Импортировать события.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Список узлов действий</param>
    private void ImportEventNodes(string componentPath, XmlNodeList nodes)
    {
      var eventsPath = Path.Combine(componentPath, EventsFolder);
      if (Directory.Exists(eventsPath))
      {
        var encoding = TransformerEnvironment.CurrentEncoding;
        foreach (XmlElement node in nodes)
        {
          foreach (var element in node.ChildNodes.Cast<XmlNode>())
          {
            var eventFileName = Path.Combine(eventsPath, (eventFileNamesTM.ContainsKey(element.Name)) ? eventFileNamesTM[element.Name] : string.Format("{0}.isbl", element.Name));

            if (File.Exists(eventFileName))
            {
              var bytes = File.ReadAllBytes(eventFileName);
              var encodedActionText = string.Format("{0}{1}", MimeCodedTextGuid, Convert.ToBase64String(bytes));
              XmlDocument doc = node.OwnerDocument;
              var cData = doc.CreateCDataSection(encodedActionText);
              element.AppendChild(cData);
            }
          }
        }
      }
    }


    /// <summary>
    /// Экспортировать блоки.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Список узлов действий</param>
    private void ExportBlockNodes(string componentPath, XmlNodeList nodes)
    {
      var blockPath = Path.Combine(componentPath, BlocksFolder);
      foreach (XmlElement node in nodes)
      {
        var nameValue = node.GetAttribute("Name");
        if (string.IsNullOrEmpty(nameValue))
          nameValue = string.Format("{0}", node.GetAttribute("ID"));
        var path = Path.Combine(blockPath, nameValue);

        var propertiesNodes = node.SelectNodes(PropertiesXPath);
        if (propertiesNodes != null)
          this.ExportPropertyNodes(path, propertiesNodes);

        var eventsNodes = node.SelectNodes(EventXPath);
        if (eventsNodes != null)
          this.ExportEventNodes(path, eventsNodes);

        var ribbonNode = node.SelectSingleNode(RibbonXPath);
        if (ribbonNode != null)
          this.ExportRibbonNode(path, ribbonNode);
      }
    }

    /// <summary>
    /// Импортировать блоки.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="nodes">Список узлов действий</param>
    private void ImportBlockNodes(string componentPath, XmlNodeList nodes)
    {
      var blocksPath = Path.Combine(componentPath, BlocksFolder);
      if (Directory.Exists(blocksPath))
      {
        var encoding = TransformerEnvironment.CurrentEncoding;
        foreach (XmlElement node in nodes)
        {
          var nameValue = node.GetAttribute("Name");
          if (string.IsNullOrEmpty(nameValue))
            nameValue = node.GetAttribute("ID");
          var path = Path.Combine(blocksPath, nameValue);
          var propertiesNodes = node.SelectNodes(PropertiesXPath);
          if (propertiesNodes != null)
            this.ImportPropertyNodes(path, propertiesNodes);

          var eventsNodes = node.SelectNodes(EventXPath);
          if (eventsNodes != null)
            this.ImportEventNodes(path, eventsNodes);

          var ribbonNode = node.SelectSingleNode(RibbonXPath);
          if (ribbonNode != null)
            this.ImportRibbonNode(path, ribbonNode);
        }
      }
    }

    /// <summary>
    /// Экспортировать узел риббона.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="node">Узел риббона.</param>
    private void ExportRibbonNode(string componentPath, XmlNode node)
    {
      var ribbonTextNode = node.InnerText;
      if (string.IsNullOrEmpty(ribbonTextNode))
        return;

      var ribbonFilePath = Path.Combine(componentPath, RibbonFileName);
      this.ExportTextToFile(ribbonFilePath, ribbonTextNode);

      node.InnerText = string.Empty;
    }

    /// <summary>
    /// Импортировать узел риббона.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="node">Узел риббона.</param>
    private void ImportRibbonNode(string componentPath, XmlNode node)
    {
      var ribbonFilePath = Path.Combine(componentPath, RibbonFileName);
      if (File.Exists(ribbonFilePath))
      {
        var bytes = File.ReadAllBytes(ribbonFilePath);
        XmlDocument doc = node.OwnerDocument;
        var cData = doc.CreateCDataSection(TransformerEnvironment.CurrentEncoding.GetString(bytes));
        if (node.Name == RouteRibbonXPath)
        {
          var value = doc.CreateElement("Value");
          value.AppendChild(cData);
          node.AppendChild(value);
        }
        else
        {
          node.AppendChild(cData);
        }
      }
    }

    /// <summary>
    /// Экспортировать свойства типового маршрута.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="node">Xml узел свойств.</param>
    private void PropertiesExport(string componentPath, XmlNode node)
    {
      var propertiesNodes = node.SelectNodes(PropertiesXPath);
      if (propertiesNodes != null)
        this.ExportPropertyNodes(componentPath, propertiesNodes);

      var actionsNodes = node.SelectNodes(ActionsXPath);
      if (actionsNodes != null)
        this.ExportActionNodes(componentPath, actionsNodes);

      var eventsNodes = node.SelectNodes(EventXPath);
      if (eventsNodes != null)
        this.ExportEventNodes(componentPath, eventsNodes);

      var ribbonNode = node.SelectSingleNode(RouteRibbonXPath);
      if (ribbonNode != null)
        this.ExportRibbonNode(componentPath, ribbonNode);

      var blockNode = node.SelectNodes(BlockXPath);
      if (blockNode != null)
        this.ExportBlockNodes(componentPath, blockNode);
    }

    /// <summary>
    /// Импортировать свойства типового маршрута.
    /// </summary>
    /// <param name="componentPath">Путь компоненты.</param>
    /// <param name="node">Xml узел свойств.</param>
    private void PropertiesImport(string componentPath, XmlNode node)
    {
      var propertiesNodes = node.SelectNodes(PropertiesXPath);
      if (propertiesNodes != null)
        this.ImportPropertyNodes(componentPath, propertiesNodes);

      var actionsNodes = node.SelectNodes(ActionsXPath);
      if (actionsNodes != null)
        this.ImportActionNodes(componentPath, actionsNodes);

      var eventsNodes = node.SelectNodes(EventXPath);
      if (eventsNodes != null)
        this.ImportEventNodes(componentPath, eventsNodes);

      var ribbonNode = node.SelectSingleNode(RouteRibbonXPath);
      if (ribbonNode != null)
        this.ImportRibbonNode(componentPath, ribbonNode);

      var blockNode = node.SelectNodes(BlockXPath);
      if (blockNode != null)
        this.ImportBlockNodes(componentPath, blockNode);
    }
    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="inputFile">Наименование входящего файла.</param>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public RouteRecordHandler(string inputFile, string developmentPath) : base(developmentPath)
    {
      this.inputFile = inputFile;
    }

    #endregion

  }
}