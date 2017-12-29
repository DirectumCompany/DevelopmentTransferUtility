using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Models.Base
{
  /// <summary>
  /// Модель информации о пакете.
  /// </summary>
  [XmlRoot("PackageInfo")]
  public class PackageInfoModel
  {
    /// <summary>
    /// Признак режима имитеации.
    /// </summary>
    [XmlAttribute("ImitationMode")]
    public bool ImitationMode { get; set; }

    /// <summary>
    /// Признак пакета для главного сервера.
    /// </summary>
    [XmlAttribute("ForMainServer")]
    public bool ForMainServer { get; set; }

    /// <summary>
    /// Маска системы.
    /// </summary>
    [XmlAttribute("SystemMask")]
    public string SystemMask { get; set; }

    /// <summary>
    /// Версия платформы.
    /// </summary>
    [XmlAttribute("PlatformVersion")]
    public string PlatformVersion { get; set; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="componentsModel">Модель списка компонент.</param>
    /// <returns>Модель информации о пакете.</returns>
    public static PackageInfoModel CreateFromComponentsModel(ComponentsModel componentsModel)
    {
      var result = new PackageInfoModel()
      {
        ImitationMode = componentsModel.ImitationMode,
        ForMainServer = componentsModel.ForMainServer,
        SystemMask = componentsModel.SystemMask,
        PlatformVersion = componentsModel.PlatformVersion
      };
      return result;
    }
  }
}
