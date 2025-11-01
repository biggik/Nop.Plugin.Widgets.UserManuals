@echo off
set sln_file=Nop.Plugin.Widgets.UserManuals

cd %1

cd "%sln_file%"
dotnet restore
dotnet build "%sln_file%.csproj" --configuration=Debug --no-restore

echo Starting Release build...
dotnet build "%sln_file%.csproj" --configuration=Release --no-restore

cd ..
cd ..
