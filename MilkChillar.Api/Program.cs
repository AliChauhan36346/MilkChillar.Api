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



var builder = WebApplication.CreateBuilder(args);

// 1. Add CORS service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

//builder.Services.AddAuthorization();

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
        "userpermission.create", "userpermission.read", "userpermission.delete"
    };





    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});


builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IRoleService,RoleService>();


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

app.UseCors("AllowLocalhost3000");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // 🛡️ must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.Run();
