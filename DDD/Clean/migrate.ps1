param (
    [string]$migrationsProject = "src/Clean.Infrastructure",
    [string]$startupProject = "src/Clean.Web"
)

# Define a prefix for the migration name
$prefix = "M"

# Get the current date and time (formatted as YYYYMMDDHHMMSS)
$timestamp = Get-Date -Format "HHmm"

# Combine the prefix and timestamp for the migration name
$migrationName = "$prefix" + "_" + "$timestamp"

# Run the EF Core migrations command with the auto-generated migration name
dotnet ef migrations add $migrationName --project $migrationsProject --startup-project $startupProject

# Output the migration name for confirmation
Write-Host "Migration created with name: $migrationName"

dotnet ef -v database update --project $migrationsProject

Write-Host "Database updated."