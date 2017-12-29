using System.Text;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Окружение преобразователя.
  /// </summary>
  internal static class TransformerEnvironment
  {
    #region Константы

    /// <summary>
    /// Кодировка русского пакета.
    /// </summary>
    public const int RussianCodePage = 1251;
    /// <summary>
    /// Кодировка английского пакета.
    /// </summary>
    public const int EnglishCodePage = 1250;

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Текущая кодировка.
    /// </summary>
    private static Encoding currentEncoding = Encoding.GetEncoding(1251);
    /// <summary>
    /// Текущая кодировка.
    /// </summary>
    public static Encoding CurrentEncoding
    {
      get { return currentEncoding; }
      set { currentEncoding = value; }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Вернуть истину, если кодовая страница русская.
    /// </summary>
    public static bool IsRussianCodePage()
    {
      return CurrentEncoding.CodePage == RussianCodePage;
    }

    /// <summary>
    /// Вернуть истину, если кодовая страница английская.
    /// </summary>
    public static bool IsEnglishCodePage()
    {
      return CurrentEncoding.CodePage == EnglishCodePage;
    }
    #endregion
  }
}
