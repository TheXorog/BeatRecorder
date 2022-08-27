rmdir /S /Q BeatRecorder\bin\Release
dotnet publish -p:PublishSingleFile=true -c Release -p:DebugType=None -p:DebugSymbols=false
explorer BeatRecorder\bin\Release\net6.0-windows\win-x64\publish