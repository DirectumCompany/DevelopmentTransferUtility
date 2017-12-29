using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Тип буфера.
  /// </summary>
  internal enum BufferType
  {
    /// <summary>
    /// Неизвестный.
    /// </summary>
    Unknown,
    /// <summary>
    /// Символьный.
    /// </summary>
    Chars,
    /// <summary>
    /// Байтовый.
    /// </summary>
    Bytes
  }

  /// <summary>
  /// Класс для записи XML-документов без лишних пробелов в закрывающем теге.
  /// Исходники взяты отсюда: https://www.codeproject.com/Articles/491669/Remove-space-in-NET-serialization-of-empty-XML-ele
  /// </summary>
  internal class SpacelessClosingTagXmlWriter : XmlWriter
  {
    #region Поля и свойства

    /// <summary>
    /// Исходный обернутый XmlWriter.
    /// </summary>
    private readonly XmlWriter xmlWriter;

    /// <summary>
    /// Внутренний XmlRawWriter.
    /// </summary>
    private readonly object internalWriter;

    /// <summary>
    /// Метод получения символов буфера.
    /// </summary>
    private readonly Func<object, char[]> getBufferChars;

    /// <summary>
    /// Метод получения байт буфера.
    /// </summary>
    private readonly Func<object, byte[]> getBufferBytes;

    /// <summary>
    /// Метод получения позиции в буфере.
    /// </summary>
    private readonly Func<object, int> getBufferPosition;

    /// <summary>
    /// Метод установки позиции в буфере.
    /// </summary>
    private readonly Action<object, int> setBufferPosition;

    /// <summary>
    /// Метод возврата длины буфера.
    /// </summary>
    private readonly Func<object, int> getBufferLength;

    /// <summary>
    /// Действие сброса буфера.
    /// </summary>
    private readonly Action<object> flushBuffer;

    /// <summary>
    /// Тип буфера.
    /// </summary>
    private readonly BufferType bufferType;

    #endregion

    #region Конструкторы

    public SpacelessClosingTagXmlWriter(XmlWriter xmlWriter)
    {
      this.xmlWriter = xmlWriter;

      var assembly = Assembly.GetAssembly(typeof(XmlWriter));
      var xmlWellFormedWriterType = assembly.GetType("System.Xml.XmlWellFormedWriter");
      var flags = BindingFlags.NonPublic | BindingFlags.Instance;
      var writerField = xmlWellFormedWriterType.GetField("writer", flags);
      Func<XmlWriter, object> getWriter = w => writerField.GetValue(w);
      this.internalWriter = getWriter(this.xmlWriter);

      var internalWriterType = this.internalWriter.GetType();
      var xmlEncodedRawTextWriterType = assembly.GetType("System.Xml.XmlEncodedRawTextWriter");
      var xmlEncodedRawTextWriterIndentType = assembly.GetType("System.Xml.XmlEncodedRawTextWriterIndent");
      var xmlUtf8RawTextWriterType = assembly.GetType("System.Xml.XmlUtf8RawTextWriter");
      var xmlUtf8RawTextWriterIndentType = assembly.GetType("System.Xml.XmlUtf8RawTextWriterIndent");
      FieldInfo bufferCharsBytesField;
      FieldInfo bufferPosField;
      FieldInfo bufferLenField;
      MethodInfo flushBufferMethod;
      if (internalWriterType == xmlEncodedRawTextWriterType)
      {
        this.bufferType = BufferType.Chars;
        bufferCharsBytesField = xmlEncodedRawTextWriterType.GetField("bufChars", flags);
        bufferPosField = xmlEncodedRawTextWriterType.GetField("bufPos", flags);
        bufferLenField = xmlEncodedRawTextWriterType.GetField("bufLen", flags);
        flushBufferMethod = xmlEncodedRawTextWriterType.GetMethod("FlushBuffer", flags);
      }
      else if (internalWriterType == xmlEncodedRawTextWriterIndentType)
      {
        this.bufferType = BufferType.Chars;
        bufferCharsBytesField = xmlEncodedRawTextWriterIndentType.GetField("bufChars", flags);
        bufferPosField = xmlEncodedRawTextWriterIndentType.GetField("bufPos", flags);
        bufferLenField = xmlEncodedRawTextWriterIndentType.GetField("bufLen", flags);
        flushBufferMethod = xmlEncodedRawTextWriterIndentType.GetMethod("FlushBuffer", flags);
      }
      else if (internalWriterType == xmlUtf8RawTextWriterType)
      {
        this.bufferType = BufferType.Bytes;
        bufferCharsBytesField = xmlUtf8RawTextWriterType.GetField("bufBytes", flags);
        bufferPosField = xmlUtf8RawTextWriterType.GetField("bufPos", flags);
        bufferLenField = xmlUtf8RawTextWriterType.GetField("bufLen", flags);
        flushBufferMethod = xmlUtf8RawTextWriterType.GetMethod("FlushBuffer", flags);
      }
      else if (internalWriterType == xmlUtf8RawTextWriterIndentType)
      {
        this.bufferType = BufferType.Bytes;
        bufferCharsBytesField = xmlUtf8RawTextWriterIndentType.GetField("bufBytes", flags);
        bufferPosField = xmlUtf8RawTextWriterIndentType.GetField("bufPos", flags);
        bufferLenField = xmlUtf8RawTextWriterIndentType.GetField("bufLen", flags);
        flushBufferMethod = xmlUtf8RawTextWriterIndentType.GetMethod("FlushBuffer", flags);
      }
      else
      {
        this.bufferType = BufferType.Unknown;
        throw new Exception("Unkown internal XmlWriter class");
      }
      switch (this.bufferType)
      {
        case BufferType.Unknown:
          break;
        case BufferType.Chars:
          this.getBufferChars = w => (char[])bufferCharsBytesField.GetValue(w);
          this.getBufferPosition = w => (int)bufferPosField.GetValue(w);
          this.setBufferPosition = (w, i) => bufferPosField.SetValue(w, i);
          this.getBufferLength = w => (int)bufferLenField.GetValue(w);
          this.flushBuffer = w => flushBufferMethod.Invoke(w, new object[0]);
          break;
        case BufferType.Bytes:
          this.getBufferBytes = w => (byte[])bufferCharsBytesField.GetValue(w);
          this.getBufferPosition = w => (int)bufferPosField.GetValue(w);
          this.setBufferPosition = (w, i) => bufferPosField.SetValue(w, i);
          this.getBufferLength = w => (int)bufferLenField.GetValue(w);
          this.flushBuffer = w => flushBufferMethod.Invoke(w, new object[0]);
          break;
      }
    }

    #endregion

    #region Методы

    public static new SpacelessClosingTagXmlWriter Create(Stream output)
    {
      return new SpacelessClosingTagXmlWriter(XmlWriter.Create(output));
    }

    public static new SpacelessClosingTagXmlWriter Create(string outputFileName)
    {
      return new SpacelessClosingTagXmlWriter(XmlWriter.Create(outputFileName));
    }

    public static new SpacelessClosingTagXmlWriter Create(StringBuilder output)
    {
      return new SpacelessClosingTagXmlWriter(XmlWriter.Create(output));
    }

    public static new SpacelessClosingTagXmlWriter Create(TextWriter output)
    {
      return new SpacelessClosingTagXmlWriter(XmlWriter.Create(output));
    }

    public static new SpacelessClosingTagXmlWriter Create(XmlWriter output)
    {
      return new SpacelessClosingTagXmlWriter(XmlWriter.Create(output));
    }

    public static new SpacelessClosingTagXmlWriter Create(string outputFileName, XmlWriterSettings settings)
    {
      return new SpacelessClosingTagXmlWriter(XmlWriter.Create(outputFileName, settings));
    }

    public static new SpacelessClosingTagXmlWriter Create(Stream output, XmlWriterSettings settings)
    {
      return new SpacelessClosingTagXmlWriter(XmlWriter.Create(output, settings));
    }

    public static new SpacelessClosingTagXmlWriter Create(StringBuilder output, XmlWriterSettings settings)
    {
      return new SpacelessClosingTagXmlWriter(XmlWriter.Create(output, settings));
    }

    public static new SpacelessClosingTagXmlWriter Create(TextWriter output, XmlWriterSettings settings)
    {
      return new SpacelessClosingTagXmlWriter(XmlWriter.Create(output, settings));
    }

    public static new SpacelessClosingTagXmlWriter Create(XmlWriter output, XmlWriterSettings settings)
    {
      return new SpacelessClosingTagXmlWriter(XmlWriter.Create(output, settings));
    }

    public new void WriteAttributeString(string localName, string value)
    {
      this.xmlWriter.WriteAttributeString(localName, value);
    }

    public new void WriteAttributeString(string localName, string ns, string value)
    {
      this.xmlWriter.WriteAttributeString(localName, ns, value);
    }

    public new void WriteAttributeString(string prefix, string localName, string ns, string value)
    {
      this.xmlWriter.WriteAttributeString(prefix, localName, ns, value);
    }

    public new void WriteElementString(string localName, string value)
    {
      this.xmlWriter.WriteElementString(localName, value);
    }

    public new void WriteElementString(string localName, string ns, string value)
    {
      this.xmlWriter.WriteElementString(localName, ns, value);
    }

    public new void WriteElementString(string prefix, string localName, string ns, string value)
    {
      this.xmlWriter.WriteElementString(prefix, localName, ns, value);
    }

    public new void WriteStartAttribute(string localName)
    {
      this.xmlWriter.WriteStartAttribute(localName);
    }

    public new void WriteStartAttribute(string localName, string ns)
    {
      this.xmlWriter.WriteStartAttribute(localName, ns);
    }

    #endregion

    #region XmlWriter

    public override XmlWriterSettings Settings
    {
      get { return this.xmlWriter.Settings; }
    }

    public override WriteState WriteState
    {
      get { return this.xmlWriter.WriteState; }
    }

    public override string XmlLang
    {
      get { return this.xmlWriter.XmlLang; }
    }

    public override XmlSpace XmlSpace
    {
      get { return this.xmlWriter.XmlSpace; }
    }

    public override void Close()
    {
      this.xmlWriter.Close();
    }

    public override void Flush()
    {
      this.xmlWriter.Flush();
    }

    public override string LookupPrefix(string ns)
    {
      return this.xmlWriter.LookupPrefix(ns);
    }

    public override void WriteAttributes(XmlReader reader, bool defattr)
    {
      this.xmlWriter.WriteAttributes(reader, defattr);
    }

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      this.xmlWriter.WriteBase64(buffer, index, count);
    }

    public override void WriteBinHex(byte[] buffer, int index, int count)
    {
      this.xmlWriter.WriteBinHex(buffer, index, count);
    }

    public override void WriteCData(string text)
    {
      this.xmlWriter.WriteCData(text);
    }

    public override void WriteCharEntity(char ch)
    {
      this.xmlWriter.WriteCharEntity(ch);
    }

    public override void WriteChars(char[] buffer, int index, int count)
    {
      this.xmlWriter.WriteChars(buffer, index, count);
    }

    public override void WriteComment(string text)
    {
      this.xmlWriter.WriteComment(text);
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
      this.xmlWriter.WriteDocType(name, pubid, sysid, subset);
    }

    public override void WriteEndAttribute()
    {
      this.xmlWriter.WriteEndAttribute();
    }

    public override void WriteEndDocument()
    {
      this.xmlWriter.WriteEndDocument();
    }

    public override void WriteEndElement()
    {
      if (this.bufferType != BufferType.Unknown)
      {
        var bufPos = this.getBufferPosition(this.internalWriter);
        var bufLen = this.getBufferLength(this.internalWriter);
        if ((bufPos + 3) >= bufLen)
          this.flushBuffer(this.internalWriter);
      }
      this.xmlWriter.WriteEndElement();
      if (this.bufferType == BufferType.Chars)
      {
        var bufPos = this.getBufferPosition(this.internalWriter);
        var bufChars = this.getBufferChars(this.internalWriter);
        if (bufPos > 3 && bufChars[bufPos - 3] == ' ' &&
              bufChars[bufPos - 2] == '/' && bufChars[bufPos - 1] == '>')
        {
          bufChars[bufPos - 3] = '/';
          bufChars[bufPos - 2] = '>';
          bufPos--;
          this.setBufferPosition(this.internalWriter, bufPos);
        }
      }
      else if (this.bufferType == BufferType.Bytes)
      {
        var bufPos = this.getBufferPosition(this.internalWriter);
        var bufBytes = this.getBufferBytes(this.internalWriter);
        if (bufPos > 3 && bufBytes[bufPos - 3] == ' ' &&
              bufBytes[bufPos - 2] == '/' && bufBytes[bufPos - 1] == '>')
        {
          bufBytes[bufPos - 3] = (byte)'/';
          bufBytes[bufPos - 2] = (byte)'>';
          bufPos--;
          this.setBufferPosition(this.internalWriter, bufPos);
        }
      }
    }

    public override void WriteEntityRef(string name)
    {
      this.xmlWriter.WriteEntityRef(name);
    }

    public override void WriteFullEndElement()
    {
      this.xmlWriter.WriteFullEndElement();
    }

    public override void WriteName(string name)
    {
      this.xmlWriter.WriteName(name);
    }

    public override void WriteNmToken(string name)
    {
      this.xmlWriter.WriteNmToken(name);
    }

    public override void WriteNode(XmlReader reader, bool defattr)
    {
      this.xmlWriter.WriteNode(reader, defattr);
    }

    public override void WriteNode(XPathNavigator navigator, bool defattr)
    {
      this.xmlWriter.WriteNode(navigator, defattr);
    }

    public override void WriteProcessingInstruction(string name, string text)
    {
      this.xmlWriter.WriteProcessingInstruction(name, text);
    }

    public override void WriteQualifiedName(string localName, string ns)
    {
      this.xmlWriter.WriteQualifiedName(localName, ns);
    }

    public override void WriteRaw(string data)
    {
      this.xmlWriter.WriteRaw(data);
    }

    public override void WriteRaw(char[] buffer, int index, int count)
    {
      this.xmlWriter.WriteRaw(buffer, index, count);
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      this.xmlWriter.WriteStartAttribute(prefix, localName, ns);
    }

    public override void WriteStartDocument()
    {
      this.xmlWriter.WriteStartDocument();
    }

    public override void WriteStartDocument(bool standalone)
    {
      this.xmlWriter.WriteStartDocument(standalone);
    }

    public new void WriteStartElement(string localName)
    {
      this.xmlWriter.WriteStartElement(localName);
    }

    public new void WriteStartElement(string localName, string ns)
    {
      this.xmlWriter.WriteStartElement(localName, ns);
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      this.xmlWriter.WriteStartElement(prefix, localName, ns);
    }

    public override void WriteString(string text)
    {
      this.xmlWriter.WriteString(text);
    }

    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
      this.xmlWriter.WriteSurrogateCharEntity(lowChar, highChar);
    }

    public override void WriteValue(bool value)
    {
      this.xmlWriter.WriteValue(value);
    }

    public override void WriteValue(DateTime value)
    {
      this.xmlWriter.WriteValue(value);
    }

    public override void WriteValue(decimal value)
    {
      this.xmlWriter.WriteValue(value);
    }

    public override void WriteValue(double value)
    {
      this.xmlWriter.WriteValue(value);
    }

    public override void WriteValue(int value)
    {
      this.xmlWriter.WriteValue(value);
    }

    public override void WriteValue(long value)
    {
      this.xmlWriter.WriteValue(value);
    }

    public override void WriteValue(Object value)
    {
      this.xmlWriter.WriteValue(value);
    }

    public override void WriteValue(float value)
    {
      this.xmlWriter.WriteValue(value);
    }

    public override void WriteValue(string value)
    {
      this.xmlWriter.WriteValue(value);
    }

    public override void WriteWhitespace(string value)
    {
      this.xmlWriter.WriteWhitespace(value);
    }

    #endregion
  }
}
