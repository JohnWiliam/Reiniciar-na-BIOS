# Script de Compilação para Reiniciar-na-BIOS
# Requer PowerShell 7.5+ e .NET 10 SDK

$projectName = "Bios Reboot"
$projectPath = "./Bios-Reboot/Bios Reboot.csproj"
$outputDir = "./Publish"

Write-Host "--- Iniciando processo de build: $projectName ---" -ForegroundColor Cyan

# 1. Limpeza de builds anteriores
if (Test-Path $outputDir) {
    Write-Host "Limpando diretório de saída..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $outputDir
}

# 2. Restauração de dependências NuGet
Write-Host "Restaurando pacotes NuGet..." -ForegroundColor Blue
dotnet restore $projectPath

# 3. Compilação e Publicação
# Utiliza as configurações definidas no .csproj: SingleFile, SelfContained e Win-x64
Write-Host "Compilando e Gerando Executável Único (Release)..." -ForegroundColor Green
dotnet publish $projectPath `
    -c Release `
    -o $outputDir `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishReadyToRun=true

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n--- Sucesso! ---" -ForegroundColor Green
    Write-Host "O executável foi gerado em: $outputDir"
} else {
    Write-Host "`n--- Erro na Compilação ---" -ForegroundColor Red
    exit $LASTEXITCODE
}
