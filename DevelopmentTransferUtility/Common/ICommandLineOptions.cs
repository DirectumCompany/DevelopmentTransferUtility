namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Интерфейс параметров экспорта.
  /// </summary>
  internal interface ICommandLineOptions
  {
    /// <summary>
    /// Режим работы утилиты.
    /// </summary>
    string Mode { get; set; }

    /// <summary>
    /// Имя файла пакета разработки.
    /// </summary>
    string DevelopmentPackageFileName { get; set; }

    /// <summary>
    /// Входные данные.
    /// </summary>
    string Type { get; set; }

    /// <summary>
    /// Имя папки с разработкой.
    /// </summary>
    string DevelopmentFolderName { get; set; }

    /// <summary>
    /// Имя файла конфигурации.
    /// </summary>
    string ConfigurationFileName { get; set; }

    /// <summary>
    /// Путь к файлам клиентской части.
    /// </summary>
    string ClientPartPath { get; set; }

    /// <summary>
    /// Имя сервера (дяля подключения к утилите переноса разработки).
    /// </summary>
    string Server { get; set; }

    /// <summary>
    /// Имя база данных (для подключения к утилите переноса разработки).
    /// </summary>
    string Database { get; set; }

    /// <summary>
    /// Имя пользователя (для подключения к утилите переноса разработки).
    /// </summary>
    string UserName { get; set; }

    /// <summary>
    /// Пароль пользователя (для подключения к утилите переноса разработки).
    /// </summary>
    string Password { get; set; }

    /// <summary>
    /// Тип аутентификации.
    /// </summary>
    AuthenticationType AuthType { get; set; }

    /// <summary>
    /// Левая граница фильтра по дате.
    /// </summary>
    string FromDateFilter { get; set; }

    /// <summary>
    /// Правая граница фильтра по дате.
    /// </summary>
    string ToDateFilter { get; set; }

    /// <summary>
    /// Фильтр по имени пользователя.
    /// </summary>
    string UserFilter { get; set; }

    /// <summary>
    /// Фильтр по имени пользователя.
    /// </summary>
    string RouteIDsFilter { get; set; }

    /// <summary>
    /// Закрыть окно приложения после окончания работы.
    /// </summary>
    bool CloseWindow { get; set; }

    /// <summary>
    /// Адрес коллекции TFS-сервера.
    /// </summary>
    string TfsCollection { get; set; }

    /// <summary>
    /// Путь к папке с разработкой в TFS.
    /// </summary>
    string TfsDevelopmentPath { get; set; }

    /// <summary>
    /// Список обрабатываемых Changeset.
    /// </summary>
    string Changesets { get; set; }

    /// <summary>
    /// Признак запуска утилиты импорта в скрытом режиме (без показа окна).
    /// </summary>
    bool HiddenImportMode { get; set; }

    /// <summary>
    /// Список импортируемых папок компонент через запятую.
    /// </summary>
    string IncludedImportFolders { get; set; }

    /// <summary>
    /// Не экспортировать автоматически выбранные элементы.
    /// </summary>
    bool SkipAutoAddedElements { get; set; }

    /// <summary>
    /// Конвертировать файлы в UTF-8
    /// </summary>
    bool convertToUTF8 { get; set; }
  }
}
