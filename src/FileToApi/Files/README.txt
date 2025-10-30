This folder contains files served by the File API.

You can add PNG, JPG, GIF, PDF, TXT, and JSON files here.

Files can be accessed via the API endpoints:
- GET /api/files - List all files
- GET /api/files/{filename} - Download a specific file
- GET /api/files/{filename}/metadata - Get file metadata
- POST /api/files - Upload a new file
- DELETE /api/files/{filename} - Delete a file

Maximum file size: 50MB (configurable in appsettings.json)
