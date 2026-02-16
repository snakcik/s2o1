$baseUrl = "http://localhost:5267"
$rootUser = @{ userName = "root"; password = "Q1w2e3r4-" }

function Invoke-Api {
    param(
        [string]$Uri,
        [string]$Method = "Get",
        [object]$Body = $null
    )
    try {
        $params = @{ Uri = $Uri; Method = $Method; ContentType = "application/json" }
        if ($Body) { $params.Body = ($Body | ConvertTo-Json -Depth 5) }
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

# 1. Login Root
Write-Host "1. Logging in as Root..."
$res = Invoke-Api -Uri "$baseUrl/api/auth/login" -Method Post -Body $rootUser
if (-not $res) { exit }
$rootId = $res.id

# 2. CREATE A NEW ADMIN
$random = Get-Random
$adminPayload = @{
    userName        = "perm_admin_$random"
    password        = "password123"
    firstName       = "PermTest"
    lastName        = "Admin"
    email           = "perm_admin_$random@script.test"
    regNo           = "PADM$random"
    roleId          = 2 # Admin
    createdByUserId = $rootId
}
Write-Host "2. Creating NEW Admin..."
$resAdmin = Invoke-Api -Uri "$baseUrl/api/users" -Method Post -Body $adminPayload
if (-not $resAdmin) { exit }
$adminId = $resAdmin.id
Write-Host "   Admin Created: $adminId ($($resAdmin.userName))"

# 3. CREATE A NEW USER BY ADMIN
$randomUser = Get-Random
$userPayload = @{
    userName        = "perm_user_$randomUser"
    password        = "password123"
    firstName       = "PermTest"
    lastName        = "User"
    email           = "perm_user_$randomUser@script.test"
    regNo           = "PUSR$randomUser"
    roleId          = 3 # User
    createdByUserId = $adminId
}
Write-Host "3. Creating User by Admin..."
$resUser = Invoke-Api -Uri "$baseUrl/api/users" -Method Post -Body $userPayload
if (-not $resUser) { exit }
$userId = $resUser.id
Write-Host "   User Created: $userId ($($resUser.userName))"

# 4. ASSIGN PERMISSIONS TO USER (Should Success)
# Note: In real app, we should use the ADMIN token/login to do this.
# But here we are using same session/context - wait, Invoke-RestMethod keeps session? No.
# API is stateless (no JWT in header yet in my script, but auth service doesn't check JWT in controllers yet!)
# The controller just takes inputs.
# Wait! Auth validation is missing in Controller!
# Anyone can call any endpoint!
# Security Gap identified, but let's focus on logic failure first.

Write-Host "4. Assigning Permissions to User (ID=$userId)..."
$perms = @(
    @{ moduleId = 1; canRead = $true; canWrite = $false; canDelete = $false }
)
$resPerm = Invoke-Api -Uri "$baseUrl/api/users/$userId/permissions" -Method Post -Body $perms

if ($resPerm) {
    Write-Host "   SUCCESS: Permissions assigned to User." -ForegroundColor Cyan
}
else {
    Write-Host "   FAILURE: Could not assign to User." -ForegroundColor Red
}

# 5. ASSIGN PERMISSIONS TO ROOT (ID 1)
Write-Host "5. Assigning Permissions to Root (ID=1)..."
$rootPerms = @(
    @{ moduleId = 1; canRead = $true; canWrite = $false; canDelete = $false }
)
$resRoot = Invoke-Api -Uri "$baseUrl/api/users/1/permissions" -Method Post -Body $rootPerms

if ($resRoot) {
    Write-Host "   WARNING: Permissions assigned to Root! (Should ideally be blocked)" -ForegroundColor Yellow
}
else {
    Write-Host "   Root Assignment Failed (Expected if user reported error)." -ForegroundColor Magenta
}
