@echo off
:main
set inpFile=%1
	call ".\SRM\SymbTabGen\bin\Debug\SymbTabGen.exe" %inpFile%
	call ".\SRM\Assembler\bin\Debug\Assembler.exe" %inpFile%
	call ".\SRM\Machine\bin\Debug\Machine.exe" %inpFile% /D /Dm 0..16 
exit /b
