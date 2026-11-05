using JwtAuth.Data;
using JwtAuth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;
using Resend;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserDatabase")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Appsettings:Key"]!)),
            ValidateIssuerSigningKey = true,
        };

    });

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.AddScoped<ITaxCaseService, TaxCaseService>();

builder.Services.AddScoped<IProfileService, ProfileService>();

builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddScoped<IDocumentRequestService, DocumentRequestService>();

builder.Services.AddScoped<IDocumentRequestService, DocumentRequestService>();

builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddHttpClient<IGoHighLevelService, GoHighLevelService>();

builder.Services.AddScoped<IAdminUserService, AdminUserService>();

builder.Services.AddScoped<IClientNotificationService, ClientNotificationService>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

app.UseCors("ReactFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

string uploadsPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Uploads"
);

Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthorization();

app.MapControllers();

app.Run();
