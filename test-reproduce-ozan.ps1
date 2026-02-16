$baseUrl = "http://localhost:5267"
$adminUser = @{ userName = "ozan"; password = "123456" }
$targetUserName = "samet"

function Invoke-Api {
    param(
        [string]$Uri,
        [string]$Method = "Get",
        [object]$Body = $null
    )
    try {
        $params = @{ Uri = $Uri; Method = $Method; ContentType = "application/json" }
        if ($Body) { 
            $json = $Body | ConvertTo-Json -Depth 5 
            if ($Body -is [System.Array] -and $json.StartsWith("{")) { $json = "[$json]" }
            $params.Body = $json
        }
        $res = Invoke-RestMethod @params
        return $res
    }
    catch {
        Write-Host "ERROR ($Method $Uri): $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "RESPONSE BODY: $responseBody" -ForegroundColor Yellow
        }
        return $null
    }
}

# 1. Login as Ozan (Admin)
Write-Host "1. Logging in as 'ozan'..."
$resLogin = Invoke-Api -Uri "$baseUrl/api/auth/login" -Method Post -Body $adminUser

if (-not $resLogin) {
    Write-Host "FAILED: Could not login as 'ozan'. User might not exist or wrong password." -ForegroundColor Red
    exit
}

$ozanId = $resLogin.id
Write-Host "   Login SUCCESS. ID=$ozanId, Role=$($resLogin.role)" -ForegroundColor Green

# 2. Find Samet (User created by Ozan)
Write-Host "2. Searching for user '$targetUserName' created by ozan..."
# We search in users list filtered by creatorId (since admins only see their own users)
$users = Invoke-Api -Uri "$baseUrl/api/users?creatorId=$ozanId" -Method Get

$sametUser = $users | Where-Object { $_.userName -eq $targetUserName }

if (-not $sametUser) {
    Write-Host "FAILED: User 'samet' not found in Ozan's list." -ForegroundColor Red
    Write-Host "   Trying global search (in case admin logic is strict but we want to debug)..."
    # Try global (without filter - Note: API might filter all permissions? No, filtering is in service)
    # Actually, let's just try to login as samet to get ID if needed? No, let's assume filtering works.
    exit
}

$sametId = $sametUser.id
Write-Host "   User '$targetUserName' FOUND. ID=$sametId" -ForegroundColor Green

# 3. Assign Permissions to Samet
Write-Host "3. Assigning permissions to '$targetUserName' (ID=$sametId)..."

# Mock Permission Data (Module ID 1 assumed to exist)
# Check modules first
$modules = Invoke-Api -Uri "$baseUrl/api/users/modules"
if ($modules.Count -eq 0) {
    Write-Host "FAILED: No modules found in system!"
    exit
}
$firstModuleId = $modules[0].id
Write-Host "   Using Module ID: $firstModuleId ($($modules[0].name))"

$perms = @(
    @{ 
        moduleId  = $firstModuleId; 
        canRead   = $true; 
        canWrite  = $true; 
        canDelete = $false 
    }
)

$resPerm = Invoke-Api -Uri "$baseUrl/api/users/$sametId/permissions" -Method Post -Body $perms

if ($resPerm) {
    Write-Host "SUCCESS: Permissions assigned." -ForegroundColor Cyan
}
else {
    Write-Host "FAILURE: Could not assign permissions." -ForegroundColor Red
}
