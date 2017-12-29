' Скрипт выгрузки разработки по конфигурации.

Set shell = CreateObject("WScript.Shell")

packageTransformerPath = "" 
developmentFolderName = ""
configurationFileName = "" 
clientPartPathName = ""
exportServerName = ""
exportDatabaseName = ""
exportUserName = ""
exportUserPassword = ""
exportUserAuthentication = "Sql" ' Возможные варианты: Sql/Windows

commandLine = _
  """{0}\DevelopmentTransferUtility.exe"" " & _
  "--mode export " & _
  "--skipautoadded " & _ ' Выгружаем строго по конфигурации (без автоматически добавляемых)
  "--devfolder ""{1}"" " & _
  "--isc ""{2}"" " & _
  "--clientpartpath ""{3}"" " & _
  "--server ""{4}"" " & _
  "--database ""{5}"" " & _
  "--username ""{6}"" " & _
  "--password ""{7}"" " & _
  "--authtype ""{8}"""
commandLine = Replace(commandLine, "{0}", packageTransformerPath)
commandLine = Replace(commandLine, "{1}", developmentFolderName)
commandLine = Replace(commandLine, "{2}", configurationFileName)
commandLine = Replace(commandLine, "{3}", clientPartPathName)
commandLine = Replace(commandLine, "{4}", exportServerName)
commandLine = Replace(commandLine, "{5}", exportDatabaseName)
commandLine = Replace(commandLine, "{6}", exportUserName)
commandLine = Replace(commandLine, "{7}", exportUserPassword)
commandLine = Replace(commandLine, "{8}", exportUserAuthentication)

Call shell.Run(commandLine, 10, True)
