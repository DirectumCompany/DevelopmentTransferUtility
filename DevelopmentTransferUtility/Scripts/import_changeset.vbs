' Скрипт импорта разработки из папки в БД с фильтрацией по списку ChangeSet.

Set shell = CreateObject("WScript.Shell")

packageTransformerPath = "" 
developmentFolderName = ""
clientPartPathName = ""	
importServerName = ""
importDatabaseName = ""
importUserName = ""
importUserPassword = ""
importUserAuthentication = "Sql" ' Возможные варианты: Sql/Windows
tfsServer = ""
tfsDevelopmentPath = ""
changesetFilter = ""

commandLine = _
  """{0}\DevelopmentTransferUtility.exe"" " & _
  "--mode import " & _
  "--devfolder ""{1}"" " & _
  "--clientpartpath ""{2}"" " & _
  "--server ""{3}"" " & _
  "--database ""{4}"" " & _
  "--username ""{5}"" " & _
  "--password ""{6}"" " & _
  "--authtype ""{7}"" " & _
  "--tfs ""{8}"" " & _
  "--tfsdevpath ""{9}"" " & _
  "--changesets ""{10}"""
commandLine = Replace(commandLine, "{0}", packageTransformerPath)
commandLine = Replace(commandLine, "{1}", developmentFolderName)
commandLine = Replace(commandLine, "{2}", clientPartPathName)
commandLine = Replace(commandLine, "{3}", importServerName)
commandLine = Replace(commandLine, "{4}", importDatabaseName)
commandLine = Replace(commandLine, "{5}", importUserName)
commandLine = Replace(commandLine, "{6}", importUserPassword)
commandLine = Replace(commandLine, "{7}", importUserAuthentication)
commandLine = Replace(commandLine, "{8}", tfsServer)
commandLine = Replace(commandLine, "{9}", tfsDevelopmentPath)
commandLine = Replace(commandLine, "{10}", changesetFilter)

Call shell.Run(commandLine, 10, True)
