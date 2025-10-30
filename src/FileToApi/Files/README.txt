This folder contains files served by the File API.

Place your PNG, JPG, GIF, PDF, TXT, and JSON files in this directory.
You can organize files in subdirectories (e.g., images/, documents/, etc.)

FOLDER STRUCTURE EXAMPLE:
Files/
├── images/
│   ├── test.png
│   └── test2.png
├── documents/
│   └── report.pdf
└── root-file.txt

Files can be accessed via the API endpoints (READ-ONLY):
- GET /api/files - List all files (includes subdirectories)
- GET /api/files/test.png - Download file from root
- GET /api/files/images/test.png - Download file from subdirectory
- GET /api/files/images/test.png/metadata - Get file metadata

This API provides READ-ONLY access. Files must be added to this directory manually
or through other means (file copy, network share, etc.).

The API does not support file upload or deletion operations.
The API automatically scans all subdirectories recursively.
