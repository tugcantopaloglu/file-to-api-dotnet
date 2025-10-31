# ============================================================================
# File & Image API - Quick Deployment Script
# ============================================================================
# Simple deployment script with common presets
# ============================================================================

Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "File & Image API - Quick Deployment" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""

# Show menu
Write-Host "Select deployment scenario:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Windows Server (No Authentication)" -ForegroundColor White
Write-Host "   - Self-contained executable"
Write-Host "   - Ready to run on any Windows server"
Write-Host "   - No .NET installation required"
Write-Host ""
Write-Host "2. Windows Server (With Active Directory)" -ForegroundColor White
Write-Host "   - Self-contained executable"
Write-Host "   - JWT authentication with AD"
Write-Host "   - No .NET installation required"
Write-Host ""
Write-Host "3. Linux Server (No Authentication)" -ForegroundColor White
Write-Host "   - Self-contained executable"
Write-Host "   - Ready to run on any Linux x64"
Write-Host "   - No .NET installation required"
Write-Host ""
Write-Host "4. Docker Container" -ForegroundColor White
Write-Host "   - Containerized deployment"
Write-Host "   - Easy to transfer and deploy"
Write-Host "   - Isolated environment"
Write-Host ""
Write-Host "5. Custom (Advanced Options)" -ForegroundColor White
Write-Host "   - Specify all parameters manually"
Write-Host ""

$choice = Read-Host "Enter your choice (1-5)"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "Deploying: Windows Server (No Authentication)" -ForegroundColor Green
        $filesPath = Read-Host "Enter files storage path (e.g., D:\Files) [Default: C:\Files]"
        if ([string]::IsNullOrEmpty($filesPath)) { $filesPath = "C:\Files" }

        .\deploy-closed-network.ps1 `
            -DeploymentType Windows `
            -SelfContained $true `
            -EnableAuthentication $false `
            -FileStoragePath $filesPath `
            -OutputPath ".\deploy-package-windows"
    }

    "2" {
        Write-Host ""
        Write-Host "Deploying: Windows Server (With Active Directory)" -ForegroundColor Green

        $filesPath = Read-Host "Enter files storage path (e.g., D:\Files) [Default: C:\Files]"
        if ([string]::IsNullOrEmpty($filesPath)) { $filesPath = "C:\Files" }

        $domain = Read-Host "Enter AD domain (e.g., company.local)"
        $ldapPath = "LDAP://$domain"
        $container = "DC=$($domain -replace '\.', ',DC=')"

        Write-Host ""
        Write-Host "Active Directory Configuration:" -ForegroundColor Yellow
        Write-Host "  Domain: $domain"
        Write-Host "  LDAP Path: $ldapPath"
        Write-Host "  Container: $container"
        Write-Host ""

        $confirm = Read-Host "Is this correct? (Y/N)"
        if ($confirm -eq "Y" -or $confirm -eq "y") {
            .\deploy-closed-network.ps1 `
                -DeploymentType Windows `
                -SelfContained $true `
                -EnableAuthentication $true `
                -FileStoragePath $filesPath `
                -ADDomain $domain `
                -ADLdapPath $ldapPath `
                -ADContainer $container `
                -OutputPath ".\deploy-package-windows-ad"
        }
        else {
            Write-Host "Deployment cancelled" -ForegroundColor Red
        }
    }

    "3" {
        Write-Host ""
        Write-Host "Deploying: Linux Server (No Authentication)" -ForegroundColor Green
        $filesPath = Read-Host "Enter files storage path (e.g., /var/files) [Default: /app/Files]"
        if ([string]::IsNullOrEmpty($filesPath)) { $filesPath = "/app/Files" }

        .\deploy-closed-network.ps1 `
            -DeploymentType Linux `
            -SelfContained $true `
            -EnableAuthentication $false `
            -FileStoragePath $filesPath `
            -OutputPath ".\deploy-package-linux"
    }

    "4" {
        Write-Host ""
        Write-Host "Deploying: Docker Container" -ForegroundColor Green
        Write-Host "Creating Docker build assets..." -ForegroundColor Yellow

        .\deploy-closed-network.ps1 `
            -DeploymentType Docker `
            -EnableAuthentication $false `
            -FileStoragePath "/app/Files" `
            -OutputPath ".\deploy-package-docker"

        Write-Host ""
        Write-Host "Docker assets created!" -ForegroundColor Green
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "  1. cd deploy-package-docker\docker"
        Write-Host "  2. Run: sh build-and-save.sh"
        Write-Host "  3. Transfer file-to-api.tar to closed network"
    }

    "5" {
        Write-Host ""
        Write-Host "Custom Deployment - Advanced Options" -ForegroundColor Green
        Write-Host ""
        Write-Host "Available parameters:" -ForegroundColor Yellow
        Write-Host "  -DeploymentType [Windows|Linux|Docker]"
        Write-Host "  -SelfContained [\$true|\$false]"
        Write-Host "  -EnableAuthentication [\$true|\$false]"
        Write-Host "  -FileStoragePath [path]"
        Write-Host "  -ADDomain [domain]"
        Write-Host "  -ThumbnailMaxWidth [pixels]"
        Write-Host "  -MobileMaxWidth [pixels]"
        Write-Host "  -CompressionQuality [1-100]"
        Write-Host "  And more..."
        Write-Host ""
        Write-Host "Run: Get-Help .\deploy-closed-network.ps1 -Detailed"
        Write-Host "For full parameter list and examples"
    }

    default {
        Write-Host "Invalid choice" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "============================================================================" -ForegroundColor Green
Write-Host "Deployment package ready!" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green
