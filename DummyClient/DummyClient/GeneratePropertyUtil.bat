rem PropertyUtil.cs�� �����ϱ� ���� ��ġ�����Դϴ�. ������ ������ ��� PropertyUtil.cs�� ������Ʈ ���� �ʽ��ϴ�.

setlocal

set sdir=%1
set pdir=%2
set datadir=%3
set namespace=%4


"%sdir%ExternLib\TextTemplating\textTransform.exe"  -r "%sdir%..\Bin\Server\IES.dll" -out %pdir%PropertyUtilTemp.cs -a !!XmlPath!"%pdir%..\..\Bin\%datadir%\XML" -a !!Namespace!%namespace%  %sdir%PropertyUtil.tt

if %ERRORLEVEL% NEQ 0 (
	echo PropertyUtil Generation Failed
) else (
	Copy %pdir%PropertyUtilTemp.cs %pdir%PropertyUtil.cs
	del %pdir%PropertyUtilTemp.cs%
)

endlocal