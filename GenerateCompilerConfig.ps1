# Define the folders to prioritize and scan
$priorityFolder = "wwwroot/themes/_components/blazor"
$baseFolder = "wwwroot/themes"

# Initialize an array to hold the configuration entries
$config = @()

# Get the base project directory
$baseDir = (Get-Location).Path

# Function to add files to configuration
function AddFilesToConfig($folderPath) {
    if (Test-Path $folderPath) {
        Get-ChildItem -Path $folderPath -Recurse -Filter "*.scss" | ForEach-Object {
            $relativeInput = $_.FullName -replace [regex]::Escape($baseDir + "\"), "" -replace "\\", "/"
            $relativeOutput = $relativeInput -replace ".scss$", ".css"

            # Add the input and output files to the configuration
            $config += @{
                inputFile = $relativeInput
                outputFile = $relativeOutput
            }
        }
    } else {
        Write-Host "Folder not found: $folderPath" -ForegroundColor Yellow
    }
}

# Process the priority folder first
AddFilesToConfig $priorityFolder

# Process the base folder (excluding the priority folder)
Get-ChildItem -Path $baseFolder -Recurse -Filter "*.scss" | Where-Object {
    $_.DirectoryName -notlike "$priorityFolder*"
} | ForEach-Object {
    $relativeInput = $_.FullName -replace [regex]::Escape($baseDir + "\"), "" -replace "\\", "/"
    $relativeOutput = $relativeInput -replace ".scss$", ".css"

    # Add the input and output files to the configuration
    $config += @{
        inputFile = $relativeInput
        outputFile = $relativeOutput
    }
}

# Convert the configuration to JSON format
$config | ConvertTo-Json -Depth 10 | Set-Content -Path "compilerconfig.json"

Write-Host "compilerconfig.json has been generated successfully!" -ForegroundColor Green
