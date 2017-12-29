' Скрипт импорта ТМ из папки в БД с фильтрацией по наличию изменений, выполненных текущим пользователем, в TFS

Set shell = CreateObject("WScript.Shell")
Set network = CreateObject("WScript.Network")

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
fromDateFilter = Date()
toDateFilter = Now()
currentUserName = network.UserName
packageType = "routes"

commandLine = _
  "{0}\DevelopmentTransferUtility.exe " & _
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
  "--fromdate ""{10}"" " & _
  "--todate ""{11}"" " & _
  "--userfilter ""{12}"" " & _
  "--type ""{13}"""
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
commandLine = Replace(commandLine, "{10}", fromDateFilter)
commandLine = Replace(commandLine, "{11}", toDateFilter)
commandLine = Replace(commandLine, "{12}", currentUserName)
commandLine = Replace(commandLine, "{13}", packageType)

Call shell.Run(commandLine, 10, True)
