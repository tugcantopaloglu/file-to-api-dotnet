using FileToApi.Authorization;
using FileToApi.Models;
using FileToApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection("Authentication"));
builder.Services.Configure<AuthorizationSettings>(builder.Configuration.GetSection("Authorization"));
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<ActiveDirectorySettings>(builder.Configuration.GetSection("ActiveDirectory"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<ImageProcessingSettings>(builder.Configuration.GetSection("ImageProcessing"));

var authSettings = builder.Configuration.GetSection("Authentication").Get<AuthenticationSettings>();
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

if (authSettings?.Enabled == true)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings?.Issuer ?? throw new InvalidOperationException("JWT Issuer not configured"),
                ValidAudience = jwtSettings?.Audience ?? throw new InvalidOperationException("JWT Audience not configured"),
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? throw new InvalidOperationException("JWT SecretKey not configured")))
            };
        });

    builder.Services.AddAuthorization();
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserGroupPolicy", policy =>
        policy.Requirements.Add(new UserGroupAuthorizationRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, UserGroupAuthorizationHandler>();

builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<IRateLimitingService, RateLimitingService>();

builder.Services.AddHealthChecks();

builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "File & Image API",
        Version = "v1.0.0",
        Description = @"
# Advanced Image & File API

A production-ready REST API with powerful image processing capabilities, perfect for mobile apps and web applications.

## 🚀 Key Features

### Image Processing
- **Thumbnails**: 150x150px optimized previews
- **Mobile Optimization**: Compressed images for mobile devices (800x800px default)
- **WebP Support**: Modern format with 25-35% better compression
- **Batch Operations**: Load multiple images in a single request
- **Auto Extension Detection**: Works with or without file extensions

### Performance
- **Response Caching**: 1-hour cache duration
- **Gzip Compression**: Automatic compression for all responses
- **Parallel Processing**: Concurrent batch operations
- **Smart Resizing**: Only resizes when necessary

### Formats Supported
- JPEG, PNG, WebP, GIF
- Base64 encoding for easy embedding
- Binary file downloads
- JSON metadata responses

## 📱 Perfect for Mobile Apps

### Single Request Examples
```
GET /img/base64/thumbnail/user-avatar
GET /img/mobile/hero-image?quality=85
```

### Batch Request Example
```
POST /img/batch/mobile?quality=85
{
  ""filePaths"": [""photo1"", ""photo2"", ""photo3""]
}
```

## 🔐 Authentication

When enabled, authenticate via Active Directory and use JWT tokens:
1. POST to `/api/auth/login` with your credentials
2. Use the returned token in the `Authorization: Bearer {token}` header
3. Refresh tokens available for long-lived sessions

## 📊 Monitoring

- **Health Check**: `GET /health`
- **Response Headers**: Cache-Control headers included
- **Comprehensive Logging**: All operations logged

## 🎯 Quick Start

1. Try the `/health` endpoint to verify the API is running
2. Browse endpoints below to explore capabilities
3. Use 'Try it out' to test endpoints interactively
4. Check response schemas for detailed format information

For complete documentation, visit the [GitHub Repository](https://github.com/yourusername/file-to-api-dotnet)
",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "API Support",
            Email = "support@yourcompany.com",
            Url = new Uri("https://github.com/yourusername/file-to-api-dotnet")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    if (authSettings?.Enabled == true)
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. First login via /api/auth/login to get token. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseCors("AllowAll");

if (authSettings?.Enabled == true)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
