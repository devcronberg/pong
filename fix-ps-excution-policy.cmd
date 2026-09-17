@echo off
setlocal

powershell.exe -NoProfile -Command ^
    "try { Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned -Force -ErrorAction Stop; $policy = Get-ExecutionPolicy; if ($policy -notin @('RemoteSigned', 'Unrestricted', 'Bypass')) { throw ('The effective policy is still ' + $policy + '. A Group Policy may be overriding CurrentUser.') }; Write-Host ('PowerShell execution policy is now ' + $policy + '.') -ForegroundColor Green; Write-Host 'Open a new PowerShell window, then run .\publish.ps1.' } catch { Write-Host ('Could not change the execution policy: ' + $_.Exception.Message) -ForegroundColor Red; exit 1 }"

exit /b %errorlevel%