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
using MilkChillar.Infrastructure.Services.Helpers;
using MilkChillar.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Support dynamic port binding for cloud providers (Render, Koyeb, Railway)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

// 1. Add CORS service supporting local and production frontend origins
var allowedOriginsCsv = builder.Configuration["ALLOWED_ORIGINS"] 
    ?? builder.Configuration["Cors:AllowedOrigins"] 
    ?? "http://localhost:3000,http://127.0.0.1:3000,https://chuhandaries189.vercel.app";

var allowedOrigins = allowedOriginsCsv
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// ⬇️ 1. Load and bind JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// ⬇️ 2. Register TokenService
builder.Services.AddScoped<ITokenService, TokenService>();

// ⬇️ 3. Configure JWT Authentication
builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.FromMinutes(5) // ✅ Allow 5 minute clock skew
    };

    // ✅ Handle authentication failures better
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"Authentication challenge: {context.Error}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // Register dynamic policies based on permission name
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
        "cashPayment.create", "cashPayment.read", "cashPayment.update", "cashPayment.delete", "ledger.read", "parchi.read"
        ,"profitloss.read","roznamcha.read", "bankPayment.read", "bankPayment.create", "bankPayment.update", "bankPayment.delete",
        "bankReceipt.read", "bankReceipt.create", "bankReceipt.update", "bankReceipt.delete", "cashReceipt.read",
        "cashReceipt.create", "cashReceipt.update", "cashReceipt.delete"
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
builder.Services.AddScoped<ITenantSetupService, TenantSetupService>();
builder.Services.AddScoped<ITenantsService, TenantsService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IDateTimeFilterService, DateTimeFilterService>();
builder.Services.AddScoped<IStockCalculationService, StockCalculationService>();
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
builder.Services.AddScoped<ICashReceiptService, CashReceiptService>();
builder.Services.AddScoped<IBankTransactionService, BankTransactionService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IFinancialYearService, FinancialYearService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Register ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token.\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6..."
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

var app = builder.Build();

// Forward headers from reverse proxies (Render, Koyeb, Railway, AWS, Cloudflare)
app.UseForwardedHeaders(new Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// ✅ CORS must be early in the pipeline
app.UseCors("AllowFrontend");

// Swagger UI - available in development or when explicitly enabled
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", true);
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Milk Chillar API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication(); // 🛡️ must come before UseAuthorization
app.UseAuthorization();

// Root health check endpoint for cloud liveness probes (Render/Koyeb/Railway)
app.MapGet("/", () => Results.Ok(new 
{ 
    status = "Healthy", 
    service = "MilkChillar.Api", 
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow 
}));

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapControllers();
app.Run();