using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Режим запуска утилиты переноса разработки.
  /// </summary>
  internal enum TransferDevelopmentMode
  {
    /// <summary>
    /// Режим экспорта разработки.
    /// </summary>
    Export,
    /// <summary>
    /// Режим импорта разработки.
    /// </summary>
    Import
  }

  /// <summary>
  /// Класс, отвечающий за запуск утилиты переноса разработки.
  /// </summary>
  internal class TransferDevelopmentRunner
  {
    #region Константы 

    /// <summary>
    /// Имя утилиты переноса разработки.
    /// </summary>
    private const string ExportUtilityExecutableName = "sbdte.exe";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с именем сервера.
    /// </summary>
    private const string ServerCommandLineKey = "-S";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с именем базы данных.
    /// </summary>
    private const string DatabaseCommandLineKey = "-D";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с именем пользователя.
    /// </summary>
    private const string UserNameCommandLineKey = "-N";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с паролем пользователя.
    /// </summary>
    private const string PasswordCommandLineKey = "-W";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с именем файла конфигурации.
    /// </summary>
    private const string ConfigurationFileCommandLineKey = "-C";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с именем файла с разработкой.
    /// </summary>
    private const string PackageFileCommandLineKey = "-F";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с левой границей фильтра по дате.
    /// </summary>
    private const string FromDateFilterCommandLineKey = "-FD";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с правой границей фильтра по дате.
    /// </summary>
    private const string ToDateFilterCommandLineKey = "-TD";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки со значением филтра по имени пользователя.
    /// </summary>
    private const string UserFilterCommandLineKey = "-EL";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки, отвечающий за принудительный показ окна утилиты импорта.
    /// </summary>
    private const string ForceShowWindowCommandKey = "-FS";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с признаком Windows-аутентификации.
    /// </summary>
    private const string WindowsAuthenticationCommandLineKey = "-IsOSAuth=True ";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с признаком режима экспорта.
    /// </summary>
    private const string ExportModeCommandLineKey = "-CT=Export ";
    /// <summary>
    /// Параметр командной строки утилиты переноса разработки с признаком режима импорта.
    /// </summary>
    private const string ImportModeCommandLineKey = "-CT=Import ";

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Параметры командной строки.
    /// </summary>
    public ICommandLineOptions Options { get; set; }

    /// <summary>
    /// Имя файла пакета разработки.
    /// </summary>
    public string DevelopmentPackageFileName { get; set; }

    /// <summary>
    /// Режим запуска утилиты переноса разработки.
    /// </summary>
    private TransferDevelopmentMode TransferDevelopmentMode { get; set; }

    /// <summary>
    /// Полное имя файла утилиты экспорта разработки.
    /// </summary>
    private string ExportUtilityFullFileName
    {
      get
      {
        return Path.Combine(this.Options.ClientPartPath, ExportUtilityExecutableName);
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Сформировать командную строку для запуска.
    /// </summary>
    /// <returns></returns>
    private string BuildCommandLine()
    {
      const string NameValueTemplate = "{0}=\"{1}\" ";
      
      var commandLineBuilder = new StringBuilder();
      commandLineBuilder.AppendFormat(NameValueTemplate, ServerCommandLineKey, this.Options.Server);
      commandLineBuilder.AppendFormat(NameValueTemplate, DatabaseCommandLineKey, this.Options.Database);
      switch (this.Options.AuthType)
      {
        case AuthenticationType.Sql:
          commandLineBuilder.AppendFormat(NameValueTemplate, UserNameCommandLineKey, this.Options.UserName);
          commandLineBuilder.AppendFormat(NameValueTemplate, PasswordCommandLineKey, this.Options.Password);
          break;
        case AuthenticationType.Windows:
          commandLineBuilder.Append(WindowsAuthenticationCommandLineKey);
          break;
      }
      commandLineBuilder.AppendFormat(NameValueTemplate, PackageFileCommandLineKey, this.DevelopmentPackageFileName);
      switch (this.TransferDevelopmentMode)
      {
        case TransferDevelopmentMode.Export:
          commandLineBuilder.Append(ExportModeCommandLineKey);
          break;
        case TransferDevelopmentMode.Import:
          commandLineBuilder.Append(ImportModeCommandLineKey);
          if (!this.Options.HiddenImportMode)
            commandLineBuilder.AppendFormat(ForceShowWindowCommandKey);
          break;
      }

      if (this.TransferDevelopmentMode == TransferDevelopmentMode.Export)
      {
        if (!string.IsNullOrEmpty(this.Options.ConfigurationFileName))
          commandLineBuilder.AppendFormat(NameValueTemplate, ConfigurationFileCommandLineKey,
            this.Options.ConfigurationFileName);
        if (!string.IsNullOrEmpty(this.Options.FromDateFilter))
          commandLineBuilder.AppendFormat(NameValueTemplate, FromDateFilterCommandLineKey, this.Options.FromDateFilter);
        if (!string.IsNullOrEmpty(this.Options.ToDateFilter))
          commandLineBuilder.AppendFormat(NameValueTemplate, ToDateFilterCommandLineKey, this.Options.ToDateFilter);
        if (!string.IsNullOrEmpty(this.Options.UserFilter))
          commandLineBuilder.AppendFormat(NameValueTemplate, UserFilterCommandLineKey, this.Options.UserFilter);
        if (this.Options.SkipAutoAddedElements)
          commandLineBuilder.Append(" -SAE");
      }
      return commandLineBuilder.ToString();
    }

    /// <summary>
    /// Выполнить запуск утилиты.
    /// </summary>
    private void Execute()
    {
      var startInfo = new ProcessStartInfo();
      startInfo.Arguments = this.BuildCommandLine();
      startInfo.FileName = Path.GetFullPath(this.ExportUtilityFullFileName);
      startInfo.WindowStyle = ProcessWindowStyle.Hidden;
      startInfo.CreateNoWindow = true;

      using (var proc = Process.Start(startInfo))
      {
        if (proc == null)
          throw new Exception(Localization.LaunchFailedErrorMessage);

        proc.WaitForExit();

        if (proc.ExitCode != 0)
          throw new Exception(string.Format(Localization.ProcessExitCodeErrorMessage, proc.ExitCode));
      }
    }

    /// <summary>
    /// Экспортировать разработку.
    /// </summary>
    public void Export()
    {
      this.TransferDevelopmentMode = TransferDevelopmentMode.Export;
      this.Execute();
    }

    /// <summary>
    /// Импортировать разработку.
    /// </summary>
    public void Import()
    {
      this.TransferDevelopmentMode = TransferDevelopmentMode.Import;
      this.Execute();
    }

    #endregion
  }
}
