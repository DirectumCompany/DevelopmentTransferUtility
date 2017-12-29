using NpoComputer.DevelopmentTransferUtility.Common;
using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace NpoComputer.DevelopmentTransferUtility.Models.Base
{
  /// <summary>
  /// Модель реквизита компоненты.
  /// </summary>
  public class RequisiteModel : IXmlSerializable
  {
    #region Константы

    /// <summary>
    /// Имя атрибута с кодом реквизита.
    /// </summary>
    private const string CodeAttributeName = "Code";

    /// <summary>
    /// Имя атрибута со значением реквизита.
    /// </summary>
    private const string ValueAttributeName = "Value";

    /// <summary>
    /// Имя атрибута с локализованным значением реквизита.
    /// </summary>
    private const string ValueLocalizeIdAttributeName = "ValueLocalizeID";

    /// <summary>
    /// Имя атрибута с признаком текстового реквизита в русской локализации.
    /// </summary>
    private const string TypeRuAttributeName = "Текст";

    /// <summary>
    /// Имя атрибута с признаком текстового реквизита в английской локализации.
    /// </summary>
    private const string TypeEnAttributeName = "Text";

    /// <summary>
    /// Значение признака текстового реквизита.
    /// </summary>
    private const string TextRequisiteType = "Text";

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Код реквизита.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Значение реквизита.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Локализованное значение реквизита.
    /// </summary>
    public string ValueLocalizeID { get; set; }

    /// <summary>
    /// Тип элемента.
    /// </summary>
    public string TypeRu { get; set; }

    /// <summary>
    /// Тип элемента для английского пакета.
    /// </summary>
    public string TypeEn { get; set; }

    /// <summary>
    /// Тип элемента.
    /// </summary>
    public string Type
    {
      get
      {
        if (TransformerEnvironment.IsRussianCodePage())
          return this.TypeRu;
        else
          return this.TypeEn;
      }
      set
      {
        if (TransformerEnvironment.IsRussianCodePage())
          this.TypeRu = value;
        else
          this.TypeEn = value;
      }
    }

    /// <summary>
    /// Текст.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Декодированное значение текстового поля.
    /// </summary>
    public string DecodedText
    {
      get
      {
        if (string.IsNullOrWhiteSpace(this.Text))
          return null;
        var encoding = TransformerEnvironment.CurrentEncoding;
        return encoding.GetString(Convert.FromBase64String(this.Text));
      }
      set
      {
        if (string.IsNullOrEmpty(value))
        {
          this.Text = string.Empty;
          return;
        }
        var encoding = TransformerEnvironment.CurrentEncoding;
        this.Text = Convert.ToBase64String(encoding.GetBytes(value));
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Подготовить к экспорту.
    /// </summary>
    public void PrepareForExport()
    {
      if (this.DecodedText != null)
        this.Value = this.DecodedText;

      this.Text = null;
    }

    /// <summary>
    /// Подготовить к импорту.
    /// </summary>
    public void PrepareForImport()
    {
      if (this.Type == TextRequisiteType)
      {
        this.DecodedText = this.Value;
        this.Value = null;
      }
    }

    /// <summary>
    /// Загрузить реквизит из файла.
    /// </summary>
    /// <param name="code">Код реквизита.</param>
    /// <param name="fileName">Имя файла.</param>
    /// <returns>Модель реквизита.</returns>
    public static RequisiteModel CreateFromFile(string code, string fileName)
    {
      var requisite = new RequisiteModel();
      requisite.Code = code;
      requisite.Type = TextRequisiteType;
      if (File.Exists(fileName))
      {
        var bytes = File.ReadAllBytes(fileName);
        requisite.DecodedText = TransformerEnvironment.CurrentEncoding.GetString(bytes);
      }
      return requisite;
    }

    /// <summary>
    /// Создать реквизит из текста.
    /// </summary>
    /// <param name="code">Код реквизита.</param>
    /// <param name="text">Текст реквизита.</param>
    /// <returns>Модель реквизита.</returns>
    public static RequisiteModel CreateFromText(string code, string text)
    {
      var requisite = new RequisiteModel();
      requisite.Code = code;
      requisite.Type = TextRequisiteType;
      requisite.DecodedText = text;
      return requisite;
    }

    #endregion

    #region IXmlSerializable

    public XmlSchema GetSchema()
    {
      return null;
    }

    public void ReadXml(XmlReader reader)
    {
      reader.MoveToContent();
      this.Code = reader.GetAttribute(CodeAttributeName);
      this.Value = reader.GetAttribute(ValueAttributeName);
      this.ValueLocalizeID = reader.GetAttribute(ValueLocalizeIdAttributeName);
      this.TypeRu = reader.GetAttribute(TypeRuAttributeName);
      this.TypeEn = reader.GetAttribute(TypeEnAttributeName);
      var isEmptyElement = reader.IsEmptyElement;
      reader.ReadStartElement();
      if (!isEmptyElement)
      {
        this.Text = reader.ReadContentAsString();
        reader.ReadEndElement();
      }
    }

    public void WriteXml(XmlWriter writer)
    {
      if (this.Code != null)
        writer.WriteAttributeString(CodeAttributeName, this.Code);
      if (this.Value != null)
        writer.WriteAttributeString(ValueAttributeName, this.Value);
      if (this.ValueLocalizeID != null)
        writer.WriteAttributeString(ValueLocalizeIdAttributeName, this.ValueLocalizeID);
      if (this.TypeRu != null)
        writer.WriteAttributeString(TypeRuAttributeName, this.TypeRu);
      if (this.TypeEn != null)
        writer.WriteAttributeString(TypeEnAttributeName, this.TypeEn);
      if (this.Text != null)
        writer.WriteCData(this.Text);
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
      this.Code = model.Code;
      this.Value = model.Value;
      this.ValueLocalizeID = model.ValueLocalizeID;
      this.TypeRu = model.TypeRu;
      this.TypeEn = model.TypeEn;
      this.Text = model.Text;
    }

    #endregion
  }
}
