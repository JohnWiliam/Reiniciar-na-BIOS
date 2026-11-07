# Script de Compilacao Otimizado para Bios-Reboot
# Compativel com Windows PowerShell 5.1 e PowerShell 7.5+ (.NET 10 SDK)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Garante exibicao correta de acentos no console em qualquer versao do PowerShell
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

function Exit-WithPause {
    param([int]$ExitCode = 0)
    Write-Host ""
    Read-Host "Pressione ENTER para fechar esta janela"
    exit $ExitCode
}

try {
    $scriptRoot = $PSScriptRoot
    if ([string]::IsNullOrWhiteSpace($scriptRoot)) {
        $scriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
    }

    $projectName = "Bios Reboot"
    $projectFile = "Bios Reboot.csproj"
    $projectDir  = Join-Path -Path $scriptRoot -ChildPath "Bios-Reboot"
    $projectPath = Join-Path -Path $projectDir -ChildPath $projectFile
    $outputDir   = Join-Path -Path $scriptRoot -ChildPath "Publish"

    # Remove bloqueio NTFS (Mark of the Web) se o ZIP veio da internet
    Get-ChildItem -Path $scriptRoot -Recurse -File -ErrorAction SilentlyContinue |
        Unblock-File -ErrorAction SilentlyContinue

    Clear-Host
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "   BUILD SYSTEM: $projectName" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan

    # Verificacao Basica do Ambiente
    if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
        Write-Host "[ERRO] .NET SDK nao encontrado no PATH." -ForegroundColor Red
        Exit-WithPause 1
    }

    if (-not (Test-Path -LiteralPath $projectPath)) {
        Write-Host "[ERRO] Arquivo de projeto nao encontrado em: $projectPath" -ForegroundColor Red
        Exit-WithPause 1
    }

    # 1. Limpeza
    if (Test-Path -LiteralPath $outputDir) {
        Write-Host "`n[1/3] Limpando diretorio de saida..." -ForegroundColor Yellow
        Remove-Item -LiteralPath $outputDir -Recurse -Force
    } else {
        Write-Host "`n[1/3] Diretorio de saida limpo." -ForegroundColor Yellow
    }

    # 2. Restauracao (com RuntimeIdentifier win-x64 para baixar os pacotes ReadyToRun/CrossGen2)
    Write-Host "`n[2/3] Restaurando pacotes NuGet e Runtime (win-x64)..." -ForegroundColor Blue
    & dotnet restore "$projectPath" `
        -r win-x64 `
        -p:PublishReadyToRun=true `
        -p:PublishSingleFile=true `
        -p:SelfContained=true

    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n[FALHA] Erro ao restaurar pacotes." -ForegroundColor Red
        Exit-WithPause $LASTEXITCODE
    }

    # 3. Compilacao SingleFile
    Write-Host "`n[3/3] Compilando Binario Standalone (Win-x64)..." -ForegroundColor Green
    & dotnet publish "$projectPath" `
        -c Release `
        -o "$outputDir" `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -p:PublishReadyToRun=true `
        -p:DebugType=none `
        -p:DebugSymbols=false

    if ($LASTEXITCODE -eq 0) {
        $exePath = Join-Path -Path $outputDir -ChildPath "$projectName.exe"
        Write-Host "`n==========================================" -ForegroundColor Green
        Write-Host " BUILD SUCESSO" -ForegroundColor Green
        Write-Host " Executavel: $exePath" -ForegroundColor Green
        Write-Host "==========================================" -ForegroundColor Green
        Exit-WithPause 0
    } else {
        Write-Host "`n[FALHA] Erro durante a compilacao (Codigo: $LASTEXITCODE)." -ForegroundColor Red
        Exit-WithPause $LASTEXITCODE
    }
}
catch {
    Write-Host "`n[EXCECAO CRITICA] $($_.Exception.Message)" -ForegroundColor Red
    Exit-WithPause 1
}