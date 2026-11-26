param(
    [string]$GatewayUrl = "http://localhost:5000",
    [string]$CitasDirectUrl = "http://localhost:5056"
)

$ErrorActionPreference = 'Stop'

function Invoke-ApiTest {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [int]$ExpectedStatus,
        [hashtable]$Headers = @{},
        [object]$Body = $null
    )

    $params = @{
        Method          = $Method
        Uri             = $Url
        Headers         = $Headers
        UseBasicParsing = $true
        ErrorAction     = 'Stop'
    }
    if ($null -ne $Body) {
        $params.ContentType = 'application/json'
        $params.Body = ($Body | ConvertTo-Json -Depth 5)
    }

    $status = 0
    $content = ""
    try {
        $resp = Invoke-WebRequest @params
        $status = [int]$resp.StatusCode
        $content = $resp.Content
    } catch {
        if ($_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode
            $stream = $_.Exception.Response.GetResponseStream()
            if ($stream) {
                $reader = New-Object System.IO.StreamReader($stream)
                $content = $reader.ReadToEnd()
            }
        } else {
            throw
        }
    }

    $ok = ($status -eq $ExpectedStatus)
    $mark = if ($ok) { "PASS" } else { "FAIL" }
    Write-Host ("[{0}] {1} -> HTTP {2} (esperado {3})" -f $mark, $Name, $status, $ExpectedStatus)
    if (-not $ok) {
        Write-Host ("       Body: {0}" -f $content)
    }
    return [pscustomobject]@{
        Name = $Name
        Status = $status
        Expected = $ExpectedStatus
        Passed = $ok
        Body = $content
    }
}

$results = @()

# 1. Login Veterinario (200)
$rLoginVet = Invoke-ApiTest -Name "POST /gateway/auth/login (Veterinario)" -Method POST -Url "$GatewayUrl/gateway/auth/login" -ExpectedStatus 200 -Body @{ correo = "veterinario@vet.local"; clave = "VetS14-Admin123!" }
$results += $rLoginVet
$tokenVet = ($rLoginVet.Body | ConvertFrom-Json).token

# 2. Login Recepcionista (200)
$rLoginRecep = Invoke-ApiTest -Name "POST /gateway/auth/login (Recepcionista)" -Method POST -Url "$GatewayUrl/gateway/auth/login" -ExpectedStatus 200 -Body @{ correo = "recepcion@vet.local"; clave = "RecepS14-User123!" }
$results += $rLoginRecep
$tokenRecep = ($rLoginRecep.Body | ConvertFrom-Json).token

# 3. GET mascotas publico (200)
$results += Invoke-ApiTest -Name "GET /gateway/mascotas (publico)" -Method GET -Url "$GatewayUrl/gateway/mascotas" -ExpectedStatus 200
$results += Invoke-ApiTest -Name "GET /gateway/mascotas/1 (publico)" -Method GET -Url "$GatewayUrl/gateway/mascotas/1" -ExpectedStatus 200

# 4. GET mascota inexistente (404)
$results += Invoke-ApiTest -Name "GET /gateway/mascotas/999 (no existe)" -Method GET -Url "$GatewayUrl/gateway/mascotas/999" -ExpectedStatus 404

# 5. POST mascota sin token (401)
$results += Invoke-ApiTest -Name "POST /gateway/mascotas sin token (Gateway 401)" -Method POST -Url "$GatewayUrl/gateway/mascotas" -ExpectedStatus 401 -Body @{ nombre = "Toby"; especie = "Perro"; edad = 4; peso = 14.5; propietarioId = 1 }

# 6. POST mascota con rol Recepcionista (403)
$results += Invoke-ApiTest -Name "POST /gateway/mascotas con rol Recepcionista (API 403)" -Method POST -Url "$GatewayUrl/gateway/mascotas" -ExpectedStatus 403 -Headers @{ Authorization = "Bearer $tokenRecep" } -Body @{ nombre = "Toby"; especie = "Perro"; edad = 4; peso = 14.5; propietarioId = 1 }

# 7. POST mascota con rol Veterinario (201)
$rCreate = Invoke-ApiTest -Name "POST /gateway/mascotas con rol Veterinario (201)" -Method POST -Url "$GatewayUrl/gateway/mascotas" -ExpectedStatus 201 -Headers @{ Authorization = "Bearer $tokenVet" } -Body @{ nombre = "TobyS15"; especie = "Perro"; edad = 4; peso = 14.5; propietarioId = 1 }
$results += $rCreate
$createdId = ($rCreate.Body | ConvertFrom-Json).id

# 8. PUT mascota (204)
$results += Invoke-ApiTest -Name "PUT /gateway/mascotas/$createdId (204)" -Method PUT -Url "$GatewayUrl/gateway/mascotas/$createdId" -ExpectedStatus 204 -Headers @{ Authorization = "Bearer $tokenVet" } -Body @{ nombre = "TobyS15Edit"; especie = "Perro"; edad = 5; peso = 15.0; propietarioId = 1 }

# 9. DELETE mascota (204)
$results += Invoke-ApiTest -Name "DELETE /gateway/mascotas/$createdId (204)" -Method DELETE -Url "$GatewayUrl/gateway/mascotas/$createdId" -ExpectedStatus 204 -Headers @{ Authorization = "Bearer $tokenVet" }

# 10. CitasApi por Gateway sin token (401) y con token (200, 404)
$results += Invoke-ApiTest -Name "GET /gateway/citas sin token (Gateway 401)" -Method GET -Url "$GatewayUrl/gateway/citas" -ExpectedStatus 401
$results += Invoke-ApiTest -Name "GET /gateway/citas con token (200)" -Method GET -Url "$GatewayUrl/gateway/citas" -ExpectedStatus 200 -Headers @{ Authorization = "Bearer $tokenVet" }
$results += Invoke-ApiTest -Name "GET /gateway/citas/1 con token (200)" -Method GET -Url "$GatewayUrl/gateway/citas/1" -ExpectedStatus 200 -Headers @{ Authorization = "Bearer $tokenVet" }
$results += Invoke-ApiTest -Name "GET /gateway/citas/999 con token (404)" -Method GET -Url "$GatewayUrl/gateway/citas/999" -ExpectedStatus 404 -Headers @{ Authorization = "Bearer $tokenVet" }

# 11. Llamada directa a CitasApi sin token (401)
if ($CitasDirectUrl) {
    $results += Invoke-ApiTest -Name "GET directo a CitasApi sin token (401)" -Method GET -Url "$CitasDirectUrl/api/citas" -ExpectedStatus 401
}

$failed = ($results | Where-Object { -not $_.Passed }).Count
Write-Host ("--- Resumen: {0}/{1} pruebas exitosas ---" -f ($results.Count - $failed), $results.Count)
if ($failed -gt 0) { exit 1 }
