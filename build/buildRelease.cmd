set upackage=..\..\updateservice\src\UPackage\bin\Debug\UPackage.exe

call "c:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\VSVARS32.bat"

msbuild ..\Imb.sln /t:clean
msbuild ..\Imb.sln /t:Rebuild /p:Configuration=Release

if NOT EXIST "Imb %imbversion%" md "Imb %imbversion%"

%upackage% imb Imb %imbversion% ..\imb\bin\release "Imb %imbversion%\imb_%imbversion%.pkg"

for %%i in ("Imb %imbversion%\*") do git add "%%i" -f

echo "About to commit, unless you Ctrl-break"

pause
pause

git commit -m "Added a build."
pause
:exit