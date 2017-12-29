using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
  /// <summary>
  /// Режим запуска утилиты переноса разработки.
  /// </summary>
  internal enum TransferRecordsMode
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
  /// Класс, отвечающий за запуск сценариев экспорта/импорта ТМ и МД.
  /// </summary>
  internal class TransferRecordsRunner
  {
    #region Константы 

    /// <summary>
    /// Имя исполняемого файла для запуска сценариев.
    /// </summary>
    private const string LauncherName = "SBRTE.exe";
    /// <summary>
    /// Параметр командной строки SBLauncher с именем сервера.
    /// </summary>
    private const string ServerCommandLineKey = "-S";
    /// <summary>
    /// Параметр командной строки SBLauncher с именем базы данных.
    /// </summary>
    private const string DatabaseCommandLineKey = "-D";
    /// <summary>
    /// Параметр командной строки SBLauncher с именем пользователя.
    /// </summary>
    private const string UserNameCommandLineKey = "-N";
    /// <summary>
    /// Параметр командной строки SBLauncher с паролем пользователя.
    /// </summary>
    private const string PasswordCommandLineKey = "-W";
    /// <summary>
    /// Параметр командной строки SBLauncher с параметрами фильтра.
    /// </summary>
    private const string FilterCommandLineKey = "-R";
    /// <summary>
    /// Параметр командной строки SBLauncher с именем сценария экспорта.
    /// </summary>
    private const string ScriptNameCommandLineKey = "-F";
    /// <summary>
    /// Имя сценария экспорта типовых маршрутов.
    /// </summary>
    private const string RoutesExportScriptName = "Экспорт типовых маршрутов";
    /// <summary>
    /// Имя сценария экспорта мастеров действий.
    /// </summary>
    private const string WizardsExportScriptName = "ExportWizard";
    /// <summary>
    /// Имя сценария экспорта типовых маршрутов.
    /// </summary>
    private const string RoutesImportScriptName = "Импорт типовых маршрутов";
    /// <summary>
    /// Имя сценария экспорта мастеров действий.
    /// </summary>
    private const string WizardsImportScriptName = "ImportWizard";
    /// <summary>
    /// Параметр сценария выгрузки с ID типовых маршрутов.
    /// </summary>
    private const string RoutIDsFilterScriptKey = "SRIDs";
    /// <summary>
    /// Параметр командной строки SBLauncher с именем выходного файла с разработкой.
    /// </summary>
    private const string FileNameScriptKey = "FileName";
    /// <summary>
    /// Параметр командной строки SBLauncher с левой границей фильтра по дате.
    /// </summary>
    private const string FromDateFilterScriptKey = "FirstDate";
    /// <summary>
    /// Параметр командной строки SBLauncher с правой границей фильтра по дате.
    /// </summary>
    private const string ToDateFilterScriptKey = "SecondDate";
    /// <summary>
    /// Параметр командной строки SBLauncher со значением филтра по имени пользователя.
    /// </summary>
    private const string UserFilterScriptKey = "UserLogin";
    /// <summary>
    /// Параметр командной строки SBLauncher с признаком Windows-аутентификации.
    /// </summary>
    private const string WindowsAuthenticationCommandLineKey = "-IsOSAuth=True ";
    /// <summary>
    /// Параметр командной строки SBLauncher для запуска сценария.
    /// </summary>
    private const string ScriptLaunchCommandLineKey = "-CT=Script ";

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Параметры командной строки.
    /// </summary>
    public ICommandLineOptions Options { get; set; }

    /// <summary>
    /// Режим запуска утилиты переноса разработки.
    /// </summary>
    private TransferRecordsMode TransferRecordsMode { get; set; }

    /// <summary>
    /// Имя выходного файла с разработкой.
    /// </summary>
    public string OutputFileName { get; set; }

    /// <summary>
    /// Полное имя файла утилиты экспорта разработки.
    /// </summary>
    private string SBLauncherFullFileName
    {
      get
      {
        return Path.Combine(this.Options.ClientPartPath, LauncherName);
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
      const string ScriptParamsTemplate = "{0}={1}";
      List<string> scriptParamsValue = new List<string>();

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

      switch (this.Options.Type)
      {
        default:
        case "standard":
          throw new Exception(Localization.WrongFileType);
        case "wizards":
          if (this.TransferRecordsMode == TransferRecordsMode.Export)
          {
            commandLineBuilder.AppendFormat(NameValueTemplate, ScriptNameCommandLineKey, WizardsExportScriptName);
          }
          else
          {
            commandLineBuilder.AppendFormat(NameValueTemplate, ScriptNameCommandLineKey, WizardsImportScriptName);
          }
          break;
        case "routes":
          if (this.TransferRecordsMode == TransferRecordsMode.Export)
          {
            commandLineBuilder.AppendFormat(NameValueTemplate, ScriptNameCommandLineKey, RoutesExportScriptName);
          }
          else
          {
            commandLineBuilder.AppendFormat(NameValueTemplate, ScriptNameCommandLineKey, RoutesImportScriptName);
          }
          break;
      }
      scriptParamsValue.Add(string.Format(ScriptParamsTemplate, FileNameScriptKey, this.OutputFileName));
      commandLineBuilder.Append(ScriptLaunchCommandLineKey);
      if (this.TransferRecordsMode == TransferRecordsMode.Export)
      {
        if (!string.IsNullOrEmpty(this.Options.FromDateFilter))
          scriptParamsValue.Add(string.Format(ScriptParamsTemplate, FromDateFilterScriptKey, Convert.ToDateTime(this.Options.FromDateFilter).ToShortDateString()));
        if (!string.IsNullOrEmpty(this.Options.ToDateFilter))
          scriptParamsValue.Add(string.Format(ScriptParamsTemplate, ToDateFilterScriptKey, Convert.ToDateTime(this.Options.ToDateFilter).ToShortDateString()));
        if (!string.IsNullOrEmpty(this.Options.UserFilter))
          scriptParamsValue.Add(string.Format(ScriptParamsTemplate, UserFilterScriptKey, this.Options.UserFilter));
        if (!string.IsNullOrEmpty(this.Options.RouteIDsFilter))
          scriptParamsValue.Add(string.Format(ScriptParamsTemplate, RoutIDsFilterScriptKey, this.Options.RouteIDsFilter));
      }
      commandLineBuilder.AppendFormat(NameValueTemplate, FilterCommandLineKey, string.Join("|", scriptParamsValue));
      return commandLineBuilder.ToString();
    }

    /// <summary>
    /// Выполнить запуск утилиты.
    /// </summary>
    public void Execute()
    {
      var startInfo = new ProcessStartInfo();
      startInfo.Arguments = this.BuildCommandLine();
      startInfo.FileName = Path.GetFullPath(this.SBLauncherFullFileName);
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
      this.TransferRecordsMode = TransferRecordsMode.Export;
      this.Execute();
    }

    /// <summary>
    /// Импортировать разработку.
    /// </summary>
    public void Import()
    {
      this.TransferRecordsMode = TransferRecordsMode.Import;
      this.Execute();
    }

    #endregion
  }
}