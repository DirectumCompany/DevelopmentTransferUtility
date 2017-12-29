using System.Collections.Generic;
using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Models.Records
{
  /// <summary>
  /// Модель корневого элемента.
  /// </summary>
  [XmlRoot("ROOT")]
  public class RootModel
  {
    /// <summary>
    /// Версия.
    /// </summary>
    [XmlAttribute("Version")]
    public string Version { get { return "2.0"; } }

    /// <summary>
    /// Записи справочников.
    /// </summary>
    [XmlElement("RecordRef")]
    public List<RecordRefModel> Records { get; set; }
    
    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    public RootModel()
    {
      this.Records = new List<RecordRefModel>();
    }

    #endregion
  }
}
