param(
    [string]$JwtClave = $env:Jwt__Clave,
    [string]$ConnectionString = $env:ConnectionStrings__Veterinaria
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

if ([string]::IsNullOrWhiteSpace($JwtClave)) {
    $bytes = New-Object byte[] 48
    [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    $JwtClave = [Convert]::ToBase64String($bytes)
    Write-Host "Se genero una clave JWT temporal para esta sesion local."
}
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = "Server=.\SQLEXPRESS;Database=VeterinariaS14DB;Trusted_Connection=True;TrustServerCertificate=True;"
}

$env:Jwt__Clave = $JwtClave
$env:ConnectionStrings__Veterinaria = $ConnectionString

Write-Host "Iniciando Veterinaria.Api (5055), CitasApi (5056) y GatewayApi (5000)..."
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root'; `$env:Jwt__Clave='$JwtClave'; `$env:ConnectionStrings__Veterinaria='$ConnectionString'; dotnet run --project Veterinaria.Api"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root'; `$env:Jwt__Clave='$JwtClave'; dotnet run --project CitasApi"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root'; `$env:Jwt__Clave='$JwtClave'; dotnet run --project GatewayApi"
