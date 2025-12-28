# För att göra .ps1-filen körbar, kör följande kommando i PowerShell (Behöver bara köras första gången):
# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# If EF Core tools needs update use:
# dotnet tool update --global dotnet-ef

# To execute:
# .\database-rebuild-all.ps1 <databasename> [sqlserver|mysql|postgresql] [docker|azure|loopia] [root|dbo|supusr|usr|gstusr] <appsettingsFolder>
#
# Examples:
# .\database-rebuild-all.ps1 sql-music sqlserver docker dbo ../AppWebApi
# .\database-rebuild-all.ps1 sql-music sqlserver docker dbo ../AppRazor
# .\database-rebuild-all.ps1 sql-music sqlserver docker dbo ../AppMvc

# Exit immediately if any command fails
$ErrorActionPreference = "Stop"

param(
    [Parameter(Mandatory = $true)]
    [string]$DatabaseName,

    [Parameter(Mandatory = $true)]
    [ValidateSet("sqlserver", "mysql", "postgresql")]
    [string]$DatabaseType,

    [Parameter(Mandatory = $true)]
    [ValidateSet("docker", "azure", "loopia")]
    [string]$DeploymentTarget,

    [Parameter(Mandatory = $true)]
    [ValidateSet("root", "dbo", "supusr", "usr", "gstusr")]
    [string]$UserLevel,

    [Parameter(Mandatory = $true)]
    [string]$AppSettingsFolder
)

# Resolve absolute path for AppSettingsFolder (fails fast if it doesn't exist)
$resolved = Resolve-Path -Path $AppSettingsFolder -ErrorAction Stop
$AppSettingsFolder = $resolved.Path

# Set DbContext name based on DatabaseType
switch ($DatabaseType) {
    "sqlserver"   { $DBContext = "SqlServerDbContext" }
    "mysql"       { $DBContext = "MySqlDbContext" }        # ändra här om ditt context heter något annat
    "postgresql"  { $DBContext = "PostgresDbContext" }
    default       { throw "Unsupported DatabaseType: $DatabaseType" }
}

Write-Host "DatabaseName      : $DatabaseName"
Write-Host "DatabaseType      : $DatabaseType"
Write-Host "DeploymentTarget  : $DeploymentTarget"
Write-Host "UserLevel         : $UserLevel"
Write-Host "AppSettingsFolder : $AppSettingsFolder"
Write-Host "DbContext         : $DBContext"
Write-Host ""

# Update appsettings.json
$AppSettingsPath = Join-Path $AppSettingsFolder "appsettings.json"
if (!(Test-Path $AppSettingsPath)) {
    throw "Could not find appsettings.json at: $AppSettingsPath"
}

# 1) set UseDataSetWithTag to "<db_name>.<db_type>.<env>"
$tagValue = "$DatabaseName.$DatabaseType.$DeploymentTarget"
$contentRaw = Get-Content -Path $AppSettingsPath -Raw

# Replace UseDataSetWithTag value (keeps formatting mostly intact)
$contentRaw = [regex]::Replace(
    $contentRaw,
    '("UseDataSetWithTag"\s*:\s*)"(.*?)"',
    "`$1`"$tagValue`""
)

# 2) set DefaultDataUser to specified user level
$contentRaw = [regex]::Replace(
    $contentRaw,
    '("DefaultDataUser"\s*:\s*)"(.*?)"',
    "`$1`"$UserLevel`""
)

Set-Content -Path $AppSettingsPath -Value $contentRaw -Encoding UTF8
Write-Host "Updated appsettings.json:"
Write-Host " - UseDataSetWithTag = $tagValue"
Write-Host " - DefaultDataUser   = $UserLevel"
Write-Host ""

# Make EF pick up the correct appsettings folder
$env:EFC_AppSettingsFolder = $AppSettingsFolder

if ($DeploymentTarget -eq "docker") {
    Write-Host "Dropping database (docker) ..."
    dotnet ef database drop -f -c $DBContext -p ../DbContext -s ../DbContext
    Write-Host ""
}

# Remove migrations only for the selected context
$migrationsPath = Join-Path "../DbContext/Migrations" $DBContext
if (Test-Path $migrationsPath) {
    Write-Host "Removing migrations folder: $migrationsPath"
    Remove-Item -Recurse -Force $migrationsPath
    Write-Host ""
}

# Create new migration
Write-Host "Creating migration miInitial ..."
dotnet ef migrations add miInitial -c $DBContext -p ../DbContext -s ../DbContext -o "../DbContext/Migrations/$DBContext"
Write-Host ""

# Update database
Write-Host "Updating database ..."
dotnet ef database update -c $DBContext -p ../DbContext -s ../DbContext
Write-Host ""

Write-Host "DONE"
# To initialize the database you need to run the sql scripts:
# ../DbContext/SqlScripts/<db_type>/initDatabase.sql
