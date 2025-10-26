
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using MilkChillar.Infrastructure;
//using MilkChillar.Application.Common.Settings;
//using Microsoft.EntityFrameworkCore;
//using MilkChillar.Application;
//using Microsoft.AspNetCore.Authorization;
//using MilkChillar.Infrastructure.Authorization;
//using MilkChillar.Application.Interfaces;
//using MilkChillar.Infrastructure.Services;

//var builder = WebApplication.CreateBuilder(args);

//// 1. Add CORS service - FIXED VERSION
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowLocalhost3000",
//        policy =>
//        {
//            policy.WithOrigins("http://localhost:3000")
//                  .AllowAnyHeader()
//                  .AllowAnyMethod();
//            //.AllowCredentials()  // ✅ CRITICAL: This was missing
//            //.SetPreflightMaxAge(TimeSpan.FromHours(1)); // ✅ Cache preflight requests
//        });
//});

//// ⬇️ 1. Load and bind JwtSettings
//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
//var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

//// ⬇️ 2. Register TokenService
//builder.Services.AddScoped<ITokenService, TokenService>();

//// ⬇️ 3. Configure JWT Authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtSettings.Issuer,
//        ValidAudience = jwtSettings.Audience,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
//        ClockSkew = TimeSpan.FromMinutes(5) // ✅ Allow 5 minute clock skew
//    };

//    // ✅ Handle authentication failures better
//    options.Events = new JwtBearerEvents
//    {
//        OnAuthenticationFailed = context =>
//        {
//            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
//            return Task.CompletedTask;
//        },
//        OnChallenge = context =>
//        {
//            Console.WriteLine($"Authentication challenge: {context.Error}");
//            return Task.CompletedTask;
//        }
//    };
//});

//builder.Services.AddAuthorization(options =>
//{
//    // Register dynamic policies based on permission name
//    var permissions = new[]
//    {
//        "mainaccount.create", "mainaccount.read", "mainaccount.update", "mainaccount.delete",
//        "subaccount.create", "subaccount.read", "subaccount.update", "subaccount.delete",
//        "account.create", "account.read", "account.update", "account.delete",
//        "supplier.create", "supplier.read", "supplier.update", "supplier.delete",
//        "buyer.create", "buyer.read", "buyer.update", "buyer.delete",
//        "employee.create", "employee.read", "employee.update", "employee.delete",
//        "user.create", "user.read", "user.update", "user.delete",
//        "role.create", "role.read", "role.update", "role.delete",
//        "rolepermission.create", "rolepermission.read", "rolepermission.delete",
//        "userpermission.create", "userpermission.read", "userpermission.delete",
//        "permission.read",
//        "chillar.create", "chillar.read", "chillar.update", "chillar.delete",
//        "chillarreceive.create", "chillarreceive.read", "chillarreceive.update", "chillarreceive.delete",
//        "sales.create", "sales.read", "sales.update", "sales.delete",
//        "purchase.create", "purchase.read", "purchase.update", "purchase.delete",
//        "stock.create", "stock.read", "stock.update", "stock.delete",
//        "openingBalance.read", "openingBalance.create", "openingBalance.update", "openingBalance.delete",
//        "cashPayment.create", "cashPayment.read", "cashPayment.update", "cashPayment.delete", "ledger.read", "parchi.read"
//        ,"profitloss.read","roznamcha.read"
//    };

//    foreach (var permission in permissions)
//    {
//        options.AddPolicy(permission, policy =>
//            policy.Requirements.Add(new PermissionRequirement(permission)));
//    }
//});

//builder.Services.AddHttpContextAccessor();

//builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
//builder.Services.AddScoped<IAccountService, AccountService>();
//builder.Services.AddScoped<ISupplierService, SupplierService>();
//builder.Services.AddScoped<IBuyerService, BuyerService>();
//builder.Services.AddScoped<IEmployeeService, EmployeeService>();
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
//builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
//builder.Services.AddScoped<IRoleService, RoleService>();
//builder.Services.AddScoped<IChillarReceiveService, ChillarReceiveService>();
//builder.Services.AddScoped<IChillarService, ChillarService>();
//builder.Services.AddScoped<ISaleService, SalesService>();
//builder.Services.AddScoped<IPurchaseService, PurchaseService>();
//builder.Services.AddScoped<IReportService, ReportService>();
//builder.Services.AddScoped<IAccountOpeningBalanceService, AccountOpeningBalanceService>();
//builder.Services.AddScoped<ICashPaymentService, CashPaymentService>();
//builder.Services.AddScoped<IAccountLedgerService, AccountLedgerService>();
//builder.Services.AddScoped<IParchiService, ParchiService>();
//builder.Services.AddScoped<IProfitLossService, ProfitLossService>();
//builder.Services.AddScoped<IRoznamchaService, RoznamchaService>();

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// ✅ Register ApplicationDbContext
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddSwaggerGen(options =>
//{
//    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
//        Description = "Enter 'Bearer' followed by your JWT token.\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6..."
//    });

//    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
//    {
//        {
//            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//            {
//                Reference = new Microsoft.OpenApi.Models.OpenApiReference
//                {
//                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});

//var app = builder.Build();

//// ✅ CORS must be first in the pipeline
//app.UseCors("AllowLocalhost3000");

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthentication(); // 🛡️ must come before UseAuthorization
//app.UseAuthorization();

//app.MapControllers();
//app.Run();




using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MilkChillar.Infrastructure;
using MilkChillar.Application.Common.Settings;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;
using Microsoft.AspNetCore.Authorization;
using MilkChillar.Infrastructure.Authorization;
using MilkChillar.Application.Interfaces;
using MilkChillar.Infrastructure.Services;
using Npgsql;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net; // Add this for IPv4 configuration

var builder = WebApplication.CreateBuilder(args);

// Force IPv4 for better compatibility with Railway
AppContext.SetSwitch("System.Net.DisableIPv6", true);

// Read env too (Render/Railway/Supabase will inject these)
builder.Configuration.AddEnvironmentVariables();

// ---------- CORS (multi-origin via env) ----------
//var allowedOriginsCsv = builder.Configuration["ALLOWED_ORIGINS"] ?? "http://localhost:3000";
var allowedOriginsCsv = builder.Configuration["ALLOWED_ORIGINS"] ?? "https://chuhandaries189.vercel.app";
var allowedOrigins = allowedOriginsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend",
        policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// ---------- JWT ----------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();

var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? jwtSettings.Issuer;
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? jwtSettings.Audience;
var secret = Environment.GetEnvironmentVariable("JWT_SECRETKEY") ?? jwtSettings.SecretKey;

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    });

// ---------- Authorization (unchanged) ----------
builder.Services.AddAuthorization(options =>
{
    var permissions = new[]
    {
        "mainaccount.create", "mainaccount.read", "mainaccount.update", "mainaccount.delete",
        "subaccount.create", "subaccount.read", "subaccount.update", "subaccount.delete",
        "account.create", "account.read", "account.update", "account.delete",
        "supplier.create", "supplier.read", "supplier.update", "supplier.delete",
        "buyer.create", "buyer.read", "buyer.update", "buyer.delete",
        "employee.create", "employee.read", "employee.update", "employee.delete",
        "user.create", "user.read", "user.update", "user.delete",
        "role.create", "role.read", "role.update", "role.delete",
        "rolepermission.create", "rolepermission.read", "rolepermission.delete",
        "userpermission.create", "userpermission.read", "userpermission.delete",
        "permission.read",
        "chillar.create", "chillar.read", "chillar.update", "chillar.delete",
        "chillarreceive.create", "chillarreceive.read", "chillarreceive.update", "chillarreceive.delete",
        "sales.create", "sales.read", "sales.update", "sales.delete",
        "purchase.create", "purchase.read", "purchase.update", "purchase.delete",
        "stock.create", "stock.read", "stock.update", "stock.delete",
        "openingBalance.read", "openingBalance.create", "openingBalance.update", "openingBalance.delete",
        "cashPayment.create", "cashPayment.read", "cashPayment.update", "cashPayment.delete", "ledger.read", "parchi.read","profitloss.read","roznamcha.read"
    };

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IChillarReceiveService, ChillarReceiveService>();
builder.Services.AddScoped<IChillarService, ChillarService>();
builder.Services.AddScoped<ISaleService, SalesService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAccountOpeningBalanceService, AccountOpeningBalanceService>();
builder.Services.AddScoped<ICashPaymentService, CashPaymentService>();
builder.Services.AddScoped<IAccountLedgerService, AccountLedgerService>();
builder.Services.AddScoped<IParchiService, ParchiService>();
builder.Services.AddScoped<IProfitLossService, ProfitLossService>();
builder.Services.AddScoped<IRoznamchaService, RoznamchaService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ---------- Swagger ----------
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ---------- Database (Railway/Supabase friendly) ----------
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Prefer DATABASE_URL (Railway, Supabase) if provided
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrWhiteSpace(databaseUrl) && databaseUrl.Contains("://"))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : "",
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require,
        TrustServerCertificate = true,
        // Force IPv4 and add connection timeout
        CommandTimeout = 30,
        Timeout = 30
    };
    connectionString = csb.ToString();
}
else if (builder.Environment.IsProduction())
{
    var host = Environment.GetEnvironmentVariable("db.hnyoaqctfaetyqvpenek.supabase.co");
    var db = Environment.GetEnvironmentVariable("postgres");
    var user = Environment.GetEnvironmentVariable("postgres");
    var pwd = Environment.GetEnvironmentVariable("aliAbbas346");

    if (!string.IsNullOrWhiteSpace(host))
        connectionString = $"Host={host};Database={db};Username={user};Password={pwd};SSL Mode=Require;Trust Server Certificate=true;Timeout=30;Command Timeout=30";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ---------- Host binding for Railway ----------
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// CORS
app.UseCors("Frontend");

// (optional) if using a proxy, accept x-forwarded-*
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});



// Swagger in dev OR when ENABLE_SWAGGER=true in env
if (app.Environment.IsDevelopment() || string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In container/proxy hosting, HTTPS redirection can cause loops.
// Keep it in dev only; let the platform terminate TLS.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Skip auto-migration in production - run migrations manually
if (app.Environment.IsDevelopment())
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Console.WriteLine("Development environment: Running migrations...");
            await db.Database.MigrateAsync();
            Console.WriteLine("Migrations completed successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed in development: {ex.Message}");
        throw;
    }
}
else
{
    Console.WriteLine("Production environment: Skipping auto-migration. Ensure database is properly migrated.");
}

app.Run();