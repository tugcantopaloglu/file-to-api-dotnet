# Quick Reference Card

## 📦 Deployment Commands

### Interactive Menu (Easiest)
```powershell
.\deploy-quick.ps1
```

### One-Command Deployments

**Windows (No Auth):**
```powershell
.\deploy-closed-network.ps1 -DeploymentType Windows -FileStoragePath "D:\Files"
```

**Windows (With AD):**
```powershell
.\deploy-closed-network.ps1 -DeploymentType Windows -EnableAuthentication $true -FileStoragePath "D:\Files" -ADDomain "company.local"
```

**Linux:**
```powershell
.\deploy-closed-network.ps1 -DeploymentType Linux -FileStoragePath "/var/files"
```

**Docker:**
```powershell
.\deploy-closed-network.ps1 -DeploymentType Docker
```

## 🌐 API Endpoints

### Single Files
```
GET  /img/{path}                    - Raw file
GET  /img/{path}/metadata           - Metadata
GET  /img/base64/{path}             - Base64 JSON
GET  /img/thumbnail/{path}          - Thumbnail (150x150)
GET  /img/base64/thumbnail/{path}   - Thumbnail base64
GET  /img/mobile/{path}             - Mobile (800x800)
GET  /img/base64/mobile/{path}      - Mobile base64
```

### Batch Operations
```
POST /img/batch/base64      - Multiple files
POST /img/batch/thumbnail   - Multiple thumbnails
POST /img/batch/mobile      - Multiple mobile images
```

**Request Body:**
```json
{
  "filePaths": ["photo1.jpg", "photo2", "photo3.png"]
}
```

### System
```
GET  /health    - Health check
GET  /swagger   - API docs
POST /api/auth/login    - Login
POST /api/auth/refresh  - Refresh token
```

## ⚙️ Key Configuration

### Minimal (No Auth)
```json
{
  "Authentication": { "Enabled": false },
  "FileStorage": { "RootPath": "C:\\Files" }
}
```

### With Active Directory
```json
{
  "Authentication": { "Enabled": true },
  "ActiveDirectory": {
    "Domain": "company.local",
    "LdapPath": "LDAP://company.local",
    "Container": "DC=company,DC=local"
  }
}
```

### Image Processing
```json
{
  "ImageProcessing": {
    "ThumbnailMaxWidth": 150,
    "ThumbnailMaxHeight": 150,
    "MobileMaxWidth": 800,
    "MobileMaxHeight": 800,
    "CompressionQuality": 75,
    "CacheDurationSeconds": 3600
  }
}
```

## 🚀 Running the Application

**Windows:**
```cmd
.\run.bat
```

**Linux:**
```bash
./run.sh
```

**Docker:**
```bash
docker run -d -p 5000:80 -v /files:/app/Files file-to-api:v1.0.0
```

**Custom Port:**
```bash
set ASPNETCORE_URLS=http://0.0.0.0:8080
.\FileToApi.exe
```

## 🔍 Testing

**Health Check:**
```bash
curl http://localhost:5000/health
```

**Get File:**
```bash
curl http://localhost:5000/img/test.jpg -o test.jpg
```

**Get Thumbnail:**
```bash
curl http://localhost:5000/img/thumbnail/test.jpg -o thumb.jpg
```

**Get Mobile Image:**
```bash
curl "http://localhost:5000/img/mobile/test.jpg?quality=80" -o mobile.jpg
```

**Batch Request:**
```bash
curl -X POST http://localhost:5000/img/batch/base64 \
  -H "Content-Type: application/json" \
  -d '{"filePaths": ["photo1.jpg", "photo2.jpg"]}'
```

## 🔐 Authentication

**Login:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"jdoe","password":"password"}'
```

**Use Token:**
```bash
curl http://localhost:5000/img/photo.jpg \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -o photo.jpg
```

## 📊 Common Parameters

### deploy-closed-network.ps1
```powershell
-DeploymentType [Windows|Linux|Docker]
-SelfContained $true
-EnableAuthentication $false
-FileStoragePath "C:\Files"
-ThumbnailMaxWidth 150
-MobileMaxWidth 800
-CompressionQuality 75
-ADDomain "company.local"
-OutputPath ".\deploy-package"
```

### Deployment Script Shortcuts
```powershell
# Windows with custom storage
.\deploy-closed-network.ps1 -DeploymentType Windows -FileStoragePath "E:\Images"

# High quality images
.\deploy-closed-network.ps1 -CompressionQuality 90 -MobileMaxWidth 1920

# Low bandwidth optimization
.\deploy-closed-network.ps1 -CompressionQuality 60 -MobileMaxWidth 480

# Larger thumbnails
.\deploy-closed-network.ps1 -ThumbnailMaxWidth 200 -ThumbnailMaxHeight 200
```

## 🛠️ Troubleshooting

| Problem | Solution |
|---------|----------|
| Port 5000 in use | Change port: `set ASPNETCORE_URLS=http://0.0.0.0:8080` |
| Files not found | Check RootPath in config, verify permissions |
| Auth fails | Verify AD domain accessible, check LDAP path |
| Swagger not loading | Check firewall, try http://localhost:5000/swagger |
| Build fails | Run `dotnet restore` then rebuild |

## 📁 File Organization

```
Files/
├── users/
│   ├── john/
│   │   ├── avatar.jpg
│   │   └── photo.png
│   └── jane/
│       └── profile.jpg
└── shared/
    └── logo.png
```

**Access:**
```
/img/users/john/avatar.jpg
/img/users/john/avatar         (auto-detect extension)
/img/shared/logo.png
```

## 🎯 Performance Tips

**High Traffic:**
```json
{
  "ImageProcessing": {
    "CompressionQuality": 70,
    "CacheDurationSeconds": 7200
  }
}
```

**High Quality:**
```json
{
  "ImageProcessing": {
    "CompressionQuality": 90,
    "MobileMaxWidth": 1920
  }
}
```

**Low Bandwidth:**
```json
{
  "ImageProcessing": {
    "CompressionQuality": 60,
    "MobileMaxWidth": 480
  }
}
```

## 📚 Documentation Links

- **Full README:** [README.md](README.md)
- **Deployment Guide:** [DEPLOYMENT.md](DEPLOYMENT.md)
- **Swagger UI:** http://localhost:5000/swagger
- **Health Check:** http://localhost:5000/health

## ✅ Pre-Deployment Checklist

- [ ] Run deployment script on internet-connected machine
- [ ] Review generated appsettings.Production.json
- [ ] Test package locally if possible
- [ ] Transfer to closed network (USB/CD/network)
- [ ] Extract on target server
- [ ] Verify file storage path exists
- [ ] Run application
- [ ] Check health endpoint
- [ ] Test file access
- [ ] View Swagger documentation

## 🔄 Update Process

1. Build new package
2. Stop application
3. Backup current deployment
4. Replace files (keep config)
5. Start application
6. Verify health check

---

**Everything works offline! No internet required after deployment.**
