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
    "RootPath": "Files",
    "MaxFileSize": 52428800,
    "AllowedExtensions": [".png", ".jpg", ".jpeg", ".gif", ".pdf", ".txt", ".json"]
  }
}
```

- `RootPath`: Directory where files are stored (read-only access)
- `MaxFileSize`: Not applicable for read-only API (kept for compatibility)
- `AllowedExtensions`: Not applicable for read-only API (kept for compatibility)

**Note**: Files must be placed in the `RootPath` directory manually or through other means. This API only provides read access to existing files.

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

#### List All Files
```
GET /api/files
```

Returns metadata for all stored files.

#### Get File
```
GET /api/files/{fileName}
```

Downloads the specified file.

#### Get File Metadata
```
GET /api/files/{fileName}/metadata
```

Returns metadata for a specific file (size, content type, dates).

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

### List All Files

```bash
curl https://localhost:5001/api/files
```

### List All Files (with authentication)

```bash
TOKEN="your-jwt-token-here"
curl https://localhost:5001/api/files \
  -H "Authorization: Bearer $TOKEN"
```

### Download a File

```bash
curl https://localhost:5001/api/files/example.png \
  -o downloaded-file.png
```

### Get File Metadata

```bash
curl https://localhost:5001/api/files/example.png/metadata
```

## Environment Variables

You can override configuration using environment variables:

```bash
export Authentication__Enabled=false
export FileStorage__MaxFileSize=104857600
dotnet run --project src/FileToApi
```

## Production Deployment

1. Deploy on Windows Server joined to your Active Directory domain
2. Update `appsettings.json` with production settings:
   - Configure your Active Directory domain settings
   - Generate a strong JWT secret key (min 32 characters)
   - Enable authentication
3. Set appropriate CORS policies (restrict to specific origins)
4. Configure HTTPS certificates
5. Ensure the application pool identity has permissions to query Active Directory
6. Consider using a network file share or cloud storage instead of local file storage

### Publishing

```bash
dotnet publish -c Release -o ./publish
```

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
