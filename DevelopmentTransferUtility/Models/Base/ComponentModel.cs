using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Models.Base
{
  /// <summary>
  /// Модель компоненты.
  /// </summary>
  public class ComponentModel
  {
    #region Поля и свойства

    /// <summary>
    /// Ключевое значение.
    /// </summary>
    [XmlAttribute("KeyValue")]
    public string KeyValue { get; set; }

    /// <summary>
    /// Отображаемое значение.
    /// </summary>
    [XmlAttribute("DisplayValue")]
    public string DisplayValue { get; set; }

    /// <summary>
    /// Тип связанного справочника.
    /// </summary>
    /// <remarks>
    /// Для интегрированных отчетов.
    /// </remarks>
    [XmlAttribute("ReferenceName")]
    public string ReferenceName { get; set; }

    /// <summary>
    /// Главный раздел компоненты.
    /// </summary>
    [XmlElement("Requisites")]
    public RowModel Card { get; set; }

    /// <summary>
    /// Детальные разделы компоненты.
    /// </summary>
    [XmlElement("DetailDataSet")]
    public DataSetsModel DetailDataSets { get; set; }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    public ComponentModel()
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="model">Модель.</param>
    public ComponentModel(ComponentModel model)
    {
      this.KeyValue = model.KeyValue;
      this.DisplayValue = model.DisplayValue;
      if (model.Card != null)
        this.Card = new RowModel(model.Card);
      if (model.DetailDataSets != null)
        this.DetailDataSets = new DataSetsModel(model.DetailDataSets);
    }

    #endregion
  }
}
