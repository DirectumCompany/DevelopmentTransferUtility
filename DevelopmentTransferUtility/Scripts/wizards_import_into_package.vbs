' Скрипт импорта МД из папки в пакет.

Set shell = CreateObject("WScript.Shell")

packageTransformerPath = "" 
developmentFolderName = ""
developmentPackageFileName = ""
packageType = "wizards"

commandLine = _
  "{0}\DevelopmentTransferUtility.exe " & _
  "--mode import " & _
  "--devfolder ""{1}"" " & _
  "--isx ""{2}"" " & _
  "--type ""{3}"""
commandLine = Replace(commandLine, "{0}", packageTransformerPath)
commandLine = Replace(commandLine, "{1}", developmentFolderName)
commandLine = Replace(commandLine, "{2}", developmentPackageFileName)
commandLine = Replace(commandLine, "{3}", packageType)

Call shell.Run(commandLine, 10, True)
