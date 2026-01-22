# KoClip Project Reorganization Script
# Project folder reorganization script

param(
    [switch]$WhatIf,  # Show what would be done without making changes
    [switch]$Force    # Execute without confirmation
)

function Write-Step {
    param([string]$Message, [string]$Color = "Cyan")
    Write-Host ""
    Write-Host $Message -ForegroundColor $Color
}

function Write-Success {
    param([string]$Message)
    Write-Host "   OK $Message" -ForegroundColor Green
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "   WARN $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "   INFO $Message" -ForegroundColor Blue
}

# Script start
Clear-Host
Write-Host "=== KoClip Project Reorganization Script ===" -ForegroundColor Green
Write-Host ""

if ($WhatIf) {
    Write-Host "WhatIf Mode: No actual changes will be made" -ForegroundColor Yellow
    Write-Host ""
}

# Check current directory structure
Write-Step "1. Checking current structure..."

$currentStructure = @{
    "delphi-code" = Test-Path "delphi-code"
    "KoClipConverter" = Test-Path "KoClipConverter"
    "KoClipCS_Generated" = Test-Path "KoClipConverter\Generated\KoClipCS"
    "KoClipCS_Tests_Generated" = Test-Path "KoClipConverter\Generated\KoClipCS.Tests"
    "KoClipCS_Root" = Test-Path "KoClipCS"
    "KoClipCS_Tests_Root" = Test-Path "KoClipCS.Tests"
}

foreach ($item in $currentStructure.GetEnumerator()) {
    $status = if ($item.Value) { "EXISTS" } else { "MISSING" }
    $color = if ($item.Value) { "Green" } else { "Gray" }
    Write-Host "   $($item.Key): $status" -ForegroundColor $color
}

# Plan move operations
Write-Step "2. Planning operations..."

$moveOperations = @()

if ($currentStructure["KoClipCS_Generated"]) {
    if (-not $currentStructure["KoClipCS_Root"]) {
        $moveOperations += @{
            Source = "KoClipConverter\Generated\KoClipCS"
            Target = "KoClipCS"
            Type = "Move"
            Description = "Move main application to root"
        }
    } else {
        Write-Warning-Custom "KoClipCS folder already exists"
    }
}

if ($currentStructure["KoClipCS_Tests_Generated"]) {
    if (-not $currentStructure["KoClipCS_Tests_Root"]) {
        $moveOperations += @{
            Source = "KoClipConverter\Generated\KoClipCS.Tests"
            Target = "KoClipCS.Tests"
            Type = "Move"
            Description = "Move test project to root"
        }
    } else {
        Write-Warning-Custom "KoClipCS.Tests folder already exists"
    }
}

$deleteOperations = @()

if ($currentStructure["delphi-code"]) {
    $deleteOperations += @{
        Path = "delphi-code"
        Description = "Delete original Delphi code folder"
    }
}

if ($currentStructure["KoClipConverter"]) {
    $deleteOperations += @{
        Path = "KoClipConverter"
        Description = "Delete converter tool folder"
    }
}

# Display planned operations
if ($moveOperations.Count -gt 0) {
    Write-Info "Move operations:"
    foreach ($op in $moveOperations) {
        Write-Host "     $($op.Source) -> $($op.Target)" -ForegroundColor White
        Write-Host "     ($($op.Description))" -ForegroundColor Gray
    }
}

if ($deleteOperations.Count -gt 0) {
    Write-Info "Delete operations:"
    foreach ($op in $deleteOperations) {
        Write-Host "     $($op.Path)" -ForegroundColor White
        Write-Host "     ($($op.Description))" -ForegroundColor Gray
    }
}

if ($moveOperations.Count -eq 0 -and $deleteOperations.Count -eq 0) {
    Write-Success "Reorganization already completed"
    exit 0
}

# Confirmation
if (-not $WhatIf -and -not $Force) {
    Write-Host ""
    $confirmation = Read-Host "Execute the above operations? (y/N)"
    if ($confirmation -ne "y" -and $confirmation -ne "Y") {
        Write-Host "Operation cancelled" -ForegroundColor Yellow
        exit 0
    }
}

if ($WhatIf) {
    Write-Host ""
    Write-Host "WhatIf mode - no actual operations performed" -ForegroundColor Yellow
    exit 0
}

# Execute actual operations
Write-Step "3. Executing folder reorganization..."

try {
    # Move operations
    foreach ($op in $moveOperations) {
        if (Test-Path $op.Source) {
            Move-Item -Path $op.Source -Destination $op.Target -Force
            Write-Success "$($op.Description)"
        } else {
            Write-Warning-Custom "Source not found: $($op.Source)"
        }
    }
    
    # Delete operations
    foreach ($op in $deleteOperations) {
        if (Test-Path $op.Path) {
            Remove-Item -Path $op.Path -Recurse -Force
            Write-Success "$($op.Description)"
        } else {
            Write-Warning-Custom "Delete target not found: $($op.Path)"
        }
    }
    
} catch {
    Write-Error "Error during operations: $($_.Exception.Message)"
    exit 1
}

# Update build scripts
Write-Step "4. Updating build scripts..."

$buildScripts = @("build-release.ps1", "build-release-advanced.ps1")

foreach ($script in $buildScripts) {
    if (Test-Path $script) {
        try {
            $content = Get-Content $script -Raw
            
            # Update paths
            $updatedContent = $content -replace 'KoClipConverter\\Generated\\KoClipCS', 'KoClipCS'
            $updatedContent = $updatedContent -replace 'KoClipConverter\\Generated\\KoClipCS\.Tests', 'KoClipCS.Tests'
            
            if ($content -ne $updatedContent) {
                Set-Content -Path $script -Value $updatedContent -Encoding UTF8
                Write-Success "Updated paths in $script"
            } else {
                Write-Info "$script does not need updates"
            }
        } catch {
            Write-Warning-Custom "Error updating $script : $($_.Exception.Message)"
        }
    }
}

# Verify final structure
Write-Step "5. Verifying reorganized structure..."

$finalStructure = @(
    "KoClipCS",
    "KoClipCS.Tests",
    "README.md",
    "build-release.ps1",
    "build-release-advanced.ps1",
    "build-release.bat",
    "BUILD-README.md",
    ".kiro"
)

foreach ($item in $finalStructure) {
    $exists = Test-Path $item
    $status = if ($exists) { "OK" } else { "MISSING" }
    $color = if ($exists) { "Green" } else { "Red" }
    Write-Host "   $status $item" -ForegroundColor $color
}

# Completion message
Write-Host ""
Write-Host "=== Folder Reorganization Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Final project structure:" -ForegroundColor Cyan
Write-Host "   KoClipCS/           - Main application" -ForegroundColor White
Write-Host "   KoClipCS.Tests/     - Test project" -ForegroundColor White
Write-Host "   build-*.ps1         - Build scripts" -ForegroundColor White
Write-Host "   README.md           - Application documentation" -ForegroundColor White
Write-Host "   .kiro/              - Kiro settings" -ForegroundColor White
Write-Host ""
Write-Host "Project reorganization completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Test build scripts" -ForegroundColor White
Write-Host "2. Verify build with new structure" -ForegroundColor White
Write-Host "3. Final cleanup verification" -ForegroundColor White