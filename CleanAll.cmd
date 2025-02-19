
REM Cleanup all intermediate files from Visual Studio

attrib -h "*.suo"
del "*.suo"
del "Compiled\*.pdb"
del "Compiled\*.ilk"
del "Compiled\*.vshost.exe"
del "Compiled\*.vshost.exe.manifest"

rmdir "Sourcecode\obj" /S /Q
rmdir "Sourcecode\bin" /S /Q
rmdir "Sourcecode\Debug" /S /Q
