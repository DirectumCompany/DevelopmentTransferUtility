using System.Collections.Generic;
using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Models.Base
{
  /// <summary>
  /// Модель строки детального раздела.
  /// </summary>
  public class RowModel
  {
    [XmlElement("Requisite")]
    public List<RequisiteModel> Requisites { get; set; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public RowModel()
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="model">Модель.</param>
    public RowModel(RowModel model)
    {
      if (model.Requisites != null)
      {
        this.Requisites = new List<RequisiteModel>();
        foreach (var requisite in model.Requisites)
          this.Requisites.Add(new RequisiteModel(requisite));
      }
    }
  }
}
