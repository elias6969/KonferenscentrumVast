using KonferenscentrumVast.Data;
using KonferenscentrumVast.Repository.Implementations;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Services;
using KonferenscentrumVast.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Google.Cloud.SecretManager.V1;

var builder = WebApplication.CreateBuilder(args);

// --- Logging ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

string connectionString;

if (builder.Environment.IsDevelopment())
{
    // Use appsettings.Development.json locally
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
else
{
    // Use Secret Manager in Cloud Run
    var secretClient = SecretManagerServiceClient.Create();
    var secretName = new SecretVersionName("konferenscentrum-vast", "db-connection-string", "latest");
    var secret = secretClient.AccessSecretVersion(secretName);
    connectionString = secret.Payload.Data.ToStringUtf8();
}

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

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
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

var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionMapping();
app.UseHttpsRedirection();
app.UseCors("dev");
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
