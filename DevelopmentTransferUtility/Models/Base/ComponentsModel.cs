using System.Collections.Generic;
using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Models.Base
{
  /// <summary>
  /// Модель списка компонент.
  /// </summary>
  [XmlRoot("Components")]
  public class ComponentsModel
  {
    #region Поля и свойства

    /// <summary>
    /// Версия платформы.
    /// </summary>
    [XmlAttribute("PlatformVesion")]
    public string PlatformVersion { get; set; }

    /// <summary>
    /// Маска системы.
    /// </summary>
    [XmlAttribute("SystemMask")]
    public string SystemMask { get; set; }

    /// <summary>
    /// Признак пакета для главного сервера.
    /// </summary>
    [XmlAttribute("ForMainServer")]
    public bool ForMainServer { get; set; }

    /// <summary>
    /// Признак режима имитации.
    /// </summary>
    [XmlAttribute("ImitationMode")]
    public bool ImitationMode { get; set; }

    /// <summary>
    /// Модель констант.
    /// </summary>
    [XmlArray("Constants")]
    [XmlArrayItem("Constants")]
    public List<ComponentModel> Constants { get; set; }

    /// <summary>
    /// Модель реквизитов диалогов.
    /// </summary>
    [XmlArray("DialogRequisites")]
    [XmlArrayItem("DialogRequisites")]
    public List<ComponentModel> DialogRequisites { get; set; }

    /// <summary>
    /// Модель диалогов.
    /// </summary>
    [XmlArray("Dialogs")]
    [XmlArrayItem("Dialogs")]
    public List<ComponentModel> Dialogs { get; set; }

    /// <summary>
    /// Модели реквизитов документов.
    /// </summary>
    [XmlArray("EDocRequisites")]
    [XmlArrayItem("EDocRequisites")]
    public List<ComponentModel> DocumentRequisites { get; set; }

    /// <summary>
    /// Модели типов карточек электронных документов.
    /// </summary>
    [XmlArray("EDCardTypes")]
    [XmlArrayItem("EDCardTypes")]
    public List<ComponentModel> DocumentCardTypes { get; set; }

    /// <summary>
    /// Модели групп функций.
    /// </summary>
    [XmlArray("GrFunctions")]
    [XmlArrayItem("GrFunctions")]
    public List<ComponentModel> FunctionGroups { get; set; }

    /// <summary>
    /// Модели функций.
    /// </summary>
    [XmlArray("Functions")]
    [XmlArrayItem("Functions")]
    public List<ComponentModel> Functions { get; set; }

    /// <summary>
    /// Модели строк локализации.
    /// </summary>
    [XmlArray("LocalizedStrings")]
    [XmlArrayItem("LocalizedStrings")]
    public List<ComponentModel> LocalizationStrings { get; set; }

    /// <summary>
    /// Модели модулей.
    /// </summary>
    [XmlArray("Modules")]
    [XmlArrayItem("Modules")]
    public List<ComponentModel> Modules { get; set; }

    /// <summary>
    /// Модели типов справочников.
    /// </summary>
    [XmlArray("RefTypes")]
    [XmlArrayItem("RefTypes")]
    public List<ComponentModel> ReferenceTypes { get; set; }

    /// <summary>
    /// Модели реквизитов справочников.
    /// </summary>
    [XmlArray("RefRequisites")]
    [XmlArrayItem("RefRequisites")]
    public List<ComponentModel> ReferenceRequisites { get; set; }

    /// <summary>
    /// Модели отчетов.
    /// </summary>
    [XmlArray("Reports")]
    [XmlArrayItem("Reports")]
    public List<ComponentModel> Reports { get; set; }

    /// <summary>
    /// Модели групп блоков типовых маршрутов.
    /// </summary>
    [XmlArray("WorkflowBlockGroups")]
    [XmlArrayItem("WorkflowBlockGroups")]
    public List<ComponentModel> RouteBlockGroups { get; set; }

    /// <summary>
    /// Модели блоков типовых маршрутов.
    /// </summary>
    [XmlArray("WorkflowBlocks")]
    [XmlArrayItem("WorkflowBlocks")]
    public List<ComponentModel> RouteBlocks { get; set; }

    /// <summary>
    /// Модели сценариев.
    /// </summary>
    [XmlArray("Scripts")]
    [XmlArrayItem("Scripts")]
    public List<ComponentModel> Scripts { get; set; }

    /// <summary>
    /// Модели серверных событий.
    /// </summary>
    [XmlArray("ServerEvents")]
    [XmlArrayItem("ServerEvents")]
    public List<ComponentModel> ServerEvents { get; set; }

    /// <summary>
    /// Модели приложений просмотрщиков.
    /// </summary>
    [XmlArray("Viewers")]
    [XmlArrayItem("Viewers")]
    public List<ComponentModel> Viewers { get; set; }

    #endregion

    #region Методы

    /// <summary>
    /// Вернуть null, если список пуст, иначе вернуть сам исходный список.
    /// </summary>
    /// <param name="list">Исходный список.</param>
    /// <returns>Null, если список пуст, иначе сам исходный список.</returns>
    private List<ComponentModel> ClearIfEmpty(List<ComponentModel> list)
    {
      return list.Count == 0 ? null : list;
    }

    /// <summary>
    /// Установить информацию о пакете.
    /// </summary>
    /// <param name="packageInfoModel">Модель информации о пакете.</param>
    public void SetupPackageInfo(PackageInfoModel packageInfoModel)
    {
      this.ImitationMode = packageInfoModel.ImitationMode;
      this.ForMainServer = packageInfoModel.ForMainServer;
      this.SystemMask = packageInfoModel.SystemMask;
      this.PlatformVersion = packageInfoModel.PlatformVersion;
    }

    /// <summary>
    /// Очистить пустые списки.
    /// </summary>
    public void ClearEmptyLists()
    {
      this.Constants = this.ClearIfEmpty(this.Constants);      
      this.DialogRequisites = this.ClearIfEmpty(this.DialogRequisites);
      this.Dialogs = this.ClearIfEmpty(this.Dialogs);
      this.DocumentCardTypes = this.ClearIfEmpty(this.DocumentCardTypes);
      this.DocumentRequisites = this.ClearIfEmpty(this.DocumentRequisites);
      this.FunctionGroups = this.ClearIfEmpty(this.FunctionGroups);
      this.Functions = this.ClearIfEmpty(this.Functions);
      this.LocalizationStrings = this.ClearIfEmpty(this.LocalizationStrings);
      this.Modules = this.ClearIfEmpty(this.Modules);
      this.ReferenceRequisites = this.ClearIfEmpty(this.ReferenceRequisites);
      this.ReferenceTypes = this.ClearIfEmpty(this.ReferenceTypes);
      this.Reports = this.ClearIfEmpty(this.Reports);
      this.RouteBlockGroups = this.ClearIfEmpty(this.RouteBlockGroups);
      this.RouteBlocks = this.ClearIfEmpty(this.RouteBlocks);
      this.Scripts = this.ClearIfEmpty(this.Scripts);
      this.Viewers = this.ClearIfEmpty(this.Viewers);
      this.ServerEvents = this.ClearIfEmpty(this.ServerEvents);
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    public ComponentsModel()
    {
      this.Constants = new List<ComponentModel>();
      this.DialogRequisites = new List<ComponentModel>();
      this.Dialogs = new List<ComponentModel>();
      this.DocumentCardTypes = new List<ComponentModel>();
      this.DocumentRequisites = new List<ComponentModel>();
      this.FunctionGroups = new List<ComponentModel>();
      this.Functions = new List<ComponentModel>();
      this.LocalizationStrings = new List<ComponentModel>();
      this.Modules = new List<ComponentModel>();
      this.ReferenceRequisites = new List<ComponentModel>();
      this.ReferenceTypes = new List<ComponentModel>();
      this.Reports = new List<ComponentModel>();
      this.RouteBlockGroups = new List<ComponentModel>();
      this.RouteBlocks = new List<ComponentModel>();
      this.Scripts = new List<ComponentModel>();
      this.Viewers = new List<ComponentModel>();
      this.ServerEvents = new List<ComponentModel>();
    }

    #endregion
  }
}
