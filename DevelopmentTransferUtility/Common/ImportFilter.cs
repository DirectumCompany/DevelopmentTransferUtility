using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Компаратор Changeset.
  /// </summary>
  internal class ChangesetEqualityComparer : IEqualityComparer<Changeset>
  {
    #region IEqualityComparer<Changeset>  

    public bool Equals(Changeset x, Changeset y)
    {
      return x.ChangesetId == y.ChangesetId;
    }

    public int GetHashCode(Changeset obj)
    {
      return obj.ChangesetId;
    }

    #endregion
  }

  /// <summary>
  /// Класс фильтра при импорте.
  /// </summary>
  internal class ImportFilter
  {
    #region Константы

    /// <summary>
    /// Количество запрашиваемых ChangeSet для обработки.
    /// </summary>
    private const int ProcessedChangeSetCount = 500;

    /// <summary>
    /// Количество запрашиваемых изменений для обработки.
    /// </summary>
    private const int ProcessedChangeCount = 1000;

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Список подпутей к папкам компонент, включенных в фильтр.
    /// Пример подпути: ReferenceTypes\код_типа_справочника
    /// </summary>
    private readonly List<string> includedComponentSubpaths;

    #endregion

    #region Методы

    /// <summary>
    /// Извлечь подпуть к компоненте.
    /// </summary>
    /// <param name="componentServerPath">Полный серверный путь к папке компоненты.</param>
    /// <param name="developmentServerPath">Серверный путь к папке с разработкой.</param>
    /// <returns>Подпуть к папке компоненты.</returns>
    private string ExtractComponentSubpath(string componentServerPath, string developmentServerPath)
    {
      if (!componentServerPath.StartsWith(developmentServerPath))
        return null;

      if (componentServerPath.Length <= developmentServerPath.Length)
        return null;
      var subpath = componentServerPath.Substring(developmentServerPath.Length + 1);
      subpath = subpath.Replace("/", "\\");

      var subpathParts = subpath.Split('\\');
      if (subpathParts.Length < 2)
        return null;
      return $"{subpathParts[0]}\\{subpathParts[1]}";
    }

    /// <summary>
    /// Распарсить строку с датой и временем.
    /// </summary>
    /// <param name="dateTimeString">Строка с датой и временем</param>
    /// <returns>Результат парсинга строки с датой и временем.</returns>
    private DateTime? ParseDateTime(string dateTimeString)
    {
      if (string.IsNullOrEmpty(dateTimeString))
        return null;
      else
        return DateTime.Parse(dateTimeString);
    }

    /// <summary>
    /// Создать спецификацию версии по номеру ChangeSet.
    /// </summary>
    /// <param name="changeSetId">Номер ChangeSet.</param>
    /// <returns>Спецификация версии.</returns>
    private VersionSpec CreateVersionSpecForChangeset(int changeSetId)
    {
      return VersionSpec.ParseSingleSpec($"C{changeSetId}", string.Empty);
    }

    /// <summary>
    /// Извлечь все изменения из ChangeSet.
    /// </summary>
    /// <param name="changeset">Исходный ChangeSet.</param>
    /// <param name="versionControlServer">Класс для доступа к TFS.</param>
    /// <returns>Список изменений.</returns>
    private List<Change> GetChangesFrom(Changeset changeset, VersionControlServer versionControlServer)
    {
      var result = new List<Change>();
      Change[] changes = null;
      do
      {
        ItemSpec lastItemSpec;
        if (changes == null)
          lastItemSpec = null;
        else
        {
          var lastChange = changes[changes.Length - 1];
          lastItemSpec = new ItemSpec(lastChange.Item.ServerItem, RecursionType.None);
        }
        changes = versionControlServer.GetChangesForChangeset(changeset.ChangesetId, false, ProcessedChangeCount, lastItemSpec);
        result.AddRange(changes);
      } while (changes.Length > 0);
      return result;
    }

    /// <summary>
    /// Распарсить список номеров ChangeSet.
    /// </summary>
    /// <param name="changesets">Строка с номерами ChangeSet через запятую.</param>
    /// <returns>Список номеров ChangeSet.</returns>
    private List<int> GetChangeSetIds(string changesets)
    {
      if (string.IsNullOrEmpty(changesets))
        return null;
      return changesets.Split(',').Select(int.Parse).ToList();
    }

    /// <summary>
    /// Получить список ChangeSet, отфильтрованный по заданным номерам ChangeSet.
    /// </summary>
    /// <param name="versionControlServer">Класс для доступа к TFS.</param>
    /// <param name="tfsDevelopmentPath">Путь к папке с разработкой в TFS.</param>
    /// <param name="changeSetIds">Список номеров ChangeSet.</param>
    /// <returns>Список ChangeSet, отфильтрованный по заданным номерам ChangeSet.</returns>
    private List<Changeset> GetHistoryFilteredByChangeSets(VersionControlServer versionControlServer,
      string tfsDevelopmentPath, List<int> changeSetIds)
    {
      if (changeSetIds == null)
        return null;
      var result = new List<Changeset>();
      foreach (var changesetId in changeSetIds)
      {
        var changesets = versionControlServer.QueryHistory(tfsDevelopmentPath, VersionSpec.Latest, 0, RecursionType.Full, null,
          this.CreateVersionSpecForChangeset(changesetId), this.CreateVersionSpecForChangeset(changesetId), 1, true, false);
        result.AddRange(changesets.Cast<Changeset>());
      }
      return result;
    }

    /// <summary>
    /// Получить список ChangeSet, отфильтрованный по предикату.
    /// </summary>
    /// <param name="versionControlServer">Класс для доступа к TFS.</param>
    /// <param name="tfsDevelopmentPath">Путь к папке с разработкой в TFS.</param>
    /// <param name="predicate">Предикат.</param>
    /// <returns>Список ChangeSet, отфильтрованный по предикату.</returns>
    private List<Changeset> GetHistoryFilteredByPredicate(VersionControlServer versionControlServer,
      string tfsDevelopmentPath, Predicate<Changeset> predicate)
    {
      var result = new List<Changeset>();
      var changesets = versionControlServer.QueryHistory(tfsDevelopmentPath, VersionSpec.Latest, 0, RecursionType.Full, null,
        null, VersionSpec.Latest, ProcessedChangeSetCount, true, false);
      foreach (Changeset changeset in changesets)
      {
        if (predicate(changeset))
          result.Add(changeset);
      }
      return result;
    }

    /// <summary>
    /// Получить список ChangeSet, отфильтрованный по датам.
    /// </summary>
    /// <param name="versionControlServer">Класс для доступа к TFS.</param>
    /// <param name="tfsDevelopmentPath">Путь к папке с разработкой в TFS.</param>
    /// <param name="fromDate">Левая граница фильтра по дате.</param>
    /// <param name="toDate">Правая граница фильтра по дате.</param>
    /// <returns>Список ChangeSet, отфильтрованный по датам.</returns>
    private List<Changeset> GetHistoryFilteredByDates(VersionControlServer versionControlServer, string tfsDevelopmentPath, DateTime? fromDate,
      DateTime? toDate)
    {
      if (fromDate == null && toDate == null)
        return null;
      return this.GetHistoryFilteredByPredicate(versionControlServer, tfsDevelopmentPath,
        changeset => (fromDate == null || changeset.CreationDate >= fromDate) &&
                     (toDate == null || changeset.CreationDate <= toDate));
    }

    /// <summary>
    /// Получить список ChangeSet, отфильтрованный по имени пользователя.
    /// </summary>
    /// <param name="versionControlServer">Класс для доступа к TFS.</param>
    /// <param name="tfsDevelopmentPath">Путь к папке с разработкой в TFS.</param>
    /// <param name="userName">Имя пользователя.</param>
    /// <returns>Список ChangeSet, отфильтрованный по имени пользователя.</returns>
    private List<Changeset> GetHistoryFilteredByUser(VersionControlServer versionControlServer, string tfsDevelopmentPath, string userName)
    {
      if (userName == null)
        return null;
      return this.GetHistoryFilteredByPredicate(versionControlServer, tfsDevelopmentPath,
        changeset => (changeset.Committer.Equals(userName)));
    }

    /// <summary>
    /// Вычислить пересечение списков с обработкой null.
    /// </summary>
    /// <param name="leftList">Первый список.</param>
    /// <param name="rightList">Второй список.</param>
    /// <returns>Пересечение списков.</returns>
    private List<Changeset> IntersectWithNullsHandled(List<Changeset> leftList, List<Changeset> rightList)
    {
      if (leftList == null)
        return rightList;
      else
      {
        if (rightList == null)
          return leftList;
        else
          return leftList.Intersect(rightList, new ChangesetEqualityComparer()).ToList();
      }
    }

    /// <summary>
    /// Вычислить объединение списокв с обработкой null.
    /// </summary>
    /// <param name="leftList">Первый список.</param>
    /// <param name="rightList">Второй список.</param>
    /// <returns>Пересечение списков.</returns>
    private List<Changeset> UnionWithNullsHandled(List<Changeset> leftList, List<Changeset> rightList)
    {
      if (leftList == null)
        return rightList;
      else
      {
        if (rightList == null)
          return leftList;
        else
          return leftList.Union(rightList, new ChangesetEqualityComparer()).ToList();
      }
    }

    /// <summary>
    /// Сформировать список серверных путей к измененным компонентам на основе списка Changeset.
    /// </summary>
    /// <param name="changesets">Список Changeset.</param>
    /// <param name="versionControlServer">Класс для доступа к TFS.</param>
    /// <param name="tfsDevelopmentPath">Путь к разработке в TFS.</param>
    /// <returns>Список серверных путей к измененным компонентам.</returns>
    private List<string> CreateComponentServerPathsFrom(List<Changeset> changesets, VersionControlServer versionControlServer, string tfsDevelopmentPath)
    {
      var result = new List<string>();
      foreach (var changeset in changesets)
      {
        var changes = this.GetChangesFrom(changeset, versionControlServer);
        foreach (var change in changes)
          if (change.ChangeType.HasFlag(ChangeType.Add) ||
              change.ChangeType.HasFlag(ChangeType.Edit) ||
              change.ChangeType.HasFlag(ChangeType.Merge))
          {
            var subpath = this.ExtractComponentSubpath(change.Item.ServerItem, tfsDevelopmentPath);
            if (!string.IsNullOrEmpty(subpath) && !result.Exists(itempath => itempath.Equals(subpath)))
              result.Add(subpath);
          }
      }
      return result;
    }

    /// <summary>
    /// Проверить, нужно ли импортировать компоненту, находящуюся по заданному пути.
    /// </summary>
    /// <param name="componentPath">Путь к файлам компоненты.</param>
    /// <param name="developmentFolder">Путь к папке с разработкой.</param>
    /// <returns>Истина, если компоненту нужно импортировать согласно условиям фильтрации.</returns>
    public bool NeedImport(string componentPath, string developmentFolder)
    {
      if (this.includedComponentSubpaths == null)
        return true;
      if (!componentPath.StartsWith(developmentFolder))
        return false;

      var componentSubpath = componentPath.Substring(developmentFolder.Length + 1);
      return this.includedComponentSubpaths.Exists(subpath => subpath.Equals(componentSubpath, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="tfsCollection">Адрес коллекции TFS-сервера.</param>
    /// <param name="tfsDevelopmentPath">Путь к разработке в TFS.</param>
    /// <param name="changesets">Список номеров Changeset через запятую.</param>
    /// <param name="fromDate">Левая граница фильтра по дате.</param>
    /// <param name="toDate">Правая граница фильтра по дате.</param>
    /// <param name="userName">Фильтр по имени пользователя.</param>
    public ImportFilter(string tfsCollection, string tfsDevelopmentPath, string changesets, string fromDate, string toDate,
      string userName)
    {
      var teamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(tfsCollection));
      var versionServer = teamProjectCollection.GetService<VersionControlServer>();

      var historyFilteredByChangeSets = this.GetHistoryFilteredByChangeSets(versionServer, tfsDevelopmentPath, this.GetChangeSetIds(changesets));
      var historyFilteredByDates = this.GetHistoryFilteredByDates(versionServer, tfsDevelopmentPath, this.ParseDateTime(fromDate), this.ParseDateTime(toDate));
      var historyFilteredByUser = this.GetHistoryFilteredByUser(versionServer, tfsDevelopmentPath, $"{Environment.UserDomainName}\\{userName}");
      var historyFilteredByDatesAndUser = this.IntersectWithNullsHandled(historyFilteredByDates, historyFilteredByUser);
      var includedChangeSets = this.UnionWithNullsHandled(historyFilteredByChangeSets, historyFilteredByDatesAndUser);

      this.includedComponentSubpaths = this.CreateComponentServerPathsFrom(includedChangeSets, versionServer, tfsDevelopmentPath);
    }

    /// <summary>
    /// Конструктор по списку импортируемых папок.
    /// </summary>
    /// <param name="includedFolders">Список импортируемых папок через запятую.</param>
    public ImportFilter(string includedFolders)
    {
      this.includedComponentSubpaths = new List<string>();
      this.includedComponentSubpaths.AddRange(includedFolders.Split(','));
    }

    /// <summary>
    /// Конструктор пустого фильтра.
    /// </summary>
    public ImportFilter()
    {
    }

    #endregion
  }
}
