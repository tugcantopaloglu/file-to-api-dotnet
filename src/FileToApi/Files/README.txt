This folder contains files served by the File API.

Place your PNG, JPG, GIF, PDF, TXT, and JSON files in this directory.

Files can be accessed via the API endpoints (READ-ONLY):
- GET /api/files - List all files
- GET /api/files/{filename} - Download a specific file
- GET /api/files/{filename}/metadata - Get file metadata

This API provides READ-ONLY access. Files must be added to this directory manually
or through other means (file copy, network share, etc.).

The API does not support file upload or deletion operations.
