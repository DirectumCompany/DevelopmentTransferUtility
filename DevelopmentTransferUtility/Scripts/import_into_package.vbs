' Скрипт импорта разработки из папки в пакет.

Set shell = CreateObject("WScript.Shell")

packageTransformerPath = "" 
developmentFolderName = ""
developmentPackageFileName = ""

commandLine = _
  """{0}\DevelopmentTransferUtility.exe"" " & _
  "--mode import " & _
  "--devfolder ""{1}"" " & _
  "--isx ""{2}"""
commandLine = Replace(commandLine, "{0}", packageTransformerPath)
commandLine = Replace(commandLine, "{1}", developmentFolderName)
commandLine = Replace(commandLine, "{2}", developmentPackageFileName)

Call shell.Run(commandLine, 10, True)
