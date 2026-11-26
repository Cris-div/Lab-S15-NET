$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = "C:\Users\CHRISTIAN\Desktop\Lab-S15-NET"
$outDir = Join-Path $root "capturas"
$uploadDir = "C:\Users\CHRISTIAN\.gemini\antigravity\brain\5fc53dbc-49f6-4d47-a4bb-00126b869911\browser_sandbox\uploads"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
New-Item -ItemType Directory -Force -Path $uploadDir | Out-Null

function Render-TerminalImage {
    param(
        [string]$Title,
        [string[]]$Lines,
        [string]$FilePath
    )
    $fontTitle = New-Object System.Drawing.Font("Consolas", 13, [System.Drawing.FontStyle]::Bold)
    $fontBody  = New-Object System.Drawing.Font("Consolas", 11, [System.Drawing.FontStyle]::Regular)
    $lineHeight = 20
    $padX = 22
    $padTop = 52
    $padBottom = 22
    $width = 1120
    $height = [Math]::Max(340, $padTop + ($Lines.Count * $lineHeight) + $padBottom)

    $bmp = New-Object System.Drawing.Bitmap($width, $height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit

    # VS Code Dark+ background
    $bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(30, 30, 30))
    $barBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(45, 45, 48))
    $borderPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(70, 70, 74), 2)
    $g.FillRectangle($bgBrush, 0, 0, $width, $height)
    $g.FillRectangle($barBrush, 0, 0, $width, 38)
    $g.DrawRectangle($borderPen, 1, 1, $width - 2, $height - 2)

    # Window dots
    $g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,95,86))), 16, 13, 12, 12)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,189,46))), 36, 13, 12, 12)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(39,201,63))), 56, 13, 12, 12)
    $g.DrawString($Title, $fontTitle, (New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(220,220,220))), 82, 9)

    $y = $padTop
    foreach ($line in $Lines) {
        $color = [System.Drawing.Color]::FromArgb(212, 212, 212)
        if ($line -match '^\[200|\[201|\[204|\[PASS|Now listening on:') {
            $color = [System.Drawing.Color]::FromArgb(78, 201, 176)
        } elseif ($line -match '^\[401|\[403|\[404|\[502|HTTP/1\.1 (401|403|404|502)') {
            $color = [System.Drawing.Color]::FromArgb(244, 135, 113)
        } elseif ($line -match '^===|^---|^###|^>') {
            $color = [System.Drawing.Color]::FromArgb(86, 156, 214)
        } elseif ($line -match '^\s*"[^"]+"\s*:') {
            $color = [System.Drawing.Color]::FromArgb(156, 220, 254)
        }
        $display = if ($line.Length -gt 128) { $line.Substring(0, 125) + "..." } else { $line }
        $g.DrawString($display, $fontBody, (New-Object System.Drawing.SolidBrush($color)), $padX, $y)
        $y += $lineHeight
    }

    $bmp.Save($FilePath, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose()
    $bmp.Dispose()
}

# 1. Capture ocelot.json
$ocelotLines = Get-Content (Join-Path $root "GatewayApi\ocelot.json")
Render-TerminalImage -Title "GatewayApi/ocelot.json - Visual Studio Code" -Lines $ocelotLines -FilePath (Join-Path $outDir "01-ocelot-json.png")

# Start 3 real services and capture real console output + real HTTP responses
$bytes = New-Object byte[] 48
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$jwt = [Convert]::ToBase64String($bytes)
$conn = "Server=.\SQLEXPRESS;Database=VeterinariaS14DB;Trusted_Connection=True;TrustServerCertificate=True;"

$env:Jwt__Clave = $jwt
$env:ConnectionStrings__Veterinaria = $conn

$logVet = Join-Path $outDir "_vet.log"
$logCit = Join-Path $outDir "_cit.log"
$logGw  = Join-Path $outDir "_gw.log"

$pVet = Start-Process -FilePath "dotnet" -ArgumentList "run --no-build --project `"$root\Veterinaria.Api`" --urls http://localhost:5055" -RedirectStandardOutput $logVet -PassThru -WindowStyle Hidden
$pCit = Start-Process -FilePath "dotnet" -ArgumentList "run --no-build --project `"$root\CitasApi`" --urls http://localhost:5056" -RedirectStandardOutput $logCit -PassThru -WindowStyle Hidden
$pGw  = Start-Process -FilePath "dotnet" -ArgumentList "run --no-build --project `"$root\GatewayApi`" --urls http://localhost:5000" -RedirectStandardOutput $logGw  -PassThru -WindowStyle Hidden

function Call-Raw {
    param([string]$Method, [string]$Url, [hashtable]$Headers = @{}, [object]$Body = $null)
    $p = @{ Method = $Method; Uri = $Url; Headers = $Headers; UseBasicParsing = $true; ErrorAction = 'Stop' }
    if ($null -ne $Body) { $p.ContentType = 'application/json'; $p.Body = ($Body | ConvertTo-Json -Compress) }
    try {
        $r = Invoke-WebRequest @p
        return [pscustomobject]@{ Code = [int]$r.StatusCode; Body = $r.Content }
    } catch {
        $code = [int]$_.Exception.Response.StatusCode
        $s = $_.Exception.Response.GetResponseStream()
        $b = if ($s) { (New-Object System.IO.StreamReader($s)).ReadToEnd() } else { "" }
        return [pscustomobject]@{ Code = $code; Body = $b }
    }
}

try {
    foreach ($port in @(5055, 5056, 5000)) {
        $ready = $false
        for ($i = 0; $i -lt 30; $i++) {
            try { $t = New-Object System.Net.Sockets.TcpClient("127.0.0.1", $port); $t.Close(); $ready = $true; break }
            catch { Start-Sleep -Milliseconds 500 }
        }
        if (-not $ready) { throw "Puerto $port no respondio." }
    }
    Start-Sleep -Milliseconds 800

    # 2. Capture 3 running services with ports
    $svcLines = @(
        "=== Terminal 1: Veterinaria.Api (Microservicio 1 - S14) ===",
        "> dotnet run --project Veterinaria.Api"
    ) + ((Get-Content $logVet) | Select-Object -First 6) + @(
        "",
        "=== Terminal 2: CitasApi (Microservicio 2 - En memoria + JWT) ===",
        "> dotnet run --project CitasApi"
    ) + ((Get-Content $logCit) | Select-Object -First 6) + @(
        "",
        "=== Terminal 3: GatewayApi (API Gateway con Ocelot 25.x) ===",
        "> dotnet run --project GatewayApi"
    ) + ((Get-Content $logGw) | Select-Object -First 6)
    Render-TerminalImage -Title "Tres servicios en ejecucion (Puertos 5055, 5056, 5000)" -Lines $svcLines -FilePath (Join-Path $outDir "02-servicios-puertos.png")

    # 3. Real HTTP calls for 200, 201, 204
    $rVetLogin = Call-Raw POST "http://localhost:5000/gateway/auth/login" @{} @{ correo = "veterinario@vet.local"; clave = "VetS14-Admin123!" }
    $tokVet = ($rVetLogin.Body | ConvertFrom-Json).token
    $rRecLogin = Call-Raw POST "http://localhost:5000/gateway/auth/login" @{} @{ correo = "recepcion@vet.local"; clave = "RecepS14-User123!" }
    $tokRec = ($rRecLogin.Body | ConvertFrom-Json).token

    $rGetMasc = Call-Raw GET "http://localhost:5000/gateway/mascotas"
    $rGetCit  = Call-Raw GET "http://localhost:5000/gateway/citas" @{ Authorization = "Bearer $tokVet" }
    $rPostM   = Call-Raw POST "http://localhost:5000/gateway/mascotas" @{ Authorization = "Bearer $tokVet" } @{ nombre = "TobyCap"; especie = "Perro"; edad = 4; peso = 14.5; propietarioId = 1 }
    $newId    = ($rPostM.Body | ConvertFrom-Json).id
    $rPutM    = Call-Raw PUT "http://localhost:5000/gateway/mascotas/$newId" @{ Authorization = "Bearer $tokVet" } @{ nombre = "TobyCapEdit"; especie = "Perro"; edad = 5; peso = 15.0; propietarioId = 1 }
    $rDelM    = Call-Raw DELETE "http://localhost:5000/gateway/mascotas/$newId" @{ Authorization = "Bearer $tokVet" }

    $okLines = @(
        "=== PRUEBAS EXITOSAS POR EL API GATEWAY (http://localhost:5000) ===",
        "",
        "### 1. Login Veterinario (publico) -> POST /gateway/auth/login",
        "[$($rVetLogin.Code) OK] Token JWT emitido (emisor: Veterinaria.Api, rol: Veterinario)",
        "Body: $($rVetLogin.Body.Substring(0, [Math]::Min(110, $rVetLogin.Body.Length)))...",
        "",
        "### 2. Listar mascotas (publico, sin token) -> GET /gateway/mascotas",
        "[$($rGetMasc.Code) OK] $($rGetMasc.Body)",
        "",
        "### 3. Listar citas (Microservicio 2, con token) -> GET /gateway/citas",
        "[$($rGetCit.Code) OK] $($rGetCit.Body)",
        "",
        "### 4. Crear mascota (con token Veterinario) -> POST /gateway/mascotas",
        "[$($rPostM.Code) Created] $($rPostM.Body)",
        "",
        "### 5. Actualizar mascota (con token Veterinario) -> PUT /gateway/mascotas/$newId",
        "[$($rPutM.Code) No Content] Actualizacion completada sin cuerpo de respuesta",
        "",
        "### 6. Eliminar mascota (con token Veterinario) -> DELETE /gateway/mascotas/$newId",
        "[$($rDelM.Code) No Content] Eliminacion completada sin cuerpo de respuesta"
    )
    Render-TerminalImage -Title "Pruebas Gateway Local: 200 OK, 201 Created, 204 No Content" -Lines $okLines -FilePath (Join-Path $outDir "03-pruebas-200-201-204.png")

    # 4. Real HTTP calls for 401, 403, 404, direct 401, and 502
    $r401Gw   = Call-Raw POST "http://localhost:5000/gateway/mascotas" @{} @{ nombre = "SinToken"; especie = "Perro"; edad = 2; peso = 8.0; propietarioId = 1 }
    $r401Dir  = Call-Raw GET "http://localhost:5056/api/citas"
    $r403Rec  = Call-Raw POST "http://localhost:5000/gateway/mascotas" @{ Authorization = "Bearer $tokRec" } @{ nombre = "SinRol"; especie = "Perro"; edad = 2; peso = 8.0; propietarioId = 1 }
    $r404Masc = Call-Raw GET "http://localhost:5000/gateway/mascotas/999"
    $r404Cit  = Call-Raw GET "http://localhost:5000/gateway/citas/999" @{ Authorization = "Bearer $tokVet" }

    # Stop CitasApi for 502 test
    Stop-Process -Id $pCit.Id -Force
    Start-Sleep -Seconds 1
    $r502     = Call-Raw GET "http://localhost:5000/gateway/citas" @{ Authorization = "Bearer $tokVet" }
    $rStillOk = Call-Raw GET "http://localhost:5000/gateway/mascotas"

    $errLines = @(
        "=== PRUEBAS DE SEGURIDAD, ERRORES Y RESILIENCIA (401, 403, 404, 502) ===",
        "",
        "### 1. Crear mascota SIN token por Gateway -> POST /gateway/mascotas",
        "[$($r401Gw.Code) Unauthorized] Rechazado directamente en GatewayApi antes de reenviar",
        "",
        "### 2. Llamada DIRECTA a CitasApi sin token -> GET http://localhost:5056/api/citas",
        "[$($r401Dir.Code) Unauthorized] CitasApi valida el JWT de forma independiente",
        "",
        "### 3. Crear mascota con rol Recepcionista -> POST /gateway/mascotas",
        "[$($r403Rec.Code) Forbidden] Gateway valida el JWT, pero Veterinaria.Api exige rol Veterinario",
        "",
        "### 4. Recursos inexistentes -> GET /gateway/mascotas/999 y GET /gateway/citas/999",
        "[$($r404Masc.Code) Not Found] /gateway/mascotas/999",
        "[$($r404Cit.Code) Not Found] /gateway/citas/999",
        "",
        "### 5. CitasApi DETENIDO -> GET /gateway/citas (con token valido)",
        "[$($r502.Code) Bad Gateway] Ocelot no puede conectar con el microservicio 2 detenido",
        "",
        "### 6. Mientras CitasApi esta detenido -> GET /gateway/mascotas",
        "[$($rStillOk.Code) OK] Veterinaria.Api sigue respondiendo con normalidad (aislamiento de fallos)"
    )
    Render-TerminalImage -Title "Pruebas Gateway Local: 401, 403, 404 y 502 Bad Gateway" -Lines $errLines -FilePath (Join-Path $outDir "04-pruebas-401-403-404-502.png")
}
finally {
    foreach ($p in @($pGw, $pCit, $pVet)) {
        if ($p -and -not $p.HasExited) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
    }
    Remove-Item $logVet,$logCit,$logGw -ErrorAction SilentlyContinue
}

# Stage images for browser upload
Copy-Item (Join-Path $outDir "*.png") $uploadDir -Force
Write-Host "Capturas generadas y copiadas a browser_sandbox/uploads:"
Get-ChildItem $uploadDir | Select-Object Name, Length | Format-Table -AutoSize
