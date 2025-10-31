# Closed Network Deployment Guide

This guide explains how to deploy the File & Image API in closed/air-gapped networks without internet connectivity.

## 📦 Deployment Scripts

Two PowerShell scripts are provided:

### 1. **deploy-quick.ps1** - Simple Interactive Menu
Best for most users. Provides guided deployment with common presets.

### 2. **deploy-closed-network.ps1** - Advanced Configuration
Full control over all settings with command-line parameters.

## 🚀 Quick Start

### Option 1: Interactive Menu (Recommended)

```powershell
# Run the quick deployment script
.\deploy-quick.ps1

# Follow the interactive menu to select:
# 1. Windows Server (No Authentication)
# 2. Windows Server (With Active Directory)
# 3. Linux Server (No Authentication)
# 4. Docker Container
# 5. Custom (Advanced Options)
```

### Option 2: One-Command Deployment

```powershell
# Windows - No Authentication
.\deploy-closed-network.ps1 -DeploymentType Windows -FileStoragePath "D:\Files"

# Windows - With Active Directory
.\deploy-closed-network.ps1 `
    -DeploymentType Windows `
    -EnableAuthentication $true `
    -FileStoragePath "D:\Files" `
    -ADDomain "company.local"

# Linux - No Authentication
.\deploy-closed-network.ps1 `
    -DeploymentType Linux `
    -FileStoragePath "/var/files"

# Docker
.\deploy-closed-network.ps1 -DeploymentType Docker
```

## 📋 Complete Parameter Reference

### Basic Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-DeploymentType` | String | Windows | Windows, Linux, or Docker |
| `-SelfContained` | Bool | $true | Include .NET runtime in package |
| `-OutputPath` | String | .\deploy-package | Where to create package |
| `-EnableAuthentication` | Bool | $false | Enable JWT/AD authentication |

### File Storage Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-FileStoragePath` | String | C:\Files | Directory for file storage |
| `-MaxFileSize` | Int | 52428800 | Max file size (50MB default) |
| `-AllowedExtensions` | Array | .png,.jpg,.jpeg,.webp,.gif | Allowed file extensions |

### Active Directory Parameters (if authentication enabled)

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-ADDomain` | String | domain.local | Active Directory domain |
| `-ADLdapPath` | String | LDAP://domain.local | LDAP connection string |
| `-ADContainer` | String | DC=domain,DC=local | AD container DN |

### JWT Parameters (if authentication enabled)

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-JwtSecretKey` | String | (generated) | JWT signing key (min 32 chars) |
| `-JwtIssuer` | String | file-api | Token issuer |
| `-JwtAudience` | String | file-api-users | Token audience |
| `-JwtExpirationMinutes` | Int | 120 | Token validity duration |

### Image Processing Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-ThumbnailMaxWidth` | Int | 150 | Thumbnail max width |
| `-ThumbnailMaxHeight` | Int | 150 | Thumbnail max height |
| `-MobileMaxWidth` | Int | 800 | Mobile image max width |
| `-MobileMaxHeight` | Int | 800 | Mobile image max height |
| `-CompressionQuality` | Int | 75 | JPEG/WebP quality (1-100) |
| `-CacheDurationSeconds` | Int | 3600 | Response cache duration |

## 📝 Usage Examples

### Example 1: Basic Windows Deployment

```powershell
.\deploy-closed-network.ps1 `
    -DeploymentType Windows `
    -FileStoragePath "D:\SharedFiles" `
    -OutputPath ".\deploy-windows"
```

**Result:**
- Self-contained Windows executable
- No authentication required
- Files stored in D:\SharedFiles
- Default image processing settings
- Package created in .\deploy-windows

### Example 2: Windows with Active Directory

```powershell
.\deploy-closed-network.ps1 `
    -DeploymentType Windows `
    -EnableAuthentication $true `
    -FileStoragePath "D:\Files" `
    -ADDomain "corporate.local" `
    -ADLdapPath "LDAP://dc01.corporate.local" `
    -ADContainer "DC=corporate,DC=local" `
    -JwtExpirationMinutes 240 `
    -OutputPath ".\deploy-windows-ad"
```

**Result:**
- Windows deployment with AD authentication
- 4-hour JWT token expiration
- Custom AD configuration

### Example 3: Custom Image Processing

```powershell
.\deploy-closed-network.ps1 `
    -DeploymentType Windows `
    -FileStoragePath "E:\Images" `
    -ThumbnailMaxWidth 200 `
    -ThumbnailMaxHeight 200 `
    -MobileMaxWidth 1024 `
    -MobileMaxHeight 1024 `
    -CompressionQuality 85 `
    -CacheDurationSeconds 7200 `
    -OutputPath ".\deploy-custom"
```

**Result:**
- Larger thumbnails (200x200)
- Larger mobile images (1024x1024)
- Higher quality (85%)
- Longer cache (2 hours)

### Example 4: Linux Production Server

```powershell
.\deploy-closed-network.ps1 `
    -DeploymentType Linux `
    -FileStoragePath "/var/www/files" `
    -ThumbnailMaxWidth 100 `
    -MobileMaxWidth 600 `
    -CompressionQuality 70 `
    -AllowedExtensions @(".png", ".jpg", ".jpeg", ".webp") `
    -OutputPath ".\deploy-linux-prod"
```

**Result:**
- Linux x64 self-contained binary
- Optimized for bandwidth (smaller sizes, lower quality)
- Only common image formats allowed

### Example 5: Docker with Custom Settings

```powershell
.\deploy-closed-network.ps1 `
    -DeploymentType Docker `
    -FileStoragePath "/app/Files" `
    -EnableAuthentication $true `
    -ADDomain "internal.local" `
    -MobileMaxWidth 800 `
    -CompressionQuality 75 `
    -OutputPath ".\deploy-docker-custom"
```

**Result:**
- Docker deployment with AD authentication
- Custom image processing settings
- Ready to build and save image

## 🔄 Deployment Process

### Phase 1: Build (Internet-Connected Machine)

1. **Run deployment script:**
```powershell
.\deploy-quick.ps1
# or
.\deploy-closed-network.ps1 [parameters]
```

2. **Script will:**
   - Build the application
   - Apply your configuration
   - Create deployment package
   - Generate run scripts
   - Create documentation

3. **Output package contains:**
   - Application binaries
   - Configuration file (appsettings.Production.json)
   - Run script (run.bat or run.sh)
   - Deployment guide
   - Summary file

### Phase 2: Transfer (Copy to Closed Network)

1. **Copy deployment package:**
```powershell
# Windows
Copy-Item -Path ".\deploy-package" -Destination "\\server\share\" -Recurse

# Or compress first
Compress-Archive -Path ".\deploy-package" -DestinationPath ".\file-api-deployment.zip"
```

2. **Transfer to closed network** using:
   - USB drive
   - CD/DVD
   - Secure file transfer
   - Network share

### Phase 3: Deploy (Closed Network Machine)

**Windows:**
```powershell
# Extract package
Expand-Archive -Path "file-api-deployment.zip" -DestinationPath "C:\FileApi"

# Review configuration
notepad C:\FileApi\publish\appsettings.Production.json

# Run application
cd C:\FileApi\publish
.\run.bat
```

**Linux:**
```bash
# Extract package
unzip file-api-deployment.zip -d /opt/file-api

# Review configuration
nano /opt/file-api/publish/appsettings.Production.json

# Make script executable
chmod +x /opt/file-api/publish/run.sh

# Run application
cd /opt/file-api/publish
./run.sh
```

**Docker:**
```bash
# Load image
docker load -i file-to-api.tar

# Run with docker-compose
docker-compose up -d

# Or run directly
docker run -d -p 5000:80 -v /files:/app/Files file-to-api:v1.0.0
```

## 🎯 Post-Deployment Verification

### 1. Check Health
```bash
curl http://localhost:5000/health
```

### 2. View Swagger Documentation
Open browser: `http://localhost:5000/swagger`

### 3. Test File Access
```bash
# Place a test file
copy test.jpg C:\Files\

# Access via API
curl http://localhost:5000/img/test.jpg -o downloaded.jpg
```

### 4. Test Image Processing
```bash
# Get thumbnail
curl http://localhost:5000/img/thumbnail/test.jpg -o thumbnail.jpg

# Get mobile version
curl http://localhost:5000/img/mobile/test.jpg?quality=80 -o mobile.jpg
```

## 🔐 Security Configuration

### No Authentication (Development/Testing)
```json
{
  "Authentication": {
    "Enabled": false
  }
}
```

### With Active Directory (Production)
```json
{
  "Authentication": {
    "Enabled": true
  },
  "Authorization": {
    "AllowedUsers": ["user1", "user2"],
    "AllowedGroups": ["IT-Team", "Developers"]
  },
  "ActiveDirectory": {
    "Domain": "company.local",
    "LdapPath": "LDAP://dc01.company.local",
    "Container": "DC=company,DC=local"
  }
}
```

## 📊 Configuration Management

### Update Configuration After Deployment

**Method 1: Edit appsettings.json**
```powershell
notepad .\appsettings.Production.json
# Restart application
```

**Method 2: Environment Variables**
```powershell
# Windows
set FileStorage__RootPath=D:\NewPath
set ImageProcessing__CompressionQuality=90

# Linux
export FileStorage__RootPath=/new/path
export ImageProcessing__CompressionQuality=90
```

**Method 3: Command Line (for testing)**
```bash
.\FileToApi.exe --FileStorage:RootPath="D:\Temp"
```

## 🛠️ Troubleshooting

### Build Issues

**Problem:** .NET SDK not found
```powershell
# Install .NET 8.0 SDK
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
```

**Problem:** Build fails
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
# Then run deployment script again
```

### Deployment Issues

**Problem:** Application won't start on closed network
- Ensure you used `-SelfContained $true`
- Check server meets minimum requirements (Windows Server 2016+, Linux x64)

**Problem:** Cannot access Swagger UI
- Check firewall allows port 5000
- Verify application is running: `netstat -an | findstr 5000`
- Try accessing from localhost first

**Problem:** Files not found
- Verify FileStoragePath exists
- Check permissions: application needs read access
- Ensure files are in correct location

### Authentication Issues

**Problem:** AD authentication fails
- Verify domain controller is accessible
- Check LDAP path is correct
- Ensure server can resolve domain names
- Test with: `nltest /dsgetdc:domain.local`

## 📈 Performance Tuning

### For High Traffic
```powershell
.\deploy-closed-network.ps1 `
    -CacheDurationSeconds 7200 `
    -CompressionQuality 70 `
    -MobileMaxWidth 600
```
- Longer cache duration
- Lower quality (smaller files)
- Smaller image sizes

### For High Quality
```powershell
.\deploy-closed-network.ps1 `
    -CompressionQuality 90 `
    -MobileMaxWidth 1920 `
    -ThumbnailMaxWidth 300
```
- Higher quality
- Larger images
- Better thumbnails

### For Low Bandwidth Networks
```powershell
.\deploy-closed-network.ps1 `
    -CompressionQuality 60 `
    -MobileMaxWidth 480 `
    -ThumbnailMaxWidth 100
```
- Very small files
- Optimized for slow connections

## 🔄 Updating the Application

### Option 1: Rebuild and Redeploy
1. Run deployment script with new settings
2. Transfer new package
3. Stop old application
4. Replace files
5. Start new application

### Option 2: In-Place Update
1. Build new version
2. Stop application
3. Backup current files
4. Copy new binaries
5. Keep existing appsettings.Production.json
6. Start application

## 📚 Additional Resources

- Full API documentation in README.md
- Swagger UI: http://localhost:5000/swagger
- Health check: http://localhost:5000/health
- Logs directory: ./logs/

## 💡 Pro Tips

1. **Always test locally first** before deploying to closed network
2. **Document your settings** - save the deployment command you used
3. **Keep a backup** of the working deployment package
4. **Version your deployments** - include version in folder name
5. **Use Docker** for easiest deployment and rollback
6. **Enable logging** to troubleshoot issues
7. **Test without authentication first** then add it later

## 🆘 Getting Help

For issues or questions:
1. Check DEPLOYMENT-GUIDE.txt in deployment package
2. Review logs in ./logs/ directory
3. Verify configuration in appsettings.Production.json
4. Check health endpoint: /health
5. Review Swagger docs: /swagger

---

**Remember:** Everything works completely offline once deployed!
