using System.Collections.Generic;
using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Models.Records
{
  /// <summary>
  /// Модель записи справочника.
  /// </summary>
  public class RecordRefModel
  {
    /// <summary>
    /// Имя справочника.
    /// </summary>
    [XmlAttribute("Vid")]
    public string ReferenceName { get; set; }

    /// <summary>
    /// Код записи.
    /// </summary>
    [XmlAttribute("Kod")]
    public string Code { get; set; }

    /// <summary>
    /// Имя записи.
    /// </summary>
    [XmlAttribute("Name")]
    public string Name { get; set; }

    /// <summary>
    /// Реквизиты.
    /// </summary>
    [XmlElement("Requisite")]
    public List<RequisiteModel> Requisites { get; set; }
  }
}
