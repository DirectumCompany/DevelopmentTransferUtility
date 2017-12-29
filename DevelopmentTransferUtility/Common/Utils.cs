using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Утилиты.
  /// </summary>
  internal static class Utils
  {
    /// <summary>
    /// Получить хеш потока.
    /// </summary>
    /// <param name="stream">Поток.</param>
    /// <returns>Хеш потока.</returns>
    public static string GetHashCode(Stream stream)
    {
      MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
      byte[] hash = md5Provider.ComputeHash(stream);
      return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Получить хеш файла.
    /// </summary>
    /// <param name="filename">Имя файла.</param>
    /// <returns>Хеш файла.</returns>
    public static string GetHashCode(string filename)
    {
      using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      {
        return GetHashCode(stream);
      }
    }

    /// <summary>
    /// Заменить спец символы в пути.
    /// </summary>
    /// <param name="path">Путь.</param>
    /// <returns>Путь с заменой спец символов.</returns>
    public static string EscapeFilePath(string path)
    {
      string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
      Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
      return r.Replace(path, "_");
    }

    /// <summary>
    /// Получить уникальное имя временной папки.
    /// </summary>
    /// <returns>Уникальное имя папки.</returns>
    public static string GetUniqueTempFolderName()
    {
      var tempPath = Path.GetTempPath();
      string resultPath;
      // Будем перебирать Guid-ы до тех пор, пока не найдем такой, который будет уникален
      // в качестве имени временной папки (как правило, сразу будем получать уникальный).
      do
      {
        resultPath = Path.Combine(tempPath, Guid.NewGuid().ToString());
      }
      while (Directory.Exists(resultPath));
      return resultPath;
    }

    /// <summary>
    /// Проверить, что таблица существует.
    /// </summary>
    /// <param name="tableName">Имя таблицы.</param>
    /// <param name="connection">Соединение.</param>
    /// <returns>Истоина, если таблица существует.</returns>
    public static bool TableExists(string tableName, SqlConnection connection)
    {
      const string CheckTableExistsCommandTextTemplate = "select object_id('{0}')";
      const int TableIdFieldIndex = 0;
      using (var command = connection.CreateCommand())
      {
        command.CommandText = string.Format(CheckTableExistsCommandTextTemplate, tableName);
        using (var reader = command.ExecuteReader())
        {
          return reader.Read() && !reader.IsDBNull(TableIdFieldIndex);
        }
      }
    }
  }
}
