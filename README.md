# File to API - .NET

A production-ready read-only REST API for serving files with configurable Microsoft Active Directory (LDAP) authentication.

## Features

- RESTful API for file operations (list, download, view metadata)
- Read-only access - no upload or delete capabilities
- Configurable authentication (enable/disable via config)
- Microsoft Active Directory (LDAP) authentication with JWT tokens
- User and group information from Active Directory
- Comprehensive error handling and logging
- Swagger/OpenAPI documentation
- CORS support

## Project Structure

```
src/FileToApi/
├── Controllers/         - API endpoints
├── Services/           - Business logic
├── Models/             - Data models and settings
├── Attributes/         - Custom attributes
├── Files/              - File storage location
└── appsettings.json    - Configuration
```

## Configuration

### Authentication Settings

Edit `appsettings.json` to configure authentication:

```json
{
  "Authentication": {
    "Enabled": true
  }
}
```

- `Enabled`: Set to `true` to require authentication, `false` to disable

### Active Directory Configuration

Configure your on-premises Active Directory settings:

```json
{
  "ActiveDirectory": {
    "Domain": "yourdomain.local",
    "LdapPath": "LDAP://yourdomain.local",
    "Container": "DC=yourdomain,DC=local"
  }
}
```

- `Domain`: Your Active Directory domain name
- `LdapPath`: LDAP connection string to your domain controller
- `Container`: The distinguished name (DN) of your AD container

### JWT Token Configuration

Configure JWT token settings for authenticated sessions:

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-must-be-at-least-32-characters-long",
    "Issuer": "https://yourcompany.com",
    "Audience": "file-api",
    "ExpirationMinutes": 60
  }
}
```

- `SecretKey`: Secret key for signing JWT tokens (min 32 characters)
- `Issuer`: Token issuer identifier
- `Audience`: Token audience identifier
- `ExpirationMinutes`: Token validity duration

### File Storage Settings

```json
{
  "FileStorage": {
    "RootPath": "C:\\SharedFiles",
    "MaxFileSize": 52428800,
    "AllowedExtensions": [".png", ".jpg", ".jpeg", ".gif", ".pdf", ".txt", ".json"]
  }
}
```

- `RootPath`: Directory where files are stored (supports both absolute and relative paths)
  - **Absolute path**: `C:\\SharedFiles` or `D:\\Data\\Files`
  - **Relative path**: `Files` (relative to application directory)
  - **UNC path**: `\\\\ServerName\\SharedFolder\\Files`
- `MaxFileSize`: Not applicable for read-only API (kept for compatibility)
- `AllowedExtensions`: Not applicable for read-only API (kept for compatibility)

**Note**: Files must be placed in the `RootPath` directory manually or through other means. This API only provides read access to existing files.

### Folder Structure

You can organize files in subdirectories:

```
Files/
├── images/
│   ├── test.png
│   └── test2.png
├── documents/
│   └── report.pdf
└── root-file.txt
```

Access files using their relative path:
- `/img/photos/vacation.jpg`
- `/img/logos/company.png`
- `/img/banner.jpg`

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Windows Server with Active Directory (for authentication)
- Server must be domain-joined (if authentication is enabled)

### Running the Application

1. Clone the repository:
```bash
git clone https://github.com/yourusername/file-to-api-dotnet.git
cd file-to-api-dotnet
```

2. Build the project:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run --project src/FileToApi
```

4. Access the API:
- Swagger UI: https://localhost:5001/swagger
- API Base URL: https://localhost:5001/api

### Development Mode

By default, authentication is disabled in development mode (see `appsettings.Development.json`).

To enable authentication in development:
```json
{
  "Authentication": {
    "Enabled": true
  }
}
```

## API Endpoints

### Authentication

#### Login
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "your-ad-username",
  "password": "your-password"
}
```

Returns a JWT token for authenticated requests. The username should be the Active Directory SAM account name (e.g., "jdoe" not "jdoe@domain.com").

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "jdoe",
  "expiresAt": "2025-10-30T10:30:00Z"
}
```

#### Check Auth Status
```
GET /api/auth/status
```

Returns current authentication configuration status.

### File Operations

#### Get File
```
GET /img/{filePath}
```

Downloads the specified file. Supports subdirectories.

Examples:
- `GET /img/photo.jpg` - File in root
- `GET /img/gallery/photo.jpg` - File in gallery folder
- `GET /img/2024/vacation/beach.png` - Nested folders

#### Get File Metadata
```
GET /img/{filePath}/metadata
```

Returns metadata for a specific file (size, content type, dates).

Examples:
- `GET /img/photo.jpg/metadata`
- `GET /img/gallery/photo.jpg/metadata`

## Authentication

When authentication is enabled, include the JWT token in the Authorization header:

```
Authorization: Bearer <your-token>
```

### Getting a Token (Active Directory)

Login with your Active Directory credentials to obtain a JWT token:

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "your-ad-username",
    "password": "your-password"
  }'
```

The API will validate your credentials against Active Directory and return a JWT token that includes your AD groups as roles.

### Testing Without Authentication

Set `"Enabled": false` in the Authentication section of `appsettings.json` or `appsettings.Development.json`.

## Examples

### Login to Get Token

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"jdoe","password":"yourpassword"}'
```

### Download a File (from root)

```bash
curl https://localhost:5001/img/photo.jpg \
  -o photo.jpg
```

### Download a File (from subdirectory)

```bash
curl https://localhost:5001/img/gallery/vacation.jpg \
  -o vacation.jpg
```

### Download with Authentication

```bash
TOKEN="your-jwt-token-here"
curl https://localhost:5001/img/photo.jpg \
  -H "Authorization: Bearer $TOKEN" \
  -o photo.jpg
```

### Get File Metadata

```bash
curl https://localhost:5001/img/photo.jpg/metadata
curl https://localhost:5001/img/gallery/vacation.jpg/metadata
```

## Environment Variables

You can override configuration using environment variables:

```bash
export Authentication__Enabled=false
export FileStorage__MaxFileSize=104857600
dotnet run --project src/FileToApi
```

## Production Deployment

### IIS Deployment

#### Prerequisites
1. Windows Server joined to your Active Directory domain
2. IIS installed with ASP.NET Core Hosting Bundle
3. .NET 8.0 Runtime installed
4. Shared folder or drive configured for file storage

#### Step 1: Publish the Application

```bash
dotnet publish -c Release -o ./publish
```

#### Step 2: Prepare the File Storage Directory

Create your file storage directory and set permissions:

```powershell
# Create directory
New-Item -Path "D:\SharedFiles" -ItemType Directory

# Grant IIS AppPool identity read permissions
icacls "D:\SharedFiles" /grant "IIS AppPool\YourAppPoolName:(OI)(CI)R"
```

#### Step 3: Configure appsettings.Production.json

Edit `appsettings.Production.json` in the publish folder:

```json
{
  "FileStorage": {
    "RootPath": "D:\\SharedFiles"
  },
  "ActiveDirectory": {
    "Domain": "yourdomain.local",
    "LdapPath": "LDAP://dc01.yourdomain.local",
    "Container": "DC=yourdomain,DC=local"
  },
  "JwtSettings": {
    "SecretKey": "your-production-secret-key-at-least-32-characters"
  }
}
```

#### Step 4: Create IIS Application

1. Open IIS Manager
2. Create a new Application Pool:
   - Name: `FileToApiAppPool`
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated
   - Identity: Custom account with AD query permissions (or ApplicationPoolIdentity)
3. Create a new Website or Application:
   - Physical path: Point to your publish folder
   - Application Pool: Select `FileToApiAppPool`
   - Binding: Configure HTTPS binding

#### Step 5: Configure Application Pool Identity

For Active Directory authentication, the Application Pool identity needs AD permissions:

**Option 1: Use a domain service account**
```powershell
# In IIS Manager, set Application Pool Identity to:
# Custom Account -> domain\serviceaccount
```

**Option 2: Grant ApplicationPoolIdentity AD permissions**
```powershell
# Add computer account to AD users group with query permissions
```

#### Step 6: Set Folder Permissions

```powershell
# Grant IIS AppPool read access to publish folder
icacls "C:\inetpub\wwwroot\FileToApi" /grant "IIS AppPool\FileToApiAppPool:(OI)(CI)RX"

# Grant read access to shared files
icacls "D:\SharedFiles" /grant "IIS AppPool\FileToApiAppPool:(OI)(CI)R"
```

#### Step 7: Configure web.config (already included)

The `web.config` is included and pre-configured. You can modify it if needed:

```xml
<aspNetCore processPath="dotnet"
            arguments=".\FileToApi.dll"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  </environmentVariables>
</aspNetCore>
```

#### Step 8: Test the Deployment

1. Browse to `https://your-server/api/auth/status`
2. Check logs in `.\logs\` folder if issues occur
3. Verify file access: `https://your-server/img/photo.jpg`

### Configuration via web.config

You can override appsettings.json values in web.config:

```xml
<environmentVariables>
  <environmentVariable name="FileStorage__RootPath" value="D:\SharedFiles" />
  <environmentVariable name="Authentication__Enabled" value="true" />
  <environmentVariable name="ActiveDirectory__Domain" value="yourdomain.local" />
</environmentVariables>
```

### Troubleshooting IIS Deployment

- **500.19 Error**: Check web.config syntax and install ASP.NET Core Hosting Bundle
- **500.30 Error**: Verify .NET 8.0 Runtime is installed
- **403 Forbidden**: Check Application Pool identity has permissions
- **AD Authentication Fails**: Ensure AppPool identity can query Active Directory
- **File Not Found**: Verify RootPath exists and has correct permissions

## Security Considerations

- Always enable authentication in production
- Use HTTPS in production
- Rotate JWT secret keys regularly
- Implement rate limiting for file uploads
- Scan uploaded files for malware
- Configure appropriate file size limits
- Restrict CORS to specific origins in production

## Troubleshooting

### Authentication Issues

- Verify Active Directory configuration (Domain, LdapPath, Container)
- Ensure the server is domain-joined
- Check that the application has permissions to query Active Directory
- Verify username format (use SAM account name, not UPN)
- Check token expiration
- Ensure Bearer token is correctly formatted
- Verify JWT secret key is properly configured

### File Upload Issues

- Check file size limits
- Verify file extension is allowed
- Ensure Files directory exists and has write permissions

## License

MIT License

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.
