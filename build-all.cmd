@echo off
set project=Nop.Plugin.Widgets.UserManuals

REM cd 4.30\%project%
REM Echo building 4.30 of %project%
REM rd ..\_build\release\. /s /q > 0
REM dotnet build %project%.csproj --configuration=Release --no-incremental
rem cd ..\..

cd 4.20\%project%
Echo building 4.20 of %project%
rd ..\_build\release\. /s /q > 0
dotnet build %project%.csproj --configuration=Release --no-incremental

cd ..