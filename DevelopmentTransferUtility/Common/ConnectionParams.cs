namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Класс параметров соединения.
  /// </summary>
  internal class ConnectionParams
  {
    #region Поля и свойства

    /// <summary>
    /// Имя сервера.
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// Имя БД.
    /// </summary>
    public string DatabaseName { get; set; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    public string Password { get; set; }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="serverName">Имя сервера.</param>
    /// <param name="databaseName">Имя БД.</param>
    /// <param name="userName">Имя пользователя.</param>
    /// <param name="password">Пароль.</param>
    public ConnectionParams(string serverName, string databaseName, string userName, string password)
    {
      this.ServerName = serverName;
      this.DatabaseName = databaseName;
      this.UserName = userName;
      this.Password = password;
    }

    #endregion

  }
}
