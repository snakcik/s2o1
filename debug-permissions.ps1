$baseUrl = "http://localhost:5267"
$adminUser = @{ userName = "ozan"; password = "123456" }
$targetUserName = "samet"

function Invoke-ApiDebug {
    param(
        [string]$Uri,
        [string]$Method = "Get",
        [object]$Body = $null
    )
    try {
        $params = @{ Uri = $Uri; Method = $Method; ContentType = "application/json" }
        if ($Body) { 
            $json = $Body | ConvertTo-Json -Depth 5 
            # Fix single element array issue in PS
            if ($Body -is [System.Array] -and $json.StartsWith("{")) {
                $json = "[$json]"
            }
            Write-Host "DEBUG JSON BODY: $json" -ForegroundColor Cyan
            $params.Body = $json
        }
        $res = Invoke-RestMethod @params
        return $res
    }
    catch {
        Write-Host "ERROR ($Method $Uri):" -ForegroundColor Red
        Write-Host "  Exception: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            if ($stream) {
                $reader = New-Object System.IO.StreamReader($stream)
                $responseBody = $reader.ReadToEnd()
                Write-Host "  RESPONSE BODY: $responseBody" -ForegroundColor Yellow
            }
        }
        return $null
    }
}

# 1. Login
Write-Host "Logging in..."
$resLogin = Invoke-ApiDebug -Uri "$baseUrl/api/auth/login" -Method Post -Body $adminUser
if (-not $resLogin) { exit }
$ozanId = $resLogin.id
Write-Host "Login OK. ID=$ozanId"

# 2. Find Samet
Write-Host "Finding Samet..."
$users = Invoke-ApiDebug -Uri "$baseUrl/api/users?creatorId=$ozanId"
$sametUser = $users | Where-Object { $_.userName -eq $targetUserName }
if (-not $sametUser) { Write-Host "Samet not found"; exit }
$sametId = $sametUser.id
Write-Host "Samet ID=$sametId"

# 3. Assign Permission
Write-Host "Assigning Permission..."
$perms = @(
    @{ 
        moduleId  = 1; 
        canRead   = $true; 
        canWrite  = $true; 
        canDelete = $false 
    }
)
Invoke-ApiDebug -Uri "$baseUrl/api/users/$sametId/permissions" -Method Post -Body $perms
