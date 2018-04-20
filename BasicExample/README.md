
From the csproj directory
dotnet publish -c release -r win-x86 -o "Z:\Dropbox\tmp\windows" --self-contained
dotnet publish -c release -r osx-x64 -o "Z:\Dropbox\tmp\mac" --self-contained
dotnet publish -c release -r linux-x64 -o "Z:\Dropbox\tmp\linux" --self-contained

