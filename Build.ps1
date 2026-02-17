# Script de Compilação Otimizado para Bios-Reboot
# Requer PowerShell 7.5+ e .NET 10 SDK

$projectName = "Bios Reboot"
# Assegure que o caminho corresponda à estrutura da pasta
$projectFile = "Bios Reboot.csproj" 
$projectPath = Join-Path -Path $PSScriptRoot -ChildPath "Bios-Reboot" | Join-Path -ChildPath $projectFile
$outputDir = Join-Path -Path $PSScriptRoot -ChildPath "Publish"

Clear-Host
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   BUILD SYSTEM: $projectName" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Verificação Básica do Ambiente
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Host "[ERRO] .NET SDK não encontrado no PATH." -ForegroundColor Red
    exit 1
}

# 1. Limpeza
if (Test-Path $outputDir) {
    Write-Host "`n[1/3] Limpando diretório de saída..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $outputDir | Out-Null
} else {
    Write-Host "`n[1/3] Diretório de saída limpo." -ForegroundColor Yellow
}

# 2. Restauração
Write-Host "`n[2/3] Restaurando pacotes NuGet..." -ForegroundColor Blue
dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "[FALHA] Erro ao restaurar pacotes." -ForegroundColor Red
    exit $LASTEXITCODE
}

# 3. Compilação SingleFile
Write-Host "`n[3/3] Compilando Binário Standalone (Win-x64)..." -ForegroundColor Green
dotnet publish $projectPath `
    -c Release `
    -o $outputDir `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishReadyToRun=true `
    -p:DebugType=None

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n==========================================" -ForegroundColor Green
    Write-Host " BUILD SUCESSO" -ForegroundColor Green
    Write-Host " Executável: $outputDir\$projectName.exe"
    Write-Host "==========================================" -ForegroundColor Green
} else {
    Write-Host "`n[FALHA] Erro durante a compilação." -ForegroundColor Red
    exit $LASTEXITCODE
}
