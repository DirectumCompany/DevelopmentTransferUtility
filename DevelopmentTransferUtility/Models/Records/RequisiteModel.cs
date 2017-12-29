using NpoComputer.DevelopmentTransferUtility.Common;
using System;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace NpoComputer.DevelopmentTransferUtility.Models.Records
{
  /// <summary>
  /// Модель реквизита.
  /// </summary>
  public class RequisiteModel
  {
    #region Константы

    /// <summary>
    /// Значение признака текстового реквизита.
    /// </summary>
    private const string TextRequisiteType = "Text";

    /// <summary>
    /// Значение признака текстового реквизита.
    /// </summary>
    private const string NameAttributeName = "Name";

    /// <summary>
    /// Значение признака текстового реквизита.
    /// </summary>
    private const string ValueAttributeName = "Value";

    /// <summary>
    /// Значение признака текстового реквизита.
    /// </summary>
    private const string TypeAttributeName = "Type";

    #endregion

    /// <summary>
    /// Имя.
    /// </summary>
    [XmlAttribute("Name")]
    public string Name { get; set; }

    /// <summary>
    /// Значение.
    /// </summary>
    [XmlAttribute("Value")]
    public string Value { get; set; }

    /// <summary>
    /// Номер строки в детальном разделе.
    /// </summary>
    [XmlAttribute("NumStr")]
    public string Number { get; set; }

    /// <summary>
    /// Запись справочника.
    /// </summary>
    [XmlElement("RecordRef")]
    public RecordRefModel Record { get; set; }

    /// <summary>
    /// Тип элемента.
    /// </summary>
    [XmlAttribute("Type")]
    public string Type { get; set; }

    /// <summary>
    /// Текст.
    /// </summary>
    [XmlText]
    public string Text { get; set; }

    /// <summary>
    /// Данные.
    /// </summary>
    [XmlAnyElement]
    public XmlNode Data { get; set; }

    /// <summary>
    /// Декодированное значение текстового поля.
    /// </summary>
    [XmlIgnore]
    public string DecodedValue
    {
      get
      {
        if (this.Text == null)
        {
          if (this.Data != null)
          {
            var encoding = TransformerEnvironment.CurrentEncoding;
            string dataString = Data.InnerText;
            return encoding.GetString(Convert.FromBase64String(dataString));
          }
          else
          {
            return null;
          }
        }
        else
        {
          var encoding = TransformerEnvironment.CurrentEncoding;
          string dataString = Text;
          return encoding.GetString(Convert.FromBase64String(dataString));
        }
      }
      set
      {
        if (string.IsNullOrWhiteSpace(value))
        {
          this.Data = null;
          return;
        }
        var encoding = TransformerEnvironment.CurrentEncoding;
        var encodedText = Convert.ToBase64String(encoding.GetBytes(value));
        XmlDocument doc = new XmlDocument();
        this.Data = doc.CreateCDataSection(encodedText);
      }
    }

    #region Методы

    /// <summary>
    /// Подготовить к экспорту.
    /// </summary>
    public void PrepareForExport()
    {
      if (this.DecodedValue != null)
        this.Value = this.DecodedValue;
    }

    /// <summary>
    /// Подготовить к импорту.
    /// </summary>
    public void PrepareForImport()
    {
      if (this.Type == TextRequisiteType)
      {
        this.DecodedValue = this.Value;
        this.Value = null;
      }
    }

    /// <summary>
    /// Загрузить реквизит из файла.
    /// </summary>
    /// <param name="code">Код реквизита.</param>
    /// <param name="fileName">Имя файла.</param>
    /// <returns>Модель реквизита.</returns>
    public static RequisiteModel CreateFromFile(string name, string fileName)
    {
      var requisite = new RequisiteModel();
      requisite.Name = name;
      requisite.Type = TextRequisiteType;
      if (File.Exists(fileName))
      {
        var bytes = File.ReadAllBytes(fileName);
        requisite.DecodedValue = TransformerEnvironment.CurrentEncoding.GetString(bytes);
      }
      return requisite;
    }

    /// <summary>
    /// Создать реквизит из текста.
    /// </summary>
    /// <param name="code">Код реквизита.</param>
    /// <param name="text">Текст реквизита.</param>
    /// <returns>Модель реквизита.</returns>
    public static RequisiteModel CreateFromText(string name, string text)
    {
      var requisite = new RequisiteModel();
      requisite.Name = name;
      requisite.Type = TextRequisiteType;
      requisite.DecodedValue = text;
      return requisite;
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    public RequisiteModel()
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="model">Модель.</param>
    public RequisiteModel(RequisiteModel model)
    {
      this.Name = model.Name;
      this.Type = model.Type;
      this.Value = model.Value;
      this.Number = model.Number;
      this.Record = model.Record;
      this.Text = model.Text;
    }

    #endregion
    }
}
