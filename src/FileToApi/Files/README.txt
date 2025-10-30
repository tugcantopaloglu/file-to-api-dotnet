This folder contains files served by the File API.

Place your PNG, JPG, and JPEG files in this directory.
You can organize files in subdirectories (e.g., gallery/, photos/, etc.)

FOLDER STRUCTURE EXAMPLE:
Files/
├── gallery/
│   ├── photo1.jpg
│   └── photo2.png
├── vacation/
│   └── beach.jpg
└── logo.png

Files can be accessed via the API endpoints (READ-ONLY):
- GET /img/logo.png - Download file from root
- GET /img/gallery/photo1.jpg - Download file from subdirectory
- GET /img/gallery/photo1.jpg/metadata - Get file metadata

This API provides READ-ONLY access. Files must be added to this directory manually
or through other means (file copy, network share, etc.).

The API does not support file upload or deletion operations.
The API automatically scans all subdirectories recursively.
