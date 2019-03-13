using CommandLine;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Типы аутентификации.
  /// </summary>
  internal enum AuthenticationType
  {
    /// <summary>
    /// SQL-аутентификация.
    /// </summary>
    Sql,

    /// <summary>
    /// Windows-аутентификация.
    /// </summary>
    Windows
  }
  /// <summary>
  /// Опции командной строки.
  /// </summary>
  internal class CommandLineOptions : ICommandLineOptions
  {
    #region ICommandLineOptions

    /// <summary>
    /// Режим работы утилиты.
    /// </summary>
    [Option('m', "mode", Required = false, DefaultValue = "export")]
    public string Mode { get; set; }

    /// <summary>
    /// Имя файла пакета разработки.
    /// </summary>
    [Option('i', "isx", Required = false)]
    public string DevelopmentPackageFileName { get; set; }

    /// <summary>
    /// Тип данных.
    /// </summary>
    [Option('y', "type", Required = false, DefaultValue = "standard")]
    public string Type { get; set; }

    /// <summary>
    /// Имя папки с разработкой.
    /// </summary>
    [Option('o', "devfolder", Required = true)]
    public string DevelopmentFolderName { get; set; }

    /// <summary>
    /// Имя файла конфигурации.
    /// </summary>
    [Option('c', "isc", Required = false)]
    public string ConfigurationFileName { get; set; }

    /// <summary>
    /// Путь к файлам клиентской части.
    /// </summary>
    [Option('p', "clientpartpath", Required = false)]
    public string ClientPartPath { get; set; }

    /// <summary>
    /// Имя сервера (для подключения к утилите переноса разработки).
    /// </summary>
    [Option('s', "server", Required = false)]
    public string Server { get; set; }

    /// <summary>
    /// Имя база данных (для подключения к утилите переноса разработки).
    /// </summary>
    [Option('d', "database", Required = false)]
    public string Database { get; set; }

    /// <summary>
    /// Имя пользователя (для подключения к утилите переноса разработки).
    /// </summary>
    [Option('n', "username", Required = false)]
    public string UserName { get; set; }

    /// <summary>
    /// Пароль пользователя (для подключения к утилите переноса разработки).
    /// </summary>
    [Option('w', "password", Required = false)]
    public string Password { get; set; }

    /// <summary>
    /// Тип аутентификации.
    /// </summary>
    [Option('a', "authtype", Required = false, DefaultValue = AuthenticationType.Sql)]
    public AuthenticationType AuthType { get; set; }

    /// <summary>
    /// Левая граница фильтра по дате.
    /// </summary>
    [Option('f', "fromdate", Required = false)]
    public string FromDateFilter { get; set; }

    /// <summary>
    /// Правая граница фильтра по дате.
    /// </summary>
    [Option('t', "todate", Required = false)]
    public string ToDateFilter { get; set; }

    /// <summary>
    /// Фильтр по имени пользователя.
    /// </summary>
    [Option('u', "userfilter", Required = false)]
    public string UserFilter { get; set; }

    /// <summary>
    /// Фильтр по ID типовых маршрутов.
    /// </summary>
    [Option('r', "routeids", Required = false)]
    public string RouteIDsFilter { get; set; }

    /// <summary>
    /// Закрыть окно приложения после окончания работы.
    /// </summary>
    [Option('l', "closewindow", Required = false, DefaultValue = false)]
    public bool CloseWindow { get; set; }

    /// <summary>
    /// Адрес коллекции TFS-сервера.
    /// </summary>
    [Option("tfs", Required = false)]
    public string TfsCollection { get; set; }

    /// <summary>
    /// Путь к папке с разработкой в TFS.
    /// </summary>
    [Option("tfsdevpath", Required = false)]
    public string TfsDevelopmentPath { get; set; }

    /// <summary>
    /// Список обрабатываемых Changeset.
    /// </summary>
    [Option("changesets", Required = false)]
    public string Changesets { get; set; }

    /// <summary>
    /// Признак запуска утилиты импорта в скрытом режиме (без показа окна).
    /// </summary>
    [Option('h', "hiddenimport", Required = false, DefaultValue = false)]
    public bool HiddenImportMode { get; set; }

    /// <summary>
    /// Список импортируемых папок компонент через запятую.
    /// </summary>
    [Option("importfolders", Required = false)]
    public string IncludedImportFolders { get; set; }

    /// <summary>
    /// Не экспортировать автоматически выбранные элементы конфигурации.
    /// </summary>
    [Option("skipautoadded", Required = false, DefaultValue = false)]
    public bool SkipAutoAddedElements { get; set; }

    [Option("utf8", Required =false,DefaultValue =false)]
    public bool convertToUTF8 { get; set; }

    #endregion

    /// <summary>
    /// Получить справку об использовании.
    /// </summary>
    /// <returns>Справка об использовании.</returns>
    [HelpOption]
    public string GetUsage()
    {
      return
        "Параметры командной строки:\n" +
        " --mode           - Режим работы (export/import, по умолчанию export).\n" +
        " --isx            - Файл пакета разработки (если параметр указан, то sbdte не будет запущен).\n" +
        " --type           - Тип пакета разработки (standard/routes/wizards, по умолчанию standard).\n" +
        " --devfolder      - Папка с разработкой.\n" +
        " --isc            - Имя файла конфигурации (для экспорта по конфигурации).\n" +
        " --clientpartpath - Путь к файлам клиентской части IS-Builder.\n" +
        " --server         - Имя сервера (используется для запуска sbdte и обработки удалений).\n" +
        " --database       - Имя базы данных (используется для запуска sbdte и обработки удалений).\n" +
        " --username       - Имя пользователя (используется для запуска sbdte и обработки удалений).\n" +
        " --password       - Пароль (используется для запуска sbdte и обработки удалений).\n" +
        " --authtype       - Тип аутентификации (win/sql, по умолчанию sql).\n" +
        " --fromdate       - Левая граница фильтра по дате изменения (используется для фильтрации при экспорте или импорте).\n" +
        " --todate         - Правая граница фильтра по дате изменения (используется для фильтрации при экспроте или импорте).\n" +
        " --userfilter     - Имя пользователя (без домена) для фильтрации (используется для фильтрации при экспорте или импорте).\n" +
        " --routeids       - Список ИД экспортируемых ТМ.\n" +
        " --tfs            - Путь к коллекции проектов TFS-сервера (используется для фильтрации при импорте).\n" +
        " --tfsdevpath     - Путь к папке с разработкой в TFS (используется для фильтрации при импорте).\n" +
        " --changesets     - Список ChangeSet через запятую (используется для фильтрации при импорте).\n" +
        " --closewindow    - Закрыть окно после окончания работы.\n" +
        " --hiddenimport   - Признак импорта в скрытом режиме (без показа окна утилиты импорта).\n" +
        " --importfolders  - Список импортируемых папок (используется как фильтр при импорте).\n" +
        " --skipautoadded  - Игнорировать автоматически выбранные элементы (используется только при экспорте).\n" +
        " --utf8           - Конвертировать файлы в UTF-8.\n" +
        " --help           - Вывести справку по параметрам командной строки.\n"; 
    }
  }
}
