# Security Improvements and New Features

This document outlines the security improvements and new features added to the File to API application.

## 🔐 Security Improvements

### 1. JWT Configuration with Environment Variables

**Problem:** JWT secret keys were hardcoded in appsettings.json files.

**Solution:**
- Changed default values to placeholders
- Added documentation for using environment variables in IIS

**How to configure in Production (IIS):**

In IIS Application Settings, add these environment variables:
```
JwtSettings__SecretKey = your-secure-random-secret-key-min-32-chars
JwtSettings__Issuer = https://yourcompany.com
JwtSettings__Audience = file-api
```

**How to configure in Development:**
```bash
# Windows
set JwtSettings__SecretKey=your-dev-secret-key
set JwtSettings__Issuer=https://localhost
set JwtSettings__Audience=file-api

# Linux/Mac
export JwtSettings__SecretKey="your-dev-secret-key"
export JwtSettings__Issuer="https://localhost"
export JwtSettings__Audience="file-api"
```

### 2. Path Traversal Protection (Enhanced)

**Protection:** The FileService already includes path traversal protection via the `IsPathSafe()` method.

**How it works:**
- Normalizes all file paths using `Path.GetFullPath()`
- Ensures requested files are within the configured storage root
- Prevents access to files outside the allowed directory using `StartsWith()` check

**Location:** `FileService.cs:173-179`

### 3. Rate Limiting for Login Endpoint

**Problem:** No protection against brute force login attempts.

**Solution:**
- Implemented in-memory rate limiting service
- Default: 5 attempts per 15 minutes per username
- Returns HTTP 429 (Too Many Requests) when limit exceeded
- Rate limit resets on successful login

**Configuration:**
```csharp
// In AuthController.cs
var isAllowed = await _rateLimitingService.IsAllowedAsync(
    rateLimitKey,
    maxAttempts: 5,
    window: TimeSpan.FromMinutes(15)
);
```

**Files Added:**
- `Services/IRateLimitingService.cs`
- `Services/RateLimitingService.cs`

### 4. Comprehensive Input Validation

**Improvements:**
- Added `ValidUsernameAttribute` for username validation
- Added DataAnnotations to `LoginRequest` model
- Username: 2-256 characters, alphanumeric + `._@-\`
- Password: 1-256 characters (required)
- Automatic validation via ModelState

**Files Added:**
- `Attributes/ValidUsernameAttribute.cs`

**Updated Files:**
- `Models/LoginRequest.cs`

## 🚀 New Features

### 5. Token Refresh Mechanism

**Feature:** Secure token refresh without re-entering credentials.

**How it works:**
1. User logs in and receives both JWT token and refresh token
2. JWT token expires in 60-120 minutes
3. Refresh token expires in 7 days
4. Client uses refresh token to get new JWT token without re-login
5. Old refresh token is revoked when new one is issued

**API Endpoints:**

**Login** - `POST /api/auth/login`
```json
{
  "username": "domain\\user",
  "password": "password"
}
```

Response:
```json
{
  "token": "eyJhbGc...",
  "refreshToken": "base64-encoded-refresh-token",
  "username": "domain\\user",
  "expiresAt": "2025-10-30T15:30:00Z",
  "refreshTokenExpiresAt": "2025-11-06T14:30:00Z"
}
```

**Refresh Token** - `POST /api/auth/refresh`
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

Response: Same as login response with new tokens

**Revoke Token** - `POST /api/auth/revoke` (requires authorization)
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

**Files Added:**
- `Models/RefreshToken.cs`
- `Models/RefreshTokenRequest.cs`
- `Services/IRefreshTokenService.cs`
- `Services/RefreshTokenService.cs`

**Updated Files:**
- `Models/JwtSettings.cs` - Added `RefreshTokenExpirationDays`
- `Models/LoginResponse.cs` - Added refresh token fields
- `Services/IJwtTokenService.cs` - Added `ValidateToken()` method
- `Services/JwtTokenService.cs` - Implemented token validation
- `Controllers/AuthController.cs` - Added refresh and revoke endpoints

**Configuration:**
```json
"JwtSettings": {
  "ExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7
}
```

### 6. Auto-Extension Detection for Images

**Feature:** Access images without specifying file extension.

**How it works:**
1. Request: `GET /img/myimage` (no extension)
2. System tries: `myimage.png`, `myimage.jpg`, `myimage.jpeg` (in order)
3. Returns first match found
4. Falls back to exact filename if provided with extension

**Examples:**
```
GET /img/avatar          -> Finds avatar.jpg automatically
GET /img/logo            -> Finds logo.png automatically
GET /img/photo.jpeg      -> Exact match (no auto-detection)
GET /img/subfolder/pic   -> Finds subfolder/pic.jpg
```

**Location:** `FileService.cs:72-103`

## 📝 Configuration Changes

### appsettings.json
```json
{
  "Authentication": {
    "Enabled": false  // Set to true to enable authentication
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here-min-32-chars",
    "Issuer": "your-issuer",
    "Audience": "file-api",
    "ExpirationMinutes": 120,
    "RefreshTokenExpirationDays": 7
  }
}
```

### appsettings.Production.json
```json
{
  "Authentication": {
    "Enabled": false  // ⚠️ IMPORTANT: Enable this in production!
  },
  "JwtSettings": {
    "SecretKey": "CHANGE-THIS-USE-ENV-VARIABLE"
  }
}
```

## 🔧 Service Registration

Added in `Program.cs`:
```csharp
builder.Services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<IRateLimitingService, RateLimitingService>();
```

## ⚠️ Important Security Notes

1. **Enable Authentication in Production:**
   - Set `"Authentication": { "Enabled": true }` in `appsettings.Production.json`

2. **Use Environment Variables for Secrets:**
   - Never commit real JWT secret keys to source control
   - Use IIS Application Settings or Azure Key Vault for production

3. **Refresh Token Storage:**
   - Current implementation uses in-memory storage
   - Tokens are lost on application restart
   - For production, consider using Redis or database storage

4. **Rate Limiting Storage:**
   - Current implementation uses in-memory storage
   - For load-balanced environments, use distributed cache (Redis)

5. **HTTPS Only:**
   - Always use HTTPS in production
   - JWT tokens should never be transmitted over HTTP

## 🧪 Testing the New Features

### Test Rate Limiting
```bash
# Try logging in 6 times with wrong password
for i in {1..6}; do
  curl -X POST http://localhost:5000/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"username":"test","password":"wrong"}'
done
# 6th request should return HTTP 429
```

### Test Token Refresh
```bash
# 1. Login
TOKEN_RESPONSE=$(curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"domain\\user","password":"password"}')

# 2. Extract refresh token
REFRESH_TOKEN=$(echo $TOKEN_RESPONSE | jq -r '.refreshToken')

# 3. Refresh the token
curl -X POST http://localhost:5000/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\":\"$REFRESH_TOKEN\"}"
```

### Test Auto-Extension Detection
```bash
# Assuming you have avatar.jpg in C:\Test
curl http://localhost:5000/img/avatar
# Should return the image even without .jpg extension
```

## 📚 Next Steps (Recommendations)

1. **Implement persistent refresh token storage** (Database/Redis)
2. **Add distributed rate limiting** for load-balanced environments
3. **Implement token rotation policies** (force re-login after X days)
4. **Add audit logging** for security events
5. **Implement account lockout** after repeated failed attempts
6. **Add MFA (Multi-Factor Authentication)** support
7. **Implement CORS whitelist** instead of `AllowAnyOrigin()`
8. **Add request/response logging middleware**
9. **Implement API versioning**
10. **Add health check endpoints**
