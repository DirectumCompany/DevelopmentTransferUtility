using NpoComputer.DevelopmentTransferUtility.Models.Records;
using System.Xml;
using System.Xml.Serialization;
using NpoComputer.DevelopmentTransferUtility.Common;

namespace NpoComputer.DevelopmentTransferUtility.Handlers
{
  /// <summary>
  /// Интерфейс обработчика записей.
  /// </summary>
  internal interface IRecordHandler
  {
    /// <summary>
    /// Обработать экспорт записей.
    /// </summary>
    /// <param name="recordModel">Модель записи.</param>
    /// <param name="settings">Настройки экспорта в XML.</param>
    /// <param name="namespaces">Пространства имен XML.</param>
    void HandleExport(RootModel rootModel, XmlWriterSettings settings, XmlSerializerNamespaces namespaces);

    /// <summary>
    /// Обработать удаление записей.
    /// </summary>
    /// <param name="connectionParams">Параметры соединения.</param>
    void HandleDelete(ConnectionParams connectionParams);

    /// <summary>
    /// Обработать импорт записей.
    /// </summary>
    /// <param name="packageModel">Модель пакета разработки.</param>
    /// <param name="importFilter">Фильтр импорта.</param>
    void HandleImport(RootModel RootModel, ImportFilter importFilter);
  }
}
