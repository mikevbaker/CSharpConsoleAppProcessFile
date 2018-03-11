@echo off
cls
echo(
echo(
echo Ready to run Bad Data test
pause
cls
ConsoleApplication1.exe File=TestBadData.txt

cls
echo(
echo(
echo Ready to run Reordered Column test
pause
cls
ConsoleApplication1.exe File=TestColumnReorder.txt

cls
echo(
echo(
echo Ready to run Sort By Date test
pause
cls
ConsoleApplication1.exe File=Source.txt SortByStartDate

cls
echo(
echo(
echo Ready to run Filter By Project test
pause
cls
ConsoleApplication1.exe File=Source.txt Project=3

cls
echo(
echo(
echo Ready to run Filter and Sort test
pause
cls
ConsoleApplication1.exe File=Source.txt SortByStartDate Project=6
