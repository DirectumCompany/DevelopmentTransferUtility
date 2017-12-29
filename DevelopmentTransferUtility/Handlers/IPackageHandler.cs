using NpoComputer.DevelopmentTransferUtility.Models.Base;
using System.Xml;
using System.Xml.Serialization;
using NpoComputer.DevelopmentTransferUtility.Common;

namespace NpoComputer.DevelopmentTransferUtility.Handlers
{
  /// <summary>
  /// Интерфейс обработчика пакета.
  /// </summary>
  internal interface IPackageHandler
  {
    /// <summary>
    /// Обработать экспорт разработки.
    /// </summary>
    /// <param name="packageModel">Модель пакета разработки.</param>
    /// <param name="settings">Настройки экспорта в XML.</param>
    /// <param name="namespaces">Пространства имен XML.</param>
    void HandleExport(ComponentsModel packageModel, XmlWriterSettings settings, XmlSerializerNamespaces namespaces);

    /// <summary>
    /// Обработать удаление элементов разработки.
    /// </summary>
    /// <param name="connectionParams">Параметры соединения с сервером.</param>
    /// <param name="settings">Настройки экспорта в XML.</param>
    /// <param name="namespaces">Пространства имен XML.</param>
    void HandleDelete(ConnectionParams connectionParams, XmlWriterSettings settings, XmlSerializerNamespaces namespaces);

    /// <summary>
    /// Обработать импорт разработки.
    /// </summary>
    /// <param name="packageModel">Модель пакета разработки.</param>
    /// <param name="importFilter">Фильтр импорта.</param>
    void HandleImport(ComponentsModel packageModel, ImportFilter importFilter);
  }
}
