# File to API - .NET

A production-ready **Image & File API** with advanced image processing capabilities, built on .NET 8. Perfect for mobile apps, web applications, and microservices architectures.

## 🚀 Features

### Core Features
- **RESTful API** for file operations (download, metadata, base64 encoding)
- **Image Processing** - Thumbnails, compression, and mobile optimization
- **WebP Support** - Modern image format with superior compression
- **Batch Operations** - Load multiple files in a single request
- **Response Caching** - Built-in caching for optimal performance
- **Gzip Compression** - Automatic response compression
- **Auto Extension Detection** - Files work with or without extensions
- **Health Checks** - Monitoring endpoints for load balancers

### Security & Authentication
- **JWT Authentication** - Token-based security
- **Active Directory Integration** - LDAP authentication
- **Rate Limiting** - Protection against abuse
- **Refresh Tokens** - Long-lived sessions
- **User/Group Authorization** - Fine-grained access control
- **Path Sanitization** - Security against directory traversal

### Developer Experience
- **Swagger/OpenAPI** - Interactive API documentation
- **CORS Support** - Cross-origin resource sharing
- **Comprehensive Logging** - Detailed error tracking
- **Docker Ready** - Easy containerization
- **IIS Compatible** - Windows Server deployment

## 📋 API Endpoints

### Single File Endpoints (GET)

| Endpoint | Format | Description | Example |
|----------|--------|-------------|---------|
| `/img/{path}` | Binary | Get raw file | `/img/photo.jpg` |
| `/img/{path}/metadata` | JSON | Get file metadata | `/img/photo.jpg/metadata` |
| `/img/base64/{path}` | JSON + Base64 | Get file as base64 | `/img/base64/photo` |
| `/img/thumbnail/{path}` | Binary | Get 150x150 thumbnail | `/img/thumbnail/photo.jpg` |
| `/img/base64/thumbnail/{path}` | JSON + Base64 | Thumbnail as base64 | `/img/base64/thumbnail/photo` |
| `/img/mobile/{path}` | Binary | Mobile-optimized (800x800) | `/img/mobile/photo.jpg?quality=80` |
| `/img/base64/mobile/{path}` | JSON + Base64 | Mobile image as base64 | `/img/base64/mobile/photo?quality=80` |

### Batch Endpoints (POST)

| Endpoint | Purpose | Request Body |
|----------|---------|--------------|
| `POST /img/batch/base64` | Multiple files as base64 | `{"filePaths": ["photo1.jpg", "photo2"]}` |
| `POST /img/batch/thumbnail` | Multiple thumbnails | `{"filePaths": ["photo1", "photo2"]}` |
| `POST /img/batch/mobile` | Multiple mobile images | `{"filePaths": ["photo1.jpg", "photo2"]}` |

### System Endpoints

| Endpoint | Purpose |
|----------|---------|
| `GET /health` | Health check for monitoring |
| `GET /swagger` | Interactive API documentation |
| `POST /api/auth/login` | Authenticate with Active Directory |
| `POST /api/auth/refresh` | Refresh JWT token |
| `GET /api/auth/status` | Check authentication status |

## 🎨 Image Processing Features

### Thumbnails
- **Size:** 150x150px (configurable)
- **Aspect Ratio:** Maintained automatically
- **Format:** Preserves original format (JPEG, PNG, WebP, GIF)
- **Quality:** Configurable compression

### Mobile Optimization
- **Default Size:** 800x800px (configurable)
- **Smart Resizing:** Only resizes if image is larger
- **Quality Control:** Adjustable JPEG/WebP quality (1-100)
- **Format Support:** JPEG, PNG, WebP, GIF

### Batch Operations
```json
POST /img/batch/mobile?maxWidth=600&maxHeight=600&quality=85
{
  "filePaths": ["user1/avatar", "user2/avatar", "user3/avatar"]
}

Response:
{
  "files": [
    {
      "requestedPath": "user1/avatar",
      "fileName": "avatar.jpg",
      "contentType": "image/jpeg",
      "base64Data": "iVBORw0KGgoAAAA...",
      "found": true,
      "error": null
    }
  ],
  "totalRequested": 3,
  "totalFound": 3,
  "totalNotFound": 0
}
```

## ⚙️ Configuration

### appsettings.json

```json
{
  "Authentication": {
    "Enabled": false
  },
  "Authorization": {
    "AllowedUsers": ["user1", "user2"],
    "AllowedGroups": ["Domain Admins", "File-Readers"]
  },
  "ActiveDirectory": {
    "Domain": "domain.local",
    "LdapPath": "LDAP://domain.local",
    "Container": "DC=domain,DC=local"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here-min-32-chars",
    "Issuer": "your-issuer",
    "Audience": "file-api",
    "ExpirationMinutes": 120,
    "RefreshTokenExpirationDays": 7
  },
  "FileStorage": {
    "RootPath": "C:\\Files",
    "MaxFileSize": 52428800,
    "AllowedExtensions": [".png", ".jpg", ".jpeg", ".webp", ".gif"]
  },
  "ImageProcessing": {
    "ThumbnailMaxWidth": 150,
    "ThumbnailMaxHeight": 150,
    "MobileMaxWidth": 800,
    "MobileMaxHeight": 800,
    "CompressionQuality": 75,
    "CacheDurationSeconds": 3600,
    "EnableResponseCaching": true
  }
}
```

## 🚀 Getting Started

### Prerequisites
- **.NET 8.0 SDK** or later (for building)
- **Windows Server** with Active Directory (optional, for authentication)
- **PowerShell** (for deployment scripts)

### Quick Start (Development)

1. **Clone the repository:**
```bash
git clone https://github.com/yourusername/file-to-api-dotnet.git
cd file-to-api-dotnet
```

2. **Build the project:**
```bash
dotnet build
```

3. **Run the application:**
```bash
dotnet run --project src/FileToApi
```

4. **Access the API:**
- Swagger UI: `https://localhost:5001/swagger`
- Health Check: `https://localhost:5001/health`

### Quick Start (Closed Network Deployment)

For deploying to **closed/air-gapped networks** without internet:

1. **Run deployment script** (on internet-connected machine):
```powershell
.\deploy-quick.ps1
```

2. **Select deployment type:**
   - Windows Server (No Authentication)
   - Windows Server (With Active Directory)
   - Linux Server
   - Docker Container

3. **Transfer package** to closed network

4. **Deploy and run** on target server

**📖 Complete deployment guide:** See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed instructions and all configuration options.

## 📱 Mobile App Integration

### Loading a User Gallery
```javascript
// Load multiple thumbnails in one request
POST /img/batch/thumbnail
{
  "filePaths": ["user/photo1", "user/photo2", "user/photo3", "user/photo4"]
}

// Returns all 4 thumbnails as base64 in one response
// No extensions needed - auto-detection included!
```

### Feed with Images
```javascript
// Load mobile-optimized images for a feed
POST /img/batch/mobile?quality=85
{
  "filePaths": ["posts/post1", "posts/post2", "posts/post3"]
}

// Returns compressed images perfect for mobile scrolling
```

### Profile Picture
```javascript
// Get single thumbnail
GET /img/base64/thumbnail/users/john-doe

// Response:
{
  "fileName": "john-doe.jpg",
  "contentType": "image/jpeg",
  "base64Data": "iVBORw0KGgoAAAANSUhEUg..."
}
```

## 🔐 Authentication

### Login (Active Directory)
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "your-ad-username",
    "password": "your-password"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-here",
  "username": "jdoe",
  "displayName": "John Doe",
  "email": "jdoe@company.com",
  "expiresAt": "2025-10-30T10:30:00Z"
}
```

### Using the Token
```bash
curl https://localhost:5001/img/photo.jpg \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -o photo.jpg
```

### Refresh Token
```bash
curl -X POST https://localhost:5001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your-refresh-token"
  }'
```

## 📊 Performance Features

### Response Caching
- **Duration:** 1 hour (configurable)
- **Cache Headers:** Automatic `Cache-Control` headers
- **Vary By:** Path and query parameters
- **Benefits:** Reduced server load, faster responses

### Gzip Compression
- **Automatic:** All JSON/text responses compressed
- **HTTPS Compatible:** Works with SSL/TLS
- **Savings:** 60-80% bandwidth reduction for base64 responses

### Parallel Processing
- Batch operations process files concurrently
- Maximum performance for multi-file requests
- Individual file errors don't fail entire batch

## 🐳 Docker Deployment

### Dockerfile
```dockerfile
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
```

### Docker Compose
```yaml
version: '3.8'
services:
  file-api:
    image: file-to-api:latest
    ports:
      - "5000:80"
      - "5001:443"
    volumes:
      - ./files:/app/Files
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Authentication__Enabled=false
      - FileStorage__RootPath=/app/Files
```

## 🏢 IIS Deployment

### Prerequisites
1. Windows Server with IIS
2. ASP.NET Core Hosting Bundle
3. .NET 8.0 Runtime

### Steps
1. **Publish the application:**
```bash
dotnet publish -c Release -o ./publish
```

2. **Create IIS Application Pool:**
   - Name: `FileToApiAppPool`
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated

3. **Create Website/Application:**
   - Physical path: Point to publish folder
   - Application Pool: `FileToApiAppPool`
   - Binding: Configure HTTPS

4. **Set Permissions:**
```powershell
icacls "C:\publish" /grant "IIS AppPool\FileToApiAppPool:(OI)(CI)RX"
icacls "C:\Files" /grant "IIS AppPool\FileToApiAppPool:(OI)(CI)R"
```

## 🔧 Troubleshooting

### Image Processing Issues
- Ensure sufficient memory for large images
- Check file format is supported (JPEG, PNG, WebP, GIF)
- Verify ImageSharp packages are installed

### Authentication Issues
- Verify Active Directory configuration
- Ensure server is domain-joined
- Check JWT secret key length (min 32 chars)
- Verify token expiration settings

### Performance Issues
- Enable response caching in config
- Use batch endpoints for multiple files
- Consider CDN for static files
- Monitor with `/health` endpoint

## 📈 Monitoring

### Health Check
```bash
curl https://localhost:5001/health
```

### Metrics (via logs)
- File access counts
- Image processing times
- Batch operation statistics
- Cache hit rates
- Authentication attempts

## 🔒 Security Best Practices

- ✅ Enable authentication in production
- ✅ Use HTTPS everywhere
- ✅ Rotate JWT secret keys regularly
- ✅ Implement rate limiting (included)
- ✅ Restrict CORS to specific origins
- ✅ Use refresh tokens for long sessions
- ✅ Monitor authentication logs
- ✅ Keep .NET runtime updated

## 📚 Additional Resources

- [Swagger Documentation](https://localhost:5001/swagger) - Interactive API docs
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [ImageSharp Documentation](https://docs.sixlabors.com/articles/imagesharp/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)

## 🤝 Contributing

Contributions are welcome! Please open an issue or submit a pull request.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

MIT License - See LICENSE file for details

## 🙏 Acknowledgments

- Built with [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- Image processing by [ImageSharp](https://sixlabors.com/products/imagesharp/)
- API documentation by [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

---

**Made with ❤️ for modern web and mobile applications**
