# File to API - .NET

A production-ready REST API for file management with configurable JWT and Azure Active Directory authentication.

## Features

- RESTful API for file operations (upload, download, list, delete)
- Configurable authentication (enable/disable via config)
- Support for both Azure AD and custom JWT authentication
- File type validation and size limits
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
    "Enabled": true,
    "Type": "AzureAD"
  }
}
```

- `Enabled`: Set to `true` to require authentication, `false` to disable
- `Type`: Choose `"AzureAD"` or `"JWT"`

### Azure AD Configuration

If using Azure AD authentication, configure these settings:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "Audience": "api://your-client-id"
  }
}
```

### Custom JWT Configuration

If using custom JWT authentication, configure these settings:

```json
{
  "JwtBearer": {
    "Issuer": "https://your-issuer.com",
    "Audience": "your-audience",
    "SecretKey": "your-secret-key-min-32-characters-long"
  }
}
```

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

- `RootPath`: Directory for file storage
- `MaxFileSize`: Maximum file size in bytes (default: 50MB)
- `AllowedExtensions`: List of allowed file extensions

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- (Optional) Azure AD tenant for Azure AD authentication

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

### List All Files
```
GET /api/files
```

Returns metadata for all stored files.

### Get File
```
GET /api/files/{fileName}
```

Downloads the specified file.

### Get File Metadata
```
GET /api/files/{fileName}/metadata
```

Returns metadata for a specific file (size, content type, dates).

### Upload File
```
POST /api/files
Content-Type: multipart/form-data

file: <binary>
```

Uploads a new file. Returns the generated filename and URL.

### Delete File
```
DELETE /api/files/{fileName}
```

Deletes the specified file.

## Authentication

When authentication is enabled, include the JWT token in the Authorization header:

```
Authorization: Bearer <your-token>
```

### Getting a Token (Azure AD)

Use the Azure AD OAuth 2.0 flow to obtain a token. Example using client credentials:

```bash
curl -X POST https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id={client-id}" \
  -d "client_secret={client-secret}" \
  -d "scope={audience}/.default" \
  -d "grant_type=client_credentials"
```

### Testing Without Authentication

Set `"Enabled": false` in the Authentication section of `appsettings.json` or `appsettings.Development.json`.

## Examples

### Upload a File (curl)

```bash
curl -X POST https://localhost:5001/api/files \
  -F "file=@/path/to/image.png"
```

### Upload with Authentication

```bash
curl -X POST https://localhost:5001/api/files \
  -H "Authorization: Bearer <token>" \
  -F "file=@/path/to/image.png"
```

### Download a File

```bash
curl https://localhost:5001/api/files/{fileName} \
  -o downloaded-file.png
```

### List All Files

```bash
curl https://localhost:5001/api/files
```

## Environment Variables

You can override configuration using environment variables:

```bash
export Authentication__Enabled=false
export FileStorage__MaxFileSize=104857600
dotnet run --project src/FileToApi
```

## Production Deployment

1. Update `appsettings.json` with production settings
2. Enable authentication and configure Azure AD or JWT
3. Set appropriate CORS policies
4. Configure HTTPS certificates
5. Consider using a cloud storage service instead of local file storage

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

- Verify Azure AD configuration (TenantId, ClientId, Audience)
- Check token expiration
- Ensure Bearer token is correctly formatted

### File Upload Issues

- Check file size limits
- Verify file extension is allowed
- Ensure Files directory exists and has write permissions

## License

MIT License

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.
