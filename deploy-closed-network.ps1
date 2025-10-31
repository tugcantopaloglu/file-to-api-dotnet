# ============================================================================
# File & Image API - Closed Network Deployment Script
# ============================================================================
# This script builds and prepares the API for deployment in a closed/air-gapped network
# Run this script on an internet-connected machine before transferring to closed network
# ============================================================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Windows", "Linux", "Docker")]
    [string]$DeploymentType = "Windows",

    [Parameter(Mandatory=$false)]
    [bool]$SelfContained = $true,

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".\deploy-package",

    # Application Settings
    [Parameter(Mandatory=$false)]
    [bool]$EnableAuthentication = $false,

    [Parameter(Mandatory=$false)]
    [string]$FileStoragePath = "C:\Files",

    [Parameter(Mandatory=$false)]
    [int]$MaxFileSize = 52428800,

    [Parameter(Mandatory=$false)]
    [string[]]$AllowedExtensions = @(".png", ".jpg", ".jpeg", ".webp", ".gif"),

    # Active Directory Settings (if authentication enabled)
    [Parameter(Mandatory=$false)]
    [string]$ADDomain = "domain.local",

    [Parameter(Mandatory=$false)]
    [string]$ADLdapPath = "LDAP://domain.local",

    [Parameter(Mandatory=$false)]
    [string]$ADContainer = "DC=domain,DC=local",

    # JWT Settings (if authentication enabled)
    [Parameter(Mandatory=$false)]
    [string]$JwtSecretKey = "",

    [Parameter(Mandatory=$false)]
    [string]$JwtIssuer = "file-api",

    [Parameter(Mandatory=$false)]
    [string]$JwtAudience = "file-api-users",

    [Parameter(Mandatory=$false)]
    [int]$JwtExpirationMinutes = 120,

    # Image Processing Settings
    [Parameter(Mandatory=$false)]
    [int]$ThumbnailMaxWidth = 150,

    [Parameter(Mandatory=$false)]
    [int]$ThumbnailMaxHeight = 150,

    [Parameter(Mandatory=$false)]
    [int]$MobileMaxWidth = 800,

    [Parameter(Mandatory=$false)]
    [int]$MobileMaxHeight = 800,

    [Parameter(Mandatory=$false)]
    [int]$CompressionQuality = 75,

    [Parameter(Mandatory=$false)]
    [int]$CacheDurationSeconds = 3600,

    # CORS Settings
    [Parameter(Mandatory=$false)]
    [bool]$CorsAllowAnyOrigin = $true,

    [Parameter(Mandatory=$false)]
    [string[]]$CorsAllowedOrigins = @(),

    [Parameter(Mandatory=$false)]
    [bool]$CorsAllowAnyMethod = $true,

    [Parameter(Mandatory=$false)]
    [bool]$CorsAllowAnyHeader = $true,

    [Parameter(Mandatory=$false)]
    [bool]$CorsAllowCredentials = $false
)

# ============================================================================
# Functions
# ============================================================================

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-ColorOutput "============================================================================" "Cyan"
    Write-ColorOutput $Message "Cyan"
    Write-ColorOutput "============================================================================" "Cyan"
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-ColorOutput "✓ $Message" "Green"
}

function Write-Error {
    param([string]$Message)
    Write-ColorOutput "✗ $Message" "Red"
}

function Write-Info {
    param([string]$Message)
    Write-ColorOutput "→ $Message" "Yellow"
}

function Test-DotNetInstalled {
    try {
        $dotnetVersion = dotnet --version
        Write-Success ".NET SDK $dotnetVersion found"
        return $true
    }
    catch {
        Write-Error ".NET SDK not found. Please install .NET 8.0 SDK or later"
        Write-Info "Download from: https://dotnet.microsoft.com/download"
        return $false
    }
}

function New-DeploymentConfig {
    param(
        [string]$ConfigPath
    )

    # Generate JWT secret key if not provided
    $jwtKey = $JwtSecretKey
    if ([string]::IsNullOrEmpty($jwtKey)) {
        $bytes = New-Object byte[] 32
        [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($bytes)
        $jwtKey = [Convert]::ToBase64String($bytes)
        Write-Info "Generated new JWT secret key"
    }

    # Escape backslashes for JSON
    $escapedFileStoragePath = $FileStoragePath -replace '\\', '\\'
    $escapedAllowedExtensions = ($AllowedExtensions | ForEach-Object { "`"$_`"" }) -join ", "
    $escapedCorsOrigins = if ($CorsAllowedOrigins.Count -gt 0) {
        ($CorsAllowedOrigins | ForEach-Object { "`"$_`"" }) -join ", "
    } else {
        ""
    }

    $config = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Authentication": {
    "Enabled": $($EnableAuthentication.ToString().ToLower())
  },
  "Authorization": {
    "AllowedUsers": [],
    "AllowedGroups": []
  },
  "ActiveDirectory": {
    "Domain": "$ADDomain",
    "LdapPath": "$ADLdapPath",
    "Container": "$ADContainer"
  },
  "JwtSettings": {
    "SecretKey": "$jwtKey",
    "Issuer": "$JwtIssuer",
    "Audience": "$JwtAudience",
    "ExpirationMinutes": $JwtExpirationMinutes,
    "RefreshTokenExpirationDays": 7
  },
  "FileStorage": {
    "RootPath": "$escapedFileStoragePath",
    "MaxFileSize": $MaxFileSize,
    "AllowedExtensions": [$escapedAllowedExtensions]
  },
  "ImageProcessing": {
    "ThumbnailMaxWidth": $ThumbnailMaxWidth,
    "ThumbnailMaxHeight": $ThumbnailMaxHeight,
    "MobileMaxWidth": $MobileMaxWidth,
    "MobileMaxHeight": $MobileMaxHeight,
    "CompressionQuality": $CompressionQuality,
    "CacheDurationSeconds": $CacheDurationSeconds,
    "EnableResponseCaching": true
  },
  "Cors": {
    "AllowAnyOrigin": $($CorsAllowAnyOrigin.ToString().ToLower()),
    "AllowedOrigins": [$escapedCorsOrigins],
    "AllowAnyMethod": $($CorsAllowAnyMethod.ToString().ToLower()),
    "AllowAnyHeader": $($CorsAllowAnyHeader.ToString().ToLower()),
    "AllowCredentials": $($CorsAllowCredentials.ToString().ToLower())
  }
}
"@

    $config | Out-File -FilePath $ConfigPath -Encoding UTF8
    Write-Success "Configuration file created: $ConfigPath"
}

function New-DeploymentReadme {
    param([string]$ReadmePath, [string]$DeployType)

    $readme = @"
# File & Image API - Closed Network Deployment Package

## Package Contents

- **Application Files**: All compiled binaries and dependencies
- **appsettings.Production.json**: Pre-configured settings
- **DEPLOYMENT-GUIDE.txt**: This file
- **run.bat** (Windows) or **run.sh** (Linux): Start script

## Deployment Type: $DeployType

## Quick Start

### Windows:
1. Extract this package to your target server
2. Edit 'appsettings.Production.json' if needed
3. Run 'run.bat' to start the API
4. Access Swagger UI at: http://localhost:5000/swagger

### Linux:
1. Extract this package to your target server
2. Edit 'appsettings.Production.json' if needed
3. Make run script executable: chmod +x run.sh
4. Run './run.sh' to start the API
5. Access Swagger UI at: http://localhost:5000/swagger

### Docker:
1. Load the image: docker load -i file-to-api.tar
2. Run container: docker run -d -p 5000:80 -v /path/to/files:/app/Files file-to-api:v1.0.0

## Configuration

Edit 'appsettings.Production.json' to customize:

### File Storage:
- RootPath: Directory where files are stored
- AllowedExtensions: File types allowed

### Image Processing:
- ThumbnailMaxWidth/Height: Thumbnail dimensions
- MobileMaxWidth/Height: Mobile image max dimensions
- CompressionQuality: JPEG/WebP quality (1-100)
- CacheDurationSeconds: Response cache duration

### Authentication (Optional):
- Enabled: Set to true to require authentication
- ActiveDirectory: Configure if using AD
- JwtSettings: JWT token configuration

## API Endpoints

### Single File Endpoints (GET):
- GET /img/{path} - Get raw file
- GET /img/{path}/metadata - Get file metadata
- GET /img/base64/{path} - Get file as base64
- GET /img/thumbnail/{path} - Get 150x150 thumbnail
- GET /img/base64/thumbnail/{path} - Thumbnail as base64
- GET /img/mobile/{path} - Mobile-optimized (800x800)
- GET /img/base64/mobile/{path} - Mobile image as base64

### Batch Endpoints (POST):
- POST /img/batch/base64 - Multiple files as base64
- POST /img/batch/thumbnail - Multiple thumbnails
- POST /img/batch/mobile - Multiple mobile images

### System Endpoints:
- GET /health - Health check
- GET /swagger - API documentation

## File Organization

Place your files in the configured RootPath directory:
```
Files/
├── user1/
│   ├── avatar.jpg
│   └── photo.png
├── user2/
│   └── profile.jpg
└── shared/
    └── logo.png
```

Access files via API:
- /img/user1/avatar.jpg
- /img/user1/avatar (extension auto-detected)

## Network Requirements

✓ No internet connection required
✓ Works in completely isolated/air-gapped networks
✓ Optional: Local Active Directory for authentication
✓ All dependencies included in this package

## Troubleshooting

### Port already in use:
Edit run script and change port (default: 5000)

### Cannot access API:
- Check firewall settings
- Verify the service is running
- Check logs in 'logs/' directory

### Files not found:
- Verify RootPath in configuration
- Check file permissions
- Ensure files exist in correct directory

## Support

For issues or questions, refer to README.md in the source repository.

## Package Information

- Build Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
- Deployment Type: $DeployType
- Self-Contained: $SelfContained
- Authentication: $($EnableAuthentication -eq $true ? "Enabled" : "Disabled")
- File Storage Path: $FileStoragePath

"@

    $readme | Out-File -FilePath $ReadmePath -Encoding UTF8
    Write-Success "Deployment guide created: $ReadmePath"
}

function New-WindowsRunScript {
    param([string]$ScriptPath, [bool]$IsSelfContained)

    $exeName = if ($IsSelfContained) { "FileToApi.exe" } else { "dotnet FileToApi.dll" }

    $script = @"
@echo off
echo ============================================================================
echo File ^& Image API - Starting...
echo ============================================================================
echo.

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running with administrator privileges
) else (
    echo WARNING: Not running as administrator. Some features may not work.
    echo.
)

REM Set environment
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=http://0.0.0.0:5000

echo Starting API on port 5000...
echo Access Swagger UI at: http://localhost:5000/swagger
echo Access Health Check at: http://localhost:5000/health
echo.
echo Press Ctrl+C to stop the server
echo ============================================================================
echo.

REM Start the application
$exeName

pause
"@

    $script | Out-File -FilePath $ScriptPath -Encoding ASCII
    Write-Success "Windows run script created: $ScriptPath"
}

function New-LinuxRunScript {
    param([string]$ScriptPath, [bool]$IsSelfContained)

    $exeName = if ($IsSelfContained) { "./FileToApi" } else { "dotnet FileToApi.dll" }

    $script = @"
#!/bin/bash

echo "============================================================================"
echo "File & Image API - Starting..."
echo "============================================================================"
echo ""

# Set environment
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000

echo "Starting API on port 5000..."
echo "Access Swagger UI at: http://localhost:5000/swagger"
echo "Access Health Check at: http://localhost:5000/health"
echo ""
echo "Press Ctrl+C to stop the server"
echo "============================================================================"
echo ""

# Start the application
$exeName
"@

    $script | Out-File -FilePath $ScriptPath -Encoding UTF8
    Write-Success "Linux run script created: $ScriptPath"
}

function New-DockerAssets {
    param([string]$DockerPath)

    # Dockerfile
    $dockerfile = @"
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FileToApi/FileToApi.csproj", "src/FileToApi/"]
RUN dotnet restore "src/FileToApi/FileToApi.csproj"
COPY . .
WORKDIR "/src/src/FileToApi"
RUN dotnet build "FileToApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileToApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileToApi.dll"]
"@

    $dockerfile | Out-File -FilePath (Join-Path $DockerPath "Dockerfile") -Encoding UTF8

    # Docker Compose
    $dockerCompose = @"
version: '3.8'
services:
  file-api:
    image: file-to-api:v1.0.0
    container_name: file-image-api
    ports:
      - "5000:80"
      - "5001:443"
    volumes:
      - ./files:/app/Files
      - ./logs:/app/logs
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - FileStorage__RootPath=/app/Files
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
"@

    $dockerCompose | Out-File -FilePath (Join-Path $DockerPath "docker-compose.yml") -Encoding UTF8

    # Docker build and save script
    $dockerScript = @"
#!/bin/bash

echo "============================================================================"
echo "Building Docker Image for Closed Network"
echo "============================================================================"

# Build the image
echo "Building image..."
docker build -t file-to-api:v1.0.0 .

if [ `$? -eq 0 ]; then
    echo "✓ Image built successfully"

    # Save the image to tar file
    echo "Saving image to tar file..."
    docker save -o file-to-api.tar file-to-api:v1.0.0

    if [ `$? -eq 0 ]; then
        echo "✓ Image saved to file-to-api.tar"
        echo ""
        echo "Transfer file-to-api.tar to your closed network"
        echo ""
        echo "On the closed network machine, run:"
        echo "  docker load -i file-to-api.tar"
        echo "  docker-compose up -d"
    else
        echo "✗ Failed to save image"
        exit 1
    fi
else
    echo "✗ Failed to build image"
    exit 1
fi
"@

    $dockerScript | Out-File -FilePath (Join-Path $DockerPath "build-and-save.sh") -Encoding UTF8

    Write-Success "Docker assets created in: $DockerPath"
}

# ============================================================================
# Main Script
# ============================================================================

Write-Header "File & Image API - Closed Network Deployment Preparation"

# Verify .NET is installed
if (-not (Test-DotNetInstalled)) {
    exit 1
}

# Create output directory
Write-Info "Creating deployment package directory: $OutputPath"
if (Test-Path $OutputPath) {
    Write-Info "Removing existing deployment package..."
    Remove-Item -Path $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Build and publish based on deployment type
Write-Header "Building Application for $DeploymentType"

$publishPath = Join-Path $OutputPath "publish"

switch ($DeploymentType) {
    "Windows" {
        Write-Info "Building for Windows x64..."
        $runtime = "win-x64"
        $publishArgs = @(
            "publish",
            "-c", "Release",
            "-r", $runtime,
            "--self-contained", $SelfContained,
            "-o", $publishPath,
            "src/FileToApi/FileToApi.csproj"
        )

        & dotnet @publishArgs

        if ($LASTEXITCODE -eq 0) {
            Write-Success "Build completed successfully"

            # Create configuration
            New-DeploymentConfig -ConfigPath (Join-Path $publishPath "appsettings.Production.json")

            # Create run script
            New-WindowsRunScript -ScriptPath (Join-Path $publishPath "run.bat") -IsSelfContained $SelfContained

            # Create deployment guide
            New-DeploymentReadme -ReadmePath (Join-Path $publishPath "DEPLOYMENT-GUIDE.txt") -DeployType "Windows"
        }
        else {
            Write-Error "Build failed"
            exit 1
        }
    }

    "Linux" {
        Write-Info "Building for Linux x64..."
        $runtime = "linux-x64"
        $publishArgs = @(
            "publish",
            "-c", "Release",
            "-r", $runtime,
            "--self-contained", $SelfContained,
            "-o", $publishPath,
            "src/FileToApi/FileToApi.csproj"
        )

        & dotnet @publishArgs

        if ($LASTEXITCODE -eq 0) {
            Write-Success "Build completed successfully"

            # Create configuration
            New-DeploymentConfig -ConfigPath (Join-Path $publishPath "appsettings.Production.json")

            # Create run script
            New-LinuxRunScript -ScriptPath (Join-Path $publishPath "run.sh") -IsSelfContained $SelfContained

            # Create deployment guide
            New-DeploymentReadme -ReadmePath (Join-Path $publishPath "DEPLOYMENT-GUIDE.txt") -DeployType "Linux"
        }
        else {
            Write-Error "Build failed"
            exit 1
        }
    }

    "Docker" {
        Write-Info "Preparing Docker deployment assets..."
        $dockerPath = Join-Path $OutputPath "docker"
        New-Item -ItemType Directory -Path $dockerPath -Force | Out-Null

        # Copy source files
        Copy-Item -Path "src" -Destination $dockerPath -Recurse

        # Create Docker assets
        New-DockerAssets -DockerPath $dockerPath

        # Create configuration
        New-DeploymentConfig -ConfigPath (Join-Path $dockerPath "appsettings.Production.json")

        # Create deployment guide
        New-DeploymentReadme -ReadmePath (Join-Path $dockerPath "DEPLOYMENT-GUIDE.txt") -DeployType "Docker"

        Write-Success "Docker assets created"
        Write-Info "Run 'build-and-save.sh' in the docker directory to build and save the image"
    }
}

# Create summary file
Write-Header "Creating Deployment Summary"

$summaryPath = Join-Path $OutputPath "DEPLOYMENT-SUMMARY.txt"
$summary = @"
# File & Image API - Deployment Package Summary

Package Created: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Deployment Type: $DeploymentType
Self-Contained: $SelfContained

## Configuration Applied:

### Authentication:
- Enabled: $EnableAuthentication

### File Storage:
- Root Path: $FileStoragePath
- Max File Size: $MaxFileSize bytes
- Allowed Extensions: $($AllowedExtensions -join ", ")

### Image Processing:
- Thumbnail Size: ${ThumbnailMaxWidth}x${ThumbnailMaxHeight}px
- Mobile Max Size: ${MobileMaxWidth}x${MobileMaxHeight}px
- Compression Quality: $CompressionQuality
- Cache Duration: $CacheDurationSeconds seconds

### Active Directory (if enabled):
- Domain: $ADDomain
- LDAP Path: $ADLdapPath
- Container: $ADContainer

### JWT Settings (if authentication enabled):
- Issuer: $JwtIssuer
- Audience: $JwtAudience
- Expiration: $JwtExpirationMinutes minutes

## Next Steps:

1. Transfer the deployment package to your closed network
2. Extract on target server
3. Review and edit appsettings.Production.json if needed
4. Run the application using the provided script
5. Access API at http://server-ip:5000
6. View documentation at http://server-ip:5000/swagger

## Package Location:
$OutputPath

## Files Included:
- Application binaries and dependencies
- Configuration file (appsettings.Production.json)
- Run script (run.bat or run.sh)
- Deployment guide (DEPLOYMENT-GUIDE.txt)
- This summary file

## Support:
All features work without internet connectivity.
Optional Active Directory integration requires local domain controller access.

"@

$summary | Out-File -FilePath $summaryPath -Encoding UTF8

Write-Success "Deployment package created successfully!"
Write-Host ""
Write-ColorOutput "============================================================================" "Green"
Write-ColorOutput "Deployment package is ready at: $OutputPath" "Green"
Write-ColorOutput "============================================================================" "Green"
Write-Host ""
Write-Info "Next steps:"
Write-Host "  1. Transfer the package to your closed network"
Write-Host "  2. Extract on target server"
Write-Host "  3. Review appsettings.Production.json"
Write-Host "  4. Run the application"
Write-Host ""
Write-Info "For detailed instructions, see: DEPLOYMENT-GUIDE.txt in the package"
Write-Host ""
