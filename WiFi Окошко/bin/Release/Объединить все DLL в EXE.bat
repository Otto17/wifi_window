@echo off
setlocal enabledelayedexpansion

:: ���� � ILMerge.exe
set ILMERGE="%USERPROFILE%\.nuget\packages\ilmerge\3.0.41\tools\net452\ILMerge.exe"

:: ��� ��室���� �ᯮ��塞��� 䠩��
set TARGET="WiFi ���誮.exe"

:: �६����� ��� ��� ��室���� 䠩��
set TEMP_TARGET="basic - WiFi ���誮.exe"

:: ��� ��室���� 䠩�� (�㤥� ⠪�� ��, ��� � ��室��)
set OUTPUT=%TARGET%

:: ��२��������� ��室���� 䠩��
if exist %TARGET% (
    ren %TARGET% %TEMP_TARGET%
) else (
    echo ��室�� 䠩� %TARGET% �� ������.
    goto :end
)

:: ������� ��ப� � ����᫥���� ��� DLL 䠩��� � ⥪�饩 ��४�ਨ
set DLLS=
for %%f in (*.dll) do (
    set DLLS=!DLLS! %%f
)

:: ����� ILMerge � ��ࠬ��ࠬ� (/target:exe - ��� ���᮫��� �ணࠬ�, /target:winexe - ��� ���⮯��� �ணࠬ�, ��뢠�� ���᮫�)
%ILMERGE% /target:winexe /out:%OUTPUT% %TEMP_TARGET% %DLLS%

:: �஢�ઠ �ᯥ譮�� ᫨ﭨ�
if %errorlevel% neq 0 (
    echo �訡�� �� ᫨ﭨ� 䠩���. �⪠�뢠�� ���������.
    ren %TEMP_TARGET% %TARGET%
    goto :end
)

:: �������� �६������ 䠩�� ��᫥ �ᯥ譮�� ᫨ﭨ�
if exist %TEMP_TARGET% (
    del %TEMP_TARGET%
)

echo ���ﭨ� �����襭�. ������ 䠩�: %OUTPUT%

:: �������� 䠩��� � ��।���묨 ���७�ﬨ
for %%e in (.dll .xml .application .config .manifest .pdb) do (
    for %%F in (*%%e) do (
        del "%%F"
    )
)

:: �������� ����� app.publish
if exist "app.publish" (
    rmdir /s /q "app.publish"
)

:end
:: pause