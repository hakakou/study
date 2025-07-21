function Export-FolderContent {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $baseDir = Get-Location
    $fullPath = Join-Path $baseDir $RelativePath

    if (-not (Test-Path -LiteralPath $fullPath)) {
        Write-Warning "Path not found: $RelativePath (resolved to $fullPath)"
        return
    }

    # Resolve normalized absolute path
    $folderItem = Get-Item -LiteralPath $fullPath
    $folderPath = $folderItem.FullName

    # -------------------------
    # Normalize relative path for output filename
    # -------------------------
    $normalizedRelative = $RelativePath `
        -replace '^\.\\', '' `
        -replace '^\.\/', ''

    $normalizedRelative = $normalizedRelative.TrimEnd('\','/')

    if ([string]::IsNullOrWhiteSpace($normalizedRelative)) {
        $normalizedRelative = "root"
    }

    $fileNameBase = $normalizedRelative -replace '[\\/]', '-'
    $outputFile = Join-Path $baseDir ($fileNameBase + '.txt')

    Write-Host "Processing '$RelativePath' -> '$outputFile'"

    # -------------------------
    # Collect files:
    # - include *.cs and *.md
    # - ignore *.g.cs
    # - ignore everything inside any obj directory
    # -------------------------

    $files = Get-ChildItem -Path $folderPath -Recurse -Include *.cs, *.md -File |
             # Ignore files in any "obj" folder
             Where-Object { $_.FullName -notmatch '\\obj\\' } |
             # Ignore anything ending in .g.cs
             Where-Object { $_.Name -notmatch '\.g\.cs$' } |
             Sort-Object FullName

    if (-not $files) {
        "No valid *.cs or *.md files found under $RelativePath" |
            Set-Content -LiteralPath $outputFile -Encoding UTF8
        return
    }

    # -------------------------
    # Build output text
    # -------------------------
    $sb = New-Object System.Text.StringBuilder

    foreach ($file in $files) {

        # Make filename relative to the folder correctly
        $relName = $file.FullName.Substring($folderPath.Length).TrimStart('\','/')

        # LF before and after header
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("===== $relName =====")
        [void]$sb.AppendLine()

        foreach ($line in (Get-Content -LiteralPath $file.FullName)) {
            [void]$sb.AppendLine($line)
        }

        [void]$sb.AppendLine()
    }

    $sb.ToString() | Set-Content -LiteralPath $outputFile -Encoding UTF8
}

# ---------------------------------------------
# Run the function for all requested paths
# ---------------------------------------------

Export-FolderContent '.\docs\'
Export-FolderContent '.\dotnet\samples\'
Export-FolderContent '.\dotnet\src\Agents\'
Export-FolderContent '.\dotnet\src\Connectors\'
Export-FolderContent '.\dotnet\src\Experimental\'
Export-FolderContent '.\dotnet\src\Extensions\'
Export-FolderContent '.\dotnet\src\Functions\'
Export-FolderContent '.\dotnet\src\IntegrationTests\'
Export-FolderContent '.\dotnet\src\InternalUtilities\'
Export-FolderContent '.\dotnet\src\Planners\'
Export-FolderContent '.\dotnet\src\Plugins\'
Export-FolderContent '.\dotnet\src\SemanticKernel.Abstractions\'
Export-FolderContent '.\dotnet\src\SemanticKernel.AotTests\'
Export-FolderContent '.\dotnet\src\SemanticKernel.Core\'
Export-FolderContent '.\dotnet\src\SemanticKernel.MetaPackage\'
Export-FolderContent '.\dotnet\src\SemanticKernel.UnitTests\'
Export-FolderContent '.\dotnet\src\VectorData\'
