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
        return Invoke-RestMethod @params
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
Write-Host "LOGGED_IN_ROOT: ID=$rootId Role=$($res.role)" -ForegroundColor Green

# 2. Get Roles (assuming there's an API, otherwise guess)
# If no role API, we rely on standard IDs. But let's check via DbContext access not possible here.
# Let's assume 1=root, 2=Admin, 3=User for now, or fetch roles if endpoint exists.
# There is no direct role endpoint visible in UsersController except maybe implicitly?
# Wait, UsersController has no GetRoles.

# 3. Create Admin User (Role=2, created by Root)
$adminPayload = @{
    userName        = "test_admin_script_$(Get-Random)"
    password        = "password123"
    firstName       = "Test"
    lastName        = "Admin"
    email           = "admin@script.test"
    regNo           = "ADM$(Get-Random)"
    roleId          = 2  # Assuming 2 is Admin
    createdByUserId = $rootId
}

Write-Host "2. Creating Admin User..."
$resAdmin = Invoke-Api -Uri "$baseUrl/api/users" -Method Post -Body $adminPayload
if ($resAdmin) {
    $adminId = $resAdmin.id
    Write-Host "CREATED_ADMIN: ID=$adminId User=$($resAdmin.userName)" -ForegroundColor Green
}

# 3. Create Regular User (Role=3, created by Root)
$userPayload = @{
    userName        = "test_user_script_$(Get-Random)"
    password        = "password123"
    firstName       = "Test"
    lastName        = "User"
    email           = "user@script.test"
    regNo           = "USR$(Get-Random)"
    roleId          = 3 # Assuming 3 is User
    createdByUserId = $rootId
}

Write-Host "3. Creating Regular User..."
$resUser = Invoke-Api -Uri "$baseUrl/api/users" -Method Post -Body $userPayload
if ($resUser) {
    $userId = $resUser.id
    Write-Host "CREATED_USER: ID=$userId User=$($resUser.userName)" -ForegroundColor Green
}

# 4. Create User BY Admin (created by Admin) - ONLY IF ADMIN CREATED
if ($adminId) {
    $subUserPayload = @{
        userName        = "test_subuser_script_$(Get-Random)"
        password        = "password123"
        firstName       = "Sub"
        lastName        = "User"
        email           = "sub@script.test"
        regNo           = "SUB$(Get-Random)"
        roleId          = 3 # User role
        createdByUserId = $adminId
    }

    Write-Host "4. Creating SubUser by Admin..."
    $resSub = Invoke-Api -Uri "$baseUrl/api/users" -Method Post -Body $subUserPayload
    if ($resSub) {
        $subId = $resSub.id
        Write-Host "CREATED_SUBUSER: ID=$subId User=$($resSub.userName)" -ForegroundColor Green
    }
}

# 5. List Users as Root (No filter)
Write-Host "`n--- LIST AS ROOT (ALL) ---"
$allUsers = Invoke-Api -Uri "$baseUrl/api/users"
$allUsers | ForEach-Object { Write-Host " - $($_.userName) (CreatedBy: $($_.createdByUserId))" }

# 6. List Users as Admin (Filtered by Admin ID)
if ($adminId) {
    Write-Host "`n--- LIST AS ADMIN (FILTERED) ---"
    # Authenticate as Admin (if we had token, but here we just query with creatorId param as instructed)
    # The API logic relies on query param `creatorId`. In real world, we'd login as admin.
    # But let's test the filtering logic directly.
    $adminUsers = Invoke-Api -Uri "$baseUrl/api/users?creatorId=$adminId"
    $adminUsers | ForEach-Object { Write-Host " - $($_.userName) (CreatedBy: $($_.createdByUserId))" }
    
    # Verification
    $count = $adminUsers.Count
    if ($null -eq $count) { $count = 0 }
    if ($count -eq 1) {
        # Should only see subuser
        Write-Host "SUCCESS: Admin sees only their own user." -ForegroundColor Cyan
    }
    else {
        Write-Host "FAILURE: Admin sees incorrect number of users: $count" -ForegroundColor Red
    }
}
