$connStr = "Server=(localdb)\MSSQLLocalDB;Integrated Security=True;"
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection $connStr
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "IF EXISTS (SELECT name FROM sys.databases WHERE name = N'2S1O') BEGIN ALTER DATABASE [2S1O] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [2S1O]; END"
    $cmd.ExecuteNonQuery()
    $conn.Close()
    Write-Host "Database 2S1O Dropped Successfully."
}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
