using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Клас-обработчик удаления элементов разработки.
  /// </summary>
  internal class DeleteProcessor
  {
    /// <summary>
    /// Обработать удаление элементов разработки.
    /// </summary>
    /// <param name="connectionParams">Параметры соединения.</param>
    /// <param name="developmentElementsCommandText">Текст SQL-запроса на получение элементов разработки.</param>
    /// <param name="developmentElementKeyFieldName">Ключевое поле элемента разработки.</param>
    /// <param name="developmentElementTableName">Имя таблицы элемента разработки.</param>
    /// <param name="needCheckTableExists">Признак необходимости проверять существование таблицы.</param>
    /// <param name="developmentElementsFolder">Папка с элементами разработки.</param>
    /// <param name="deletedCount">Количество удаленных элементов.</param>
    public static void ProcessDelete(ConnectionParams connectionParams, string developmentElementsCommandText,
      string developmentElementKeyFieldName, string developmentElementTableName, bool needCheckTableExists, string developmentElementsFolder, out int deletedCount)
    {
      deletedCount = 0;
      using (var connection = ConnectionManager.GetConnection(connectionParams))
      {
        connection.Open();
        if (!needCheckTableExists || Utils.TableExists(developmentElementTableName, connection))
        {
          var adapter = new SqlDataAdapter(developmentElementsCommandText, connection);
          var dataSet = new DataSet();
          adapter.Fill(dataSet);
          var developmentElementsTable = dataSet.Tables[0];

          if (Directory.Exists(developmentElementsFolder))
          {
            var directoryItems = new List<string>(Directory.EnumerateDirectories(developmentElementsFolder));
            foreach (var directoryFullName in directoryItems)
            {
              var directoryName = new DirectoryInfo(directoryFullName).Name;
              var query =
                from developmentElement in developmentElementsTable.AsEnumerable()
                where
                  Utils.EscapeFilePath((string) developmentElement[developmentElementKeyFieldName])
                    .Trim()
                    .Equals(directoryName, StringComparison.CurrentCultureIgnoreCase)
                select 1;
              if (!query.Any())
              {
                Directory.Delete(directoryFullName, true);
                deletedCount++;
              }
            }
          }
        }
      }
    }
  }
}
