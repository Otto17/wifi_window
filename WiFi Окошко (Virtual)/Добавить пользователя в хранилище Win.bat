@echo off

:: Запускать скрипт с правами администратора

:: Устанавливаем модуль
PowerShell -NoProfile -Command "Install-Module -Name CredentialManager -Force -Scope AllUsers"

:: Добавляем пользователя
PowerShell -NoProfile -Command "New-StoredCredential -Target 'WiFi_Window' -Username 'User:222' -Password '12345678' -Persist LocalMachine"

pause
