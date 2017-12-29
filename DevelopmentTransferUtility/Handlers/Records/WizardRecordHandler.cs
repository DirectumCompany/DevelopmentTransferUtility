using NpoComputer.DevelopmentTransferUtility.Models.Records;
using NpoComputer.DevelopmentTransferUtility.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Text;

namespace NpoComputer.DevelopmentTransferUtility.Handlers.Records
{
  /// <summary>
  /// Обработчик мастеров действия
  /// </summary>
  internal class WizardRecordHandler : BaseRecordHandler 
  {
    #region Константы

    /// <summary>
    /// Имя справочника групп мастеров действий.
    /// </summary>
    protected const string WizardsGroupReferenceName = "WIZARD_GROUPS";

    /// <summary>
    /// Папка событий.
    /// </summary>
    protected const string EventsOutputFolder = "Events";

    /// <summary>
    /// Имя файла структуры МД.
    /// </summary>
    protected const string StructureFileName = "Structure.dfm";

    #endregion

    #region BaseRecordHandler

    protected override string ComponentsFolderSuffix { get { return "Wizards"; } }

    protected override string ElementNameInGenitive { get { return "мастеров действий"; } }

    protected override string DevelopmentElementsCommandText
    {
      get
      {
        return
          "select MBAnalit." + this.DevelopmentElementKeyFieldName + " " +
          "from MBAnalit " +
          "inner join MBVidAn " +
          "on MBVidAn.Vid = MBAnalit.Vid " +
          "  and MBVidAn.Kod = 'WIZARDS'";
      }
    }

    protected override string DevelopmentElementKeyFieldName { get { return "Kod"; } }

    /// <summary>
    /// Получить модели, соответствующие заданному обработчику.
    /// </summary>
    /// <param name = "rootModel" > Модель.</param>
    /// <returns>Модели записей.</returns>
    protected override List<RecordRefModel> GetComponentModelList(RootModel rootModel)
    {
      return rootModel.Records.FindAll(x => !x.ReferenceName.Equals(WizardsGroupReferenceName));
    }

    /// <summary>
    /// Получить имя корневого тега элемента.
    /// </summary>
    /// <returns>Имя корневого тега элемента.</returns>
    protected override string GetElementName()
    {
        return "Wizard";
    }

    /// <summary>
    /// Обработать экспорт реквизита.
    /// </summary>
    /// <param name="path">Путь к папке с моделью</param>
    /// <param name="requisite">Обрабатываемый реквизит.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected override void ProcessRequisiteExport(string path, RequisiteModel requisite, int detailIndex)
    {
      if ((requisite.Name == "ISBSearchCondition") && !(string.IsNullOrWhiteSpace(requisite.DecodedValue)))
      {
        var decodedText = DfmConverter.Convert(requisite.DecodedValue);
        ExportTextToFile(Path.Combine(path, StructureFileName), decodedText);
      }
    }

    /// <summary>
    /// Проверить, нужно ли удалить реквизит.
    /// </summary>
    /// <param name="requisites">Исходный список реквизитов.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    /// <returns>Признак того, что нужно удалять реквизит.</returns>
    protected override List<string> GetRequisitesToRemove(List<RequisiteModel> requisites, int detailIndex = 0)
    {
      var result = base.GetRequisitesToRemove(requisites, detailIndex);

      if (detailIndex == 0)
      {
        result.Add("ISBSearchCondition");
      }

      return result;
    }

    /// <summary>
    /// Импортировать реквизиты, которые надо импортировать.
    /// </summary>
    /// <param name="path">Путь к папке с моделью.</param>
    /// <param name="requisites">Список реквизитов.</param>
    /// <param name="detailIndex">Индекс детального раздела.</param>
    protected override void ImportRequisites(string path, List<RequisiteModel> requisites, int detailIndex = 0)
    {
      var commentRequisite = RequisiteModel.CreateFromFile("ISBSearchCondition", Path.Combine(path, StructureFileName));
      commentRequisite.DecodedValue = DfmConverter.DeConvert(commentRequisite.DecodedValue);
      requisites.Add(commentRequisite);
    }

    /// <summary>
    /// Обработать импорт моделей (внутренний метод).
    /// </summary>
    /// <param name="rootModel">Модель пакета разработки.</param>
    /// <param name="importFilter">Фильтр импорта.</param>
    /// <param name="importedCount">Количество импортированных элементов.</param>
    protected override void InternalHandleImport(RootModel rootModel, ImportFilter importFilter, out int importedCount)
    {
      importedCount = 0;
      var serializer = this.CreateSerializer();
      var models = rootModel.Records;

      if (!Directory.Exists(this.ComponentsFolder))
        return;

      foreach (var componentFolder in Directory.EnumerateDirectories(this.ComponentsFolder))
        if (importFilter.NeedImport(componentFolder, this.DevelopmentPath))
        {
          models.Add(this.HandleImportModel(componentFolder, serializer));
          importedCount++;
        }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="developmentPath">Папка с разработкой.</param>
    public WizardRecordHandler(string developmentPath) : base(developmentPath)
    {
    }

    #endregion
  }

  /// <summary>
  /// Конвертер Dfm. Преобразовывает Dfm перед выгрузкой разработки.
  /// </summary>
  internal class DfmConverter
  {
    #region Converter

    /// <summary>
    /// Шаблон регулярного выражения.
    /// </summary>
    protected const string RegExPattern = "(#[\\d]+|'[^']+'|\\ \\+\\r\\n\\ *)";

    private static readonly char[] RussianSymbols =
    {
      'ё','й','ц','у', 'к', 'е', 'н', 'г', 'ш', 'щ', 'з', 'х', 'ъ',
      'ф','ы','в','а','п','р','о','л','д','ж','э',
      'я','ч','с','м','и','т','ь','б','ю',
      '№','»','«','…','•','“','”'
    };

    internal static string Convert(string decodedValue)
    {
      Regex regex = new Regex(RegExPattern);
      return regex.Replace(decodedValue, SymbolDecoding);
    }

    internal static string DeConvert(string decodedValue)
    {
      var data = decodedValue.Split(new[] { "\n\r", "\r\n", "\n", "\r" }, StringSplitOptions.None).ToList();
      data = ReplaceQuotes(data);
      data = ReplaceRussianSymbols(data);
      data = AddEnterSymbolsInText(data);
      data = Split(data);
      data = data.Where(x => !string.IsNullOrEmpty(x)).Select(x => x + "\r\n").ToList();
      return string.Join(string.Empty, data);
    }
    private static string SymbolDecoding(Match match)
    {
      int symbolNum;
      if (match.ToString().StartsWith("#") && int.TryParse(match.ToString().Substring(1), out symbolNum))
      {
        return char.ConvertFromUtf32(symbolNum);
      }
      else if (match.ToString().StartsWith("'") && match.ToString().EndsWith("'"))
      {
        return match.ToString();
      }
      else
        return string.Empty;
    }

    /// <summary>
    /// Закодировать кавычки в ISBL коде
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static List<string> ReplaceQuotes(List<string> input)
    {
      var result = new List<string>();
      var builder = new StringBuilder();
      foreach (var line in input)
      {
        var symbols = line.ToCharArray();
        for (var i = 0; i < symbols.Length; ++i)
        {
          if (IsDecodedQuotes(symbols, i))
          {
            builder.Append(GetSharpCode(symbols[i]));
          }
          else
          {
            builder.Append(symbols[i]);
          }
        }
        result.Add(builder.ToString());
        builder.Clear();
      }
      return result;
    }

    private static bool IsDecodedQuotes(char[] symbols, int index)
    {
      if (symbols[index] != '\'') return false;

      if (index - 1 >= 0 && index + 1 < symbols.Length)
        return CheckNeighbor(symbols, index - 1) && CheckNeighbor(symbols, index + 1);

      if (index + 1 >= symbols.Length) return CheckNeighbor(symbols, index - 1);

      if (index - 1 < 0) return CheckNeighbor(symbols, index + 1);

      return false;
    }

    private static bool CheckNeighbor(char[] symbols, int index)
    {
      return (IsRussian(symbols[index]) || symbols[index] == '\'');
    }
    /// <summary>
    /// Закодировать русские символы
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static List<string> ReplaceRussianSymbols(List<string> input)
    {
      return input.Select(x => string.Join(string.Empty,
                                           x.Select(y =>
                                                    IsRussian(y)
                                                    ? GetSharpCode(y)
                                                    : y.ToString())
                                             .ToArray()))
                  .ToList();
    }

    private static string GetSharpCode(char symbol)
    {
      var result = char.ConvertToUtf32(symbol.ToString(), 0);
      return "#" + result;
    }

    public static bool IsRussian(char symbol)
    {
      symbol = char.ToLower(symbol);
      return RussianSymbols.Any(x => x == symbol);
    }

    #endregion

    #region SplitStringsOn64Symbols
    /// <summary>
    /// Разделить текст исходников ISBL на строки длиной 64 символа
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static List<string> Split(List<string> input)
    {
      var result = new List<string>(input.Count);
      for (var i = 0; i < input.Count; ++i)
      {
        string start = null;
        var index = -1;
        InitializeStart(input, i, ref index, ref start);

        if (!string.IsNullOrEmpty(start))
        {
          result.Add(start);
          var spaceCount = input[i].TakeWhile(x => x == ' ').Count();

          var forWork = new List<string>();
          var workStr = input[i].Substring(index);
          forWork.Add(workStr);
          ++i;
          while (input[i].StartsWith("#") || input[i].StartsWith("'"))
          {
            forWork.Add(input[i]);
            ++i;
          }
          var forAdd = CutOnLength(forWork);

          if (!string.IsNullOrEmpty(workStr))
          {
            if (forAdd.Count == 0) result[result.Count - 1] += workStr;
            else
            {
              result[result.Count - 1] += forAdd.First();
              forAdd = forAdd.Skip(1).ToList();
              AddSpace(forAdd, spaceCount);
              result.AddRange(forAdd);
            }
          }
          --i;
          continue;
        }

        result.Add(input[i]);
      }
      return result;
    }

    private static void InitializeStart(List<string> input, int i, ref int index, ref string start)
    {
      if (!input[i].Contains("#") && !input[i].Contains("'"))
        return;
      index = new[] {input[i].IndexOf("#") - 1, input[i].IndexOf("'") - 1}.Where(x => x > 0).Min();
      start = input[i].Substring(0, index);
    }

    private static void AddSpace(List<string> result, int spaceCount)
    {
      if (result.Count <= 0)
        return;
      for (var i = 0; i < result.Count; ++i)
      {
        result[i] = result[i].PadLeft(result[i].Length + spaceCount, ' ');
      }
    }

    public static List<string> CutOnLength(List<string> input)
    {
      var result = new List<string>(input.Count);
      var symbols = input.Select(x => x.Replace("''", ""))
                         .SelectMany(x => x)
                         .ToArray();
      var symbolsCount = 0;
      var builder = new StringBuilder(100);
      builder.Append(symbols[0]);
      for (var i = 1; i < symbols.Length; ++i)
      {
        switch (symbols[i])
        {
          case '#':
            i = AddSharpSymbol(builder, symbols, i);
            symbolsCount++;
            --i;
            break;
          case '\'':
            if (builder.Length != 0)
            {
              builder.Append(symbols[i]);
            }
            break;
          default:
            if (builder.Length == 0)
              builder.Append('\'');
            builder.Append(symbols[i]);
            symbolsCount++;
            break;
        }

        if (symbolsCount != 64)
          continue;
        symbolsCount = 0;
        GoToNewLine(builder, symbols, i, result);
      }
      if (builder.Length > 0)
      {
        result.Add(builder.ToString());
      }
      return result;
    }

    private static void GoToNewLine(StringBuilder builder, char[] symbols, int i, List<string> result)
    {
      if (CheckEndSymbol(symbols, i))
      {
        builder.Append('\'');
      }
      if (i + 2 < symbols.Length)
        builder.Append(" +");
      result.Add(builder.ToString());
      builder.Clear();
    }

    public static bool CheckEndSymbol(char[] symbols, int index)
    {
      var left = Math.Max(0, index - 4);
      var needQuotes = symbols[index] != '\'';
      for (var i = index; i >= left; --i)
      {
        if (char.IsNumber(symbols[i]) && i > left)
          continue;
        return symbols[i] != '#' && needQuotes;
      }
      return false;
    }

    private static int AddSharpSymbol(StringBuilder builder, char[] symbols, int i)
    {
      var sharpCount = 0;
      builder.Append(symbols[i]);
      ++i;
      while (i < symbols.Length && sharpCount < 4 && char.IsNumber(symbols[i]))
      {
        builder.Append(symbols[i]);
        ++sharpCount;
        ++i;
      }
      return i;
    }
    #endregion

    #region AddEnterSymbols
    /// <summary>
    /// Добавить закодированные символы переноса строк в ISBL коде
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static List<string> AddEnterSymbolsInText(List<string> input)
    {
      var result = new List<string>(input.Count);
      for (var i = 0; i < input.Count - 1; ++i)
      {
        if (input[i].Contains("ValueList.Strings = ("))
        {
          i = SkipFunction(input, i, result);
        }

        if (input[i].Contains("#") || input[i].Contains("\'") || input[i].TrimStart(' ') == string.Empty)
        {
          var checkedLine = input[i + 1].TrimStart(' ');
          if (checkedLine.StartsWith("\'") || checkedLine.StartsWith("#") || checkedLine == string.Empty)
          {
            result.Add(input[i] + "#13#10");
            continue;
          }
        }

        if (input[i].TrimEnd(' ').EndsWith("=") && input[i + 1] == string.Empty)
        {
          result.Add(input[i] + "#13#10");
          continue;
        }
        result.Add(input[i]);
      }
      result.Add(input[input.Count - 1]);
      return result;
    }

    private static int SkipFunction(List<string> input, int i, List<string> result)
    {
      while (!input[i].Contains(")"))
      {
        result.Add(input[i]);
        ++i;
      }
      result.Add(input[i]);
      ++i;
      return i;
    }
    #endregion
  }
}
