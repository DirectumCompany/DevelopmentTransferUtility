using System.Collections.Generic;
using System.Xml.Serialization;
using NpoComputer.DevelopmentTransferUtility.Models.Base;

namespace NpoComputer.DevelopmentTransferUtility.Models.ExportModels
{
  /// <summary>
  /// Модель экспортируемых строк локализации.
  /// </summary>
  [XmlRoot("LocalizationStrings")]
  public class LocalizationStringsExportModel
  {
    /// <summary>
    /// Список строк локализации.
    /// </summary>
    [XmlElement("LocalizationString")]
    public List<ComponentModel> LocalizationStrings { get; set; }

    public LocalizationStringsExportModel()
    {
      this.LocalizationStrings = new List<ComponentModel>();
    }
  }
}