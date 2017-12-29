using System.Data.SqlClient;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Класс-построитель строки подключения.
  /// </summary>
  internal class ConnectionManager
  {
    /// <summary>
    /// Построить строку подключения.
    /// </summary>
    /// <param name="connectionParams">Параметры соединения.</param>
    public static SqlConnection GetConnection(ConnectionParams connectionParams)
    {
      var connectionStringBuilder = new SqlConnectionStringBuilder();
      connectionStringBuilder.DataSource = connectionParams.ServerName;
      connectionStringBuilder.InitialCatalog = connectionParams.DatabaseName;
      connectionStringBuilder.UserID = connectionParams.UserName;
      connectionStringBuilder.Password = connectionParams.Password;
      var connectionString = connectionStringBuilder.ToString();
      var resultConnection = new SqlConnection(connectionString);
      return resultConnection;
    }
  }
}
