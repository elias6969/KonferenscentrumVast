using KonferenscentrumVast.Data;
using KonferenscentrumVast.Repository.Implementations;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Services;
using KonferenscentrumVast.Exceptions;
using KonferenscentrumVast.Swagger;
using KonferenscentrumVast.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Google.Cloud.SecretManager.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Logging ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// --- Secret Manager client (reused everywhere) ---
var secretClient = SecretManagerServiceClient.Create();

// --- Load secrets ---
string connectionString;
string jwtKey;
string bucketName;

if (builder.Environment.IsDevelopment())
{
    // Local dev from appsettings.Development.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    jwtKey = builder.Configuration["Jwt:Key"] ?? "local-dev-secret-change-me";
    bucketName = builder.Configuration["FileStorage:BucketName"] ?? "local-dev-bucket";
}
else
{
    // Cloud Run: load from Secret Manager
    var dbSecret = secretClient.AccessSecretVersion(
        new SecretVersionName("konferenscentrum-vast", "db-connection-string", "latest"));
    connectionString = dbSecret.Payload.Data.ToStringUtf8();

    var jwtSecret = secretClient.AccessSecretVersion(
        new SecretVersionName("konferenscentrum-vast", "jwt-key", "latest"));
    jwtKey = jwtSecret.Payload.Data.ToStringUtf8();

    var bucketSecret = secretClient.AccessSecretVersion(
        new SecretVersionName("konferenscentrum-vast", "file-bucket-name", "latest"));
    bucketName = bucketSecret.Payload.Data.ToStringUtf8();
}

// --- Register FileStorageOptions ---
builder.Services.AddSingleton(new FileStorageOptions
{
    BucketName = bucketName
});

// --- Database ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Repositories ---
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IFacilityRepository, FacilityRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingContractRepository, BookingContractRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();

// --- Application Services ---
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<FacilityService>();
builder.Services.AddScoped<BookingContractService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<FileService>();

// --- JWT Authentication ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger/OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Konferenscentrum Väst API",
        Version = "v1"
    });

    c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
    c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
    c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });

    c.OperationFilter<FileUploadOperationFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add JWT support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
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
            new string[] {}
        }
    });
});

// --- CORS ---
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("dev", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// --- Kestrel / Cloud Run port ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

// Order: exceptions -> HTTPS -> CORS -> Auth -> Controllers
app.UseExceptionMapping();
app.UseHttpsRedirection();
app.UseCors("dev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- Applies EF Core migrations at startup ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        app.Logger.LogInformation("Applying migrations...");
        db.Database.Migrate();
        app.Logger.LogInformation("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while applying migrations.");
        throw;
    }
}

app.Run();
