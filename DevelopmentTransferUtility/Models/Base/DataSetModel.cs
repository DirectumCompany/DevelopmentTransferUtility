using System.Collections.Generic;
using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Models.Base
{
  /// <summary>
  /// Модель детального раздела.
  /// </summary>
  public class DataSetModel
  {
    [XmlElement("Requisites")]
    public List<RowModel> Rows { get; set; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public DataSetModel()
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="model">Модель.</param>
    public DataSetModel(DataSetModel model)
    {
      if (model.Rows != null)
      {
        this.Rows = new List<RowModel>();
        foreach (var row in model.Rows)
          this.Rows.Add(new RowModel(row));
      }
    }
  }
}
